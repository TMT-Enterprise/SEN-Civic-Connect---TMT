# CivicConnect Test Cases 
### Test suites covering critical functional requirements. The cases include positive, negative, boundary and authorisation scenarios. Test cases will be translated into black box, white box and alternative production testing in M3.

## FR-001 - User Registration

| Test ID | Test Scenario and Test Data | Expected Result |
|---|---|---|
| TC-FR001-01 | Register with valid details and a new email | Account is created successfully. |
| TC-FR001-02 | Register using an existing email | Registration is rejected with an appropriate message. |
| TC-FR001-03 | Submit missing required fields or an invalid email | Registration is rejected with validation messages. |
| TC-FR001-04 | Submit SQL injection or script-like input | Input is handled safely; no unintended database operation or script execution occurs. |

## FR-002 - User Login

| Test ID | Test Scenario and Test Data | Expected Result |
|---|---|---|
| TC-FR002-01 | Log in using valid credentials | User is authenticated successfully. |
| TC-FR002-02 | Log in using an incorrect password, missing credentials or unregistered email | Login fails without granting access and is rejected with appropriate validation. |
| TC-FR002-03 | Submit SQL injection or script-like input | Input is handled safely; no unintended database operation or script execution occurs. |

## FR-003 - Submit a Service Request

| Test ID | Test Scenario and Test Data | Expected Result |
|---|---|---|
| TC-FR003-01 | Submit a request with valid details | Request is saved successfully, and profile information is populated automatically. |
| TC-FR003-02 | Submit without a required field or category | Submission is rejected with validation messages. |
| TC-FR003-03 | Submit a description at the minimum/maximum length and just beyond the limit | Valid boundary values are accepted; invalid lengths are rejected. |
| TC-FR003-04 | Submit SQL injection or script-like input | Input is handled safely; no unintended database operation or script execution occurs. |

## FR-005 - View Current Request Status

| Test ID | Test Scenario and Test Data | Expected Result |
|---|---|---|
| TC-FR005-01 | View requests in each permitted status | Correct status is displayed: Submitted, Assigned, In Progress, Resolved or Closed. |
| TC-FR005-02 | View an open request after its due date | Request is marked Overdue. |

## FR-006 - Email Notifications

| Test ID | Test Scenario and Test Data | Expected Result |
|---|---|---|
| TC-FR006-01 | Successfully change a request's status | Email is sent to the requester's registered email address. |
| TC-FR006-02 | Inspect a notification | Email contains the correct request ID and new status. |
| TC-FR006-03 | Update a request without changing its status | No status-change email is sent. |

## FR-007 - Requester Request History

| Test ID | Test Scenario and Test Data | Expected Result |
|---|---|---|
| TC-FR007-01 | View history containing multiple requests | All the requester's own requests and saved details are displayed correctly. |
| TC-FR007-02 | View history when no requests exist | An appropriate empty-state message is displayed. |
| TC-FR007-03 | Attempt to access another requester's history through the UI or API | Access is denied. |
| TC-FR007-04 | View a completed request | Relevant completion timestamps and current status are displayed accurately. |

## FR-008 - Admin Viewing and Assignment

| Test ID | Test Scenario and Test Data | Expected Result |
|---|---|---|
| TC-FR008-01 | Admin views unassigned requests and their details | Unassigned requests and required details are displayed. |
| TC-FR008-02 | Non-admin attempts to access the unassigned queue or assign a request | Access is denied. |
| TC-FR008-03 | Admin assigns a request to eligible staff and sets a due date at least 24 hours ahead | Assignment succeeds and details are saved. |
| TC-FR008-04 | Admin selects invalid staff, an invalid request or a due date less than 24 hours ahead | Assignment is rejected with an appropriate message. |
| TC-FR008-05 | Verify the queue after successful assignment | The request's assignment is saved and its unassigned state is updated.|

## FR-009 & FR-015 - Search and Filter 

