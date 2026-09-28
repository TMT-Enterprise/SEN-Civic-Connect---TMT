# Architecturally Significant Requirements (ASRs) and Quality Drivers

This section identifies the requirements and the qaulity attributes that materially shape CivicConnect architecture, rather
than listing every quality attribute covered in class. Each driver below is selected because it is tied to specific stakeholder evidence,
a stated constraint, or a recorded risk from the M1 baseline, rather than appearing on a generic checklist.

## 1. Security: Authenticated Access and Role-Based Authorization

**Source Requirements:** NF-001 (access restricted to authorized users); NF-002 (role-based access enforced requester, staff, admin and managements);
NF-003 (secure password hashing) [View detailed Requirements](../requirements/requirements.md).

**Linked evidence:** The stakeholder analysis identifies the System Administrator's core needs as security, reliability, and access control. The Security
constraint requires access control to be integrated from the requirements stage rather than retrofitted later. Additionally, Risk R-05 
[View Risks](../risk/risk-register.md), records the inadequate protection of sensitive requester data as a high-priority risk.

**Measurable/testable expectation:** A requester account can never reach staff-only or management-only functionality, which is verifiable through role-based access-control test cases. Furthermore, no password is ever recoverable in plaintext from the data store.

**Influence on architecture, data, technology and design:** The architecture requires an authentication and authorization layer positioned
in front of all business logic, rather than access checks scattered across individual endpoints. The data model needs a role field enforced strcitly
at the account level rather than inferred from context. The selected backend technology must cleanly support middleware-style request interception. 
Design carries directly into the layered=validation approach recommended in Assignment 2 [View Assignment](../research-material/assignment2.md), where the backend is responsible for enforcing whether a user is authorized to perform an action.

## 2. Data Integrity and Consistency for Request Lifecycle Operations

**Source requirement:** FR-004 (Unique request identifier); FR-011 (Controlled, forward-only status transitions); FR-013 (audit trail of status, staff,
and timestamp) [View detailed Requirements](../requirements/requirements.md).

**Linked evidence:** Assignment 2 Task 2 [View Assignment 2](../research-material/assignment2.md), identified request assignment as an operation where an incomplete update leaves CivicConnect with inconsistent information (for example, a request assigned to staff without that event being logged). 
The quality constraint requires measurable evidence rather than asserted correctness, and Risk R-02 [View Risks](../risk/risk-register.md), notes that reworing a baselined requirement without a controlled data model could invalidate earlier design work.

**Measurable/testable expectation:** A request-assignment operation either commits the staff assignment, the status update, and the history 
record together atomically or rolls them all back. This is verifiable through a forced-failure test confirming no partial state remains in the database.

**Influence on architecture, data, technology, and design:** The architecture requires a persistence layer that supports transactional operations rather than independent, uncoordinated writes. The data model must structure assignment, status, and history changes so they can be committed atomically. The database technology selected must support transactions at the operational level required. The design directly applies the layered validation and transactional approach recommended in Assignment 2 Task 2 [View Assignment 2](../research-material/assignment2.md), rather than re-deriving it at Milestone 2.
