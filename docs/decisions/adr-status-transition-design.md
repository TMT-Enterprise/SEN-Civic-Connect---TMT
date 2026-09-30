# ADR-Status-Rules-Design: Status Transition Rules via Table-Driven Transition Validator

**Status:** Accepted  
**Date:** 2026-09-30  
**Decision makers:** TMT Team (Tinyiko Siwele, Mutshidzi Nduvheni, Thabang Molise)  
**Related requirements:** FR-010, FR-011, FR-012  
**Related ASRs / quality drivers:** Maintainability, Single Responsibility Principle (SRP), Open/Closed Principle (OCP), Modifiability  
**Related A2 research:** Assignment 2 Task 1 – Design Quality and Design Patterns  
**RTM links:** FR-010 (staff-only transitions), FR-011 (forward-only controlled status lifecycle), FR-012 (auto-generated Overdue status)

## Context
CivicConnect requirements mandate that a service request’s status must progress through a controlled, forward-only sequence (FR-011), that specific status transitions are restricted to staff members (FR-010), and that the system must automatically transition a request to an "Overdue" status when its due date passes without resolution (FR-012). In a naive implementation, conditional logic checking whether a transition is allowed from the current status would be scattered across the UI layer, the request-handling domain service, and staff/admin views. Consequently, the core business rule governing the request lifecycle becomes duplicated in multiple places. Any future modification to the status lifecycle—such as introducing a new status or an intermediate approval step—would require locating and consistently updating every duplicate check, creating a high risk of subtle bugs and violating the Open/Closed Principle (Martin, 2003).

## Decision Drivers 
- Single Responsibility Principle (SRP) and Open/Closed Principle (OCP) (Martin, 2003)
- Centralization of lifecycle rules into a single source of truth (Sommerville, 2015)
- Simplest architecture that satisfies current requirements (FR-010, FR-011, FR-012) without unnecessary structural overhead (Fowler, 2018)
- Avoidance of over-engineering when statuses differ primarily by allowed transitions rather than state-specific behaviors
- Ease of future refactoring if complex per-status behavioral differences emerge later

## Alternatives Considered

### Option A - Table-driven Transition Validation
A single, centralized lookup table maps current statuses to allowed next statuses and required roles, consulted by a single validation function before committing status changes (Sommerville, 2015). This creates a single source of truth for FR-011 and satisfies OCP for a linear lifecycle, as adding new statuses requires updating table entries rather than modifying scattered conditional logic. However, if statuses eventually require unique behaviors beyond transition checks, the table provides no natural place for that logic.

### Option B – State pattern
Each status becomes an independent class implementing a common `State` interface, with the request delegating state logic to its current state object (Gamma et al., 1994). This maximizes extensibility and OCP compliance when statuses require distinct behaviors and exit/entry side effects (Martin, 2003). However, for a small, fixed set of statuses whose primary difference is transition validity, this introduces unnecessary structural complexity and class proliferation (Fowler, 2018).

A2 research concluded that the State pattern is the stronger choice once statuses require distinct behavior beyond transition routing. Since CivicConnect's current baseline deals strictly with valid sequences (FR-011), role permissions (FR-010), and a time-triggered override (FR-012), the table-driven validator solves the problem cleanly without extra class machinery, while remaining easy to refactor into a State pattern if complex behaviors emerge later.

## Decision
The team selects **Table-driven Transition Validation (Option A)** as the final M2 design approach for centralizing status transition rules.

## Rationale
- Current requirement set describes the problem purely as validating allowed transitions and enforcement of role permissions, rather than complex per-status behavioral changes.
- Concentrates all transition rules (including staff-only restrictions for FR-010 and the automatic transition path for FR-012) into a single, cohesive source of truth.
- Satisfies OCP for realistic scope expansion (e.g., adding a new status or transition pair) by allowing modifications to the lookup matrix without altering caller logic.
- Avoids class proliferation and structural over-engineering associated with the State pattern prior to a demonstrated behavioral need.
- Maintains a clean path for evolutionary architecture:

The decision is taken inside a Modular Monolith architecture. the `TransitionValidator` component resides within the Request module's domain core. Request service handlers delegate transition checks to this component before executing any status updates, ensuring uniform policy enforcement regardless of whether the update was triggered by direct API calls, staff actions, or an automated background process (FR-012).

## Consequences

**Expected benefits**
- Eliminates duplication of lifecycle conditional logic across UI, service, and administrative layers
- High maintainability: updating or adding a lifecycle rule requires modifying only the lookup configuration
- Simplified testing: transition rules can be exhaustively unit-tested against the table structure in isolation
- Clear enforcement of role-based transition constraints (FR-010) and system overrides (FR-012)

**Complexity / trade-off introduced**
- If individual statuses later require custom execution paths (beyond transition validation), additional conditional branches could emerge if not properly refactored to a State pattern.
- The validator must accommodate context-aware rules (such as checking caller roles for FR-010 or system automation flags for FR-012) alongside simple state pairs.

The trade-off is accepted because the current requirement scope remains focused on transition validity rather than state-bound behavior

## Affected Modules / Components / Interfaces

- Request Domain Model / Aggregate Root (`Request`)
- `TransitionValidator` (domain service / policy component)
- Transition Rule Configuration Matrix / Lookup Table (`StatusTransitionMap`)
- Request Application Service (coordinates status update requests with caller roles)
- Automated Overdue Processor / Background Job (triggers FR-012 system transitions)
- Unit tests covering state transition validity matrix and authorization checks

