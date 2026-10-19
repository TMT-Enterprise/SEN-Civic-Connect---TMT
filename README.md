# CivicConnect

**Community Service Request Management Platform**  
SEN381 Integrated Software Engineering Project; 2026

## Overview

Community-focused organisations often manage service requests, facility faults, IT support, maintenance issues, lost property, and similar concerns through a fragmented mix of emails, phone calls, and paper records. Requests can be lost or duplicated, requesters have limited visibility into progress, staff struggle to coordinate work, and management lacks reliable information about outstanding and overdue requests.

CivicConnect is a controlled web platform that provides:

- **Requesters:** A centralised way to submit requests and track their progress.
- **Staff:** A coordinated request queue with defined status transitions and accountability.
- **Administrators and management:** Visibility into requests, assignments, overdue items, and service performance.
- **The project team:** A modular, testable application with documented requirements, controlled changes, and automated quality checks.

## Team

| Name | Student Number | Primary Area |
|---|---|---|
| Tinyiko Siwele | 601726 | Frontend |
| Mutshidzi Nduvheni | 601781 | Backend |
| Thabang Molise | 601586 | Backend / GitHub Governance |

All three team members remain jointly accountable for the complete system, regardless of their primary areas of responsibility.

## Project Status

**Milestone 1 — Engineering Foundation and Requirements Baseline: Complete**

Problem and stakeholder analysis, scope baseline, requirements and acceptance criteria, initial requirements traceability matrix (RTM), risk register, forward engineering considerations, and GitHub governance were baselined and signed off.

**Milestone 2 — Architecture, Technology and Initial Design Baseline: Complete**

Architectural significance, quality drivers, and initial design decisions were documented. Persistence, technology-stack, and API/integration decisions are recorded in the relevant project documentation.

**Milestone 3 — Testing and Deployment: Current**

Focuses on comprehensive backend and frontend testing, CI/CD pipelines, containerisation, and deployment configuration.

Refer to `docs/PED/` for the controlled milestone documentation and current evidence.

## Technology Stack

| Technology | Version / Configuration | Purpose |
|---|---|---|
| Angular | 21.x | Frontend application |
| TypeScript | 5.9.x | Frontend development |
| Node.js | 24.x LTS | JavaScript runtime and Angular tooling |
| npm | 11.x | Frontend dependency management |
| C# | 14 | Backend development |
| .NET | 10.x | Backend runtime and SDK |
| ASP.NET Core | 10.x | REST API |
| PostgreSQL | Supabase-managed or compatible local PostgreSQL | Relational database |
| Entity Framework Core / Npgsql | Compatible with .NET 10 | Database access |
| xUnit | 3.x | Backend testing |
| Cypress | 16.x | End-to-end testing |
| Docker | Compatible with the project's database configuration | Local database/container support |
| OpenAPI | 3.x | API contract documentation |
| Swagger UI | Development configuration | Interactive API exploration |
| Git / GitHub | Supported versions | Source control and collaboration |

Check the repository's project files for the exact installed dependency versions.

## Prerequisites

Install the following before running CivicConnect.

| Requirement | Purpose | Verification |
|---|---|---|
| Git | Clone the repository and manage branches | `git --version` |
| .NET 10 SDK | Build, run, and test the backend | `dotnet --list-sdks` |
| Node.js 24.x LTS | Run Angular and Cypress tooling | `node --version` |
| npm 11.x | Install frontend dependencies | `npm --version` |
| Docker Desktop or Docker Engine | Run a local PostgreSQL container, if configured | `docker --version` |
| PostgreSQL database access | Connect to the local or Supabase database | Verify using your database client |

**Recommended development tools**

- Visual Studio Code for frontend and general development.
- Visual Studio or Visual Studio Code with C# tooling for backend development.
- pgAdmin or another PostgreSQL client for inspecting database tables.
- GitHub access for branches, pull requests, and CI checks.

Ensure the .NET SDK, Node.js, npm, and Docker are available in your terminal after installation.

## Getting Started

### 1. Clone the repository

Open PowerShell or a terminal and run:

```powershell
git clone https://github.com/TMT-Enterprise/SEN-Civic-Connect---TMT.git
cd SEN-Civic-Connect---TMT
```

If you have already cloned the repository, navigate to its root directory.

```powershell
git status
git branch --show-current
git pull
```

Follow the branch and pull-request workflow defined in `docs/governance/github-governance.md`. Do not work directly on protected `main`.

