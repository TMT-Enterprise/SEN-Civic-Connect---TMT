# Static and Security Checks

Every check below runs automatically on pull requests (CI workflow `.github/workflows/ci.yml`)
or on a schedule (Dependabot). Together they are the project's static and security checks for
M3. Related: R-16 (dependency and licence drift), R-17 (committed secrets), R-09 (module
boundaries), CR-003, [CI quality gate](ci-quality-gate.md).

| Check | Tool and configuration | Runs in | Fails the build when |
|---|---|---|---|
| Compiler and Roslyn analysers | `Directory.Build.props`: `AnalysisLevel=latest`, `EnforceCodeStyleInBuild=true`, `TreatWarningsAsErrors=true`, nullable enabled | Backend, Build step | Any compiler, analyser or code-style warning |
| Format check | `dotnet format --verify-no-changes`, rules from `.editorconfig` | Backend, Format check step | Any file differs from the agreed format |
| Architecture rules | `tests/api/UnitTests/ArchitectureTests.cs` (ADR-001, section 5.2) | Backend, Test step | A project reference breaks the module dependency matrix |
| NuGet vulnerability scan | NuGet audit at restore, plus `dotnet list package --vulnerable --include-transitive` | Backend, Restore and scan steps | A direct or transitive package has a known vulnerability |
| ESLint | `angular-eslint` and `typescript-eslint` via `npm run lint` | Frontend, Lint step | Any lint error in `src/web` TypeScript or templates |
| npm audit | `npm audit --audit-level=high` | Frontend, audit step | A high or critical advisory in the dependency tree |
| Secret scanning | Gitleaks v8.28.0, binary pinned by SHA-256, full git history | Secrets scan job | Any finding in any commit reachable from the checkout |
| Dependency updates | Dependabot for NuGet, npm and GitHub Actions, weekly, PRs target `development` | Scheduled | Not a gate; opens PRs that must pass the same checks |

## Secret scanning decision

The repository is private on a plan where GitHub's native secret scanning and push protection
may not be available. The team therefore relies on the Gitleaks job in CI, which meets the
"scanner runs in CI" alternative in S1-17. If native secret scanning is enabled later it adds
to, and does not replace, the CI job. Detection was demonstrated with a planted dummy secret
(see the evidence table in [ci-quality-gate.md](ci-quality-gate.md)).

## Known gaps

- Prettier is configured for the frontend (`src/web/.prettierrc`) but formatting is not
  checked in CI. ESLint covers correctness; a `format:check` script is a candidate follow-up.
- No SAST tool such as CodeQL is configured. Out of scope for S1; revisit with the M3 security
  expectations.
