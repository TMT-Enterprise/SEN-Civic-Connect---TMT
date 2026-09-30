# Architecturally Significant Requirements (ASRs) and Quality Drivers

This section identifies the requirements and the quality attributes that materially shape CivicConnect architecture, rather
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

**Source requirement:** FR-004 (Unique request identifier); FR-011 (Controlled, forward-only status transitions); FR-013 (audit trail of status, staff,and timestamp) [View detailed Requirements](../requirements/requirements.md).

**Linked evidence:** Assignment 2 Task 2 [View Assignment 2](../research-material/assignment2.md), identified request assignment as an operation where an incomplete update leaves CivicConnect with inconsistent information (for example, a request assigned to staff without that event being logged). 
The quality constraint requires measurable evidence rather than asserted correctness, and Risk R-02 [View Risks](../risk/risk-register.md), notes that reworing a baselined requirement without a controlled data model could invalidate earlier design work.

**Measurable/testable expectation:** A request-assignment operation either commits the staff assignment, the status update, and the history 
record together atomically or rolls them all back. This is verifiable through a forced-failure test confirming no partial state remains in the database.

**Influence on architecture, data, technology, and design:** The architecture requires a persistence layer that supports transactional operations rather than independent, uncoordinated writes. The data model must structure assignment, status, and history changes so they can be committed atomically. The database technology selected must support transactions at the operational level required. The design directly applies the layered validation and transactional approach recommended in Assignment 2 Task 2 [View Assignment 2](../research-material/assignment2.md), rather than re-deriving it at Milestone 2.

## 3. Timely, Reliable Notification Delivery

**Source Requirement:** FR-006 (email notification within 60 seconds of a status update), FR-017 (simulated SMS/WHatsApp notification)[View detailed Requirements](../requirements/requirements.md).

**Linked evidence: ** The stakeholder analysis names the requester's core need as visibility and confidence that a submitted request will be handled. Forward Engineering Consideration 7 flags notifications delivery reliability, including failure modes such as spam filtering and delivery delays, as an operational concern. The SMS/WhatsApp simulation itself follows directly from CR-001's resolution of the earlier defered-scope item.

**Measurable/testable expectation:** At least 95 percent of status-change notifications across both real email channel and the simulated channel are generated within 60 seconds of the triggering event. 

**Influence on architecture, data, technology, and design:** The architecture must decouple request-handling logic from the notification 
mechanism rather then invoking a specific channel directly. The data model requires notification/log record capturing the channel, timestamp, and the content for both real and simulated channels. The email service selected must be realistically capable of meeting the 60-seconds SLA, rather than merely available. The design applies the dependency-injected **Notifier** interface recommended in Assignment 2 Task 1 [View Assignment 2](../research-material/assignment2.md),extended to cover the newly in-scope simulated channel from FR-017.

## 4. Usability for Non-Technical Requesters and Staff

**Source requirements:** NF-004 (95 percent of staff/management to complete core tasks after one hour of training). NF-005 (95 percent of requesters complete the request form within 5 minutes)[View detailed Requirements](../requirements/requirements.md).

**Linked evidence:** The business case explicitly cites current fragmented, hard-to-use channels (email, phone, paper) as a driver for CivicConnect. The stakeholder analysis identifies the requester'r primary need as ease of use. FR-018's admin category management was
deliberately maintained as a fixed default list precisely so routine use of the system remains straightforward.

**Measurable/testable expectation:** Usability testing with representative, non-technical participants meets the 95 percent thresholds stated in NF-004 and NF-005 within the specified time and training constraints.

**Influence on architecture, data, technology, and design:** The architecture should avoid unnecessary client-side complexity that could hinder first-time users. The data model keeps the category list simple and fixed by default, avoiding the requirement for administrative configuration prior to request submission. The front-end technology choice should favor a mainstream, well-documented framework over one that adds steep learning curve without usability benefits.  The design reinforces the earlier table-driven transition validator decision over the State pattern, a simpler underlying design is easier for staff to troubleshoot when anomalies occur.

## 5. Cost-Constrained Availability and Deployability

**Source requirements:** NF-007 ( at least 90 percent availability during operating hours, excluding scheduled maintenance)[View detailed Requirements](../requirements/requirements.md).

**Linked evidence:** The Cost and Resources constraints requires the team to prefer free or low-cost services while remaining cognizant of their limitations. Risk R-03 notes that a chosen free-tier may impose usage limits, such as connection caps or automatic idling, that blocks demonstration or testing. Forward Engineering Consideration 3 flags deployment environments and hosting limits as key concerns to resolve before finalizing the technology-stack decision.

**Measurable/testable expectation:** Up-time monitoring during defined operating hours demonstrates at least 90 percent availability once the hosting platform is selected and deployed.

**Influence on architecture, data, technology, and design:** The architecture should avoid a distributed design that multiplies the number of hosted components (and consequently the number of concurrent free-tier constraints), aligning with Assignment 2 Task 3's recommendation against introducing unnecessary network boundaries [View Assignment 2](../research-material/assignment2.md). The data model and persistence technology must be evaluated against the actual connection and storage limits of the selected free-tier platform. Deployment planning must explicitly record which availability risks are consciously accepted at this stage versus those that necessitate a platform migration prior to release.
