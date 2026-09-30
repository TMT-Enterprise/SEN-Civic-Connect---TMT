# Technology Stack Selection 
## Frontend selection and justification
*React vs Angular Comparison*
| Criteria | React | Angular |
|---|---|---|
| **Requirements / ASRs** | Route guards + context/hooks or libraries handle RBAC (NF-001, NF-002). Forms via React Hook Form + Zod or Formik. Dashboards/tables via TanStack Table / Recharts / AG Grid. Status workflows and sequential transitions implemented with state machines or custom logic. Search/filter straightforward with client-side libraries or server queries. Strong for dynamic UIs and real-time dashboard refreshes (FR-014) (Djirdeh, 2025).| Built-in Router Guards + interceptors excel at RBAC and role-based UI (NF-002). Reactive Forms are powerful for complex validated forms with controlled lists (FR-003, FR-017 categories). Dependency injection and services make status transitions and sequential rules cleaner to enforce. (Angular Team, 2026)|
| **Team capability** | | |
| **Schedule** |  |  |
| **Cost / licensing** | Free | Free |
| **Security** |   |   |
| **Maintainability** |  |  |
| **Ecosystem / dependency risk** |  |  |
| **Deployment compatibility** |  | |

## Backend selection and justification
C# is selected as the backend/runtime for CivicConnect.

The selection aligns with the team's existing capabilities and supports the project's requirements for API development, business logic, validation, security and transactional database operations. It also fits the modular monolith architecture, where the backend can contain clearly separated business modules within one deployable application.

## Database selection and justification
*PostgreSQL vs Supabase Comparison*
| Criteria | Postgre | Supabase |
|---|---|---|
| **Requirements / ASRs** | | |
| **Team capability** |  |   |
| **Schedule** |    |  |
| **Cost / licensing** |    |   |
| **Security** |   |   |
| **Maintainability** |  |  |
| **Ecosystem / dependency risk** |  |  |
| **Deployment compatibility** |  | |

## API selection and justification
*Swagger vs RestAPI Comparison*
| Criteria | Swagger | RestAPI |
|---|---|---|
| **Requirements / ASRs** | | |
| **Team capability** |  |   |
| **Schedule** |    |  |
| **Cost / licensing** |    |   |
| **Security** |   |   |
| **Maintainability** |  |  |
| **Ecosystem / dependency risk** |  |  |
| **Deployment compatibility** |  | |

## Final selection
*dependancies etc...
