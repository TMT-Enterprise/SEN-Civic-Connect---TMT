# Technical Debt Register

Deliberate or discovered shortcuts that the team has chosen to carry for now. An entry is
required before a CI check is relaxed, bypassed or given an exception (for example an npm or
NuGet advisory that has no fix available). Each entry names an owner and a review date.

| ID | Date | Area | Debt | Why it is carried | Risk / link | Owner | Review by | Status |
|---|---|---|---|---|---|---|---|---|
| TD-001 | 12/10/2026 | Backend tests | `ApiStartupTests` hard-codes a database connection string with a password literal and overrides any configured connection. | Test only calls `/health`, which makes no database call. Must change before the first database-backed integration test. | R-17 | TBC | Before first DB integration test | Open |
| TD-002 | 12/10/2026 | Backend | `Program.cs` contains commented-out debug code, and each module still has an empty `Class1.cs`. | Scaffold leftovers from S1; none affect behaviour. | Task definition of done: no commented-out code | TBC | End of S1 | Open |
| TD-003 | 12/10/2026 | Frontend | `test:ci` passes `--headless`, which the test runner ignores when no browser is configured. | Harmless warning in the log. | Noise in CI output | TBC | When a browser runner is added | Open |

## How to log an unfixable audit finding

1. Record the advisory ID, the package and version, whether it is a direct or transitive
   dependency, and whether it reaches production code or only dev tooling.
2. State why it cannot be fixed now (no patched version, breaking upgrade, upstream issue).
3. Name the compensating control, an owner and a review date.
4. Link the entry from the PR that changes the audit threshold or adds an exception.
5. Remove the exception, and close the entry, in the PR that applies the fix.
