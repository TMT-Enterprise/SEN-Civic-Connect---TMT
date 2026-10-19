# SEN381 CivicConnect Project

## Project Engineering Document (PED) v4.0

### Community Service Request Management Platform

**Team TMT**

| Group member | Student number |
|---|---|
| Tinyiko Siwele | 601726 |
| Thabang Molise | 601586 |
| Mutshidzi Nduvheni | 601781 |

**GitHub repository:** <https://github.com/TMT-Enterprise/SEN-Civic-Connect---TMT>

**Version:** 4.0 (Milestone 4)
**Date:** 18 October 2026
**Module:** Software Engineering 381 (SEN381), 2026

---

## Version history

| Version | Milestone | What it added |
|---|---|---|
| 1.0 | M1 | Engineering foundation and requirements baseline (problem, stakeholders, scope, requirements, RTM, risks, GitHub governance, accountability, sign-off, AI register). Baseline sign-off 09/09/2026: conditionally accepted. |
| 2.0 | M2 | Architecture, technology and initial design baseline (architecture decision, ASRs, API integration, deployment capability, persistence, technology stack, change log, test cases). |
| 3.0 | M3 | Testing and delivery evidence (CI quality gate, static and security checks, technical debt register); updated RTM, AI register, technology stack and change log. |
| 4.0 | M4 | Single consolidated PED. Change requests CR-001 to CR-005 applied. Design decisions, authentication/RBAC implementation and Project Reflection added. |

---

## Contents

