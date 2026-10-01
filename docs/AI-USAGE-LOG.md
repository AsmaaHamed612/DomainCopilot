# AI Usage Log — Day 1

This file records AI assistance used during development. It is intentionally maintained from the first development day.

| Date | Task delegated | Verification | Outcome |
|---|---|---|---|
| Day 1 | Suggested initial Clean Architecture/domain decomposition | Reviewed against the ITI brief and project boundaries | Accepted after manual review |
| Day 1 | Drafted unit-test scenarios for policy versioning, payout and review queue | Reviewed business rules and test intent | Accepted after manual review |
| Day 1 | Diagnosed the .NET 9 build failure caused by the ASP.NET OpenAPI 10.0.12 package and aligned it to 9.0.6 | Restored from the local NuGet cache, built the full solution, and reviewed the framework/package compatibility | Accepted; package 10.0.12 generated compile errors against the .NET 9 target |
| Day 2 | Implemented the SQL-backed claim intake, policy adjudication, review queue API, migration, and decision audit workflow; generated test scenarios and diagnosed the EF table-name mismatch | Reviewed the ITI Day 2 requirements, built the full solution, and ran all domain/application tests (16 passed) | Accepted after manual review; LocalDB integration could not be run in this environment |

AI assistance does not replace developer verification. The submitted repository must contain only behavior that is actually implemented and tested.
