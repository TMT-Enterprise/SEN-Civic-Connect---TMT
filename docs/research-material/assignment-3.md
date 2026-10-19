# SEN381 Assignment 3

**Team Members:**

- Tinyiko Siwele [601726]
- Mutshidzi Nduvheni [601781]
- Thabang Molise [601586]

---

## Question 1 - Quality Strategy & Risk-Based Verification

Quality strategy refers to a coordinated, long-term organisational framework that defines quality objectives and priorities. It aligns leadership, people, resources, and processes with stakeholder needs, and guides implementation and continuous improvement of quality performance (Ozen et al., 2022).

### Relationship between Quality Assurance (QA), Quality Control (QC), Verification and Validation (V&V)

Quality Assurance (QA), Quality Control (QC), Verification and Validation (V&V) contribute to software quality through different activities. QA focuses on establishing and improving development processes that support the achievement of quality (Thörn et al., 2022). QC focuses on evaluating the resulting software and identifying defects or non-conformance (Cervantes-Ojeda and Gómez-Fuentes, 2022). Verification provides evidence that the system and its work products conform to specified requirements, whereas validation provides evidence that the resulting system satisfies its intended use and stakeholder needs (IEEE Standards Association, n.d.). Together, these activities contribute to an overall quality strategy by addressing both the development process and resulting software. QA helps reduce the likelihood of defects being introduced, while QC and verification and validation (V&V) provide evidence for identifying problems, confirming conformance to requirements, and determining whether the system meets its intended purpose. Therefore, software quality should not be determined solely by automated test passes, but through evidence that reveals the quality of the development process and resulting system

### Risk-based testing/verification

Risk-based testing uses identified software risks to guide decisions throughout the testing process, allowing verification effort to be prioritised according to the significance of potential failures (Foidl and Felderer, 2016). Risk can be assessed using factors such as likelihood and impact. Risk can be assessed using factors such as likelihood and impact. Likelihood considers how probable a failure is, while impact considers the severity of its consequences (NIST, 2023). Technical factors such as complexity, changes, and defect history can influence the likelihood of defects, while factors such as business importance, usage, and potential cost can influence their impact (Foidl and Felderer, 2016). And so, areas with greater risk and business/technical criticality will need deeper verification and testing. Risk-based testing therefore supports selective allocation of verification effort rather than applying the same level of testing to every part of a system.

### Forms of Verification evidence

| Verification evidence | What it verifies | Strong at exposing | What it cannot prove on its own |
| --- | --- | --- | --- |
| Unit testing | Whether Individual functions, methods, or components work in isolation (Trautsch et al., 2020). | Logic errors, incorrect calculations, boundary conditions, and business-rule defects (Felderer et al., 2014). | Cannot prove that the complete system works (Trautsch et al., 2020). |
| Integration/API testing | Interaction between application components, APIs, and external systems (Trautsch et al., 2020). | Incorrect API responses, data-transfer errors, contract and integration issues (Felderer et al., 2014). | Cannot prove that the entire system performs adequately under realistic user load (Felderer et al., 2014). |
| Performance/Load testing | System behaviour under specified levels of workload (Jin et al., 2012). | Slow response times, throughput limitations, resource bottlenecks, and failures under increased demand (for, 2023). | Cannot prove that the system's business logic or individual components are functionally correct (Jin et al. 2012). |

These forms of verification provide different evidence because they target different failure types. Unit testing mainly focuses on component correctness, integration/API testing focuses on the interactions between the components of the system, and performance testing focuses on how the system behaves under a certain amount of workload. Therefore, passing one form of verification does not compensate/provide sufficient evidence for the risks addressed by the others.

### Automation and Quality Gates

Test automation improves the repeatability of verification by allowing predefined checks to be executed consistently whenever software changes are introduced. These types of tests can be integrated into CI/CD pipelines, where selected tests can act as quality gates that prevent a build from progressing when defined verification conditions are not satisfied (Patel and Tyagi, 2022). Research has also found a positive association between test automation maturity and product quality in continuous-integration environments (Khatami et al., 2022). However, a successful pipeline should not be treated as proof of complete software quality. Automated tests only provide evidence for the conditions and scenarios they were designed to evaluate, and important risks such as usability, unforeseen behaviour, and other untested quality concerns may remain (Wiklund et al., 2017). Therefore, automation and quality gates should be treated as repeatable sources of verification evidence within a broader risk-based quality strategy rather than as proof that the system is completely defect-free or release-ready.

