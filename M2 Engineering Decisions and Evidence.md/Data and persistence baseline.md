#  Data persistence

## Important Data
| Entity | Purpose | Example |
|---|---|---|---|
| User | Stores account and personal information | userId, name, email, password | 
| Role | Defines what type of user the account representsn | Requester, Staff, Admin, Management | 
| Service Request | Stores submitted service requests | Requester creates it; staff/admin manage it |
| Request History | Records the lifecycle of a request | Status changes, timestamps, responsible staff |
| Category | Stores available request categories | IT Support. Maintenance, security |
| Notification | Supports email notifications | Status-change notificationt |
| Comment | Stores rejection/resolution comments | Staff’s resolution description |

## Justification for relational database model
CivicConnect uses a relational persistence/database model because its data contains structured entities with well-defined relationships, such as users, service requests, categories and request history. The system also requires data integrity and consistency across related operations. 

## Access patterns

| Concern | CivicConnct Implimentation |
|---|---|---|---|
| Database bottleneck | Heavy search/reporting queries could affect response time |  
| Single point of failure | If the database becomes unavailable, core request management becomes unavailable | 
| Scalability | Increasing requests/users may increase query and storage requirements |
| Availability | Database availability directly affects system availability |
| Backup/recovery | Request and history data must be recoverable after data loss |

## HOW A2 affects Data/persistence design
(Still working on it)


