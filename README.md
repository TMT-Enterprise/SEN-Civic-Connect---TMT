# CivicConnect

**Community Service Request Management Platform**
SEN381 Integrated Software Engineering Project, 2026

## Overview

Community-focused organisations often manage service requests, facility faults, IT support,
maintenance issues, lost property and similar, through a fragmented mix of email, phone calls,
and paper records. Requests get lost or duplicated, requesters have no visibility into
progress, staff struggle to prioritise and coordinate work, and management has no reliable way
to report on outstanding or overdue requests.

CivicConnect is a controlled web platform that gives requesters a single, traceable way to
submit and track service requests, gives staff a coordinated queue with defined status
transitions and accountability, and gives management real visibility into service performance.

## Team

| Name | Student Number | Primary Area |
|---|---|---|
| Tinyiko Siwele | 601726 | Frontend |
| Mutshidzi Nduvheni | 601781 | Backend |
| Thabang Molise | 601586 | Backend / GitHub Governance |

All three members remain jointly accountable for the complete system, not only their primary
area, per the project's individual accountability standard.

## Project Status

**Milestone 1 (Engineering Foundation & Requirements Baseline): complete.**
Problem and stakeholder analysis, scope baseline, constraints, requirements with acceptance
criteria, initial RTM, initial risk register, forward engineering considerations, and GitHub
governance are baselined and signed off. See `docs/PED/PED-v1.0.md`.

**Milestone 2 (Architecture, Technology & Initial Design Baseline): complete**
ASRs and quality drivers are defined. Initial design decisions (notification handling, status
transition logic) are in ADR form. Persistence, technology-stack, and API/integration decisions
are still being finalised. See `docs/PED/PED-v2.0.md`.

**Milestone 3 (Testing & Deployment): current**
Focuses on comprehensive backend/frontend testing, CI/CD pipelines, containerisation, and deployment configurations. 
See `docs/PED/PED-v3.0.md` for current status.


## Local development setup

Prerequisites: Node.js 24.x and npm 11.x, .NET 10 SDK, Docker with Compose, and a PostgreSQL database.

1. Copy `.env.example` to `.env` and set database credentials plus a strong `Jwt__Secret` (at least 32 characters). Load it into the shell before running the migration command (`set -a; source .env; set +a` in Bash).
2. Start the database with `docker compose up -d` from the repository root.
3. Apply SQL migrations using `npm install` and then `node database/scripts/migrate.js` (set `CIVICCONNECT_DATABASE_URL` or `CIVICCONNECT_DB_PASSWORD` in your shell; the migration runner does not automatically consume the ASP.NET connection-string setting).
4. Start the API with `dotnet run --project src/api/Api/CivicConnect.Api.csproj`. The API listens on `http://localhost:5228` in the checked-in launch profile.
5. In another terminal run `cd src/web && npm ci && npm start`. Angular runs on `http://localhost:4200`; `/api` and `/health` are proxied to the API.
6. Run backend tests with `dotnet test CivicConnect.slnx`; run frontend tests with `cd src/web && npm test`; run Cypress from `tests/e2e` after both services are running.

Email is disabled by default. To exercise delivery locally, set `Email__Enabled=true`, configure SMTP host/from address, and start Mailpit with `docker compose up -d mailpit`, then set `Email__Enabled=true`, `Email__Host=localhost`, `Email__Port=1025`, and `Email__FromAddress=noreply@civicconnect.local` in the API process environment. Open `http://localhost:8025` to inspect messages. Keep `Email__Enabled=false` until the SMTP settings are configured. Do not enable real email delivery without valid SMTP configuration.

## Documentation

All controlled project evidence lives under `docs/`, not in this README:

| Folder | Contents |
|---|---|
| `docs/PED/` | The Project Engineering Document (PED) and baseline sign-offs |
| `docs/requirements/` | Stakeholder analysis, scope baseline, requirements, RTM |
| `docs/architecture/` | ASRs/quality drivers, architecture decision and diagrams |
| `docs/decisions/` | ADRs for architecture, persistence, technology and design decisions |
| `docs/risk/` | Risk register and probability/impact matrix |
| `docs/change/` | Change requests and impact analyses |
| `docs/governance/` | GitHub governance, team accountability, AI usage register |
| `docs/deployment/` | Deployment compatibility and environment considerations |
| `docs/assignments/` | Assignment research (A2, A3) that has informed project decisions |

## Technology Stack

| Technology | Version | Justification |
|---|---|---|
| Angular | 21.x | Chosen frontend framework |
| TypeScript | 5.9x | Angular 21 requires >=5.9.0 <6.0.0 |
| Node.js | 24.x LTS | Angular 21 supports Node 24; LTS gives a stable development environment. |
| npm | 11.x | Bundled with Node |
| C# | 14 | Paored with .Net 10 |
| .NET | 10.x | Builds/runs the backend and provides C# 14 |
| ASP.NET Core | 10.x | Backend REST API; matches .NET 10 |
| PostgreSQL | Supabase-managed | Supabase manages actual DB version |
| xUnit | 3.x | Backend testing |
| Cypress | 16.x | End-to-end testing |
| Docker Engine | 29.8.1 | API architectural style, implemented over HTTP/HTTPS |
| OpenAPI | 3.x | Define API contracts |
| Swagger UI | Compatible with OpenAPI setup | Interactive API documentation/testing |
| GitHub | Current supported | Source/version control |


## Repository Governance

- `main` is protected: no direct pushes, pull requests require two independent approvals.
- Each developer works from a persistent personal branch, pulling from `main` regularly
  (see `docs/change/CR-002-branching-and-governance.md`).
- A dedicated hotfix branch and a CI check freeze `main` to other merges while a critical fix
  is in progress.
- Automated checks (build, test, lint, dependency audit) run on every pull request.

Full detail: `docs/governance/github-governance.md`.

## Getting Started

Setup and run instructions will be added here once the initial application scaffold is in
place, tracked under the Milestone 2 development issues. This section will be updated before
any milestone requiring a runnable application.


## Testing and Quality Assurance

*Backend Unit Testing:* Implemented using xUnit 3.x for business logic, services, and controller verification.
*End-to-End (E2E) Testing:* Uses Cypress 16.x to execute smoke and integration flows against the application.


### Run the smoke test

From the repository root, execute:

```powershell
cd tests/e2e
$env:CYPRESS_BASE_URL = "http://localhost:4200"
npx cypress run
```

## Academic Context

Developed as the integrated team project for Software Engineering 381 (SEN381), 2026 academic
year. Not intended for production use.
