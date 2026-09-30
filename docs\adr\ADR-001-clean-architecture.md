# ADR-001: Clean Architecture

## Decision
Use Clean Architecture with Domain at the center, Application depending only on Domain, and Infrastructure/API outside the business core.

## Why
The assessment explicitly requires provider and vector-store independence. This structure makes those dependencies replaceable without changing claim business rules.
