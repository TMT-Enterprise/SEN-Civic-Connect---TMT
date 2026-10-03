# Technology Stack Selection 
## Frontend selection and justification
*React vs Angular Comparison*
| Criteria | React | Angular |
|---|---|---|
| **Requirements / ASRs** | Route guards + context/hooks or libraries handle RBAC (NF-001, NF-002). Forms via React Hook Form + Zod or Formik. Dashboards/tables via TanStack Table / Recharts / AG Grid. Status workflows and sequential transitions implemented with state machines or custom logic. Search/filter straightforward with client-side libraries or server queries. Strong for dynamic UIs and real-time dashboard refreshes (FR-014) (Djirdeh, 2025).| Built-in Router Guards + interceptors excel at RBAC and role-based UI (NF-002). Reactive Forms are powerful for complex validated forms with controlled lists (FR-003, FR-017 categories). Dependency injection and services make status transitions and sequential rules cleaner to enforce. (Angular Team, 2026)|
| **Team capability** | Moderate learning curve but routing, forms, state management and other application concerns may require additional libraries or agreed team conventions. Only one team member is proficient. | Steep learning curve but provides a more opinionated framework with built-in solutions for routing, forms, dependency injection and other application concerns. Two members have experience using Angular. |
| **Cost / licensing** | Free | Free |
| **Maintainability** | Flexible structure allows the team to choose suitable libraries, but this flexibility can lead to inconsistent decisions amonsgt the developers if conventions are not established | Opinionated structure, dependency injection, services and built-in tooling provide consistent patterns for organising  larger applications, supporting maintainability. |
| **Ecosystem/dependency risk** | Large ecosystem, but common application features may require external third-party libraries which will increase the number of dependencies required to be maintained.  | Large ecosystem with many features provided directly by the framework, potentially reducing the number of core third-party dependencies. |
| **Deployment compatibility** | Produces static frontend build files that can be deployed separately and communicate with the ASP.NET Core RestAPI | Produces static frontend build files that can be deployed separately and communicate with the ASP.NET Core RestAPI. Both are compatible with the CivicConnect modular-monolith backend.|
| **Team capability** | Moderate learning curve but routing, forms, state management and other application concerns may require additional libraries or agreed team conventions. Only one team member is proficient| Steep learning curve but provides a more opinionated framework with built-in solutions for routing, forms, dependency injection and other application concerns. Two members have experience using Angular. |
| **Schedule** | Faster for experienced member but risk of slower overall progress due to mentoring and architectural overhead | Moderate start due to learning investment but later milestones benefit from greater predictability and less rework. |
| **Cost / licensing** | Free; zero licensing cost | Free; zero licensing cost |
| **Security** | Good defaults (JSX escaping) and Frontend guards remain UX-only (Djirdeh, 2025). However, token handling, route protection, and dependency hygiene must be deliberately engineered. | Router Guards, HTTP interceptors, and TypeScript discipline reduce common classes of mistake. Smaller, more predictable dependency surface simplifies security reviews and residual-risk recording (Angular Team, 2026) |
| **How it would fit CivicConnect** | Supports dynamic forms, dashboards, filtering and role-based interactions. Processes such as routing forms and state managemnet will require additional libraries | Provides routing, reactive forms and dependency injection within the framework. |
| **Trade-offs** | - More freedom and flexibility leading to more architectural decisions and third-party dependencies for application features | - Larger framework and more concepts to learn initially (steeper learning curve)|


## Final Selection
Angular is the stronger engineering choice for CivicConnect's frontend, particularly for validated request forms, role-based interfaces and service-based communication. Its reactive forms support scalable and testable form handling, while its framework structure reduces the need to select and integrate multiple libraries. The main trade-trade off is Angular's larger learning curve compared with React; however, existing team experience reduces this risk.

## Backend selection and justification
| Criteria | C# + ASP.NET Core | Node.js + TypeScript |
|---|---|---|
| **Requirements / ASRs** | Supports the backend implementation of NF-001–NF-003 through authentication, authorization and secure password handling. ASP.NET Core can also implement the business rules required by FR-008–FR-013, including assignment, sequential status changes and audit-history recording. Its transaction and database-access ecosystem supports operations where related request changes must remain consistent. | Node.js + TypeScript can also implement NF-001–NF-003 and the business rules in FR-008–FR-013. However, the specific authentication, authorization, validation, dependency-injection and database approaches depend more heavily on the frameworks and packages selected by the team. |
| **Modular Monolith fit** | Strong support for structured applications, dependency injection and service-based organisation. ASP.NET Core also includes a built-in dependency injection | Also suitable for modular applications, but the team must select and enforce its own framework/library structure. |
| **Team capability** | The team is working with C# and ASP.NET Core, reducing the need to introduce another backend ecosystem due to the available experience with the technology. | TypeScript/Node.js would require the team to develop its backend using a different runtime and supporting ecosystem. |
| **Cost / licensing** | Free | Free |
| **Maintainability** | ASP.NET Core has built-in dependency injection. Microsoft states that DI can make applications easier to test and maintain, and recommends small, well-factored, testable services. This fits CivicConnect's modular monolith structure (ardalis, 2026). | TypeScript provides static checking, but the team has greater freedom over application structure and supporting libraries. This flexibility can be useful, but the team must establish conventions for modules, services and dependencies to maintain consistency. |
| **Ecosystem/dependency risk** | The .NET ecosystem provides integrated tooling and libraries for APIs, authentication, dependency injection and database access. This can reduce the need to combine many unrelated frameworks. | Node.js has a very large npm ecosystem. This provides extensive choice but also increases the number of external packages that may need to be evaluated, updated and maintained. |
| **Deployment compatibility** | ASP.NET Core can run as a single web application, making it compatible with CivicConnect's modular-monolith deployment model. | Node.js applications can also be deployed as a single service and are compatible with the modular-monolith approach. |
| **Team capability** | The team is working with C# and ASP.NET Core, reducing the need to introduce another backend ecosystem. | TypeScript/Node.js would require the team to develop its backend using a different runtime and supporting ecosystem. |
| **Schedule** | Faster for experienced member but risk of slower overall progress due to mentoring and architectural overhead | Moderate start due to learning investment but later milestones benefit from greater predictability and less rework. |
| **Cost / licensing** | Free; zero licensing cost | Free; zero licensing cost |
| **Security** | ASP.NET Core provides framework support for authentication, authorization and middleware-based security controls. These mechanisms still require correct implementation and configuration by the team. | Node.js supports authentication and security through frameworks and packages, but security controls depend more heavily on the selected packages and their configuration. |
| **How it would fit CivicConnect** | Supports a modular monolith containing modules such as Request, User/Auth and Notification. ASP.NET Core can provide the REST API, business services, validation and database interaction within one application. | Can support the same modules and REST API, but the team would need to establish the framework and library structure required to implement them. |
| **Trade-offs** | Despite having a more structured framework and conventions (improving consistency and maintainability), C#/.NET introduces a larger framework ecosystem that the team must understand and maintain. | Includes a flexible ecosystem and large package selection; however, more architectural and dependency choices can increase maintenance and dependency-management responsibilities. |

