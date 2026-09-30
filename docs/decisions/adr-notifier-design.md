# ADR-Notifier-Design: Notification Decoupling via Dependency-Injected Notifier Interface

**Status:** Accepted  
**Date:** 2026-09-30  
**Decision makers:** TMT Team (Tinyiko Siwele, Mutshidzi Nduvheni, Thabang Molise)  
**Related requirements:** FR-006  
**Related ASRs / quality drivers:** Maintainability, Testability, SRP, Dependency Inversion  
**Related A2 research:** Assignment 2 Task 1 – Design Quality and Design Patterns  
**RTM links:** FR-006 (notification on status change)

## Context
When a service request changes status, CivicConnect must notify the requester by email (FR-006). In a naive implementation the same code responsible for creating and updating a request record would also construct and send the notification. Request-handling logic would therefore carry two unrelated responsibilities: managing the request’s own state and knowing the mechanics of a specific notification channel. SMS/WhatsApp notification is currently a deferred, unconfirmed future channel. A tightly coupled implementation would force modification of the request-handling code every time a new channel is added. This is a coupling and single-responsibility problem.

## Decision Drivers 
- Single Responsibility Principle and Dependency Inversion Principle (Martin, 2003)
- Current confirmed scope contains only one active notification channel
- Second channel remains deferred pending stakeholder confirmation
- Cost and team-capacity constraints favour the simplest solution that still satisfies the quality drivers
- Need for independent testability of request-handling logic

## Alternatives Considered

### Option A - Strategy / dependency-injected Notifier interface
A `Notifier` (or `INotifier`) interface exposes a single method such as `send(request, event)`. The request-handling service depends only on the abstraction. A concrete `EmailNotifier` is injected at construction or via the composition root. Adding a later channel requires a new concrete class and a configuration change, not modification of request-handling logic.

### Option B – Observer pattern
The request object acts as subject and maintains a list of observers. Each concrete observer (EmailObserver, later SMSObserver) reacts independently to state changes (Gamma et al., 1994). This maximises extensibility under the Open/Closed Principle but introduces subject/observer registration machinery and indirection.

A2 research concluded that Observer is the stronger choice once multiple independent channels are confirmed and need to react differently to the same event. For the current baseline the dependency-injected Notifier interface satisfies the required principles without paying for extensibility that is not yet needed (Fowler, 2018).

## Decision
The team selects the **dependency-injected Notifier interface (Strategy-style abstraction)** as the final M2 design approach for notification decoupling.

## Rationale
- Confirmed scope contains only one active notification channel (email).
- The second channel remains deferred and unconfirmed.
- Architecture is layered and already requires clear separation between domain/application services and infrastructure concerns.
- The solution keeps the request-handling service decoupled and independently testable.
- The decision remains open to controlled evolution: if a second channel is later confirmed and needs independent reactive behaviour, the same abstraction can be extended or replaced by Observer under change control.

The decision is taken inside a Modular Monolith architecture. The Request module depends only on the `INotifier` abstraction; the concrete `EmailNotifier` resides in an Infrastructure (or Notifications) module. Communication remains in-process, avoiding an unnecessary network boundary while still preserving module separation and independent testability.

## Consequences

**Expected benefits**
- Reduced coupling between request lifecycle logic and notification mechanics
- Improved cohesion of the request service
- Higher testability (service can be unit-tested with a mock or stub Notifier)
- Openness to future channels without modifying core request code

**Complexity / trade-off introduced**
- An extra interface and a concrete implementation class
- Need to wire the dependency (constructor or container injection)
- Modest indirection compared with the registration and notification machinery of a full Observer implementation

The trade-off is accepted because the current requirement set does not justify the heavier pattern.

## Affected Modules / Components / Interfaces

- Application / domain service responsible for request creation and status updates
- `INotifier` (or equivalent) interface
- `EmailNotifier` concrete implementation (infrastructure layer)
- Future concrete notifiers if additional channels are confirmed
- Unit tests that supply a test double for the Notifier


