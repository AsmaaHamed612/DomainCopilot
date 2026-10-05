# System Design — Day 4 Update

## Target architecture
The final system will contain an HTTP API, relational persistence, vector retrieval, hybrid keyword+dense search, an LLM provider abstraction, three specialized D2 agents plus an orchestrator, an approval gate, review queue, observability store, and real-time progress channel.

## Implemented MVP slice
Day 1 through Day 4 implement the domain foundations and a persisted HTTP workflow:

- Claim and policy models.
- Date-aware policy version selection.
- Coverage and exclusion models.
- Deterministic payout calculator.
- Human review queue model with SLA and decision controls.
- MySQL relational persistence with an initial EF Core migration and synthetic policy seed data.
- HTTP claim intake/adjudication and review queue operations, including decision audit records.
- Three typed adjudication stages coordinated by an orchestrator. Coverage matching uses the policy version effective on the loss date; exclusion analysis abstains to human review when there are exclusions but no retrieved evidence; payout remains deterministic domain code.
- Per-stage status and summary in the adjudication response, with every recommendation still routed to the human review queue.
- API-key authentication and server-side Adjuster/Reviewer role policies. The reviewer identity used for decisions is taken from the authenticated key.
- SSE progress events with request-abort cancellation, plus correlated, persisted run and step records.
- A 25-scenario deterministic golden set, including five adversarial exclusion-text cases.
- Domain and application unit tests for deterministic adjudication and reviewer ownership.

## Gap table
| Target component | Implemented? | Interim mitigation | Planned slice |
|---|---:|---|---|
| Relational persistence | Yes, MySQL + EF migration | Single local MySQL instance; not yet containerized or backed up | Add Compose and operational backup/restore |
| Vector store | No | None yet | RAG slice |
| LLM provider abstraction | No | Stages are deterministic and use structured policy data | LLM/RAG slice |
| Three D2 agents + orchestrator | Partial | Three typed application stages coordinate the structured workflow; no LLM prompts or document retrieval yet | Add retrieval-grounded analysis and LLM provider |
| Human review queue | Yes, API + SQL persistence + decision audit | Role-protected; adjuster assigns and authenticated reviewer identity decides | Add transaction boundaries and reviewer statistics |
| Authentication/RBAC | Yes, API keys + role policies | Keys are static per role and require secure configuration/rotation | Add a managed identity provider and key rotation |
| Observability | Yes, persisted run/step status, duration and correlation ID | Token, cost and chunk metrics are null without an LLM/RAG provider | Populate provider usage metrics and add dashboards |
| Evaluation | Partial, 25 deterministic claim scenarios | RAG hit rate and groundedness are not applicable yet | Add retrieval and groundedness evaluation with RAG |

## Key design decision
Financial calculations are deterministic domain code. The future LLM layer may interpret evidence and draft a recommendation, but it will not calculate authoritative limits, deductibles or payouts. API keys are a development/MVP authentication mechanism; deployment should use a managed identity provider.