## Final selection
C# with ASP.NET Core was selected as the backend/runtime because its framework capabilities align with CivicConnect's modular-monolith architecture and its requirements for structured business logic, API development, maintainability and transactional data operations. ASP.NET Core provides built-in dependency injection, which Microsoft identifies as supporting testability and maintainability, while its asynchronous programming model is designed to support concurrent request processing. The framework also provides documented mechanisms for performance concerns such as caching, response compression, rate limiting and load/stress testing.

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
| **How it would fit CivicConnect** | Fits the relational model containing Users, Roles, ServiceRequests, Categories, RequestHistory and Notifications. | Provides the same PostgreSQL relational model while reducing database infrastructure management. |
| **Trade-offs** | Greater infrastructure and configuration control but more responsibility for deployment, maintenance, backups and security configuration. | No significant trade-off specified yet |

## Final selection
Supabase will be selected as the database platform as it provides CivicConnect with a managed PostgreSQL environment while also offering supporting services that can reduce infrastructure and devlopment overhead.

## API selection and justification
*Swagger and RestAPI Comparison*
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
| **How it would fit CivicConnect** | Provides the boundary between Angular and the ASP.NET Core application for operations such as submitting, assigning and updating requests. | Can document those endpoints and provide an interactive interface for developers to inspect/test the API. |
| **Trade-offs** | Simple and widely supported comunication approach but requires the team to maintain consistent endpoint design and validation | Improves API visibility and developer understanding but adds documentation/tooling overhead and must remain synchronised with the actual API. |

## Final selection
REST is selected as the API architectural approach, with Swagger/OpenAPI possibly beig used to dcument and test the RestAPI. REST provides a straigthforward resources-based communication mechanism between the frontend and backend, while Swagger/OpenAPI improves API visbility by documenting endpoints, request/response structures and security requirements. Although the team has limited prior experience with Swagger, its integration with ASP.NET Core and potential benefits for API documentation and testing justify the relatively small learning requirement. The team will monitor the learning curve asto ensuree it does not negatively affect the development schedule.

## Testing technologies

| Testing area | Technology | Purpose |
|---|---|---|
| Frontend component testing | Angular testing tools | Verify Angular components and frontend behaviour |
| Backend unit testing | xUnit | Test individual C# components and business logic |
| API testing | OpenAPI | Verify REST endpoints and request/response behaviour |
| Integration testing | ASP.NET Core integration testing | Verify interactions between backend components |
| End-to-end testing | Cypress/Playwright | Verify important user workflows across frontend and backend |

For end-to-end testing, Cypress was selected instead of Playwright because it provides the required capabilities for testing CivicConnect's web workflows while keeping the learning and setup overhead manageable for the development team. Its interactive test runner also supports easier test creation and debugging, which is beneficial given the project's development schedule

## Build dependency technologies
| Area | Selected technology | Purpose |
|---|---|---|
| Frontend framework | Angular | Develop CivicConnect's frontend |
| Frontend language | TypeScript | Develop Angular components and application logic |
| Frontend build tooling | Angular CLI | Build, serve and manage the Angular application |
| Frontend dependencies | npm | Manage Angular and other JavaScript/TypeScript packages |
| Backend build | .NET SDK | Build, run and publish the C# application |
| Backend dependencies | NuGet | Manage .NET packages |
| Source control | Git/GitHub | Version control and collaboration |
| Database platform | Supabase | Provide managed PostgreSQL hosting and database |

## Version and compatibility baseline
| Technology | Version |
|---|---|
| Angular | TBD |
| TypeScript | TBD |
| Node.js | TBD |
| npm | TBD |
| C# | TBD |
| .NET / ASP.NET Core | TBD |
| PostgreSQL | Supabase-managed |
| xUnit | TBD |
| Cypress | TBD |

Compatibility assumptions: Angular, TypeScript, Node.js and npm versions must be compatible with one another. The selected .NET version must be compatible with ASP.NET Core and the PostgreSQL provider used by the backend. Cypress must support the selected browser environment and Angular application. Version changes will be recorded to prevent unexpected compatibility issues.