| Test Case ID | Test Scenario | Expected Result |
|---|---|---|
| TC-FR915-01 | Search using a valid request ID or keyword | Matching requests are displayed. |
| TC-FR915-02 | Filter requests using valid criteria | Only requests matching the selected filters are displayed. |
| TC-FR915-03 | Search with no matching results or an empty query | No-results or default results are displayed appropriately. |
| TC-FR915-04 | Search for a request using lowercase text when the stored value contains uppercase characters. | Matching results are returned regardless of letter case. |
| TC-FR915-05 | Search using special characters or SQL-injection test strings. | Input is handled safely; no query manipulation or unauthorised data exposure occurs. |

## FR-010: Accept or Reject Assigned Requests

| Test Case ID | Test Scenario | Expected Result |
|---|---|---|
| TC-FR010-01 | Staff accepts a request assigned to them | Request is accepted and the update is saved. |
| TC-FR010-02 | Staff rejects an assigned request | Request is rejected and the decision is recorded. |
| TC-FR010-03 | Staff attempts to act on another employee's request | Unauthorised action is denied. |


## FR-011: Controlled Status Transitions
For the sequential-transition tests below, the assumed normal workflow is:
`Submitted → Assigned → In Progress → Resolved → Closed`

| Test Case ID | Test Scenario | Expected Result |
|---|---|---|
| TC-FR011-01 | Change status using a permitted transition (eg. `Assigned` to `In Progress`)  | New status is saved successfully. |
| TC-FR011-02 | Attempt an invalid or out-of-sequence status transition (eg. `Resolved` to `Submitted`) | Update is rejected. |
| TC-FR011-03 | A staff member attempts to close a request without a completion description.  | The closure is rejected and a validation message is displayed. |
| TC-FR011-04 | Update a request past its due date | Overdue status is handled according to the defined rules. |
| TC-FR011-05 | An authorised staff member closes a valid request. | The closure action is permitted, subject to the required completion description. |

## FR-012: Automatic Overdue Status

| Test Case ID | Test Scenario | Expected Result |
|---|---|---|
| TC-FR012-01 | An open request passes its due date | System marks the request as Overdue. |
| TC-FR012-02 | Current time is before the due date | Request is not marked Overdue. |

## FR-014: Management Dashboard

| Test Case ID | Test Scenario | Expected Result |
|---|---|---|
| TC-FR014-01 | Open the dashboard with existing requests | Displayed statistics match the underlying request data. |
| TC-FR014-02 | Management refreshes the dashboard after request data has been changed. | Dashboard statistics reflect the saved change. |
| TC-FR014-03 | Open the dashboard when no requests exist | System displays zero values without errors. |
| TC-FR014-04 | Access the dashboard without management permission | Access is denied. |

## FR-016: Request Lifecycle History

| Test Case ID | Test Scenario | Expected Result |
|---|---|---|
| TC-FR016-01 | Management views the lifecycle history of a request. | The relevant lifecycle stages display their timestamps; stages not yet reached display `n/a`. |
| TC-FR016-02 | Attempt to access lifecycle history as a requester or normal staff member without the required management permission. | Access is denied. |
| TC-FR016-03 | Open lifecycle history for a nonexistent request ID. | The system displays a suitable not-found response or message. |

## FR-017: Service Category Management

| Test Case ID | Test Scenario | Expected Result |
|---|---|---|
| TC-FR017-01 | Admin adds, updates or deletes a valid category | Change is saved and reflected in the category list. |
| TC-FR017-02 | Add a duplicate, blank or whitespace-only category | Category is rejected. |
| TC-FR017-03 | Add or update a category at the permitted name-length boundary | Valid length is accepted; values exceeding the limit are rejected. |
| TC-FR017-04 | Attempt to modify categories without admin permission | Access is denied. |
| TC-FR017-05 | Delete the last category or a category used by existing requests | System enforces the approved deletion rules and preserves data integrity. |
| TC-FR017-06 | Add or remove a category and open the requester form | Available categories reflect the saved changes; invalid category submissions are rejected. |