### 2. Restore and build the backend

From the repository root:

```powershell
dotnet restore
dotnet build
```

If the solution file is not located at the repository root, run the commands against the appropriate `.sln` file or backend project.

A successful build confirms that the backend projects compile, but does not guarantee database connectivity.

### 3. Install frontend dependencies

Navigate to the Angular application directory:

```powershell
cd src/web
npm ci
```

Use `npm ci` when a compatible `package-lock.json` is committed. Otherwise, follow the project's dependency-management conventions.

To inspect the available frontend commands:

```powershell
npm run
```

### 4. Configure the database

CivicConnect requires a PostgreSQL database containing the expected schema and reference data.

#### Option A: Use Supabase

1. Obtain the development database connection details through the team's approved secret-sharing method.
2. Configure the backend connection string using the configuration key expected by the application.
3. Verify that the database contains the required tables, schema changes, and reference data.
4. Ensure the database network and access rules allow connections from your development environment.

#### Option B: Use local PostgreSQL

If the repository contains a Docker Compose configuration, start the configured services from the repository root:

```powershell
docker compose up -d
docker compose ps
```

If no Compose configuration exists, follow the team's documented local PostgreSQL setup.

Verify that the database contains the required CivicConnect schema and reference data, including applicable tables such as:

- `requester`
- `employee`
- `employee_role`
- `service_category`
- `request_status`
- `service_request`
- `request_assignment`
- `request_status_history`
- `notification`

**Important:** Starting PostgreSQL does not automatically create the application schema. Follow the repository's documented migration and seeding instructions. Do not execute initial-schema scripts against an existing database without checking their effects.

The project has previously used a local database named `TMT_civicconnect`. Confirm the current database name and configuration before connecting.

### 5. Configure backend environment variables

The backend requires a valid database connection and the appropriate CORS configuration.

The Angular frontend normally runs at:

```text
http://localhost:4200
```

For a local PowerShell session, the configuration may look like this:

```powershell
$env:Cors__Origins = "http://localhost:4200"
$env:ConnectionStrings__Default = "<your-development-database-connection-string>"
```

Replace the connection-string placeholder locally with your development database details.

These variable names must match the configuration used by the current backend. Never commit passwords, access tokens, or real database connection strings to GitHub.

The environment variables apply only to the current PowerShell session. Configure them again in a new terminal or use the project's approved local secrets configuration.

### 6. Start the backend API

Open a terminal at the repository root:

```powershell
cd src/api/Api
dotnet run
```

Check the API's actual listening address in the terminal output or `Properties/launchSettings.json`.

The current frontend proxy is configured to forward health requests to:

```text
http://localhost:5228
```

If the API runs on another port, update the proxy configuration or launch settings consistently.

Test the health endpoints in PowerShell:

```powershell
Invoke-RestMethod http://localhost:5228/health
Invoke-RestMethod http://localhost:5228/health/ready
```

- `/health` checks API health.
- `/health/ready` checks readiness according to the configured readiness checks, including database connectivity where configured.

If readiness fails, inspect the API logs and verify database availability, credentials, and schema.

If Swagger UI is enabled in the Development environment, open the Swagger URL printed in the API startup output.

### 7. Start the Angular frontend

Open a **second terminal** and navigate to the repository root:

```powershell
cd src/web
npm start
```

If `npm start` is not defined in `package.json`, use the configured development command, commonly:

```powershell
npx ng serve
```

Open the application in your browser:

**http://localhost:4200**

The proxy configuration is located at `src/web/proxy.conf.json`. Ensure its target matches the actual API address.

Keep the database, API, and frontend running simultaneously while testing database-dependent functionality.

## Local Development Addresses

| Component | Address / Configuration |
|---|---|
| Angular frontend | `http://localhost:4200` |
| ASP.NET Core API | Check launch settings; proxy currently targets port `5228` |
| API health | `http://localhost:5228/health` |
| API readiness | `http://localhost:5228/health/ready` |
| PostgreSQL | Configured local host/port or Supabase connection |
| Swagger UI | Development URL printed by the API |

## Running Tests

### Backend tests — xUnit

From the repository root, locate the test projects:

```powershell
Get-ChildItem -Path tests,src -Recurse -Filter *.csproj
```

Run the appropriate test project using its actual path:

```powershell
dotnet test <path-to-test-project.csproj>
```

If the repository contains a solution file, you can run:

```powershell
dotnet test <path-to-solution.sln>
```

