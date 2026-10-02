# AI Usage Log — Day 1

This file records AI assistance used during development. It is intentionally maintained from the first development day.

| Date | Task delegated | Verification | Outcome |
|---|---|---|---|
| Day 1 | Suggested initial Clean Architecture/domain decomposition | Reviewed against the ITI brief and project boundaries | Accepted after manual review |
| Day 1 | Drafted unit-test scenarios for policy versioning, payout and review queue | Reviewed business rules and test intent | Accepted after manual review |
| Day 1 | Diagnosed the .NET 9 build failure caused by the ASP.NET OpenAPI 10.0.12 package and aligned it to 9.0.6 | Restored from the local NuGet cache, built the full solution, and reviewed the framework/package compatibility | Accepted; package 10.0.12 generated compile errors against the .NET 9 target |
| Day 2 | Implemented the SQL-backed claim intake, policy adjudication, review queue API, migration, and decision audit workflow; generated test scenarios and diagnosed the EF table-name mismatch | Reviewed the ITI Day 2 requirements, built the full solution, and ran all domain/application tests (16 passed) | Accepted after manual review; the initial LocalDB connection could not run in this environment |
| Day 2 follow-up | Switched the relational provider from SQL Server to MySQL after confirming the instructor brief permits any stack and the developer machine has MySQL Server available | Rebuilt the solution, ran all 16 tests, and checked the generated MySQL schema; full startup migration still needs the developer's local MySQL login | Accepted to match the available local database; no credentials were committed |
| Day 3 | Split policy matching, exclusion review and recommendation drafting into typed stages coordinated by an adjudication orchestrator; exposed stage summaries in the API response | Full solution build passed with 0 warnings and 0 errors; all 16 tests passed; a live synthetic claim returned all three stage summaries and created a pending human review item | Accepted as a structured workflow slice; no LLM or RAG behavior is claimed |

AI assistance does not replace developer verification. The submitted repository must contain only behavior that is actually implemented and tested.