#### Risk to Verification Evidence Map

*Each risk's likelihood and impact set its criticality. What criticality means is how much verification evidence is required and the evidence outcome is what earns/blocks the quality decision below.*

| | Data integrity / consistency | Performance / load | Security vulnerability | Usability / UX |
| --- | --- | --- | --- | --- |
| **Risk** | Incorrect or inconsistent data stored or retrieved | Slow response times or timeouts under peak load | Unauthorised access or sensitive data exposure | Confusing navigation or unclear error messages |
| **Severity** | Likelihood Medium: Impact High; Criticality High | Likelihood Med-High: Impact High; Criticality High | Likelihood Low-Med: Impact Very High; Criticality High | Likelihood Medium: Impact Med–High; Criticality Medium |
| **Evidence** *(sized to criticality)* | Unit & integration/API testing of data logic<br>Static analysis for code and data quality | Performance/load testing & runtime monitoring<br>Regression testing to catch slowdowns | Static analysis (SAST) & security testing (DAST)<br>Manual penetration testing of high-risk areas | Exploratory testing of real user scenarios<br>Usability testing and UI/UX accessibility review |
| **Decision** | Pass gates test failure → pipeline blocks deploy | Pass gates response/error thresholds → deploy blocked if missed | Pass requires zero critical/high findings → gate blocks deploy | Automated checks alone insufficient → human usability review required |

---

## Question 2 - Security Engineering & Threat-Based Controls

### Threat Modelling

Threat modelling is an analysis process for addressing what could go wrong with the application that is being built (Shostack, 2014). A good model helps address groups of attacks, find security bugs early, and deliver a more secure product (Shostack, 2014). STRIDE is a popular threat modelling framework created by Microsoft that classifies potential security threats into six categories: Spoofing, Tampering, Repudiation, Information Disclosure, Denial of Service, and Elevation of Privilege (Romeo, 2025). Each category corresponds to violations found in foundational security principles, which makes it accessible to technical and non-technical team members (Romeo, 2025).

STRIDE also encourages proactive thinking and easy integration into development methodologies (Romeo, 2025). A typical STRIDE workflow includes decomposing the system, identifying threats, proposing countermeasures, prioritizing risks, and reviewing and iterating (Romeo, 2025). Threat modelling is not static and should be a continuous part of software design (Romeo, 2025). Therefore, utilising a good threat model helps clarify security requirements and leads to better engineering decisions (Shostack, 2014).

### Threat-to-Control Traceability Table

#### Authentication

| Threat/ misuse | Security requirement or control objective | Possible engineering control | Verification evidence | Residual risk / limitation |
| --- | --- | --- | --- | --- |
| An unauthorised person gains access to the system by stealing a user's credentials (such as a password) | Validating the identity of the user and making sure only authenticated users can access protected actions (Khanna, 2025). | Implement password-based authentication, session management, and secure password storage techniques (Omotunde and Ahmed, 2023). | Test results show that valid users may log in, invalid credentials are rejected, and passwords are not stored in plaintext | Weak passwords or phishing attempts can lead to security breaches (Omotunde and Ahmed, 2023). Multi-factor authentication (MFA) is recommended to add another degree of protection (Khanna, 2025; Omotunde and Ahmed, 2023). |

#### Secrets/ configuration

| Threat/ misuse | Security requirement or control objective | Possible engineering control | Verification evidence | Residual risk / limitation |
| --- | --- | --- | --- | --- |
| Service account credentials, API keys, or other secrets may be exposed through source code or publicised configuration files. | Sensitive digital credentials contained within an application shall only be accessible to authorised users and not stored in source code (IBM, 2024). | Store secrets in a secure, central location called a "secrets vault" (IBM, 2024). Secrets may be stored in environment variables. | Inspect the repository to check that no real secrets have been committed, and check the .gitignore file for evidence | Infrequent rotation of secrets may lead to hackers gaining access through a brute-force attack (IBM, 2024). Therefore, secrets and passwords should not remain unchanged for a long period of time (IBM, 2024). |

#### Input/ data handling

