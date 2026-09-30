# Architecture — Day 1

## Style
Clean Architecture is used because the assignment requires the domain and application layers to remain independent of LLM, vector-store and web-framework SDKs.

## Dependency direction
```text
Api ───────────────► Application ─────────────► Domain
  │                         ▲
  └──────► Infrastructure ──┘
```

- **Domain:** business entities, value objects, enums and deterministic rules.
- **Application:** use-case contracts and orchestration-facing application services.
- **Infrastructure:** external adapters; intentionally minimal on Day 1.
- **API:** HTTP boundary only.

## D2 flow
```text
Claim received
     │
     ▼
Match policy version by loss date
     │
     ▼
Evaluate coverage / exclusions
     │
     ▼
Deterministic limit + deductible + payout calculation
     │
     ▼
Recommendation
     │
     ▼
Human Adjuster Approval Gate
```

## T5 review queue
```text
Recommendation
     │
     ▼
Create queue item
     │
     ├── Priority
     ├── Assigned reviewer
     ├── SLA due date
     └── Escalation
     │
     ▼
Approve / Reject / Edit-and-Approve + Comment
```

## Security boundary decision
LLM output will never be treated as authoritative financial state. Later application services will validate model outputs and invoke deterministic domain calculations before a decision reaches the human approval gate.
