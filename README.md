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

**Milestone 2 (Architecture, Technology & Initial Design Baseline): in progress.**
ASRs and quality drivers are defined. Initial design decisions (notification handling, status
transition logic) are in ADR form. Persistence, technology-stack, and API/integration decisions
are still being finalised. See `docs/PED/PED-v2.0.md` for current status.

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

Not yet finalised. The technology-stack decision is being made as a formal ADR at Milestone 2,
informed by requirements, ASRs, team capability and cost constraints, see
`docs/decisions/adr-technology-stack.md` once it lands.

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

## Academic Context

Developed as the integrated team project for Software Engineering 381 (SEN381), 2026 academic
year. Not intended for production use.