| Threat/ misuse | Security requirement or control objective | Possible engineering control | Verification evidence | Residual risk / limitation |
| --- | --- | --- | --- | --- |
| Malicious input causes invalid data, unauthorised behaviours, or injection attacks. | Data entering a system needs to be sanitised, filtered, and validated against predefined rules and constraints (McCarvill, 2023). | Apply client-side (such as format checks) and server-side validation with appropriate error handling (McCarvill, 2023). Create an allow list and deny list (whitelisting and blacklisting) to manage input access (McCarvill, 2023). | Unit and integration tests shall show invalid inputs (such as injections) are rejected. | Sanitation of input may inadvertently distort user input and lead to incorrect data interpretation (Misiura, 2023). Understanding the context of data inputs determines appropriate validation rules (Misiura, 2023). |

#### Sensitive information/ privacy

| Threat/ misuse | Security requirement or control objective | Possible engineering control | Verification evidence | Residual risk / limitation |
| --- | --- | --- | --- | --- |
| Sensitive information is collected unnecessarily, exposed to unauthorised users | According to the POPIA Act, sensitive data shall only be collected, processed, stored and disclosed when authorised and necessary for the stated purpose (Werksmans, 2024). | Apply access controls by applying the principle of least privilege, encrypt stored and transit data, de-identify data (replace direct identifiers with a secure linking key), and ensure secure disposal of data from logs (University at Buffalo, 2025). | Authorisation checks showing users cannot retrieve another user's sensitive data; evidence of encryption in storage. Reviews shall also show that sensitive data is not exposed from the logs. | If the key used in de-identification is not stored securely or does not have higher security, it opens the possibility of a security breach. Anonymisation of data or stripping all identifying information may prevent this risk (University at Buffalo, 2025). |

### Secure Design, Construction and Checking

Secure design, coding, and security checks complement one another because they represent different stages of addressing security concerns. Secure design happens during the planning phase and focuses on secure system logic before implementation. Secure coding happens during the implementation phase, and security requirements are enforced in the executable code. Lastly, security and dependency checks happen during the testing phase and detect known vulnerabilities in the codebase. Therefore, they support each other rather than replacing one another.

### Automated/AI Security Limitations

Artificial intelligence (AI) systems can fabricate incorrect information called hallucinations. Security analysts may make incorrect determinations if they rely on hallucinated threat context or fabricated data (Expel team, 2026). Therefore, human judgement is essential, and AI-produced output needs to be thoroughly reviewed. Model drift is another limitation where AI models trained on historical data gradually become less accurate (Expel team, 2026). This is due to evolving attacker techniques, shifting pattern behaviours, and changing environments (Expel team, 2026). If a scanner reports no high-severity findings, business logic and contextual vulnerabilities that require human reasoning still need to be investigated.

---

## Question 3 - Production Readiness, Deployment & Operational Evidence

### Environment Strategy and Parity

A modern web-based information system is typically developed and validated across at least three distinct environments. The development environment supports rapid iteration and experimentation. The test/staging environment is intended to validate a release candidate under conditions that closely resemble production, ensuring that a pass provides genuine deployment confidence. Production is the live environment where real users and authentic data reside, and where system failure carries significant operational consequences.

The Twelve-Factor App methodology explicitly identifies dev/prod parity as a foundational principle: development, staging, and production should remain as similar as possible regarding tooling, configuration, and backing services (Wiggins, 2011). Maintaining parity minimizes divergence, which is critical for continuous and confident deployment. Uncontrolled environment differences, such as mismatched dependency versions, permissive local configurations, or truncated datasets, introduce deployment risks by allowing a system to function correctly in one environment while failing in another. Ultimately, a release candidate that succeeds in staging provides valid operational evidence only if the staging environment is genuinely representative of production.

### Secrets and Production Configuration

Configuration variables that change between environments, such as database connection strings, API keys, and feature flags, must be injected into the application runtime from the environment rather than being hardcoded or committed to source control. This principle forms the core of the "Config" factor in the Twelve-Factor App methodology (Wiggins, 2011). Separating configuration from code is essential for two primary reasons.

