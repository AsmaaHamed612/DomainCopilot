# ADR-002: Deterministic Financial Calculation

## Decision
Calculate coverage caps, deductibles and payouts in deterministic C# domain code rather than allowing an LLM to perform authoritative arithmetic.

## Why
The assigned D2 risk explicitly identifies arithmetic hallucination. Deterministic calculation is testable and auditable.
