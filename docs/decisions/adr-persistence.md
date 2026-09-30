## Initial Data Schema — Entity and Lifecycle Analysis

| Entity / Aggregate | Important Attributes | Relationships | Ownership Traceability | Lifecycle Implications |
|---|---|---|---|---|
| **EMPLOYEE_ROLE** | `role_id` (PK), `name` | One role can be assigned to many employees | Role-based access for requester, staff, admin and management | Roles are stable reference data and should not be deleted while employees still reference them. |
| **EMPLOYEE** | `employee_id` (PK), `role_id` (FK), `name`, `id_number`, `phone_number`, `email`, `password_hash`, `is_active`, `created_at` | Belongs to one employee role; can be assigned requests; can perform status changes; can create assignment records | Authentication, role-based access, employee assignment, request processing and audit responsibility | Employees should be deactivated rather than physically deleted so historical assignments and audit records remain valid. |
| **REQUESTER** | `requester_id` (PK), `name`, `email`, `password_hash`, `is_active`, `created_at` | Can submit many service requests; receives notifications through their requests | Requester registration, authentication, request submission, notifications and request history | Requesters can be deactivated rather than physically deleted where historical requests must be retained. |
| **SERVICE_CATEGORY** | `category_id` (PK), `type`, `is_active` | One category can be associated with many service requests | Controlled request categories, category-based searching/filtering and admin category management | Admins can add, update, or deactivate categories. Referenced categories should not be physically deleted. At least one active category must remain. |
| **REQUEST_STATUS** | `status_id` (PK), `name` | One status can be the current status of many requests and can appear in many history records | Request lifecycle, status changes, notifications, overdue handling, reporting and history | Status values are controlled by the system and should remain stable because they are referenced by current requests and historical records. |
| **SERVICE_REQUEST** | `request_id` (PK), `requester_id` (FK), `category_id` (FK), `assigned_employee_id` (FK), `current_status_id` (FK), `description`, `submitted_at`, `due_at`, `updated_at`, `closed_at` | Belongs to one requester and category; has one current status; may have one current assigned employee; has many history, assignment and notification records | Core service request lifecycle, identification, assignment, status tracking, searching, reporting and history | Created after successful submission and progresses through the request lifecycle. Requests should not be physically deleted because they are required for history, reporting and audit purposes. |
| **REQUEST_STATUS_HISTORY** | `history_id` (PK), `request_id` (FK), `employee_id` (FK), `status_id` (FK), `changed_at` | Belongs to one service request; records the employee responsible for a status change; references the resulting status | Records each status change, responsible employee, timestamp and lifecycle history | Append-only audit history. Each status transition creates a new record. Existing history should not be overwritten. |
| **REQUEST_ASSIGNMENT** | `assignment_id` (PK), `request_id` (FK), `employee_id` (FK), `assigned_by_employee_id` (FK), `assigned_at`, `decision`, `decision_at`, `rejection_comment` | Belongs to one request; records the assigned employee and the employee who performed the assignment/decision | Request assignment, acceptance/rejection and recording of staff actions| Assignment and rejection records are retained as history. A rejected request can return to the unassigned pool while the rejection record remains available for audit. |
| **NOTIFICATION** | `notification_id` (PK), `request_id` (FK), `status`, `created_at`, `sent_at` | Belongs to one service request; recipient is derived through the requester's relationship with the request | Status-change email notifications and delivery requirement | Created only when a qualifying status change occurs. Delivery timestamps provide evidence that the notification was sent within the required time. |

Assumptions
* An employee has one active role at a time
* Emails are unique within each account type
* Passwords are stored only as hashes
* A service request can only be assigned to one staff member
* At least one service category must remain active
* Overdue is a derived/overriding state rather than a normal lifecycle status
* Status history is append only
* created_at and sent_at provide notification timing evidence

## Class Diagram 

![class diagram](civicconnect-initial-model-schema.jpg)

## Persistence model justification
CivicConnect uses a relational persistence/database model because its data contains structured entities with well defined relationships, such as users, service requests, categories and request history. The system also requires data integrity and consistency across related operations. The relational persistence model is appropriate for CivicConnect because its structured entities and relationships require strong data integrity and consistency. Primary keys, foreign keys and database constraints can maintain valid relationships between users, service requests, categories and request history. Transactional processing is also relevant to operations such as request assignment, where the request, status and history should remain consistent. The database will contain personal and security-sensitive information such as user contact details and password hashes, requiring appropriate authentication, authorization and data-protection controls. As the number of users, service requests and history records increases, database access patterns, indexing and query performance will need to be considered to maintain the required search performance.

## Important data

| Entity | Purpose | Example |
|---|---|---|
| User | Stores account and personal information | userId, name, email, password |
| Role | Defines what type of user the account represents | Requester, Staff, Admin, Management |
| ServiceRequest | Defines what type of user the account represents | Requester, Staff, Admin, Management |
| RequestHistory | Records the lifecycle of a request | Status changes, timestamps, responsible staff |
| Category | Stores available request categories | IT Support. Maintenance, security |
| Notification | Supports email notifications | Status-change notification |
| Comment | Stores rejection/resolution comments | Staff’s resolution description |

## Access patterns

| Access pattern | Architectural implication |
|---|---|
| Staff search/filter requests | Efficient queries and appropriate indexes |
| Requester views request history | Efficient relationship between User, ServiceRequest and History |
| Admin views unassigned requests | Query/filter based on assignment state |
| Management views dashboard | Aggregation queries over service requests |
| Request lifecycle changes | Transactional updates across related records |

## Database Concerns

| Concern | CivicConnect implication |
|---|---|
| Database bottleneck | Heavy search/reporting queries could affect response time |
| Single point of failure | If the database becomes unavailable, core request management becomes unavailable |
| Scalability | Increasing requests/users may increase query and storage requirements |
| Availability | Database availability directly affects system availability |
| Backup/recovery | Request and history data must be recoverable after data loss |


