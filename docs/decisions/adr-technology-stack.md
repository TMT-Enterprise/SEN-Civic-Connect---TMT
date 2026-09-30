# Technology Stack Selection 
## Frontend selection and justification
*React vs Angular Comparison*
| Criteria | React | Angular |
|---|---|---|
| **Requirements / ASRs** | Route guards + context/hooks or libraries handle RBAC (NF-001, NF-002). Forms via React Hook Form + Zod or Formik. Dashboards/tables via TanStack Table / Recharts / AG Grid. Status workflows and sequential transitions implemented with state machines or custom logic. Search/filter straightforward with client-side libraries or server queries. Strong for dynamic UIs and real-time dashboard refreshes (FR-014) (Djirdeh, 2025).| Built-in Router Guards + interceptors excel at RBAC and role-based UI (NF-002). Reactive Forms are powerful for complex validated forms with controlled lists (FR-003, FR-017 categories). Dependency injection and services make status transitions and sequential rules cleaner to enforce. (Angular Team, 2026)|
| **Team capability** | Moderate learning curve but routing, forms, state management and other application concerns may require additional libraries or agreed team conventions. Only one team member is proficient| Steep learning curve but provides a more opinionated framework with built-in solutions for routing, forms, dependency injection and other application concerns. Two members have experience using Angular. |
| **Schedule** | Faster for experienced member but risk of slower overall progress due to mentoring and architectural overhead | Moderate start due to learning investment but later milestones benefit from greater predictability and less rework. |
| **Cost / licensing** | Free; zero licensing cost | Free; zero licensing cost |
| **Security** | Good defaults (JSX escaping) and Frontend guards remain UX-only (Djirdeh, 2025). However, token handling, route protection, and dependency hygiene must be deliberately engineered. | Router Guards, HTTP interceptors, and TypeScript discipline reduce common classes of mistake. Smaller, more predictable dependency surface simplifies security reviews and residual-risk recording (Angular Team, 2026) |
| **Maintainability** |  |  |
| **Ecosystem / dependency risk** |  |  |
| **Deployment compatibility** |  | |

## Backend selection and justification
C# is selected as the backend/runtime for CivicConnect and ASP.NET Core as the framework.

The selection aligns with the team's existing capabilities and supports the project's requirements for API development, business logic, validation, security and transactional database operations. It also fits the modular monolith architecture, where the backend can contain clearly separated business modules within one deployable application.

## Database selection and justification
*PostgreSQL vs Supabase Comparison*
| Category | PostgreSQL (Self-Hosted / Standard) | Supabase (Managed PostgreSQL Platform) |
|---|---|---|
| **Requirements / ASRs** | Provides relational tables, constraints and transactions suitable for CivicConnect's users, requests, assignments and history. | Uses PostgreSQL, so it provides the same relational and transactional capabilities while adding managed services. |
| **Team capability** | Requires the team to manage database configuration, connections and potentially deployment. | Provides a web dashboard and managed environment that can make database development and administration easier. |
| **Schedule** | More setup and configuration may be required, especially for deployment. | Can reduce setup time because the database and supporting services are already provided through the platform. |
| **Cost/licensing** | PostgreSQL is open-source and can be used without a database software licence fee. Hosting may still have infrastructure costs. | Provides a free tier, with additional usage/features but may include a paid plan. |
| **Security** | Provides established database security features, but the team is responsible for configuring and securing the deployment. | Provides managed infrastructure and security features, but the team must correctly configure authentication, access policies, API keys and permissions. |
| **Maintainability** | Gives the team direct control over database configuration and deployment. | Reduces infrastructure management but introduces dependence on the Supabase platform and its services. |
| **Ecosystem / dependency risk** | Mature and widely supported relational database with many tools and database providers. | Built on PostgreSQL but introduces an additional platform dependency and Supabase-specific services/APIs. |
| **Deployment compatibility** | Can be deployed on servers, containers and many cloud providers. | Database hosting is managed through Supabase, reducing database deployment work but creating dependency on the platform. |
| **Additional services** | Primarily provides the database itself. Additional services must be implemented separately. | Provides additional services such as authentication, file storage, APIs and database management tools. |
| **Control** | High control over configuration, infrastructure and deployment. | Less infrastructure control, but significantly less infrastructure management. |

##Final selection
Supabase will be selected as the database platform as it provides CivicConnect with a managed PostgreSQL environment while also offering supporting services that can reduce infrastructure and devlopment overhead.

## API selection and justification
*Swagger vs RestAPI Comparison*
| Criteria | REST API | Swagger / OpenAPI |
|---|---|---|
| **Requirements / ASRs** | Provides resource-based communication between CivicConnect components through HTTP methods and responses. | Documents the API's endpoints, request parameters, responses and schemas, improving API visibility and understanding. |
| **Team capability** | Familiar approach using HTTP methods such as GET, POST, PUT and DELETE. | Provides an interactive interface that makes it easier for the team to understand and test API endpoints. |
| **Schedule** | Straightforward to implement using ASP.NET Core. | Reduces time spent manually documenting and testing endpoints, especially during development. |
| **Cost / licensing** | REST itself is an architectural style and does not require a licence. | Swagger/OpenAPI tooling is available through open-source tools and libraries. |
| **Security** | Can implement authentication, authorization, HTTPS, input validation and appropriate HTTP status codes. | Can document authentication requirements and security schemes but does not itself secure the API. |
| **Maintainability** | Resource-oriented endpoints provide a consistent structure for API operations. | Keeps API documentation aligned with the API specification and helps developers understand available endpoints. |
| **Ecosystem / dependency risk** | Very mature and widely supported by web frameworks and tools. | Widely adopted API documentation standard with multiple supporting tools. |
| **Deployment compatibility** | Can operate within CivicConnect's .NET modular monolith. | Can be integrated with the .NET backend to expose interactive API documentation during development/testing. |

## Final selection
REST is selected as the API architectural approach, with Swagger/OpenAPI possibly beig used to dcument and test the RestAPI. REST provides a straigthforward resources-based communication mechanism between the frontend and backend, while Swagger/OpenAPI improves API visbility by documenting endpoints, request/response structures and security requirements. Although the team has limited prior experience with Swagger, its integration with ASP.NET Core and potential benefits for API documentation and testing justify the relatively small learning requirement. The team will monitor the learning curve asto ensuree it does not negatively affect the development schedule.

## Testing technologies

| Testing area | Technology | Purpose |
|---|---|---|
| Frontend component testing | Angular testing tools | Verify Angular components and frontend behaviour |
| Backend unit testing | xUnit | Test individual C# components and business logic |
| API testing | OpenAPI | Verify REST endpoints and request/response behaviour |
| Integration testing | ASP.NET Core integration testing | Verify interactions between backend components |
| End-to-end testing | Cypress | Verify important user workflows across frontend and backend |
| Static analysis | ESLint / .NET analyzers (if selected) | Identify code-quality issues |

For end-to-end testing, we chose cypress instead of ...

## Build dependency


## Final selection
*dependancies etc...
