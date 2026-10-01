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
| BR-06 | Receive and persist a claim | API validates claim identity, policy, coverage, loss date and amount; claim is stored in SQL Server | Implemented |
| BR-07 | Adjudicate against applicable policy | Select policy version by loss date, match coverage, calculate payout in deterministic code, and refer to a human when evidence/policy is ambiguous | Implemented |
| BR-08 | Persist and operate the review queue | API supports assignment, starting review, escalation, approve/reject/edit-and-approve; only the assigned reviewer can decide | Implemented |
| BR-09 | Audit reviewer decisions | Each decision stores reviewer, action, final decision, mandatory comment and timestamp in a separate SQL table | Implemented |

## Day 2 traceability
- BR-06: `POST /claims`, `ClaimIntakeService`, `Claims` migration table.
- BR-07: `POST /claims/{id}/adjudicate`, `ClaimAdjudicationService`, seeded date-effective policy versions.
- BR-08: `/review-queue` endpoints and `ReviewQueueService` state rules.
- BR-09: `ReviewDecisionAudits` migration table and `GET /review-queue/{id}/audit`.

API authentication/authorization and transactional updates across claim, queue and audit writes are not implemented yet; these remain security/persistence follow-up work.

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