1. [Problem, Stakeholder Analysis and Business Values](#1-problem-stakeholder-analysis-and-business-values)
   - 1.1 [Problem](#11-problem)
   - 1.2 [Stakeholder analysis](#12-stakeholder-analysis)
   - 1.3 [Business values](#13-business-values)
2. [Scope Baseline and Constraints](#2-scope-baseline-and-constraints)
   - 2.1 [In scope (committed baseline)](#21-in-scope-committed-baseline)
   - 2.2 [Out of scope (for the whole project)](#22-out-of-scope-for-the-whole-project)
   - 2.3 [Deferred and future scope](#23-deferred-and-future-scope)
   - 2.4 [Constraints](#24-constraints)
3. [Requirements and Acceptance Criteria](#3-requirements-and-acceptance-criteria)
4. [Requirements Traceability Matrix (RTM)](#4-requirements-traceability-matrix-rtm)
5. [Risk and Forward Engineering Considerations](#5-risk-and-forward-engineering-considerations)
   - 5.1 [Risk probability and impact matrix](#51-risk-probability-and-impact-matrix)
   - 5.2 [Forward engineering considerations](#52-forward-engineering-considerations)
6. [GitHub Governance](#6-github-governance)
7. [Team Accountability](#7-team-accountability)
8. [Baseline Sign-off](#8-baseline-sign-off)
9. [AI Usage Register](#9-ai-usage-register)
10. [References](#10-references)
11. [Architecture Decision](#11-architecture-decision)
12. [ASRs and Quality Drivers](#12-asrs-and-quality-drivers)
13. [API Integration](#13-api-integration)
14. [Deployment Capability](#14-deployment-capability)
15. [Data and Persistence Design](#15-data-and-persistence-design)
    - 15.1 [Entities and lifecycle](#151-entities-and-lifecycle)
    - 15.2 [Assumptions, access patterns and concerns](#152-assumptions-access-patterns-and-concerns)
    - 15.3 [Migrations](#153-migrations)
16. [Technology Stack](#16-technology-stack)
    - 16.1 [Version and compatibility baseline](#161-version-and-compatibility-baseline)
17. [Change Log](#17-change-log)
    - 17.1 [Change request detail](#171-change-request-detail)
18. [Test Cases and Test Evidence](#18-test-cases-and-test-evidence)
19. [CI Quality Gate](#19-ci-quality-gate)
20. [Static and Security Checks](#20-static-and-security-checks)
21. [Technical Debt Register](#21-technical-debt-register)
22. [Design Decisions (ADRs) *[added in v4.0]*](#22-design-decisions-adrs-added-in-v40)
23. [Authentication and Role-Based Access Control Implementation *[added in v4.0]*](#23-authentication-and-role-based-access-control-implementation-added-in-v40)
24. [Project Reflection *[added in v4.0]*](#24-project-reflection-added-in-v40)
    - 24.1 [Challenges](#241-challenges)
    - 24.2 [What worked](#242-what-worked)
    - 24.3 [What changed (refer to the change log)](#243-what-changed-refer-to-the-change-log)
    - 24.4 [Key lessons](#244-key-lessons)

> **How to read this document.** In v1.0 to v3.0 the PED was an index that linked to separate Markdown files in the repository. v4.0 consolidates every section into this single document. Each section names its source file under `docs/` so changes stay traceable to the repository. Where a change request (CR-001 to CR-005) changed a baselined statement, the section below states the current (post-change) position and names the CR.

---

## 1. Problem, Stakeholder Analysis and Business Values

*Source: `docs/requirements/stakeholder-analysis.md`*

### 1.1 Problem

The business system that CivicConnect replaces has issues that stakeholders have experienced directly: staff struggle to prioritise tasks and coordinate work; request information lacks security and privacy; reporting is difficult to audit; and requesters are uncertain about the status of their requests.

Community-focused organisations typically manage requests such as facility faults, IT support, maintenance issues and lost property through a fragmented mix of email, phone calls and paper. Left unaddressed, these problems risk project failure because stakeholder concerns would not be met.

### 1.2 Stakeholder analysis

| Stakeholder | Role | Needs | Influence on CivicConnect | Influence by CivicConnect |
|---|---|---|---|---|
| Requester / Users | Sending in requests | Easy use of the requesting system; visibility of request status. | Low/Medium | High |
| Staff | Handling requests | Convenient handling and prioritisation of requests; improved coordination of work. | High | High |
| Management | Overview of CivicConnect processes | Improved insight into business performance. | High | High |
| System Administrator | Maintaining system and security | Security, reliability, access control. | High | Low/Medium |

> **Note (CR-005).** Management and Admin now represent one access level; the two terms are used interchangeably throughout this document.

### 1.3 Business values

CivicConnect provides a controlled system that establishes a reliable and traceable way to submit, manage and monitor service requests. It reduces operational and administrative burden while improving visibility for requesters and accountability among staff.

---

## 2. Scope Baseline and Constraints

*Source: `docs/requirements/scope-baseline.md`*

The scope baseline defines the committed boundary of the solution, derived from the Master Project Brief and refined against the requester, staff and management stakeholder groups. It lets the team commit to a deliverable set that can be engineered, tested and defended across four milestones, while recording what has been excluded or deferred and why.

### 2.1 In scope (committed baseline)

| Group | Capabilities |
|---|---|
| Requester | 1) Submit a request with category, description and location/area.<br>2) View current status (Received/Submitted, Assigned, In Progress, Resolved, Closed).<br>3) View history of previous requests.<br>4) Receive feedback (in-app and email) when a request is accepted, rejected, updated or completed. |
| Staff | 5) View the queue relevant to the authorised role.<br>6) Search, filter and sort by category, status and date.<br>7) View full request detail and history.<br>8) Assign or accept responsibility.<br>9) Update status through controlled transitions (no jump from Received to Closed).<br>10) Record comments, actions and resolution information.<br>11) Resolve or close requests where authorised. |
| Management / Admin | 12) Dashboard of open, overdue, resolved and closed requests.<br>13) Filter or report by category, status, staff member or date range.<br>14) Information to support accountability and service-performance analysis. |

### 2.2 Out of scope (for the whole project)

| Excluded | Rationale |
|---|---|
| Native mobile applications (iOS/Android) | A responsive web interface meets the access need without doubling the build, test and deployment surface. |
| Integration with external case-management systems | No confirmed data-sharing agreement or integration requirement exists. |
| AI-based automatic routing or triage | Verification and bias risk the team cannot resource in a three-person, four-milestone project. |
| Public-facing API for third parties | No confirmed external consumer; would create an open-ended support and versioning obligation. |

### 2.3 Deferred and future scope

| Deferred item | Why not now | Current position |
|---|---|---|
| SMS and WhatsApp notifications | Free-tier gateways are rate-limited and add delivery/retry logic without a confirmed need. | Still deferred. CR-002 settled on email as the notification channel. |
| Advanced analytics (trend charts, SLA breach prediction) | Basic counts and overdue flags satisfy the stated management need. | Still deferred. |
| Multi-language user interface | No language requirement stated. | Still deferred. |
| Configurable, admin-defined request categories | Adds an administrative configuration subsystem. | Brought into scope by CR-001 (FR-017). |
| Service request priority | Criteria for high/low priority are ambiguous. | Still deferred. |

> **Deliberate exclusion selected for defence: SMS and WhatsApp notifications.** Free-tier SMS gateways are rarely free at any meaningful scale, and building delivery and retry logic for a channel with no confirmed stakeholder mandate is not a responsible use of a fixed-length project. In-app and email notification already satisfy the requester's need for status feedback.

### 2.4 Constraints

| Constraint | Implication for CivicConnect |
|---|---|
| Scope | One cohesive baseline rather than an expanding feature set. Additions must be justified against stakeholder value and impact on schedule, cost, quality and security; uncontrolled expansion is scope creep (PMI, 2017). |
| Schedule | Four milestone windows in the SEN381 term with no residual capacity to recover from late baseline errors; schedule risk is front-loaded (Sommerville, 2015). |
| Cost and resources | Prefer free or low-cost services for hosting, database, notification and CI, while keeping beyond-education costs visible. A three-person team is itself a hard resource limit. |
| Quality | Quality attributes must be measurable non-functional requirements with evidence (ISO/IEC/IEEE 29148:2018). To be revisited once the hosting platform is confirmed. |
| Security | Authenticated roles with role-based access control and least privilege (NIST SP 800-53 Rev. 5); protection of requester personal data; controlled status transitions so history cannot be silently altered. |

**Key constraint interactions and trade-offs**

- **Cost versus security:** free-tier options often provide weaker built-in access control, so controls may have to live in application logic.
- **Schedule versus quality:** a fixed schedule creates pressure to under-test; the trade-off must be recorded and approved rather than silently absorbed (Cunningham, 1992).
- **Scope versus cost and resources:** every extra capability enlarges the persistence, interface and testing surface and competes with governance and documentation effort.

---

## 3. Requirements and Acceptance Criteria

*Source: `docs/requirements/requirements.md`, as amended by CR-001, CR-002 and CR-005*

The table shows the requirements baseline after change requests: FR-017 was added by CR-001, FR-006 uses email per CR-002, and NF-002 and the admin/management requirements use the consolidated Admin role per CR-005. 'Admin' and 'Management' are interchangeable.

| ID | Type | Requirement | Acceptance criteria |
|---|---|---|---|
| FR-001 | Functional | Allow registration for new users using a unique email address. | • Succeeds if the email is not already stored<br>• Error message shown on failure |
| FR-002 | Functional | Allow registered users to log in with email and password. | • Succeeds with valid credentials<br>• Fails with incorrect password; error message shown |
| NF-001 | Non-functional | Protect the system from unauthorised access; restrict access to authenticated users. | • Unauthenticated users cannot access authenticated functionality<br>• A user cannot log in with failed registration details |
| NF-002 | Non-functional | Enforce role-based access for three access groups: Requester, Staff and Admin (Admin = Management) (CR-005). | • Requester cannot access staff-only or admin-only functionality<br>• Staff cannot access requester or admin-only functionality, or other staff members' restricted requests<br>• Admin can access the functionality previously split between Admin and Management |
| NF-003 | Non-functional | Store passwords using a secure hashing mechanism. | • No plain-text passwords<br>• One-way hashing algorithm |
| FR-003 | Functional | Allow an authenticated user to submit a service request with name, contact details, category and short description. | • Form auto-fills from the logged-in profile<br>• Category is a controlled list, not free text<br>• Valid input succeeds; invalid input fails with an error message |
| FR-004 | Functional | Create a unique identifier for every successfully submitted request. | • Identifier is unique; no duplicates |
| FR-005 | Functional | Display the current status of a submitted request to its requester. | • Statuses: Submitted, Assigned, In Progress, Resolved, Closed<br>• Overdue is system-generated when the due date passes before closure |
| FR-006 | Functional | Send an email to the requester when a request moves to a new status (CR-002). | • Email contains request ID and new status<br>• Sent to the requester's account email<br>• Not sent when status is unchanged<br>• Cannot be used to modify the request<br>• Sent within 60 seconds of the status update |
| FR-007 | Functional | Allow the requester to view a history of their previous requests. | • Includes all submitted information and completion timestamps<br>• Requesters cannot see other requesters' requests |
| FR-008 | Functional | Allow authenticated admins to view and assign new incoming requests. | • Only admins see unassigned requests and assign them to staff<br>• Admin sees all submitted details and sets a due date/time<br>• Minimum completion time is 1 day (24 hours) |
| FR-009 | Functional | Allow staff to view, search and filter requests by ID, status, due date, category and assigned staff. | • Only an admin sees all requests and filters by (un)assigned staff; staff see only their own<br>• Case-insensitive, partial matches; empty query shows all; zero-result messaging |
| FR-010 | Functional | Allow normal staff to accept or reject assigned requests. | • Accepted request becomes 'Assigned'<br>• Rejected request stays 'Submitted', is recorded and returns to the admin's unassigned view |
| FR-011 | Functional | Allow staff to update the request status. | • Confirmation message on change<br>• No edits to other request information<br>• Sequential, forward-only (no skipping or going back)<br>• Any staff member may move a request to 'Closed' |
| FR-012 | Functional | Generate an 'Overdue' state when the due date passes before closure. | • 'Overdue' overrides the current status<br>• E.g. due 13:00 and not closed by 13:00 → Overdue |
| FR-013 | Functional | Record each change with request ID, responsible staff and timestamp; record rejection and resolution comments. | • Only admins see the audit trail of all staff<br>• Short description of completed work required before 'Closed'<br>• Error message for invalid description |
| FR-014 | Functional | Provide admins with a dashboard of service-activity statistics. | • Real-time data after refresh<br>• Counts match underlying request records |
| FR-015 | Functional | Allow admins to search and filter reporting information across all requests by due date, status, category, timestamps and assigned staff. | • Case-insensitive, partial matches; empty query shows all; zero-result messaging |
| FR-016 | Functional | Allow admins to view the complete lifecycle history of a request. | • Timestamp and responsible staff for each status<br>• Statuses not yet reached show 'n/a' |
| NF-004 | Non-functional | 95% of staff and admins complete core tasks after one hour of training. | • Core tasks: find/filter, assign, accept/reject, view audit history<br>• Average error ≤ 5% |
| NF-005 | Non-functional | 95% of requesters complete the request form within five minutes. | • Measured from landing page to successful submission<br>• Failed or incomplete submissions are recorded |
| NF-006 | Non-functional | Search results appear in under 3 seconds. | • At least 95% of searches return within 3 seconds |
| NF-007 | Non-functional | At least 90% availability during operating hours, excluding scheduled maintenance. | • Calculated from recorded uptime/downtime during testing<br>• Available when users can access all functions |
| FR-017 | Functional | Allow admins to add, update or delete service category options (CR-001). | • Defaults: Facility faults, Damaged equipment, Security concerns, IT support, Maintenance issues, Lost property, Other<br>• No duplicates; at least one category remains<br>• Changes show in the requester's category options |

**Reference justification for acceptance criteria:**
FR-001, FR-002, NF-001: standard login/registration criteria (Rodriguez, 2026).
FR-006: standard email criteria (Rodriguez, 2026).
FR-008: admins are responsible for delegating tasks (The Indeed Editorial Team, 2026); a standard service task takes about 24 hours (Gupta, 2020).
FR-009: standard search criteria (Rodriguez, 2026).
NF-004: standard time to complete core tasks after training (Sommerville, 2015; Khomenko, 2026).
NF-006: optimal rendering time (Larmier, 2026).
NF-007: standard availability expectation (IBM, 2025).

---

## 4. Requirements Traceability Matrix (RTM)

*Source: `docs/requirements/rtm.md`; `docs/api/openapi.yaml`; `tests/test-cases.md`*

The RTM connects requirements to test cases, results and other project artefacts (Bose, 2026), improving visibility of testing across the lifecycle. The master RTM is an Excel workbook: **RTM Excel document (SharePoint)**.

> **Note:** the workbook is not version-controlled in the repository. The summary below is a repository-based view built from the OpenAPI specification and the test-case catalogue, so that each requirement can be traced inside the repo.

| Requirement | Implemented through (API / component) | Test cases (`tests/test-cases.md`) |
|---|---|---|
| FR-001 | `POST /auth/register` | TC-FR001-01 to 04 |
| FR-002 | `POST /auth/login` | TC-FR002-01 to 03 |
| FR-003 | `POST /requests`; `GET /categories` | TC-FR003-01 to 05 |
| FR-004 | `POST /requests` (generated identifier) | No dedicated case listed |
| FR-005 | `GET /requests/{id}` | TC-FR005-01 to 02 |
| FR-006 | `INotifier` / `StatusChangeNotifier` / `EmailNotifier` | TC-FR006-01 to 03 |
| FR-007 | `GET /requests`; `GET /requests/{id}` | TC-FR007-01 to 03 |
| FR-008 | `GET /admin/requests`; `POST /requests/{id}/assign` | TC-FR008-01 to 06 |
| FR-009, FR-015 | `GET /staff/requests`; `GET /admin/requests`; `GET /management/requests` | TC-FR915-01 to 06 |
| FR-010 | `POST /requests/{id}/decide` | TC-FR010-01 to 03 |
| FR-011 | `PATCH /requests/{id}/status`; `RequestStatusTransitionValidator` | TC-FR011-01 to 05 |
| FR-012 | `OverdueBackgroundService` / `OverdueProcessor` | TC-FR012-01 to 02 |
| FR-013 | `PATCH /requests/{id}/status`; `GET /requests/{id}/history` | No dedicated case listed |
| FR-014 | `GET /management/dashboard` | TC-FR014-01 to 04 |
| FR-016 | `GET /management/requests`; `GET /requests/{id}/history` | TC-FR016-01 to 03 |
| FR-017 | `GET/POST /categories`; `PUT/DELETE /categories/{id}` | TC-FR017-01 to 06 |
| NF-001 to NF-003 | JWT authentication, authorisation policies, BCrypt hashing | `AuthMiddlewareTests`, `RoleMatrixTests`, `BCryptPasswordHasherTests` (automated) |

---

## 5. Risk and Forward Engineering Considerations

*Source: `docs/risk/risk-register.md`*

### 5.1 Risk probability and impact matrix

| Impact \ Probability | Low | Medium | High |
|---|---|---|---|
| **High** | R-02, R-05, R-07, R-08 | | |
| **Medium** | R-03, R-04, R-06 | R-01 | |
| **Low** | | | |

R-05 is recorded as low-to-medium probability and placed in the medium column for visual purposes; the register is authoritative. Later documents also reference R-09 (module boundaries), R-16 (dependency and licence drift) and R-17 (committed secrets). The detailed register is an Excel workbook: **Risk register Excel document (SharePoint)**.

### 5.2 Forward engineering considerations

| Concern | Why it matters now | Later influence / risk of ignoring |
|---|---|---|
| 1. Authentication and RBAC model | Determines what a staff versus management account requires at data level. | M2 architecture and data design. Retrofitting roles after the schema is set is expensive. (Resolved to two staff roles by CR-005.) |
| 2. Status-transition rules | Requirements already assume controlled transitions with no skipped states. | M2 design, M3 implementation/testing. Ambiguous logic if the transition map stays implicit. |
| 3. Deployment environment and hosting limits | Free-tier limits affect what non-functional requirements can promise. | M2 technology decision, M4 deployment. NFRs baselined now may prove undeliverable. |
| 4. Observability and audit trail | Management oversight depends on action logging from the start. | M2 data design, M3 construction. Retrofitting audit logging is costly and error-prone. |
| 5. Data backup and recovery | Even a student project holding real-shaped data needs a recovery story. | M2 architecture, M4 operational readiness. Total data loss with no fallback. |
| 6. Automated testing approach | Deferring test strategy to M3 produces untested code under deadline pressure. | M3 quality strategy. Weak test evidence discovered too late. |
| 7. Notification delivery reliability | Even email-only notification can fail (spam filtering, delay). | M2 design, M3 testing. Requesters silently miss status updates. |

---

## 6. GitHub Governance

*Source: `docs/governance/github-governance.md`; CR-003; CR-004; `docs/quality/ci-quality-gate.md`*

The team favours small, traceable, reviewable changes over large or long-lived branches. The working chain is: **Issue → short-lived branch → commits → pull request → two peer reviews → protected branch → branch deletion.**

| Area | Rule |
|---|---|
| Repository | One official GitHub repository for CivicConnect. |
| Protected `main` | No development on `main` and no direct pushes. Pull requests into `main` may only come from `development` or `main-update/*` (Branch policy check). |
| Branches (CR-004) | Short-lived branch per GitHub Issue, from the latest base, containing only that issue's work; merged by pull request and deleted after merging. This replaced the persistent per-developer branches of CR-003. |
| Branch naming | `<type>/<issue-number>-<short-description>`, e.g. `feat/23-user-registration`. Types: `feat`, `fix`, `docs`, `test`, `refactor`. |
| Pull requests | Title `<type>: short description`. Description has a Summary and a Related Issue section using `Closes #<issue-number>`. |
| Review | Approval by at least two other team members; authors never approve their own change; meaningful review using the PR checklist (functionality, testing, security, scope); keep PRs small and focused. |
| Issue tracking | Every task is a GitHub Issue on the Kanban board; the issue number appears in the branch name; the PR references the same issue. |
| Required status checks | Repo sanity; Backend (build, test, static, deps); Frontend (lint, test, build, deps); Secrets scan; CI gate; Branch policy. Ruleset has no bypass actors, and PRs must be up to date with the base branch. |
| Dependabot | Weekly pull requests for NuGet, npm and GitHub Actions, targeting `development`. Dependabot branches are exempt from naming rules but still need green CI and two approvals. |
| Hotfix protocol (CR-003) | A critical bug on `main` is fixed on a dedicated temporary branch while `main` is frozen to other merges, enforced by a required CI check. |
| Secrets | No passwords, keys or tokens in the repository; use environment variables or secure configuration; report and revoke accidentally committed secrets. |

---

## 7. Team Accountability

*Source: `docs/governance/team-accountability.md`*

| Area | Tinyiko Siwele | Mutshidzi Nduvheni | Thabang Molise |
|---|---|---|---|
| Frontend | Development Lead | Review/Support | Review/Support |
| Backend | Development Review/Support | Lead | Lead/Support/Review |
| Database | Lead/Support | Lead/Review/Support | Review/Support |
| Documentation | Lead/Review/Support | Lead/Review/Support | Lead/Review/Support |
| GitHub Governance | Review/Support | Lead/Review/Support | Lead |
| AI Usage Register | Lead/Review/Support | Lead/Review/Support | Lead/Review/Support |

Each member has designated areas of primary responsibility while remaining accountable for understanding and contributing to the complete CivicConnect system. Primary responsibility indicates ownership, not complete responsibility.

---

## 8. Baseline Sign-off

*Source: `docs/PED/baseline-signoff.md`*

| Field | Value |
|---|---|
| Project | CivicConnect |
| Baseline type | Engineering Baseline |
| Version | v1.0 |
| Date | 09/09/2026 |
| Scope reviewed | YES |
| Requirements/traceability checked | YES |
| Risk review completed | YES |
| Repository/governance controls checked | YES |
| **Outcome** | **CONDITIONALLY ACCEPTED** |

---

## 9. AI Usage Register

*Source: `docs/governance/ai-usage-register.md` (condensed)*

All AI use is logged with its purpose, verification and the decision taken. The table condenses the repository register (27 entries); the full register with verification and issues-found detail is maintained in the repository.

| Date | Student | Tool | Task | Outcome / issues found |
|---|---|---|---|---|
| 06/09 | Mutshidzi | ChatGPT | Summarise project brief; identify sources | Verified; used to identify problem, values, stakeholders. |
| 09/09 | Tinyiko | ChatGPT | Polish functional requirements | Verified; split FR-001, reworded NF-001, added RBAC requirement. |
| 09/09 | Thabang | Claude | Optimise GitHub structure and issues | Verified; relevant parts accepted. |
| 15/09 | Tinyiko | ChatGPT | GitHub governance and CI approach | Adopted as baseline subject to real configuration. |
| 22/09 | Tinyiko | ChatGPT | Refine requirements, acceptance criteria, change control | Cross-checked; risk of unapproved changes noted. |
| 23/09 | Thabang | Gemini | Issue-creation template | Mostly adopted. |
| 27/09 | Thabang | Claude | CI pipeline and branch-policy templates | About 50% accepted; rest customised. |
| 29/09 | Tinyiko | ChatGPT | Database design and traceability | Aligned design, entity docs and RTM. |
| 01/10 | Tinyiko | ChatGPT | Functional, integration and E2E test cases | Reviewed against acceptance criteria. |
| 05–08/10 | Tinyiko | ChatGPT | Angular scaffold and health-check integration | Backend had no `/status`; DB auth, proxy and error handling needed work. |
| 06/10 | Mutshidzi | Claude | docker-compose and `.env.example` (S1-07) | Partially verified, pending members. |
| 06/10 | Mutshidzi | ChatGPT | Troubleshooting S1-07 and S1-04 | Verified; some errors needed further research. |
| 07/10 | Thabang | Claude | Debug JWT management logic | Verified by test suites; fully adopted. |
| 10/10 | Tinyiko | Gemini | Mobile requester UI | Approved with minor tweaks; mobile misalignment. |
| 12/10 | Tinyiko | ChatGPT | PostgreSQL migration and seeding approach | Supabase connection failed; rerun safety and FKs needed attention. |
| 12/10 | Tinyiko | Gemini | Admin dispatcher dashboard UI | Approved; none. |
| 12/10 | Thabang | ChatGPT | Unit-test templates | Under 30% accepted; many failures vs business logic. |
| 14/10 | Tinyiko | Gemini | Staff portal UI and ticket workflow | Approved; overdue contrast too subtle. |
| 15/10 | Tinyiko | ChatGPT | Convert portal designs to Angular layouts | Template warnings; frontend-only access restrictions needed attention. |
| 15/10 | Thabang | Gemini | Polish project documentation | Approved and applied. |
| 16/10 | Tinyiko | Gemini | Unified cross-role UI design system | Finalised; none. |
| 17/10 | Mutshidzi | Claude | API-to-database connection troubleshooting | First hypothesis wrong; cause was user secrets/`.env` overriding config, wrong port, missing migration. |
| 18/10 | Mutshidzi | Claude | Git workflow, line endings, merging `development` | AI wrongly said branch from `main`; CRLF/LF issues; conflicts in five frontend files. |
| 18/10 | Mutshidzi | Claude | New Request form submission review | Not fully verified; possible category, proxy and login-URL problems. |
| 18/10 | Thabang | Claude | Business-logic bug fixes | Adopted where criteria met. |
| 18/10 | Thabang | Grok | Stress test and audit of business logic | Significant loopholes found; flagged for refactoring. |
| 18/10 | Thabang | Claude | Analyse application bugs | 50% adopted; 50% rejected as inconsistent. |

---

## 10. References

*Source: `docs/research-material/references.md`*

Angular Team (2026) *Home • angular*. [online] Angular. Available at: <https://angular.dev/> [Accessed 30 Sept. 2026].

Bose, S. (2026) *Requirements Traceability Matrix (RTM): A How-To Guide*. [online] TestRail. Available at: <https://www.testrail.com/blog/requirements-traceability-matrix/> (Accessed: 9 September 2026).

Cunningham, W. (1992) 'The WyCash portfolio management system', *OOPSLA Addendum 1992*, pp. 29-30. Available at: <http://c2.com/doc/oopsla92.html> (Accessed: 9 September 2026).

Djirdeh, H. (2025) *What are the top libraries and tools for modern react frontend development?* [online] Telerik Blogs. Available at: <https://www.telerik.com/blogs/top-libraries-tools-modern-react-frontend-development> [Accessed 30 Sept. 2026].

Gupta, A. (2020) *Important service desk metrics to measure performance*. [online] Motadata. Available at: <https://www.motadata.com/blog/important-service-desk-metrics-to-measure> (Accessed: 8 September 2026).

IBM (2025) *Deciding what level of availability you need*. [online] Ibm.com. Available at: <https://www.ibm.com/docs/en/i/7.5.0?topic=roadmap-deciding-what-level-availability-you-need> (Accessed: 9 September 2026).

International Organization for Standardization/International Electrotechnical Commission/Institute of Electrical and Electronics Engineers (ISO/IEC/IEEE) (2018) *ISO/IEC/IEEE 29148:2018 Systems and software engineering: life cycle processes: requirements engineering*. Geneva: ISO/IEC/IEEE.

Khomenko, T. (2026) *What is an NFR? Non-Functional Requirements Definition, Examples & Template*. [online] testomat.io. Available at: <https://testomat.io/blog/non-functional-requirements-examples-definition-complete-guide/> (Accessed: 9 September 2026).

Larmier, M. (2026) *Website load time & speed statistics: Is your site fast enough?* [online] WP Rocket. Available at: <https://wp-rocket.me/blog/website-load-time-speed-statistics/> (Accessed: 9 September 2026).

Li, T.H.Y., Thomas, N.S. and Skitmore, M. (2013) 'Evaluating stakeholder satisfaction during public participation in major infrastructure and construction projects: A fuzzy approach', *Automation in Construction*, 29, pp. 123-135. doi:10.1016/j.autcon.2012.09.007.

National Institute of Standards and Technology (2020) *Security and privacy controls for information systems and organizations*. NIST Special Publication 800-53, Revision 5. Gaithersburg, MD: U.S. Department of Commerce. Available at: <https://doi.org/10.6028/NIST.SP.800-53r5> (Accessed: 9 September 2026).

Project Management Institute (2017) *A Guide to the Project Management Body of Knowledge (PMBOK Guide)*. 6th edn. Newtown Square, PA: Project Management Institute.

React Team (2024) *Quick start*. [online] react.dev. Available at: <https://react.dev/learn> [Accessed 30 Sept. 2026].

Rodriguez, M. (2026) *10 Acceptance criteria examples (with templates) for product teams*. [online] Clearlyreqs.com. Available at: <https://www.clearlyreqs.com/blog/acceptance-criteria-examples> (Accessed: 8 September 2026).

Sommerville, I. (2015) *Software engineering*. 10th ed. Boston: Pearson Education Limited.

The Indeed Editorial Team (2026) *Common duties and responsibilities of an administrator*. [online] Indeed Career Guide. Available at: <https://uk.indeed.com/career-advice/finding-a-job/duties-responsibilities-of-administrator> (Accessed: 8 September 2026).

> **Open item:** the ADRs and CRs also cite Martin (2003), Gamma et al. (1994), Fowler (2018, 2020) and Driessen (2010). Full entries for these are not yet in `references.md` and should be added.

---

## 11. Architecture Decision

*Source: `docs/architecture/architecture-decision.md`; Architecture diagram.png*

After researching the options, the team selected a **modular monolith with a layered internal structure**: a single deployable application internally divided into cohesive modules (Request, User/Auth, Notification) with controlled interfaces, organised into Web/Client, Application, Data Access and Database responsibilities.

![Figure 1. CivicConnect architecture (Web/Client → Application layer → Data Access → Database).](docs/architecture/Architecture%20diagram.png)

*Figure 1. CivicConnect architecture (Web/Client → Application layer → Data Access → Database).*

```text
Web/Client           (Submit request, View status, Staff actions)
      │
      ▼
Application Layer    [ Request module ] [ User/Auth module ]
                            └──── Notification Module ────┘
      │
      ▼
Data Access          (Repositories, Transactions)
      │
      ▼
Database             (Requests, Users, History, Notifications)
```

| Justification factor | Reasoning |
|---|---|
| ASRs | Related operations (assignment, status update, history record) stay in one application and persistence boundary, so they can run as a coordinated transaction. Layers separate application logic from data access. |
| Constraints | Four milestones and three people: one deployable application avoids the extra services, network communication and pipelines of microservices. |
| Team capability | Work can be divided around logical modules (requests, users/auth, notifications) without separate deployments. |
| Deployment | One application to configure, test and deploy; the database remains a separate persistence component. |
| Complexity | Some structural complexity (module boundaries, dependency control) is intentional to avoid an uncontrolled, tightly coupled monolith, without distributed-system complexity. |

**Logical versus physical.** Technologies are the tools used (framework, runtime, database). Logical layers and modules describe how responsibilities are organised. Physical tiers describe where software executes. Several logical modules run inside one deployed backend while the database runs separately, so multiple modules do not imply multiple services.

| Risk | Mitigation |
|---|---|
| Module coupling | Define clear module boundaries and controlled interfaces (enforced by `ArchitectureTests` in CI). |
| Layer coupling | Enforce clear dependencies between layers. |
| Shared database coupling | Define data ownership and controlled data access. |
| Transaction complexity | Clearly define transaction responsibilities for multi-step operations. |
| Added complexity | Keep modules and layers proportional to CivicConnect's scope. |

---

## 12. ASRs and Quality Drivers

*Source: `docs/architecture/asrs-quality-drivers.md`*

Each driver is tied to stakeholder evidence, a stated constraint or a recorded risk, not a generic checklist.

| Driver | Source requirements and evidence | Measurable expectation | Influence on design |
|---|---|---|---|
| 1. Security: authenticated access and RBAC | NF-001, NF-002, NF-003. System Administrator needs security and access control; Risk R-05 (protection of sensitive requester data). | A requester can never reach staff-only or admin-only functionality (RBAC tests); no password recoverable in plaintext. | Authentication/authorisation layer in front of business logic; role enforced at account level; middleware-style interception; backend enforces authorisation. |
| 2. Data integrity for request lifecycle | FR-004, FR-011, FR-013. Assignment without a logged event leaves inconsistent data; Risk R-02. | Assignment commits staff assignment, status update and history record together or rolls all back (forced-failure test). | Transactional persistence; atomic assignment/status/history changes; layered validation and transactional approach from Assignment 2. |
| 3. Timely, reliable notification | FR-006 (email within 60 s). Requester needs visibility; Forward Engineering Consideration 7. Channel is email per CR-002. | At least 95% of status-change notifications generated within 60 s. | Decouple request handling from the channel through a dependency-injected Notifier; notification log records channel, timestamp and content. |
| 4. Usability for non-technical users | NF-004, NF-005. Fragmented, hard-to-use channels motivated CivicConnect. | Usability testing meets the 95% thresholds in NF-004 and NF-005. | Mainstream, well-documented front-end framework; simple table-driven transition validator that staff can reason about. Category list is admin-managed (FR-017, CR-001) with sensible defaults. |
| 5. Cost-constrained availability and deployability | NF-007. Cost constraint; Risk R-03 (free-tier limits); Forward Engineering Consideration 3. | Uptime monitoring shows at least 90% availability once hosted. | Avoid a distributed design that multiplies free-tier limits; evaluate persistence against real platform limits; record accepted availability risks. |

> **Correction recorded in v4.0.** The source ASR file still describes FR-017 as a simulated SMS/WhatsApp channel and mentions an 'FR-018'. In the baseline, FR-017 is service-category management (CR-001) and the notification channel is email (CR-002); this section reflects that.

---

## 13. API Integration

*Source: `docs/decisions/adr-technology-stack.md` (API selection); `docs/api/openapi.yaml`*

**Decision.** REST is the architectural style for communication between the Angular client and the ASP.NET Core application, documented with OpenAPI/Swagger. REST gives straightforward resource-based communication; OpenAPI improves visibility of endpoints, schemas and security requirements and gives developers an interactive way to inspect and test the API. The team accepted a small learning curve for Swagger and agreed to monitor its schedule impact.

**Integration mechanics.** The browser calls the API under `/api/v1` with a Bearer JWT; in development the Angular dev server proxies `/api` and `/health` to the API on `http://localhost:5228`. Email is integrated behind the `INotifier` abstraction; CORS origins are configured through environment settings.

| Method and path | Purpose (requirement) | Access |
|---|---|---|
| `POST /auth/register` | Register a new requester (FR-001) | Public |
| `POST /auth/login` | Login for all roles (FR-002) | Public |
| `GET /categories` | List active service categories (FR-003) | Authenticated |
| `POST /categories`; `PUT`, `DELETE /categories/{id}` | Manage categories (FR-017) | Admin |
| `POST /requests`; `GET /requests` | Submit a request (FR-003, FR-004); list own requests (FR-007) | Requester |
| `GET /requests/{id}` | Single request (FR-005, FR-007) | Authenticated |
| `POST /requests/{id}/assign` | Assign a request to staff (FR-008) | Admin |
| `POST /requests/{id}/decide` | Accept or reject an assigned request (FR-010, FR-011) | Staff |
| `PATCH /requests/{id}/status` | Update request status (FR-011, FR-013) | Staff |
| `GET /requests/{id}/history` | Read a request's status history (FR-013) | Employees |
| `GET /staff/requests` | Staff queue: search and filter own requests (FR-009) | Staff |
| `GET /admin/requests` | All requests with filters (FR-008, FR-009) | Admin |
| `GET /management/dashboard` | Dashboard statistics (FR-014) | Admin |
| `GET /management/requests` | All requests with full lifecycle (FR-015, FR-016) | Admin |

> **Documentation status.** The file `docs/decisions/adr-api-integration.md` was empty in the repository. This section consolidates the API decision from the technology-stack ADR and the OpenAPI specification. Operation summaries in `openapi.yaml` still say 'management only'; under CR-005 this means Admin.

---

## 14. Deployment Capability

*Source: `docs/deployment/supabase-spike.md`; `docker-compose.yml`; `.env.example`; `database/scripts/migrate.js`; `.github/workflows/*`*

The file `docs/deployment/deployment-compatibility,md` was empty in the repository. The deployment position below is consolidated from the artefacts that do exist.

| Aspect | Current position |
|---|---|
| Deployable units | One ASP.NET Core 10 application (modular monolith); an Angular 21 static build that talks to the API over HTTP; a separate managed PostgreSQL database. |
| Database platform | Supabase-managed PostgreSQL selected. Connectivity spike S1-10 (08/10/2026): region Central EU (Frankfurt), Postgres 17.x, direct connection, .NET key=value (Npgsql) connection string with SSL required. |
| Local development | `docker-compose` runs PostgreSQL 16 (named volume, health check); settings come from `.env` (see `.env.example`: database, connection string, CORS origins, JWT, demo seed password, email options). |
| Database lifecycle | Versioned SQL migrations 001 to 007 applied by a Node migration runner (`database/scripts/migrate.js`) using `CIVICCONNECT_DATABASE_URL`; demo seed data is development only. |
| Pinned toolchain | .NET SDK from `global.json`; Node from `.nvmrc`; `Directory.Build.props` for compiler and analyser rules. |
| CI/CD | `ci.yml` builds, tests and audits every PR (Postgres 16 service container); `branch-policy.yml` restricts what may target `main`; `release.yml` verifies a `main` commit came from an allowed PR, then publishes the API (`dotnet publish`) and builds the frontend as release artefacts. |
| Secrets | Never committed. JWT secret, database credentials and email credentials come from user-secrets or environment variables; the API refuses to start with a missing or short JWT secret. Gitleaks scans history in CI. |
| Availability (NF-007) | 90% target during operating hours; to be evidenced with uptime monitoring once hosted. Free-tier limits (R-03) remain a recorded risk. |

**Known deployment gaps**

- No application Dockerfiles are in the repository; containerisation covers the local database only.
- Local and CI databases run PostgreSQL 16 while Supabase runs 17.x; the difference needs to be watched.
- No hosted production URL or uptime evidence is recorded in the repository.
- Several CI quality-gate demonstrations are recorded as 'TBC' (section 19).

---

## 15. Data and Persistence Design

*Source: `docs/decisions/adr-persistence.md`; `database/migrations/*`; `civicconnect-initial-model-schema.jpg`*

A relational model suits CivicConnect's structured entities and the integrity needed across users, requests, categories and history. Primary and foreign keys and constraints keep relationships valid, and transactions keep assignment, status and history consistent. The database holds personal and security-sensitive data (contact details, password hashes), so authentication, authorisation and data protection controls apply.

![Figure 2. Initial entity-relationship model.](civicconnect-initial-model-schema.jpg)

*Figure 2. Initial entity-relationship model.*

### 15.1 Entities and lifecycle

| Entity | Key attributes | Lifecycle |
|---|---|---|
| EMPLOYEE_ROLE | role_id, name | Reference data (Admin, Staff after CR-005); not deleted while employees reference it. |
| EMPLOYEE | employee_id, role_id, name, id_number, phone_number, email, password_hash, is_active, created_at | Deactivated, not deleted, so assignments and audit records stay valid. |
| REQUESTER | requester_id, name, email, password_hash, is_active, created_at (phone number added by migration 007) | Deactivated rather than deleted where history must be retained. |
| SERVICE_CATEGORY | category_id, type, is_active | Admins add, update or deactivate; referenced categories never physically deleted; at least one active. |
| REQUEST_STATUS | status_id, name | Controlled by the system; stable reference data. |
| SERVICE_REQUEST | request_id, requester_id, category_id, assigned_employee_id, current_status_id, description, submitted_at, due_at, updated_at, closed_at | Never physically deleted (history, reporting, audit). |
| REQUEST_STATUS_HISTORY | history_id, request_id, employee_id, status_id, changed_at | Append-only audit history (enforced by an append-only interceptor). |
| REQUEST_ASSIGNMENT | assignment_id, request_id, employee_id, assigned_by_employee_id, assigned_at, decision, decision_at, rejection_comment | Retained as history; rejected request returns to the unassigned pool. |
| NOTIFICATION | notification_id, request_id, status, created_at, sent_at | Created on qualifying status change; timestamps evidence the 60-second requirement. |

### 15.2 Assumptions, access patterns and concerns

- **Assumptions:** one active role per employee; emails unique within each account type; passwords stored only as hashes; a request is assigned to one staff member; at least one category stays active; Overdue is a derived, overriding state; status history is append-only; `created_at` and `sent_at` give notification timing evidence.
- **Access patterns:** staff search/filter (indexes); requester history; admin unassigned view; dashboard aggregation; transactional lifecycle changes.
- **Concerns:** database bottleneck under heavy search/reporting; single point of failure; scalability; availability tied to the database; backup and recovery.

### 15.3 Migrations

| Migration | Purpose |
|---|---|
| `001_schema` | Initial schema |
| `002_seed` | Development seed data (roles Admin and Staff, statuses, categories, demo employees) |
| `003_overdue_flag`; `003_allow_null_due_at` | Overdue support; nullable due date |
| `004_assignment_integrity`; `004_notification_log` | Assignment integrity rules; notification log |
| `005_request_search_indexes` | Indexes for search and filter |
| `006_account_last_login` | `last_login_at` on requester and employee accounts |
| `007_requester_phone_number` | Requester phone number |

---

## 16. Technology Stack

*Source: `docs/decisions/adr-technology-stack.md`; `README.md`*

| Area | Selected | Main reasons (alternative considered) |
|---|---|---|
| Frontend | Angular (TypeScript) | Built-in router guards, reactive forms, dependency injection and services suit validated forms, role-based interfaces and service-based API calls; two members have Angular experience (React considered: only one proficient member and more third-party libraries). Frontend guards remain UX-only; the backend enforces access. |
| Backend | C# with ASP.NET Core | Built-in dependency injection and middleware fit the modular monolith, authentication/authorisation and transactional operations (Node.js + TypeScript considered). |
| Database | PostgreSQL on Supabase | Managed PostgreSQL reduces infrastructure effort and has a free tier; trade-off is platform dependence (self-hosted PostgreSQL considered). Authentication stays in our own Auth module, not Supabase Auth. |
| API | REST with OpenAPI/Swagger | Simple resource-based communication with interactive documentation. |
| Testing | xUnit, ASP.NET Core integration tests, Angular testing tools, Cypress | Cypress chosen over Playwright for lower learning and setup overhead and an interactive runner. |
| Build and tooling | Angular CLI, npm, .NET SDK, NuGet, Git/GitHub, Docker | See version baseline below. |

### 16.1 Version and compatibility baseline

| Technology | Version | Justification |
|---|---|---|
| Angular | 21.x | Chosen frontend framework |
| TypeScript | 5.9.x | Angular 21 requires >=5.9.0 <6.0.0 |
| Node.js | 24.x LTS | Angular 21 supports Node 24; LTS for stability |
| npm | 11.x | Bundled with Node |
| C# | 14 | Paired with .NET 10 |
| .NET / ASP.NET Core | 10.x | Builds and runs the backend REST API |
| PostgreSQL | Supabase-managed (17.x in spike); 16 locally and in CI | Supabase manages the actual version |
| xUnit | 3.x | Backend testing |
| Cypress | 16.x | End-to-end testing |
| Docker Engine | 29.8.1 | Local containerised PostgreSQL (docker-compose) |
| OpenAPI / Swagger UI | 3.x / compatible | API contract and interactive documentation |

Angular, TypeScript, Node.js and npm versions must be compatible with one another; the .NET version must be compatible with ASP.NET Core and the PostgreSQL provider; Cypress must support the browser and Angular application. Version changes are recorded to avoid unexpected incompatibilities.

---

## 17. Change Log

*Source: `docs/change/change-log.md`; `docs/change/CR-001.md` to `CR-005.md`*

Five change requests were raised and all were approved. Each follows the CR template: requested change, reason, requirements/architecture/UI-API-data affected, security, quality/testing, scope, schedule, cost, risk, recommendation, approval and baseline sections updated.

| Change ID | Date | Summary | Status |
|---|---|---|---|
| CR-001 | 22/09/26 | Added FR-017 so authorised admins can manage service categories through add, update and delete, with duplicate prevention and a requirement to keep at least one category. | Approved |
| CR-002 | 08/09/26 | Changed the notification channel from SMS simulation to email simulation while retaining the original notification requirement (FR-006). | Approved |
| CR-003 | 22/09/26 | Proposed persistent developer branches, isolated hotfix handling, role-restricted GitHub administration and an expanded CI build/test/lint/audit gate. | Approved (branching part superseded by CR-004) |
| CR-004 | 30/09/26 | Replaced persistent developer branches with short-lived, issue-linked branches and standardised PR formatting for traceability, reviewability and integration control. | Approved |
| CR-005 | 11/10/26 | Simplified role-based access control from three staff roles (Staff, Admin, Management) to two (Staff and Admin); Admin and Management are interchangeable. | Approved |

### 17.1 Change request detail

| CR | Requested by / approved by | Affected | Impact and decision rationale |
|---|---|---|---|
| CR-001 | Tinyiko Siwele / Thabang, Mutshidzi | Requirements (FR-017); ServiceCategory entity; category endpoints; admin UI; requester form. | Moderate scope increase; persistent category data, uniqueness constraint, admin-only authorisation, new tests. Fits the existing architecture; gives admin flexibility without developer changes. |
| CR-002 | Mutshidzi Nduvheni / Tinyiko, Thabang | FR-006; scope baseline; notification component; user contact data. | Email supports richer, traceable content and easier testing of format and links; minimal effort; risks are address validity and delivery failure. Architecture unchanged. |
| CR-003 | Thabang Molise / approvers to be confirmed in record | Governance and CI only; two workflows and a CODEOWNERS file added. | Reduced branch overhead for a three-person team; least-privilege administration; main-freeze check. Longer-lived branch risk countered by frequent pulls, early draft PRs, rebasing and CODEOWNERS. |
| CR-004 | Tinyiko Siwele / Thabang, Mutshidzi | GitHub governance (PR format, branch naming); risk register (R-06 and related). | One task, one branch, one PR improves traceability and avoids re-reviewing combined changes; backed by Assignment 2 findings. Risk of mislinked branches mitigated by naming conventions. |
| CR-005 | Tinyiko Siwele / Thabang, Mutshidzi | NF-002, FR-008, FR-009, FR-013 to FR-016; RBAC design; seed data; API policies; tests. | Admin and Management had equivalent access, so two roles gave no security or operational benefit. Risk of inconsistent references mitigated by updating requirements, seed data, policies and tests together. |

---

## 18. Test Cases and Test Evidence

*Source: `tests/test-cases.md`; `tests/api`; `tests/e2e`; `src/web` (`*.spec.ts`)*

The test-case catalogue covers critical functional requirements with positive, negative, boundary and authorisation scenarios. Cases are translated into automated black-box, white-box and end-to-end tests.

| Suite | Cases | Focus |
|---|---|---|
| FR-001 Registration | 4 | Valid, duplicate email, missing fields, injection-style input |
| FR-002 Login | 3 | Valid, invalid credentials, injection-style input |
| FR-003 Submit request | 5 | Valid, missing category, length boundaries, injection, URL bypass |
| FR-005 Status view | 2 | Each status; Overdue after due date |
| FR-006 Email notifications | 3 | Sent to requester; correct ID and status; none when unchanged |
| FR-007 Request history | 3 | Multiple, empty, completed requests |
| FR-008 Admin assignment | 6 | Unassigned queue, access denied, assignment, 24-hour due date, invalid input |
| FR-009 and FR-015 Search/filter | 6 | Keyword, filters, empty, combined, case-insensitive, special characters |
| FR-010 Accept/reject | 3 | Accept, reject, other employee's request denied |
| FR-011 Status transitions | 5 | Permitted, out-of-sequence rejected, closure description, overdue, close |
| FR-012 Overdue | 2 | Marked after due date; not before |
| FR-014 Dashboard | 4 | Matches data, refresh, empty, access denied |
| FR-016 Lifecycle history | 3 | Timestamps and n/a, access denied, not found |
| FR-017 Category management | 6 | CRUD, duplicates/blank, length, access, deletion rules, requester form |

**Automated tests in the repository**

| Layer | What exists |
|---|---|
| Backend unit tests (xUnit) | 58 test methods across authentication and hashing, JWT options and tokens, roles, status-transition validator, assignment rules, overdue processing and validation, notifier behaviour and architecture rules. |
| Backend integration tests | 64 test methods using the ASP.NET Core test host against PostgreSQL: auth endpoints and middleware, role matrix, public-route allow-list, requests, search, assignment, overdue processing, notification persistence, API startup; plus a live email-timing test. |
| Frontend unit tests | 31 Angular spec files covering components, layouts, shared components and the auth service. |
| End-to-end | Cypress 16 smoke test (status page) run from `tests/e2e` against `http://localhost:4200`. |

---

## 19. CI Quality Gate

*Source: `docs/quality/ci-quality-gate.md`; `.github/workflows/ci.yml`*

Every pull request is built, tested and checked, and a failing check blocks merging. Required check names must match the job names in `ci.yml` exactly; renaming a job means updating the branch ruleset and this table in the same PR.

| Required check | What it does |
|---|---|
| Repo sanity | Critical files exist and are non-empty. |
| Backend (build, test, static, deps) | Restore, format check, build, migrations on a Postgres 16 service container, tests with coverage, NuGet vulnerability scan; uploads `backend-test-evidence`. |
| Frontend (lint, test, build, deps) | `npm ci`, ESLint, unit tests, production build, `npm audit` at high severity. |
| Secrets scan | Gitleaks over the full checked-out history. |
| CI gate | Passes only if the four jobs above succeeded. |
| Branch policy | Only `development` or `main-update/*` may open a PR into `main`. |

**Workflow settings:** runs on pull requests to `development` and `main` and on pushes to both; read-only contents permission; superseded runs on the same ref are cancelled; .NET SDK from `global.json` and Node from `.nvmrc`.

**Evidence plan (status in the repository)**

| Demonstration | Expected failing step | Evidence status |
|---|---|---|
| Green run | None | TBC |
| Build error | Backend, Build | TBC |
| Format drift | Backend, Format check | TBC |
| Failing test | Backend, Test | TBC |
| Vulnerable NuGet package | Backend, Restore/scan | TBC |
| Frontend lint failure | Frontend, Lint | TBC |
| Vulnerable npm package | Frontend, npm audit | TBC |
| Planted secret | Secrets scan | Detection demonstrated per static-checks document; link TBC |
| Blocked merge on `main` | Backend and CI gate | TBC (screenshot and PR link) |
| Check names match ruleset | n/a | TBC |

---

## 20. Static and Security Checks

*Source: `docs/quality/static-and-security-checks.md`*

| Check | Tool and configuration | Fails the build when |
|---|---|---|
| Compiler and Roslyn analysers | `AnalysisLevel=latest`, `EnforceCodeStyleInBuild`, `TreatWarningsAsErrors`, nullable enabled | Any compiler, analyser or code-style warning |
| Format check | `dotnet format --verify-no-changes` using `.editorconfig` | A file differs from the agreed format |
| Architecture rules | `ArchitectureTests.cs` | A project reference breaks the module dependency matrix |
| NuGet vulnerability scan | NuGet audit at restore plus `dotnet list package --vulnerable --include-transitive` | A direct or transitive package has a known vulnerability |
| ESLint | angular-eslint and typescript-eslint | Any lint error in TypeScript or templates |
| npm audit | `npm audit --audit-level=high` | A high or critical advisory |
| Secret scanning | Gitleaks v8.28.0 (binary pinned by SHA-256), full git history | Any finding in any reachable commit |
| Dependency updates | Dependabot (NuGet, npm, GitHub Actions), weekly | Not a gate; opens PRs that must pass the same checks |

> **Secret scanning decision.** The repository is private on a plan where GitHub's native secret scanning may not be available, so the team relies on the Gitleaks CI job. If native scanning is enabled later it adds to, not replaces, the CI job.

**Known gaps**

- Prettier is configured but formatting is not checked in CI (a `format:check` script is a candidate follow-up).
- No SAST tool such as CodeQL is configured; to revisit with later security expectations.

---

## 21. Technical Debt Register

*Source: `docs/quality/technical-debt-register.md`*

Deliberate or discovered shortcuts the team has chosen to carry. An entry is required before a CI check is relaxed, bypassed or given an exception; each entry names an owner and a review date.

| ID | Date | Area | Debt and why it is carried | Review by | Status |
|---|---|---|---|---|---|
| TD-001 | 12/10/2026 | Backend tests | `ApiStartupTests` hard-codes a connection string with a password literal and overrides configured connections. Test only calls `/health` (no DB call); must change before the first DB-backed integration test. Link: R-17. | Before first DB integration test | Open |
| TD-002 | 12/10/2026 | Backend | `Program.cs` contains commented-out debug code and each module has an empty `Class1.cs`. Scaffold leftovers; no behavioural effect. | End of S1 | Open |
| TD-003 | 12/10/2026 | Frontend | `test:ci` passes `--headless`, which the runner ignores when no browser is configured. Harmless warning. | When a browser runner is added | Open |

> **Unfixable audit finding procedure:** record advisory ID, package, version, direct or transitive, and whether it reaches production; state why it cannot be fixed; name a compensating control, owner and review date; link the entry from the PR that changes the threshold; remove the exception in the PR that applies the fix.

---

## 22. Design Decisions (ADRs) *[added in v4.0]*

*Source: `docs/decisions/adr-status-transition-design.md`; `docs/decisions/adr-notifier-design.md`*

| ADR | Decision | Alternative and why not now |
|---|---|---|
| Status transition rules (FR-010, FR-011, FR-012), 30/09/2026 | **Table-driven transition validator:** one lookup table maps current status to allowed next statuses and required roles, checked before any status change. Single source of truth; adding a status changes table entries, not caller logic. | **State pattern:** stronger once statuses need distinct behaviour, but adds class proliferation for a small, fixed set of statuses that differ only in transition validity. Can be refactored later if needed. |
| Notification decoupling (FR-006), 30/09/2026 | **Dependency-injected Notifier interface (Strategy-style):** request handling depends on the abstraction; `EmailNotifier` is injected. A new channel needs a new class and configuration, not changes to request handling; independently testable. | **Observer pattern:** better once several independent channels must react differently to the same event; premature while only email is confirmed. |

Both decisions were made by the whole team and are informed by Assignment 2 research (design quality and design patterns). In the code they appear as `RequestStatusTransitionValidator`, `INotifier`, `StatusChangeNotifier`, `EmailNotifier` and `DisabledNotifier`.

---

## 23. Authentication and Role-Based Access Control Implementation *[added in v4.0]*

*Source: `docs/security/auth-implementations.md`; `src/api`; CR-005*

Covers FR-001, FR-002, NF-001, NF-003, ASR-1 and R-05. Roles follow CR-005: Requester, Staff and Admin (Admin = Management).

| Topic | Decision | Reason / trade-off |
|---|---|---|
| Where auth lives | Custom ASP.NET Core Auth module, not Supabase Auth. | Matches the architecture decision, keeps `password_hash` in our schema, keeps criteria testable in our own code. |
| Hashing | bcrypt (BCrypt.Net-Next), work factor 12, through `IPasswordHasher`. | One-way and salted; passwords limited to 72 bytes because bcrypt ignores input beyond that. |
| Tokens | HS256 JWT with claims `sub`, `acct`, `email`, `role` (exactly one of Requester, Staff, Admin). | `acct` disambiguates requester from employee because `sub` is only unique within a table. |
| Configuration | `Jwt:Secret`, `Issuer`, `Audience`, `ExpiryMinutes` validated at startup. | App refuses to start with a missing or short secret; secret never committed. |
| Registration | Creates Requesters only; the request has no role field. | Staff and Admin accounts come from seed or admin tooling, so nobody can self-register as Admin. |
| Email uniqueness | Lower-cased; checked in both account tables; database unique index is the real guard. | Schema only guarantees uniqueness per table. |
| Failure messages | One generic 401 for unknown email, wrong password, inactive account or unusable hash; dummy verification equalises timing. | Prevents account enumeration. Register reports 409 for duplicates as FR-001 requires. |
| Authorisation | Policies `RequesterOnly`, `StaffOnly`, `AdminOnly` and `AnyEmployee` applied to endpoints. | Server enforces roles; role-specific Angular layouts present the right interface. |
| Last login | Successful login updates `last_login_at` (migration 006). | Failed logins do not change it. |

**Known limits (tracked, not hidden)**

- Rate limiting on the auth routes is not implemented yet.
- No refresh tokens or logout flow yet.
- The seed file keeps a placeholder hash; the Development-only `DemoCredentialSeeder` sets it from `Seed:DemoPassword`.
- In the supplied snapshot the frontend role-guard file is empty and routes declare no guards, so role separation rests on server-side policies and role-specific layouts.

---

## 24. Project Reflection *[added in v4.0]*

*Source: Repository evidence: change log and CRs, ADRs, AI usage register, technical debt register, README and `docs/`*

This reflection looks back across the four milestones. It is grounded in what the repository records (change requests, ADRs, the AI usage register, the technical debt register and the documents themselves) and is organised around the challenges the team faced, the practices that worked and what changed during the project (see the Change Log in section 17).

### 24.1 Challenges

#### Deployment

The modular monolith was chosen to keep deployment to one application (section 11), yet the surrounding pieces still multiplied configuration points: managed Supabase PostgreSQL, a local Docker PostgreSQL, a Node-based migration runner, user secrets and environment variables.

- The first migration run failed on Supabase connection configuration (12/10/2026), and a connectivity spike (S1-10) was needed to settle the connection mode and the Npgsql format.
- On 17/10/2026 the API could not reach the database because user secrets and a `.env` file overrode `appsettings.json`, the API pointed at port 5432 instead of Docker's 5433, and a migration (the `overdue_at` column) had not been applied.
- Local and CI databases use PostgreSQL 16 while Supabase runs 17.x. The repository has a docker-compose file for the database and a release workflow, but no application Dockerfiles, and the deployment compatibility document was never completed.
- Several CI quality-gate demonstrations are still marked 'TBC'.

**Response and lesson.** We documented configuration precedence, added `.env.example`, introduced versioned migrations (001 to 007) so databases can be rebuilt reproducibly, and pinned the SDK and Node versions. Forward Engineering Consideration 3 and Risk R-03 had flagged hosting limits at M1; in hindsight the hosting target and a deployment compatibility note should have been settled, and a test deployment done, much earlier rather than alongside final construction.

#### Integration

The Angular frontend, ASP.NET Core API, PostgreSQL and the email notifier were built in parallel by different members, which produced contract mismatches that only surfaced when the parts met.

- The frontend status page had to combine `/health` and `/health/ready` because the backend did not expose the `/status` endpoint the frontend expected (05 to 08/10).
- Review of the New Request flow found a category field missing from the backend history response, a possibly incomplete proxy configuration and a login URL difference (18/10); the end-to-end flow was not yet confirmed.
- Merging `development` produced conflicts in five frontend files and CRLF/LF line-ending failures in the format check (18/10).
- AI-assisted audits of the business-logic layer found loopholes that needed remediation, and about half of the proposed bug fixes were rejected as inconsistent (18/10).
- The API integration ADR file stayed empty, so the contract lived mainly in `openapi.yaml`.

**Response and lesson.** We relied on the OpenAPI contract, the `INotifier` abstraction to isolate email, and integration tests that run the API against PostgreSQL (64 methods) alongside 31 Angular spec files and a Cypress smoke test. Next time we would agree endpoint contracts first, record the API integration decision when it is made, and integrate frontend and backend in smaller, more frequent increments.

#### Role-based access control

The role model was ambiguous from the outset. The requirements named four access groups (requester, staff, admin and management), yet Admin and Management ended up with overlapping, and in places contradictory, duties. For example, NF-002 said admins must not reach management-only functionality while FR-008 to FR-016 gave the same people the same screens.

- Forward Engineering Consideration 1 warned at M1 that retrofitting roles after the schema is set is expensive; the authentication work (issue #113) implemented three roles (Requester, Staff, Admin) and noted that CR-005 was 'to be recorded', and the seed data creates only Admin and Staff employee roles.
- Enforcement needed four authorisation policies, a role-matrix integration test, a public-route allow-list test and JWT debugging (07/10). Registration deliberately creates requesters only so nobody can self-register as Admin.
- Documents lagged the model: requirements, the persistence ADR and OpenAPI summaries still said 'management' after CR-005.
- In the supplied snapshot the frontend role guard is an empty file and routes declare no guards, so separation depends on server-side policies and role-specific layouts. Known limits remain: no auth rate limiting, no refresh tokens or logout.

**Response and lesson.** CR-005 consolidated Admin and Management and listed every requirement, seed-data item, API policy and test to change together. We should have confirmed the role model with stakeholders before writing acceptance criteria and kept code, seed data and documents aligned in the same pull request.

#### Time management

Three people, four milestone windows and no slack: the scope baseline itself recorded that there was no residual capacity to recover from late baseline errors.

- Scope and process kept moving late: CR-001 (FR-017) and CR-003 on 22/09, CR-004 on 30/09 and CR-005 on 11/10.
- Build, debugging and merging work clustered in 12 to 18 October according to the AI usage register, leaving little room to finish evidence.
- AI-assisted work cost rework: under 30% of generated unit-test templates were usable, and some fixes were rejected.
- Items remain open: rate limiting, refresh tokens and logout, a Prettier check and SAST in CI, technical-debt items TD-001 to TD-003 and the CI evidence marked 'TBC'.

**Response and lesson.** We moved to short-lived issue-linked branches (CR-004), a Kanban board, lead/review ownership per area and a CI gate that catches defects at pull-request time, and we kept deferred scope on paper (SMS/WhatsApp, advanced analytics, priority) rather than absorbing it. Next time documentation and deployment work should be scheduled explicitly, integration front-loaded, and AI-assisted output time-boxed with verification built in.

#### Keeping track of documentation

Evidence was spread across roughly forty repository files, two SharePoint Excel workbooks (RTM and risk register) and GitHub issues. As requirements and process changed, not every file moved with them.

- The RTM and the detailed risk register are not version-controlled with the code.
- After CR-005, `requirements.md`, the persistence ADR and the OpenAPI summaries still used the Management role. The ASR file described FR-017 as simulated SMS/WhatsApp and mentioned an 'FR-018', which does not match CR-001 (category management) or CR-002 (email).
- The README still described persistent personal branches and a CR-002 branching file, although CR-004 replaced that model and CR-002 is the notification change; it also said setup instructions were still to come and pointed to `docs/assignments`, while the research folder is `docs/research-material`.
- `docs/deployment/deployment-compatibility,md` (note the comma in the name) and `docs/decisions/adr-api-integration.md` were empty, yet PED v2.0 and v3.0 link to them; the v3.0 test-case link pointed to a placeholder.
- CR-003 had no confirmed approvers recorded and CR-005 kept its placeholder title; CR-005 linked to a misspelled `adr-persistance.md`; the change log is not in date order.
- GitHub governance still says all three members are Owners, while CR-003 reserved administration to the GitHub lead; citations used in ADRs and CRs are missing from `references.md`.

**Response and lesson.** The one-file-per-section PED, ADR and CR templates, the RTM and the AI usage register gave us structure, and v4.0 corrects the clearest inconsistencies (sections 3, 12, 13, 14 and 15). We should treat documents like code: update them in the same pull request as the change, add a link check to CI, and make 'baseline sections updated' a PR-template checklist item.

### 24.2 What worked

#### Change requests

- A structured template forced an impact analysis (requirements, architecture, UI/API/data, security, testing, scope, schedule, cost, risk) before any change was accepted, and every CR was approved by members other than the requester.
- The process let us reverse a decision on evidence: CR-004 replaced the persistent branches introduced by CR-003, citing Assignment 2 findings on small pull requests and short-lived branches.
- CR-005 named exactly which requirements, seed data, policies and tests to change together, which made a security-model change safe late in the project.
- CR-001 and CR-002 handled scope growth and a channel change without disturbing the architecture: none of the five CRs needed to change the modular-monolith architecture or the technology stack.

#### Communication

- A shared accountability matrix gave each area a lead and reviewers, while all three members stayed jointly accountable.
- Governance made discussion routine: two reviewers per pull request, a PR checklist, and Issue to branch to PR to review traceability.
- ADRs recorded decision makers, alternatives and rationale, and CRs recorded who requested and who approved, so decisions were visible and challengeable.
- The AI usage register was a standing, shared record of how AI was used, how it was verified and where it was wrong.

#### Research

- Assignment 2 research directly shaped the table-driven validator, the dependency-injected Notifier, layered validation and transactional assignment, and the branching strategy.
- Technology choices (Angular versus React, ASP.NET Core versus Node.js, Supabase versus self-hosted PostgreSQL, REST with OpenAPI) were compared against ASRs, team capability, cost, security and schedule, and the Supabase spike reduced database-connection uncertainty.
- Constraints were analysed against recognised sources (ISO/IEC/IEEE 29148, NIST SP 800-53, PMBOK, Sommerville), which gave us defensible, testable requirements.
- As a result the core architecture and stack held through to the end; the changes we did need were scope, process and role-model changes, not rework of the foundation.

### 24.3 What changed (refer to the change log)

The baseline moved in three areas, all recorded in the Change Log (section 17): product scope, delivery process and the security model.

| Change | What changed | Why | Effect on the project |
|---|---|---|---|
| CR-001 (22/09/26) | FR-017 added: admins manage service categories (add, update, delete; no duplicates; at least one remains). | Keep categories current without developer changes. | Moderate scope increase; ServiceCategory persistence, category endpoints, admin UI and extra tests; requester form reads the live category list. Moves 'configurable categories' out of deferred scope. |
| CR-002 (08/09/26) | Notification channel changed from SMS simulation to email simulation (FR-006). | Email carries richer, traceable content and is easier to test. | Minimal effort; `EmailNotifier` behind `INotifier`; risks of address validity and delivery failure tracked; architecture unchanged. |
| CR-003 (22/09/26) | Persistent developer branches, hotfix isolation, role-restricted GitHub administration, expanded CI gate. | Reduce coordination overhead for three people; enforce quality automatically. | CI workflows and CODEOWNERS added; branching part later superseded; hotfix freeze and CI gate retained. |
| CR-004 (30/09/26) | Short-lived, issue-linked branches and a standard PR format replace persistent developer branches. | Persistent branches mixed tasks and caused repeated reviews. | One task, one branch, one PR; clearer traceability; smaller reviews; branch naming convention. |
| CR-005 (11/10/26) | Three staff roles reduced to two: Staff and Admin (Admin = Management). | Admin and Management had identical access, adding complexity without benefit. | Simpler authentication, database seed data, API policies, UI navigation and tests; requirements NF-002 and FR-008 to FR-016 reworded. |

### 24.4 Key lessons

- Decide the hosting target and write the deployment note early, then deploy a thin slice end to end before building features.
- Fix the contract between frontend and backend first and keep the API integration decision current.
- Settle the role model with stakeholders before writing acceptance criteria, and change code, seed data, policies, tests and documents together.
- Plan documentation, evidence capture and deployment as scheduled work, not as end-of-milestone catch-up.
- Keep every controlled artefact in version control and add automated checks for broken links and empty files.
- Use AI as a drafting aid with verification built in; the register shows the best results came when output was tested before it was adopted.

---

*CivicConnect | Project Engineering Document (PED) v4.0 | Team TMT*
