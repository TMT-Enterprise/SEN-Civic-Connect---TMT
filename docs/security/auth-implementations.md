# Authentication implementation (S2-03, issue #113)

Covers FR-001, FR-002, NF-001 (token required), NF-003, ASR-1, R-05. Roles follow the team decision to use three:
Requester, Staff, and a combined Admin/Management role (change request to be recorded as CR-005).

## Decisions

| Topic | Decision | Reason / trade-off |
|---|---|---|
| Where auth lives | Custom ASP.NET Core Auth module (not Supabase Auth) | Matches ADR-001 and PED section 7.8; keeps `password_hash` in our schema and the hash/duplicate/config criteria testable in our own code. Supabase Auth was considered and not adopted (changes a baselined decision and the schema, and its duplicate-email behaviour depends on email-confirmation settings). |
| Hashing | bcrypt (BCrypt.Net-Next), work factor 12, via `IPasswordHasher` | One-way, salted, format is assertable in tests. bcrypt ignores input past 72 bytes, so passwords are limited to 72 bytes. |
| Tokens | HS256 JWT; claims `sub`, `acct`, `email`, `role` (exactly one of Requester, Staff, Admin) | `sub` is only unique within an account table, so `acct` (requester or employee) disambiguates. |
| Configuration | `Jwt:Secret`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpiryMinutes` validated at startup | The app refuses to start with a missing or short secret. The secret comes from user-secrets or environment variables and is never committed. |
| Registration | Creates Requesters only; the request has no role field | Staff and Admin accounts come from seed or admin tooling, so nobody can self-register as Admin. |
| Web registration | The registration form posts name, email, phone number, and password to `/api/v1/auth/register`; after creation it signs the requester in and routes to the requester dashboard | Passwords are hashed by the API; the phone number is stored on the requester record. |
| Email uniqueness | Normalised to lower-case; checked in both account tables; the database unique index is the real guard | Emails are only unique per table in the schema, so the cross-table check is application-level. |
| Failure messages | One generic 401 for unknown email, wrong password, inactive account or unusable stored hash; a dummy verification equalises timing | Prevents account enumeration through login. The register endpoint does report 409 for a duplicate, as FR-001 requires an error message. |
| Web sign-in | The login form posts to `/api/v1/auth/login`, stores the returned JWT and role in browser local storage, and sends the token as a Bearer token on API requests | The server-provided role selects the Requester, Staff, or Admin dashboard; role assignment remains server-controlled. |
| Last successful login | A successful credential check updates nullable `last_login_at` on the requester or employee account | Migration `006_account_last_login.sql` adds the field to both account tables; failed logins do not change it. |

## Evidence map (issue #113 checklist)

| Acceptance criterion | Evidence |
|---|---|
| Duplicate email rejected with an error message | `AuthServiceTests.Register_rejects_a_duplicate_email_in_any_casing`; `AuthEndpointsTests.Register_rejects_a_duplicate_email_in_any_casing_with_409_and_a_message`; concurrent registration test |
| Wrong password fails; correct login returns a token | `AuthEndpointsTests.Wrong_password_and_unknown_email_both_fail_with_the_same_401_response`; `Login_returns_a_token_whose_expiry_comes_from_configuration` |
| Web sign-in calls the API, persists the token, and routes each role | `src/web/src/app/core/auth.spec.ts`; `src/web/src/app/components/login/login.spec.ts` |
| Successful requester/staff/admin login updates the database | `AuthEndpointsTests.Successful_requester_login_updates_last_login_in_the_database`; `AuthEndpointsTests.Seeded_employees_log_in_with_their_own_role` |
| Stored value is a one-way hash | `BCryptPasswordHasherTests`; `Register_creates_the_account_and_stores_only_a_bcrypt_hash` (reads the real row) |
| JWT secret and expiry come from configuration | `JwtOptionsTests`; `JwtTokenServiceTests.Expiry_comes_from_the_configured_minutes`; token expiry in the integration test equals `Jwt:ExpiryMinutes` |

## Known limits (to track, not hidden)

- Rate limiting on the auth routes (ADR-006) is not implemented yet.
- No refresh tokens or logout (Forward Engineering Consideration 10).
- The seed file keeps a placeholder hash; the Development-only `DemoCredentialSeeder` replaces it from `Seed:DemoPassword`.