First, it safeguards system security. Credentials committed to a code repository or embedded into a build artifact remain exposed for as long as that artifact or version history exists, regardless of subsequent credential rotations. Security frameworks, such as the access control and configuration management guidance published by the National Institute of Standards and Technology (2020), mandate the isolation of sensitive configuration data from widely shared build artifacts. Second, it ensures build portability. A build artifact that retrieves its configuration dynamically from the environment can be promoted unchanged across development, staging, and production, satisfying a prerequisite for automated and repeatable deployment.

### Release and Deployment

The Twelve-Factor App treats build, release, and run as three strictly isolated stages: a build converts source code into an executable artifact; a release combines that artifact with environment-specific configuration; and run executes a specific release (Wiggins, 2011). This distinction clarifies what a "successful build" demonstrates. Compiling code, packaging an application, and passing automated tests merely prove that the application behaves correctly under the conditions established by the build environment. It does not prove that the same artifact will run correctly when combined with live configuration, infrastructure limits, and real-world network latency.

Automated, repeatable deployment pipelines mitigate the risks associated with manual procedures. Human-executed deployments are prone to subtle variations, whereas automated pipelines execute identical steps from identical inputs regardless of the operator (Humble and Farley, 2010). Furthermore, formal release controls, such as requiring explicit approval before promotion or tracking active build versions across environments, ensure that teams can definitively state which version is running in production at any given time.

### Failure and Recovery

Rollback refers to the ability to revert rapidly to a known-good release when a newly deployed version exhibits defects. Humble and Farley (2010) identify safe rollback mechanisms as an essential release-process requirement rather than an ad hoc response improvised during an incident. Rollbacks become complex when a release introduces changes to the underlying database schema. Reverting application code without an accompanying, compatible reversal of schema modifications leaves the database in a state that neither the legacy nor the current code can process correctly. Consequently, backward database compatibility, ensuring schema changes support the previous application version for at least one release cycle, must be managed as a distinct engineering requirement rather than a routine redeployment.

Backup and recovery mechanisms mitigate a different class of failure: data corruption or loss. A backup that has never undergone a restoration test does not provide verifiable evidence of recoverability; it merely confirms that a copy exists. When a production release fails, mature operational practice emphasizes rapid, automated rollback or mitigation paths followed by structured, blameless post-incident reviews, rather than undocumented firefighting (Beyer et al., 2016). The AWS Well-Architected Framework's Reliability pillar aligns with this approach, framing change management and failure management as intentionally designed capabilities rather than unplanned events (Amazon Web Services, 2024).

### Operations and Observability

Logs, metrics, and alerts provide distinct yet complementary operational evidence regarding a live system. Logs are discrete, timestamped records of specific events. They are valuable for reconstructing historical incident sequences, but they are ill-suited for answering real-time aggregate queries, such as identifying system-wide latency spikes (Wiggins, 2011). Metrics represent aggregated numeric measurements captured over time, making them ideal for trend analysis and threshold evaluation. Google's site reliability engineering framework highlights four primary telemetry signals, latency, traffic, errors, and saturation, as the essential metrics for service monitoring (Beyer et al., 2016). Monitoring systems continuously evaluate these metrics against defined thresholds and trigger alerts when human intervention is required.

Collecting telemetry without structured analysis provides minimal operational value. Unmonitored data does not automatically surface system degradation; a metric without an assigned threshold or a log file that goes unreviewed offers no utility until an incident is reported through external channels, such as user complaints. Additionally, poorly calibrated alerting mechanisms, such as excessively noisy alerts or notifications lacking clear action paths, degrade operational trust and increase the risk that critical alerts will be overlooked among false positives (Beyer et al., 2016). Observability is therefore a function of actionable system design rather than sheer data volume.

### Operational Quality

Reliability, performance efficiency, maintainability, and portability are formally categorized as core software quality attributes within ISO/IEC 25010 (International Organization for Standardization, 2011). This standard specifies that these attributes must be evaluated directly rather than assumed as passive byproducts of functional correctness. Production-readiness decisions depend on these evaluations: a system may be functionally complete yet remain unready for live deployment if its reliability, performance under load, and scalability have not been validated against realistic conditions.

