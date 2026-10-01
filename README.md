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

## Day 2 status
Implemented:
- Policy-backed claim adjudication that selects the policy version for the loss date and calculates payouts in domain code.
- Conservative referral to a human when policy, version, coverage or exclusions leave uncertainty.
- MySQL persistence for policies, claims, review items and immutable reviewer decision audit entries, applied through an EF Core migration at API startup.
- HTTP endpoints for claim intake/adjudication and review assignment, start, escalation, decisions and audit history.
- Synthetic `POL-DEMO-001` seed policy with two date-effective versions.

The API is an early workflow slice: authorization, evidence-driven exclusion matching, orchestration, streaming and reviewer statistics are deferred.

Deferred to later slices:
- Document ingestion/RAG.
- Multi-agent orchestration.
- Vector storage.
- Authentication/authorization.
- Real-time progress/cancellation.
- Evaluation harness and security controls.

## Run locally (Windows)
Prerequisites: .NET 9 SDK and MySQL Server 8.0 or later. MySQL Workbench is optional; the API connects directly to the server. The API applies migrations and seeds its synthetic demo policy on startup.

1. In MySQL Workbench, connect to your local server (typically host `127.0.0.1`, port `3306`). Open a SQL tab and run:

   ```sql
   CREATE DATABASE IF NOT EXISTS DomainCopilot;
   ```

2. In PowerShell, from the repository folder, set your local MySQL username and password for this terminal session. Replace the example values with the same credentials that work in Workbench:

   ```powershell
   $env:ConnectionStrings__DomainCopilot = 'Server=127.0.0.1;Port=3306;Database=DomainCopilot;User ID=root;Password=YOUR_MYSQL_PASSWORD;'
   ```

   The password stays in your local terminal and is not part of the repository.

3. Start the API:

```powershell
dotnet run --project .\src\DomainCopilot.Api
```

The HTTP profile listens on `http://localhost:5121`; health is at `/health`. Open `src/DomainCopilot.Api/DomainCopilot.Api.http` for sample claim and review requests. The seeded demo policy number is `POL-DEMO-001`, with `WATER` coverage. Claims using the 2025 version can receive a deterministic recommendation; the 2026 version includes an exclusion and therefore refers the claim for human review.

Run verification with:

```powershell
dotnet restore .\DomainCopilot.sln --locked-mode
dotnet build .\DomainCopilot.sln --no-restore
dotnet test .\DomainCopilot.sln --no-build --no-restore
```

## Architecture rule
Domain and Application do not reference LLM SDKs, vector-store SDKs or ASP.NET Core. External providers will be introduced through interfaces and Infrastructure adapters.

## Starter attribution
This solution started from the supplied ITI starter repository/project structure. The implementation is being built independently for the assigned D2/T5 variant.
