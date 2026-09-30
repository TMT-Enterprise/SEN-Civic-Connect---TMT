## M2 Engineering Decisions and Evidence

### Architecture decision and diagrams
After researching and considering the possible architectural approaches for CivicConnect, the team selected a modular monolith with a layered internal structure. A modular monolith is a single deployable application that is internally divided into cohesive modules with clearly defined responsibilities and controlled interactions. Within CivicConnect, the application is organised into logical layers, including the Web/Client, Application, Data Access and Database responsibilities.

**What is modular monolith?**
A single deployable application but divided internally into cohesive business modules controlled interfaces.

### Justification against ASRs, constraints, team capability, deployment implications and complexity

#### ASRs
CivicConnect has architectural concerns relating to data integrity, concurrency, security, maintainability and request processing. The modular monolith supports these concerns by providing clear module boundaries while allowing closely related operations to remain within the same application.

For example, the Request module can coordinate the assignment of a service request, status update and creation of a request-history record. Because these operations remain within the same application and persistence boundary, they can be handled as a coordinated transaction where appropriate.

The layered structure also separates responsibilities, such as application logic from data-access responsibilities, supporting maintainability and controlled access to persistence.

#### Constraints
CivicConnect must be delivered within a four-milestone project schedule, with limited resources and a three-person team. The modular monolith fits these constraints by allowing the team to develop and maintain one deployable application while still separating functionality into manageable modules.

Compared with a distributed architecture such as microservices, this avoids introducing additional services, network communication, deployment pipelines and infrastructure that are not currently required by the project scope.

#### Team capability
The selected architecture must be understandable, implementable, testable and maintainable by the three-person team.

A modular monolith allows the team to divide development responsibilities around logical modules while maintaining a shared application structure. This allows team members to work on areas such as requests, users/authentication and notifications without requiring each area to become a separately deployed service.

The layered structure also provides clear responsibilities between the Web/Client, Application and Data Access areas, making the codebase easier to understand and maintain.

#### Deployment implications
The modular monolith is deployed as one application, even though its internal functionality is separated into modules.

This simplifies the deployment process because the team does not need to independently deploy and coordinate multiple services. Configuration, testing and deployment can therefore be managed around a single application.

The database remains a separate persistence component, while the logical modules and layers remain organised within the application.

#### Complexity
A modular monolith introduces some structural complexity because the team must define and enforce module boundaries, control dependencies and establish appropriate communication between modules.

However, this complexity is intentional and is used to prevent the application from becoming an uncontrolled, tightly coupled monolith. The architecture therefore introduces internal structure without the additional distributed-system complexity associated with independently deployed microservices.

#### Difference between technologies and logical layers/modules from physical deployment tiers.
Technologies describe the specific tools used to implement the system, such as the frontend framework, backend runtime and database.

Logical layers and modules describe how responsibilities are organised within the software. For CivicConnect, examples include the Web/Client, Application, Data Access, Request, User/Auth and Notification responsibilities.

Physical deployment tiers describe where software components actually execute.

These concepts should not be treated as equivalent. For example, several logical CivicConnect modules can execute within the same deployed backend application, while the database can run as a separate deployment component. Therefore, having multiple logical modules does not mean that CivicConnect has multiple physical services.

### Risks
| Factor | Guidance / Mitigation |
|---|---|
| Risk Mitigation Module coupling | Define clear module boundaries and controlled interfaces. |
| Layer coupling | Enforce clear dependencies between layers. |
| Shared database coupling | Define data ownership and controlled data access. |
| Transaction complexity | Clearly define transaction responsibilities for multi-step operations. |
| Added complexity | Keep modules and layers proportional to CivicConnect's scope. |