Scalability, cost, and supportability exist in continuous operational tension. Over-provisioning infrastructure to accommodate hypothetical demand increases operational costs unnecessarily, whereas under-provisioning threatens availability during traffic surges. The AWS Well-Architected Framework highlights this trade-off as a deliberate balance between its Reliability and Cost Optimization pillars (Amazon Web Services, 2024). Furthermore, supportability, the ease with which a deployed system can be diagnosed and maintained by engineers other than the original authors, extends software design principles into the operational domain. A code base that is easy to modify in a local repository but opaque once deployed has not achieved true maintainability; it has merely deferred operational complexity.

### Critical Question

**Why can software that passes functional tests and runs successfully on a developer's machine still be unready for production?**

Functional tests and local development environments evaluate software under narrow, highly controlled conditions: single-user access, reduced datasets, permissive security settings, and isolated service dependencies. In contrast, production environments introduce operational variables that local testing cannot replicate: concurrent traffic spikes, network latency, distributed service failures, large-scale datasets, strict security boundaries, and the requirement for automated fault recovery without developer intervention.

Passing a functional test confirms only that a feature operates as specified within a controlled test harness. It does not establish that the system can be safely deployed, monitored, and recovered within an unmanaged infrastructure. The Twelve-Factor App's emphasis on dev/prod parity (Wiggins, 2011) and the operational lifecycle principles defined in site reliability engineering (Beyer et al., 2016) reinforce this distinction: functional correctness in a development environment is a necessary prerequisite, but it is never sufficient proof of production readiness.

### Production-Readiness Evidence Matrix

| Concern | Risk if ignored | Evidence before release | Evidence observed after release |
| --- | --- | --- | --- |
| Environment/configuration parity | System behaviour diverges between staging and production environments, invalidating staging results as reliable deployment evidence (Wiggins, 2011). | Staging environments are configured to mirror production dependencies, software versions, and system settings. | Continuous verification that live configurations match intended deployment releases without undocumented manual modifications. |
| Secrets and configuration management | Hardcoded credentials or exposed config values in source repositories compromise system security upon leak or build inspection (National Institute of Standards and Technology, 2020). | Verification that source code and build artifacts contain no embedded secrets, with environment variables managed externally. | Ongoing security audits confirming runtime secrets are not exposed in system logs, stack traces, or monitoring streams. |
| Automated and repeatable deployment | Manual deployment steps introduce human error, inconsistent environment states, and deployment uncertainty (Humble and Farley, 2010). | Automated deployment pipelines capable of executing reproducible builds from source without manual intervention. | Verifiable build audit logs identifying exact active code versions across all production instances. |
| Rollback and release safety | Unsuccessful deployments cannot be reverted rapidly, causing prolonged service outages while live patches are attempted (Humble and Farley, 2010). | Validated, executable rollback procedures that have been tested prior to production deployment. | Automated health monitoring configured to initiate or signal rapid rollback actions upon detecting release regressions. |
| Backup and data recovery | Data loss or corruption becomes unrecoverable, or code rollbacks fail due to unhandled database schema incompatibilities (Beyer et al., 2016). | Automated backup systems with verified, successful data restoration test runs. | Routine operational monitoring confirming continuous backup completion and ongoing restoration viability. |
| Observability (logs, metrics, alerts) | Outages remain undetected until reported by end users due to missing alert thresholds or unmonitored telemetry (Beyer et al., 2016). | Defined telemetry metrics and alerting thresholds associated with documented, actionable response procedures. | Operational metrics confirming alerts fire correctly during anomalies without high rates of false positives. |

---

## Question 4 - Research-to-M3 Engineering Map

