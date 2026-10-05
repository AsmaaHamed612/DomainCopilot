# Business Requirements Document — Day 1 Baseline

## Context
Insurance adjusters need a grounded claims-adjudication assistant that identifies the policy version applicable to the loss date, evaluates coverage and exclusions, performs deterministic financial calculations, and routes consequential decisions to a human adjuster.

## Personas
- **Adjuster:** reviews and approves/rejects/edit-and-approves recommendations.
- **Claims analyst:** supplies claim and policy evidence and monitors the queue.
- **System administrator:** manages configuration and access.

## Day 1 requirements
| ID | Requirement | Acceptance criteria | Status |
|---|---|---|---|
| BR-01 | Model an insurance claim | Claim stores policy number, loss date, claimed amount, description and lifecycle status | Implemented |
| BR-02 | Match policy version by date | A policy can contain versions and the applicable version is selected from the loss date | Implemented |
| BR-03 | Deterministic financial calculation | Payout is calculated in domain code using coverage limit and deductible | Implemented |
| BR-04 | Human review queue | Queue item supports reviewer assignment, priority, SLA due date and status | Implemented |
| BR-05 | Human decision controls | Reviewer can approve, reject, or edit-and-approve with a mandatory comment | Implemented |

## Day 2 requirements
| ID | Requirement | Acceptance criteria | Status |
|---|---|---|---|
| BR-06 | Receive and persist a claim | API validates claim identity, policy, coverage, loss date and amount; claim is stored in MySQL | Implemented |
| BR-07 | Adjudicate against applicable policy | Select policy version by loss date, match coverage, calculate payout in deterministic code, and refer to a human when evidence/policy is ambiguous | Implemented |
| BR-08 | Persist and operate the review queue | API supports assignment, starting review, escalation, approve/reject/edit-and-approve; only the assigned reviewer can decide | Implemented |
| BR-09 | Audit reviewer decisions | Each decision stores reviewer, action, final decision, mandatory comment and timestamp in a separate SQL table | Implemented |
| BR-10 | Separate adjuster and reviewer access | Protected routes enforce distinct server-side roles; reviewer decision identity is derived from authentication | Implemented as an MVP with shared role keys; production identity and per-user scoping are deferred |
| BR-11 | Inspect adjudication runs | Persist run and step status, duration, and correlation ID; stream progress and honor client cancellation | Implemented; token/cost/chunk telemetry depends on deferred providers |
| BR-12 | Evaluate deterministic adjudication | Run 25 synthetic cases, including five adversarial exclusion-text cases, against expected outcomes | Implemented for deterministic outcomes; retrieval and groundedness evaluation are deferred |

## Day 2–5 traceability
- BR-06: `POST /claims`, `ClaimIntakeService`, and the `Claims` migration table — implemented.
- BR-07: `POST /claims/{id}/adjudicate`, `ClaimAdjudicationOrchestrator`, and seeded date-effective policy versions — implemented for deterministic structured policy data; document evidence and retrieval remain deferred.
- BR-08: `/review-queue` endpoints and `ReviewQueueService` state rules — implemented, including reviewer assignment checks.
- BR-09: `ReviewDecisionAudits` migration table and `GET /review-queue/{id}/audit` — implemented.
- BR-10: Adjuster and Reviewer API-key roles enforced on protected endpoints — implemented as an MVP; managed identities, per-user keys, rotation, and object-level read scoping remain deferred.
- BR-11: Correlated and persisted adjudication run/step status and SSE progress/cancellation — implemented; token, model-cost, and retrieved-chunk telemetry remain unavailable without RAG/LLM providers.
- BR-12: A 25-case deterministic baseline including five adversarial exclusion-text cases — implemented; retrieval hit-rate, groundedness, and LLM refusal metrics are not measured.

See [`SECURITY.md`](SECURITY.md) and [`EVALUATION.md`](EVALUATION.md) for the current controls, verification evidence, and gaps. Authentication is implemented, but it is not a production identity system. Transactional updates across claim, queue, and audit writes have not been verified end to end.

## Business rules
1. The LLM must not perform authoritative payout arithmetic.
2. A policy version must be selected using the claim loss date.
3. A consequential adjudication decision requires human approval.
4. Review decisions require a reviewer comment.
5. Queue items have an explicit SLA due date and can be escalated.

## Out of scope for Day 1
RAG, document ingestion, LLM providers, vector databases, authentication, persistence, real-time streaming, evaluation and production deployment.

## Assumptions
- Money is represented with a decimal amount and ISO-like currency code.
- Policy periods are inclusive of both dates.
- The first MVP uses synthetic/public data only.

