# CI Quality Gate

**Goal:** every pull request is built, tested and checked, and a failing check blocks merging.
Workflow: `.github/workflows/ci.yml`. Related: CR-003, R-16, R-17,
[static and security checks](static-and-security-checks.md),
[technical debt register](technical-debt-register.md).

## Required checks on `main`

Names must match the job `name` in `ci.yml` exactly. Renaming a job means updating the branch
ruleset and this table in the same PR. No job uses path filters.

| Required check (exact name) | What it does |
|---|---|
| `Repo sanity` | Critical files exist and are non-empty |
| `Backend (build, test, static, deps)` | Restore, format check, build, migrations on a Postgres 16 service container, tests with coverage, NuGet vulnerability scan; uploads `backend-test-evidence` |
| `Frontend (lint, test, build, deps)` | `npm ci`, ESLint, unit tests, production build, `npm audit --audit-level=high` |
| `Secrets scan` | Gitleaks over the full checked-out history |
| `CI gate` | Passes only if all four jobs above succeeded |
| `Branch policy` | Only `development` or `main-update/*` may open a PR into `main` (`branch-policy.yml`) |

Workflow settings: runs on pull requests to `development` and `main` and on pushes to both,
`contents: read` only, superseded runs on the same ref are cancelled, .NET SDK from
`global.json`, Node from `.nvmrc`.

## Evidence

Fill in the links as each demonstration is completed. Failure demonstrations use throwaway
branches that are never merged.

| Item | Method | Expected failing step | Evidence link |
|---|---|---|---|
| Green run | Normal PR into `development` | none | #90 |
| Build error | Reference an undefined symbol in `src/api` | Backend, Build | Evidence not tracked |
| Format drift | Add stray indentation to a `.cs` file | Backend, Format check | <img width="1417" height="148" alt="image" src="https://github.com/user-attachments/assets/fdc67421-fe62-41c0-b2ed-95f1ec28d8ef" />|
| Failing test | Invert an assertion in `tests/api` | Backend, Test | evidence not tracked |
| Vulnerable NuGet package | Add a package version with a known advisory to a test project | Backend, Restore (NuGet audit warning promoted to error) or the scan step | Evidence not tracked |
| Frontend failure | Introduce a lint error | Frontend, Lint | <img width="1415" height="457" alt="image" src="https://github.com/user-attachments/assets/6a9e57fe-696f-44a0-ba47-a9bb98cac7c8" /> |
| Vulnerable npm package | Install a version with a high or critical advisory in `src/web` | Frontend, npm audit | Evidence not tracked |
| Planted secret detected | Commit a dummy high-entropy key on a throwaway branch | Secrets scan | <img width="977" height="220" alt="image" src="https://github.com/user-attachments/assets/f9994437-a0fe-4dac-b612-9cdb07bc10d2" /> |
| Blocked merge on `main` | PR from `main-update/ci-block-demo` into `main` with a failing test | Backend and CI gate; merge button disabled | #86 <img width="1420" height="198" alt="image" src="https://github.com/user-attachments/assets/8999f1cf-f014-40b4-9fdf-782b1d596868" />|
| Check names match ruleset | Compare the ruleset's required list with the table above | n/a | <img width="956" height="233" alt="image" src="https://github.com/user-attachments/assets/24891365-67f0-4cda-b5af-9cb919f472d5" />|

