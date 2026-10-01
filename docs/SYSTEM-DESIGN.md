# System Design — Day 1 Baseline

## Target architecture
The final system will contain an HTTP API, relational persistence, vector retrieval, hybrid keyword+dense search, an LLM provider abstraction, three specialized D2 agents plus an orchestrator, an approval gate, review queue, observability store, and real-time progress channel.

## Implemented MVP slice
Day 1 and Day 2 implement the domain foundations and an initial persisted HTTP workflow:

- Claim and policy models.
- Date-aware policy version selection.
- Coverage and exclusion models.
- Deterministic payout calculator.
- Human review queue model with SLA and decision controls.
- SQL Server relational persistence with an initial EF Core migration and synthetic policy seed data.
- HTTP claim intake/adjudication and review queue operations, including decision audit records.
- Domain and application unit tests for deterministic adjudication and reviewer ownership.

## Gap table
| Target component | Implemented? | Interim mitigation | Planned slice |
|---|---:|---|---|
| Relational persistence | Yes, SQL Server + EF migration | Single SQL Server instance; not yet containerized or backed up | Add Compose and operational backup/restore |
| Vector store | No | None yet | RAG slice |
| LLM provider abstraction | No | No LLM calls on Day 1 | Agent slice |
| Three D2 agents + orchestrator | No | Domain contracts only | Agent slice |
| Human review queue | Yes, API + SQL persistence + decision audit | No authentication yet; assigned reviewer ID is request data | Add RBAC and atomic audit transactions |
| Authentication/RBAC | No | No protected production endpoint yet | Security slice |
| Observability | No | Not yet applicable | Observability slice |

## Key design decision
Financial calculations are deterministic domain code. The future LLM layer may interpret evidence and draft a recommendation, but it will not calculate authoritative limits, deductibles or payouts.
