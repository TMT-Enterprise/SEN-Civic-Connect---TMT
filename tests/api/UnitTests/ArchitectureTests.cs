using System.Xml.Linq;
using Xunit;

namespace CivicConnect.UnitTests;

/// <summary>
/// Architecture Tests enforcing ADR-001 and §5.2 Modular Monolith Dependency Rules.
/// Basis: S1-06 / ADR-001 / R-09 / Issue #61.
///
/// Dependency rules baseline:
/// - Data: zero internal project references.
/// - Auth: Data only.
/// - Requests: Data only.
/// - Notifications: Data and Requests only.
/// - Api: Auth, Data, Notifications, Requests (composition root).
/// </summary>
public class ArchitectureTests
{
    private static readonly string SolutionRoot = FindSolutionRoot();

    private static string FindSolutionRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(dir))
        {
            if (File.Exists(Path.Combine(dir, "CivicConnect.slnx")) ||
                File.Exists(Path.Combine(dir, "CivicConnect.sln")))
            {
                return dir;
            }

            var parent = Directory.GetParent(dir);
            if (parent == null) break;
            dir = parent.FullName;
        }

        throw new DirectoryNotFoundException("Could not locate solution root containing CivicConnect.slnx");
    }

    private static List<string> GetProjectReferences(string projectRelativePath)
    {
        var csprojPath = Path.Combine(SolutionRoot, projectRelativePath);
        if (!File.Exists(csprojPath))
        {
            throw new FileNotFoundException($"Project file not found: {csprojPath}");
        }

        var doc = XDocument.Load(csprojPath);
        return doc.Descendants("ProjectReference")
            .Select(pr => pr.Attribute("Include")?.Value)
            .Where(include => !string.IsNullOrWhiteSpace(include))
            .Select(include => Path.GetFileNameWithoutExtension(include!))
            .ToList();
    }

    [Fact(DisplayName = "S1-06 / ADR-001 / §5.2: Data project must have zero internal project references")]
    public void DataProject_MustNotReference_AnyInternalProjects()
    {
        // Basis: ADR-001 §5.2: Data access layer is foundational; forbidden from referencing any modules or Api.
        var references = GetProjectReferences(Path.Combine("src", "api", "Data", "CivicConnect.Data.csproj"));

        Assert.True(
            references.Count == 0,
            $"Architecture violation (ADR-001): CivicConnect.Data must not have any project references, but found: [{string.Join(", ", references)}]"
        );
    }

    [Fact(DisplayName = "S1-06 / ADR-001 / §5.2: Auth project must only reference Data")]
    public void AuthProject_MustOnlyReference_DataProject()
    {
        // Basis: ADR-001 §5.2: Auth module must only depend on Data.
        var references = GetProjectReferences(Path.Combine("src", "api", "Auth", "CivicConnect.Auth.csproj"));

        var forbidden = references.Where(r => r != "CivicConnect.Data").ToList();
        Assert.True(
            forbidden.Count == 0,
            $"Architecture violation (ADR-001): CivicConnect.Auth has forbidden references: [{string.Join(", ", forbidden)}]. Allowed: [CivicConnect.Data]"
        );
        Assert.Contains("CivicConnect.Data", references);
    }

    [Fact(DisplayName = "S1-06 / ADR-001 / §5.2: Requests project must only reference Data")]
    public void RequestsProject_MustOnlyReference_DataProject()
    {
        // Basis: ADR-001 §5.2: Requests module must only depend on Data.
        var references = GetProjectReferences(Path.Combine("src", "api", "Requests", "CivicConnect.Requests.csproj"));

        var forbidden = references.Where(r => r != "CivicConnect.Data").ToList();
        Assert.True(
            forbidden.Count == 0,
            $"Architecture violation (ADR-001): CivicConnect.Requests has forbidden references: [{string.Join(", ", forbidden)}]. Allowed: [CivicConnect.Data]"
        );
        Assert.Contains("CivicConnect.Data", references);
    }

    [Fact(DisplayName = "S1-06 / ADR-001 / §5.2: Notifications project must only reference Data and Requests")]
    public void NotificationsProject_MustOnlyReference_DataAndRequests()
    {
        // Basis: ADR-001 §5.2: Notifications module depends only on Data and Requests.
        var references = GetProjectReferences(Path.Combine("src", "api", "Notifications", "CivicConnect.Notifications.csproj"));

        var allowed = new HashSet<string> { "CivicConnect.Data", "CivicConnect.Requests" };
        var forbidden = references.Where(r => !allowed.Contains(r)).ToList();

        Assert.True(
            forbidden.Count == 0,
            $"Architecture violation (ADR-001): CivicConnect.Notifications has forbidden references: [{string.Join(", ", forbidden)}]. Allowed: [{string.Join(", ", allowed)}]"
        );
        Assert.Contains("CivicConnect.Data", references);
        Assert.Contains("CivicConnect.Requests", references);
    }

    [Fact(DisplayName = "S1-06 / ADR-001 / §5.2: Api project must only reference agreed internal modules")]
    public void ApiProject_MustOnlyReference_AgreedModules()
    {
        // Basis: ADR-001 §5.2: Composition root references all internal modules.
        var references = GetProjectReferences(Path.Combine("src", "api", "Api", "CivicConnect.Api.csproj"));

        var allowed = new HashSet<string>
        {
            "CivicConnect.Auth",
            "CivicConnect.Data",
            "CivicConnect.Notifications",
            "CivicConnect.Requests"
        };

        var forbidden = references.Where(r => !allowed.Contains(r)).ToList();
        Assert.True(
            forbidden.Count == 0,
            $"Architecture violation (ADR-001): CivicConnect.Api has forbidden references: [{string.Join(", ", forbidden)}]. Allowed: [{string.Join(", ", allowed)}]"
        );
    }

    [Theory(DisplayName = "S1-06 / ADR-001 / §5.2: Strict enforcement across all project dependency definitions")]
    [InlineData("src/api/Data/CivicConnect.Data.csproj", new string[] { })]
    [InlineData("src/api/Auth/CivicConnect.Auth.csproj", new string[] { "CivicConnect.Data" })]
    [InlineData("src/api/Requests/CivicConnect.Requests.csproj", new string[] { "CivicConnect.Data" })]
    [InlineData("src/api/Notifications/CivicConnect.Notifications.csproj", new string[] { "CivicConnect.Data", "CivicConnect.Requests" })]
    [InlineData("src/api/Api/CivicConnect.Api.csproj", new string[] { "CivicConnect.Auth", "CivicConnect.Data", "CivicConnect.Notifications", "CivicConnect.Requests" })]
    public void AllProjects_MustStrictlyComplyWith_Adr001ReferenceMatrix(string projectRelativePath, string[] expectedAllowed)
    {
        // Basis: ADR-001 / §5.2 dependency rules matrix: Any reference not in expectedAllowed is forbidden.
        var projectName = Path.GetFileNameWithoutExtension(projectRelativePath);
        var references = GetProjectReferences(projectRelativePath);
        var expectedSet = expectedAllowed.ToHashSet();

        var forbiddenReferences = references.Where(r => !expectedSet.Contains(r)).ToList();

        Assert.True(
            forbiddenReferences.Count == 0,
            $"Architecture test failed for '{projectName}'. Forbidden project reference(s) detected: [{string.Join(", ", forbiddenReferences)}]. Allowed references: [{string.Join(", ", expectedAllowed)}]."
        );
    }
}