| Research finding | Engineering concern it addresses | Candidate approach / evidence to consider | Trade-off / limitation found in research | Decision M3 must still make |
| --- | --- | --- | --- | --- |
| Risk-based verification prioritises verification effort according to likelihood, impact and business/technical criticality. | High-risk functionality may require deeper verification than lower risk functionality. | Risk-based testing and prioritising of verification effort. | Risk assessments depend on the quality of available risk information and may change as the system grows. | Which risks within CivicConnect require deeper verification? |
| Automation and quality gates make verification repeatable, but a "green" pipeline does not prove complete quality | Inconsistent verification and undetected quality risks | Automated tests and CI/CD quality gates | Automated checks only provide evidence for the conditions they test; important | Which verification checks should be used as Milestone 3 quality gates, and what additional evidence is required? |
| An authorised person can gain access to the system by stealing a user's credentials, so identity must be validated before protected actions are allowed | Security - Authentication/ access control | Password-based authentication with session management and secure password storage (Omotunde and Ahmed, 2023). Verification evidence to consider includes test results of valid login, rejected invalid credentials, and passwords not being stored in plaintext. | Weak passwords and phishing attacks can still compromise the account; MFA is recommended as an added layer of protection (Khanna, 2025; Omotunde and Ahmed, 2023). | Whether MFA will be implemented for M3, and what session management rules the team will enforce and test. |
| Service account credentials, API keys, or other secrets may be exposed through source code or committed configuration files. | Security - Secrets/ configuration management | Store secrets in a dedicated secrets vault or environment variables rather than in the source code (IBM, 2024). Verification evidence to consider includes a repository inspection confirming no real secrets have been committed and a review of the .gitignore file. | Infrequent rotation of secrets can still let an attacker in via brute force even if secrets are not committed (IBM, 2024). Therefore, they shouldn't remain unchanged indefinitely (IBM, 2024). | Which secrets management system the team will adopt (vault vs .env files) and what rotation policy, if any, will be enforced. |
| Dev/prod parity and externalising configuration from source code reduce deployment risk by keeping environments consistent; uncontrolled differences (dependency versions, permissive local settings) allow a system to behave correctly in one environment yet fail in another. | Deployment risk from environment divergence and exposed/hardcoded configuration | Environment parity checks, confirming staging mirrors production dependencies and settings; configuration and secrets injected via environment variables rather than hardcoded, verified through repository/build inspection (Wiggins, 2011; National Institute of Standards and Technology, 2020) | Even close parity cannot fully replicate production-scale traffic, real data volumes, or live network conditions, so a staging pass is only valid evidence if staging is genuinely representative (Wiggins, 2011) | Which CivicConnect environment differences are acceptable versus which must be eliminated before deployment, and how configuration/secrets will be separated from source control |
| Tested rollback and backup/restoration procedures, combined with defined telemetry thresholds across logs, metrics, and alerts, provide evidence that a system can recover from failure and that degradation will actually be detected, rather than merely assumed. | Failure recovery and undetected operational degradation | An executable, pre-tested rollback procedure; automated backups with periodic restoration tests; monitored metrics (latency, traffic, errors, saturation) with actionable alert thresholds (Humble and Farley, 2010; Beyer et al., 2016) | Rollback becomes significantly more complex when a release changes the database schema, and collecting telemetry without actionable thresholds or review provides little real operational value (Beyer et al., 2016) | Which CivicConnect releases require backward-compatible schema handling for safe rollback, and which specific metrics/alert thresholds suit the system's actual usage patterns |

---

## AI Research & Verification Record

| *Team Member* | *AI tool/ use* | *Purpose* | *Output used?* | *How independently verified* | *What was changed/rejected* |
| --- | --- | --- | --- | --- | --- |
| Tinyiko Siwele | ChatGPT<br>Claude | Clarification in constructing the threat-to-control traceability matrix<br>Creating a summary of Q2 research findings to apply them to the Research-to-M3 Engineering Map | Partially<br>Majority | Verified<br>Verified | The information in the verification evidence column was reworded, and the remaining columns were rejected because they were not backed by evidence. Sentence construction was changed slightly |
| Mutshidzi Nduvheni | Perplexity<br>ChatGPT<br>Claude | Providing possible sources that will be useful for my findings on Quality Strategy<br>Summarizing Q1 findings which was used for Q4 research to m3 engineering and will be used later for Milestone 3 decision making<br>Providing decisions for the risk verification map | Partially<br>Majority<br>All | Verified<br>Verified | If source did not have any useful findings for Q1 or the link ended up not being academically credited, then an alternate source was chosen from other sources provided by google scholar. Sentences that included misconceptions/dididn't have enough information. |
| Thabang Molise | Claude<br>Gemini | Creating a template for the Production-Readiness Evidence Matrix for Question 3.<br>Grammar checks, referencing format alignment, and academic tone refinement. | Partially<br>Majority | Verified against existing similar matrix used in academic articles.<br>Verified | Template contained fields that did not match required criteria and was slightly overcomplicated for the current application use case. Some referencing structure was incorrect, and uses of certain punctuation marks did not fit the context of the text. |

---
