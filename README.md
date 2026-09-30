# DomainCopilot

ITI Technical Instructor Assessment — **D2 Insurance Claims Adjudication + T5 Human Review Queue**.

## Assignment derivation
- Last two digits: `09`
- Domain: `09 mod 7 = 2` → D2 Insurance — Claims Adjudication
- Full digit sum supplied for the assignment: `37`
- Twist: `37 mod 8 = 5` → T5 Human Review Queue

## Day 1 status
Implemented:
- Clean Architecture project boundaries.
- Insurance claim, policy, policy version, coverage and exclusion domain models.
- Version/date-aware policy selection model.
- Deterministic payout calculation outside the LLM.
- Human review queue domain model with assignment, priority, SLA, escalation and approval/rejection/edit-and-approve operations.
- Initial unit tests for domain invariants and deterministic calculations.

Deferred to later slices:
- Document ingestion/RAG.
- Multi-agent orchestration.
- Persistence and vector storage.
- Authentication/authorization.
- Real-time progress/cancellation.
- Evaluation harness and security controls.

## Architecture rule
Domain and Application do not reference LLM SDKs, vector-store SDKs or ASP.NET Core. External providers will be introduced through interfaces and Infrastructure adapters.

## Starter attribution
This solution started from the supplied ITI starter repository/project structure. The implementation is being built independently for the assigned D2/T5 variant.

