# GitHub Governance

## Repository

* One official GitHub repository shall be used for CivicConnect.
* All three team members have **Owner** access for repository administration.

---

## Branching Strategy

### Protected `main`

* `main` represents the **controlled product state**.
* Development shall not be performed directly on `main`.
* Direct pushes to `main` shall be disabled.

### Short-Lived Development Branches

* Each development task shall have its own **short-lived branch**.
* Branches shall be created from the latest `main` and linked to a corresponding GitHub Issue.
* A branch shall only contain work relating to its associated issue/task.
* Completed branches shall be merged through a pull request and **deleted after merging**.

### Branch Naming Convention

Branches shall follow:

```text
<type>/<issue-number>-<short-description>
```

Examples:

```text
feat/23-user-registration
fix/31-login-validation
docs/18-github-governance
test/27-registration-tests
refactor/35-notification-service
```

Supported types:

* `feat` – new functionality
* `fix` – bug fix
* `docs` – documentation
* `test` – testing-related changes
* `refactor` – code restructuring without changing intended behaviour

---

## Pull Requests

Pull requests shall be used to integrate completed work into `main`.

### PR Naming Convention

PR titles shall follow:

```text
<type>: short description
```

Examples:

```text
feat: implement user registration
fix: validate duplicate email addresses
docs: update GitHub governance
test: add registration service tests
refactor: separate notification service
```

### PR Description Format

Each pull request shall use the following structure:

```markdown
## Summary

Briefly describe what was changed and why.

- Change 1
- Change 2
- Change 3

## Related Issue

Closes #<issue-number>
```

Using `Closes #<issue-number>` links the PR to the GitHub Issue and allows the issue to be automatically closed when the PR is merged.

---

## Pull Request Review

* Changes to `main` must be introduced through a pull request and approved by at least **two other team members**.
* The author of a pull request shall not approve their own changes.
* Reviewers shall perform a meaningful review rather than simply approving the request.
* Review comments should identify issues, alternatives, questions, or confirmation of relevant checks.
* Pull requests should remain **small and focused** to make meaningful review practical.
* Reviewers should use the PR checklist to consider functionality, testing, security, and scope.
* Changes requested by reviewers shall be addressed before merging where applicable.

---

## Issue and Task Tracking

* Work to be completed shall be represented by a **GitHub Issue**.
* Issues shall be tracked on the project Kanban board.
* Each implementation branch shall correspond to a specific issue.
* The issue number shall be included in the branch name.
* The pull request shall reference the same issue using `Closes #<issue-number>`.
* This establishes the following traceability:

```text
GitHub Issue
     ↓
Short-Lived Branch
     ↓
Commits
     ↓
Pull Request
     ↓
Two Peer Reviews
     ↓
Protected main
```

---

## Secrets and Sensitive Information

* Passwords, API keys, access tokens, private keys, and other confidential credentials must not be committed to the repository.
* Sensitive configuration shall be stored using appropriate environment variables or secure configuration mechanisms.
* If a secret is accidentally committed, it must be reported and removed/revoked as appropriate.

---

## Governance Principle

The team shall favour **small, traceable, reviewable changes** over large or long-lived development branches. Each task should have a clear beginning and end: **Issue → short-lived branch → PR → review → merge → branch deletion**.
