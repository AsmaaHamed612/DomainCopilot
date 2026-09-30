# System Design — Day 1 Baseline

## Target architecture
The final system will contain an HTTP API, relational persistence, vector retrieval, hybrid keyword+dense search, an LLM provider abstraction, three specialized D2 agents plus an orchestrator, an approval gate, review queue, observability store, and real-time progress channel.

## Implemented MVP slice
Day 1 intentionally implements only the domain and application foundations needed to make later capabilities safe:

- Claim and policy models.
- Date-aware policy version selection.
- Coverage and exclusion models.
- Deterministic payout calculator.
- Human review queue model with SLA and decision controls.
- Unit tests for the above.

## Gap table
| Target component | Implemented? | Interim mitigation | Planned slice |
|---|---:|---|---|
| Relational persistence | No | In-memory/domain objects only | Persistence slice |
| Vector store | No | None yet | RAG slice |
| LLM provider abstraction | No | No LLM calls on Day 1 | Agent slice |
| Three D2 agents + orchestrator | No | Domain contracts only | Agent slice |
| Human review queue | Domain model only | Unit-tested state transitions | API + persistence slice |
| Authentication/RBAC | No | No protected production endpoint yet | Security slice |
| Observability | No | Not yet applicable | Observability slice |

## Key design decision
Financial calculations are deterministic domain code. The future LLM layer may interpret evidence and draft a recommendation, but it will not calculate authoritative limits, deductibles or payouts.
