# Scope Baseline and Constraints

The scope baseline defines the committed boundary of the CivicConnect solution for this
project. It is derived from the minimum business capabilities set out in the Master Project
Brief and refined against the specific needs of the requester, staff, and management
stakeholder groups identified in the team's stakeholder analysis. Setting this boundary now
allows the team to commit to a deliverable set that can realistically be engineered, tested,
and defended across the four milestones, while explicitly recording what has been excluded or
deferred and why.

## In Scope (Committed Baseline)

**Requester capabilities**

These capabilities address the requester's core need for visibility and confidence that a
submitted request will be handled, replacing the fragmented email, phone and paper-based
channels described in the business need.

1. Submit a new service request with category, description, location or area.
2. View the current status of a submitted request (Received, Assigned, In Progress, Resolved,
   Closed)
3. View a history or list of previously submitted requests
4. Receive meaningful feedback (in-app and email) when a request is accepted, rejected, updated
   or completed

**Staff capabilities**

These capabilities give staff the operational control needed to manage, prioritize and resolve
requests, directly addressing the business need's concern that staff currently struggle to
prioritize work and coordinate ownership.

5. View the queue of requests relevant to an authorized role
6. Search, filter and sort requests by category, status and date
7. View full request detail, including submitter information and history
8. Assign or accept responsibility for a request
9. Update request status through defined, controlled transitions, so a request cannot move
   directly from Received to Closed without passing through the intermediate states
10. Record comments, actions and resolution information against a request
11. Resolve or close requests where authorized to do so

**Management capabilities**

These capabilities provide the oversight and accountability data that management currently
lacks, without introducing reporting infrastructure beyond what the stated need requires.

12. Dashboard view of open, overdue, resolved and closed requests
13. Filter or report by category, status, staff member or date range
14. Access sufficient information to support accountability and service performance analysis

## Out of Scope

The following items are excluded for the duration of the project, not only for Milestone 1.
Reintroducing any of them later would require a formal change request and impact analysis.

| Excluded | Rationale |
|---|---|
| Native mobile applications (iOS/Android) | A responsive web interface satisfies the stated access need without doubling the build, test and deployment surface. |
| Integration with other organisation's external case management systems | No confirmed data sharing agreement or integration requirement exists for this project; the scenario does not specify any such system. |
| AI-based automatic request routing or triage | Introduces a verification and bias risk problem the team cannot adequately resource within a three-person, four-milestone project. |
| Public-facing API for third-party consumption | No confirmed external consumer exists; a public API creates an open-ended support and versioning obligation with no demonstrated stakeholder demand. |

## Deferred and Future Scope

The following items are retained for future consideration rather than discarded, because they
may hold value once further evidence is available. A decision to formally adopt or permanently
exclude a deferred item will be recorded in the Engineering Decision Log once that evidence
exists.

| Deferred item | Why not now | Evidence needed before deciding |
|---|---|---|
| SMS and WhatsApp notifications | Free-tier SMS gateways are rate-limited and add delivery and retry logic that is not justified without a confirmed need. | Confirmed notification channel requirement from stakeholders, plus cost research on gateway options. |
| Advanced analytics or reporting dashboards (trend charts, SLA breach prediction) | Basic counts and overdue flags satisfy the stated management need; anything beyond that is currently unproven value. | Evidence that management needs predictive or trend data rather than operational counts. |
| Multi-language user interface | No language requirement has been stated for requesters in the scenario. | A confirmed requester language need. |
| Configurable, admin-defined request categories (instead of a fixed list) | Adds an administrative configuration subsystem that is not core to proving the request-handling workflow. | Evidence that a fixed category list is insufficient for real use. |
| Service request priority functionality | Criteria for whether a service request should be high or low priority is ambiguous. | Evidence of being able to rank a service as high or low priority, depending on the specific request itself and the organisation's strengths and weaknesses in certain sectors. |

**Deliberate exclusion selected for defence:** SMS and WhatsApp notifications. This exclusion
follows directly from the project's cost constraint. Free-tier SMS gateways are rarely free at
any meaningful scale, and building reliable delivery and retry logic for a channel with no
confirmed stakeholder mandate would not be a responsible use of a fixed-length project. In-app
and email notification already satisfy the requester's stated need for status feedback, so this
deferral does not reduce the delivered value of the baseline.

## Constraints

Teams must engineer within the constraints defined by the Master Project Brief. These
constraints are part of the assessment and must influence requirements, architecture, and
design decisions. Each constraint is analysed below for its engineering implications on
CivicConnect.

**Scope**

CivicConnect must deliver a single, cohesive baseline (see In Scope, above) rather than an
expanding feature set. Any proposed addition beyond the baseline must be justified against
stakeholder value and its impact on schedule, cost, quality and security before it is accepted.
Uncontrolled scope growth is not acceptable under the Master Project Brief.

**Schedule**

Delivery is constrained to four formal milestone windows within the SEN381 term. There is no
residual capacity to recover from late baseline errors. A requirement or design decision that
is incorrect or incomplete at Milestone 1 propagates directly into Milestone 2 architecture
work and the Milestone 3 construction window. Schedule risk is therefore front-loaded: errors
in the engineering foundation are more costly than equivalent errors discovered later.

**Cost and Resources**

The team must prefer free or low-cost services for hosting, database, notification, and
continuous integration where practical. At the same time, the team must identify the
operational cost that would apply beyond the educational context so that cost implications
remain visible. Three-person team capacity is itself a hard resource constraint: every
additional feature or unfamiliar technology carries an opportunity cost against the other
required engineering artefacts (RTM, risk register, decision log, governance evidence).

**Quality**

Quality attributes must be defined as measurable non-functional requirements and later
supported by evidence. Assertions such as "the system is fast" are not evidence, while "status
updates render within two seconds under normal load" is testable and can be verified later with
real evidence. This follows the international standard for requirements engineering, which
treats verifiability as a required characteristic of a well-formed requirement (ISO/IEC/IEEE,
2018). Quality requirements baselined now will be revisited once the hosting platform is
confirmed at Milestone 2, since platform limits may affect what is realistically achievable.

**Security**

Security is a lifecycle-wide responsibility, not a final add-on. At minimum: authenticated
staff and management roles with role-based access control applying the principle of least
privilege (National Institute of Standards and Technology, 2020), protection of requester
personal data such as contact details and complaint content, and status transitions that are
required and controlled so that request history cannot be silently altered. These implications
must be captured as requirements at this milestone even though the technical implementation is
not decided until later.

## Key Constraint Interactions and Trade-offs

Constraints do not act in isolation. The following interactions are recorded so that later
architecture and design decisions remain informed by the trade-offs already visible at
baseline.

1. **Cost versus security:** free-tier hosting and database options often provide weaker
   built-in access control or encryption than paid tiers, which may require the team to
   implement controls in application logic that a paid platform would otherwise provide.
2. **Schedule versus quality:** a fixed four-milestone schedule with a three-person team
   creates real pressure to under-test as the deadline approaches. The Master Project Brief
   requires this trade-off to be recorded and approved rather than silently absorbed by cutting
   testing, since unmanaged trade-offs of this kind are the classic origin of what is termed
   technical debt (Cunningham, 1992).
3. **Scope versus cost and resources:** every additional capability, such as richer reporting,
   increases the persistence, interface and testing surface, which competes directly with the
   same three people's time needed for governance and documentation evidence.
