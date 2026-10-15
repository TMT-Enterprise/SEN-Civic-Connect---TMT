using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.IdentityModel.JsonWebTokens;

namespace CivicConnect.IntegrationTests;

[Collection(ApiCollection.Name)]
public sealed class AuthEndpointsTests(ApiFactory factory)
{
    private const string Password = "Str0ng!Passw0rd";

    private static string NewEmail() => $"user-{Guid.NewGuid():N}@example.com";

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>())!;

    private async Task<HttpResponseMessage> RegisterAsync(string email, string password = Password, object? extra = null) =>
        await factory.CreateClient().PostAsJsonAsync("/api/v1/auth/register",
            extra ?? new { name = "Test User", email, password });

    private async Task<HttpResponseMessage> LoginAsync(string email, string password) =>
        await factory.CreateClient().PostAsJsonAsync("/api/v1/auth/login", new { email, password });

    private async Task<string> LoginForTokenAsync(string email, string password)
    {
        var response = await LoginAsync(email, password);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await ReadJsonAsync(response)).GetProperty("accessToken").GetString()!;
    }

    // ---- Duplicate email is rejected with an error message (FR-001)

    [Fact]
    public async Task Register_creates_the_account_and_stores_only_a_bcrypt_hash()
    {
        var email = NewEmail();

        var response = await RegisterAsync(email);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("Requester", (await ReadJsonAsync(response)).GetProperty("role").GetString());

        var stored = await factory.ScalarAsync<string>(
            "select password_hash from requester where lower(email) = @e", ("e", email));
        Assert.NotNull(stored);
        Assert.NotEqual(Password, stored);
        Assert.StartsWith("$2", stored, StringComparison.Ordinal);
        Assert.Equal(60, stored.Length);
    }

    [Fact]
    public async Task Register_rejects_a_duplicate_email_in_any_casing_with_409_and_a_message()
    {
        var email = NewEmail();
        Assert.Equal(HttpStatusCode.Created, (await RegisterAsync(email)).StatusCode);

        var duplicate = await RegisterAsync(email.ToUpperInvariant());

        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace((await ReadJsonAsync(duplicate)).GetProperty("detail").GetString()));
        Assert.Equal(1, await factory.ScalarAsync<long>(
            "select count(*) from requester where lower(email) = @e", ("e", email)));
    }

    [Fact]
    public async Task Register_rejects_an_email_already_used_by_an_employee()
    {
        var response = await RegisterAsync("thandi.mokoena@example.com");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Two_simultaneous_registrations_of_one_email_give_exactly_one_account()
    {
        var email = NewEmail();

        var responses = await Task.WhenAll(RegisterAsync(email), RegisterAsync(email));

        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Created);
        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Conflict);
        Assert.Equal(1, await factory.ScalarAsync<long>(
            "select count(*) from requester where lower(email) = @e", ("e", email)));
    }

    [Fact]
    public async Task Register_rejects_invalid_input_with_422_and_field_errors()
    {
        var response = await RegisterAsync("not-an-email", "short");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var errors = (await ReadJsonAsync(response)).GetProperty("errors");
        Assert.True(errors.TryGetProperty("email", out _));
        Assert.True(errors.TryGetProperty("password", out _));
    }

    [Fact]
    public async Task A_role_sent_in_the_register_body_is_ignored()
    {
        var email = NewEmail();

        var response = await RegisterAsync(email, extra: new { name = "Sneaky", email, password = Password, role = "Admin" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var token = new JsonWebTokenHandler().ReadJsonWebToken(await LoginForTokenAsync(email, Password));
        Assert.Equal("Requester", token.GetClaim("role").Value);
    }

    // ---- Wrong password fails; correct login returns a token (FR-002)

    [Fact]
    public async Task Login_returns_a_token_whose_expiry_comes_from_configuration()
    {
        var email = NewEmail();
        await RegisterAsync(email);

        var response = await LoginAsync(email, Password);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadJsonAsync(response);
        Assert.Equal("Bearer", body.GetProperty("tokenType").GetString());

        var token = new JsonWebTokenHandler().ReadJsonWebToken(body.GetProperty("accessToken").GetString()!);
        Assert.Equal(TimeSpan.FromMinutes(ApiFactory.ExpiryMinutes), token.ValidTo - token.ValidFrom);
        Assert.Equal("Requester", token.GetClaim("role").Value);
    }

    [Fact]
    public async Task Wrong_password_and_unknown_email_both_fail_with_the_same_401_response()
    {
        var email = NewEmail();
        await RegisterAsync(email);

        var wrongPassword = await LoginAsync(email, "not-the-password");
        var unknownEmail = await LoginAsync(NewEmail(), Password);

        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unknownEmail.StatusCode);

        var a = await ReadJsonAsync(wrongPassword);
        var b = await ReadJsonAsync(unknownEmail);
        Assert.Equal(a.GetProperty("title").GetString(), b.GetProperty("title").GetString());
        Assert.Equal(a.GetProperty("status").GetInt32(), b.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task An_inactive_account_cannot_log_in()
    {
        var email = NewEmail();
        await RegisterAsync(email);
        await factory.ExecuteAsync("update requester set is_active = false where lower(email) = @e", ("e", email));

        Assert.Equal(HttpStatusCode.Unauthorized, (await LoginAsync(email, Password)).StatusCode);
    }

    [Fact]
    public async Task An_unusable_stored_hash_gives_401_not_a_server_error()
    {
        var email = NewEmail();
        await RegisterAsync(email);
        await factory.ExecuteAsync(
            "update requester set password_hash = 'DEMO_ONLY_NOT_A_VALID_PASSWORD_HASH' where lower(email) = @e", ("e", email));

        Assert.Equal(HttpStatusCode.Unauthorized, (await LoginAsync(email, Password)).StatusCode);
    }

    // ---- Roles: seeded employees get Staff / Admin; exactly two employee roles exist

    [Theory]
    [InlineData("pieter.vanwyk@example.com", "Staff")]
    [InlineData("thandi.mokoena@example.com", "Admin")]
    public async Task Seeded_employees_log_in_with_their_own_role(string email, string expectedRole)
    {
        var token = new JsonWebTokenHandler().ReadJsonWebToken(await LoginForTokenAsync(email, ApiFactory.DemoPassword));

        Assert.Equal(expectedRole, token.GetClaim("role").Value);
        Assert.Equal("employee", token.GetClaim("acct").Value);
    }

    [Fact]
    public async Task The_database_seeds_only_the_staff_and_admin_employee_roles()
    {
        var names = await factory.ScalarAsync<string>("select string_agg(name, ',' order by name) from employee_role");

        Assert.Equal("Admin,Staff", names);
    }

    // ---- Authenticated access (NF-001): the token actually works, and no token does not

    [Fact]
    public async Task Me_requires_a_token_and_reports_the_callers_role()
    {
        var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/auth/me")).StatusCode);

        var email = NewEmail();
        await RegisterAsync(email);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", await LoginForTokenAsync(email, Password));

        var me = await client.GetAsync("/api/v1/auth/me");

        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
        Assert.Equal("Requester", (await ReadJsonAsync(me)).GetProperty("role").GetString());
    }

    [Fact]
    public async Task A_tampered_token_is_rejected()
    {
        var email = NewEmail();
        await RegisterAsync(email);
        var token = await LoginForTokenAsync(email, Password);

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token[..^3] + "abc");

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/auth/me")).StatusCode);
    }
}