Backend tests should cover applicable service logic, validation, request ownership, status transitions, error handling, and API behaviour.

### Frontend linting and build

From `src/web`, inspect the available scripts:

```powershell
npm run
```

Run the relevant scripts defined in `package.json`. For example:

```powershell
npm run lint
npm run build
```

Only run scripts that exist in the current project configuration.

### End-to-end tests — Cypress

Start the database, API, and frontend before running smoke tests that depend on the application.

From the repository root:

```powershell
cd tests/e2e
$env:CYPRESS_BASE_URL = "http://localhost:4200"
npx cypress run
```

If the E2E dependencies are not installed, follow the E2E project's package configuration and lockfile.

To open Cypress interactively:

```powershell
npx cypress open
```

Use test accounts and non-production data. Never include passwords or private database details in test evidence.

## Troubleshooting

| Problem | Checks and possible resolution |
|---|---|
| `dotnet` is not recognised | Install the .NET 10 SDK and reopen the terminal. |
| `node` or `npm` is not recognised | Install the supported Node.js version and verify PATH configuration. |
| Port 4200 is already in use | Stop the existing Angular process or use another port and update the test base URL. |
| API port conflict | Check `launchSettings.json` and align the frontend proxy with the actual API port. |
| `/health/ready` returns 503 | Inspect API logs, database connectivity, credentials, and required schema. |
| Database authentication fails | Verify host, port, username, password, and database name locally. Never share secrets in chat or GitHub. |
| Frontend requests fail | Check API availability, proxy configuration, CORS settings, and browser console errors. |
| Missing tables or reference data | Follow the documented schema, migration, and seeding procedure. |
| `npm ci` fails | Confirm that `package-lock.json` matches `package.json` and that supported Node.js/npm versions are installed. |
| Cypress cannot connect | Start the application services and verify `CYPRESS_BASE_URL`. |
| Lint or build failures | Review the reported file and line, fix the issue, and rerun the checks. |

## Repository Structure

Controlled project evidence is maintained under `docs/`.

| Path | Contents |
|---|---|
| `src/web/` | Angular frontend |
| `src/api/` | ASP.NET Core API and backend projects |
| `tests/` | Automated tests, including E2E tests where configured |
| `docs/PED/` | Project Engineering Document and milestone baselines |
| `docs/requirements/` | Stakeholder analysis, scope baseline, requirements, and RTM |
| `docs/architecture/` | Architectural significance, quality drivers, and diagrams |
| `docs/decisions/` | Architecture and technology decision records |
| `docs/risk/` | Risk register and probability/impact matrix |
| `docs/change/` | Change requests and impact analyses |
| `docs/governance/` | GitHub governance, team accountability, and AI usage register |
| `docs/deployment/` | Deployment compatibility and environment considerations |
| `docs/assignments/` | Assignment research informing project decisions |

Consult the repository tree and relevant documentation for database scripts, migrations, seed data, and environment-specific configuration.

## Repository Governance

- `main` is protected against direct pushes.
- Pull requests require two independent approvals, according to the configured repository rules.
- Developers follow the prescribed branching workflow and regularly integrate changes from the shared development baseline.
- A dedicated hotfix process and CI controls govern critical fixes.
- Automated checks, including build, test, lint, and dependency audit, run through the configured pull-request workflow.

Refer to `docs/governance/github-governance.md` and `docs/change/CR-002-branching-and-governance.md` for the detailed rules.

Before opening a pull request:

1. Synchronise with the appropriate base branch.
2. Run the relevant build and tests.
3. Review changed files and ensure secrets or unnecessary generated files are not committed.
4. Update controlled documentation when changes affect requirements, architecture, risks, deployment, or project decisions.
5. Provide the evidence required by the relevant issue and acceptance criteria.

## Security and Configuration

- Never commit passwords, access tokens, private connection strings, or production secrets.
- Use development-only credentials and non-production data for local testing.
- Restrict CORS origins to the intended environments.
- Do not expose sensitive configuration through logs, screenshots, or public issue comments.
- Review database migrations before applying them to shared or existing databases.
- Do not treat local development settings as production-ready deployment configuration.

## Academic Context

CivicConnect is developed as the integrated team project for Software Engineering 381 (SEN381), during the 2026 academic year. It is intended for academic demonstration and evaluation, not production use.

For authoritative requirements, implementation decisions, testing evidence, and milestone status, consult the controlled documentation under `docs/`.
