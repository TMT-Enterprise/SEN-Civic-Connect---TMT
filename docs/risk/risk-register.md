# Risk and Forward Engineering Considerations

## Risk Probability and Impact Matrix

The matrix below gives a quick visual summary of risk priority before the detailed register.
Risk R-05 is recorded in the register as low to medium probability; it is placed in the medium
probability column here for visual purposes only, and the register remains the authoritative
source for its rating.

| Impact \ Probability | Low | Medium | High |
|---|---|---|---|
| High | | R-02, R-05, R-07, R-08 | |
| Medium | | R-03, R-04, R-06 | R-01 |
| Low | | | |

The detailed risk register can be accessed from the Excel sheet linked below:

[Risk register Excel document (SharePoint)](https://belgiumcampusacza-my.sharepoint.com/:x:/g/personal/601726_student_belgiumcampus_ac_za/IQDqhGJbCs84RYbdrYMwtU3sAaE-Aa0qo_Wi0gQq7TX_zJk?e=QRLyLV&nav=MTVfe0EyOTIzOEFDLUExREYtNEZFOS05NDE5LTg5NkJFNUQ0ODVFN30)

## Forward Engineering Considerations

Certain later-lifecycle concerns already influence current requirements, constraints, risks, or
assumptions. Recording them at Milestone 1 preserves decision options and avoids unnecessary
constraints on later work. The concerns below are project-specific; they are not a restatement
of generic software-engineering advice.

**1. Authentication and role-based access control model**
- Why it matters now: determines what a staff versus management account requires at the data
  level.
- Later influence: Milestone 2 architecture and data design.
- Missing information / risk of ignoring: whether staff need sub-roles (for example a
  supervisor tier). Risk: retrofitting roles after the schema is set is expensive.

**2. Status-transition rules**
- Why it matters now: requirements already assume controlled transitions with no skipped
  states.
- Later influence: Milestone 2 design; Milestone 3 implementation and testing.
- Missing information / risk of ignoring: exact allowed transition map. Risk: inconsistent or
  ambiguous status logic if left implicit.

**3. Deployment environment and hosting limits**
- Why it matters now: free-tier constraints affect what can realistically be promised in
  non-functional requirements.
- Later influence: Milestone 2 technology decision; Milestone 4 deployment.
- Missing information / risk of ignoring: concrete platform limits (connections, storage,
  uptime). Risk: non-functional requirements baselined now may prove undeliverable.

**4. Observability and audit trail**
- Why it matters now: management oversight and accountability requirements depend on action
  logging existing from the start.
- Later influence: Milestone 2 data design; Milestone 3 construction.
- Missing information / risk of ignoring: what level of audit detail is sufficient. Risk:
  retrofitting audit logging is costly and error-prone.

**5. Data backup and recovery**
- Why it matters now: even a student project holding real-shaped requester data should have a
  recovery story.
- Later influence: Milestone 2 architecture; Milestone 4 operational readiness.
- Missing information / risk of ignoring: chosen platform backup capabilities. Risk: total data
  loss with no fallback plan.

**6. Automated testing approach**
- Why it matters now: deferring the testing strategy to Milestone 3 tends to produce untested
  code under deadline pressure.
- Later influence: Milestone 3 quality strategy.
- Missing information / risk of ignoring: which test types are realistic for a three-person
  timeline. Risk: weak or absent test evidence discovered too late to correct.

**7. Notification delivery reliability**
- Why it matters now: even email-only notification (baseline) has failure modes such as spam
  filtering and delivery delay.
- Later influence: Milestone 2 design; Milestone 3 testing.
- Missing information / risk of ignoring: confirmed email-service reliability at the chosen
  free tier. Risk: requesters silently miss status updates, undermining core value.
