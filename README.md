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

The API is an early workflow slice. Claim adjudication now runs through three typed stages: Coverage Matcher, Exclusion Analyst and Adjudication Drafter. The orchestrator records a short status for each stage in the adjudication response. These are deterministic application components; no LLM provider or document retrieval is wired in yet. Because exclusion evidence is unavailable, a policy version with listed exclusions is referred to a human adjuster. Every recommendation remains behind the existing review queue approval step.

Deferred to later slices:
- Document ingestion/RAG.
- Vector storage.
- LLM provider abstraction and evidence citations.
- Token/cost telemetry (the current deterministic workflow has no model usage).
- Reviewer statistics.
- Retrieval/LLM evaluation metrics and security hardening.

## Day 4 status
Implemented:
- API-key authentication with separate Adjuster and Reviewer roles. Reviewer decisions use the authenticated reviewer ID rather than a request-body identity.
- Server-sent progress events for claim adjudication. Disconnecting the client cancels work and sends the claim to the human review queue.
- Correlation IDs and persisted agent runs/steps, including status and duration, available from `GET /agent-runs/{runId}`.
- A 25-case golden set covering payout boundaries, missing policy/coverage, exclusions and adversarial exclusion text.

The workflow remains deterministic and does not call an LLM or retrieve documents. Retrieval hit rate and groundedness are not calculated; chunk, token and model-cost values remain null in run telemetry until those providers exist.

## Day 5 status
Added an honest security assessment and evaluation report for the current implementation:
- [`docs/SECURITY.md`](docs/SECURITY.md) lists implemented controls, known gaps and deployment blockers.
- [`docs/EVALUATION.md`](docs/EVALUATION.md) records the deterministic 25-case baseline and explains why retrieval and groundedness metrics are not available yet.

These reports do not claim that the deferred security, ingestion, retrieval, or LLM features are implemented.

## Teaching pack

The session materials are in [`teaching/`](teaching/):

- [`learning-outcomes-assessment-map.md`](teaching/learning-outcomes-assessment-map.md) — 90-minute lesson plan, measurable outcomes, and scoring criteria.
- [`slides/governed-multi-agent-workflows.pptx`](teaching/slides/governed-multi-agent-workflows.pptx) — presentation with speaker notes.
- [`lab-multi-agent-orchestration.md`](teaching/lab-multi-agent-orchestration.md) — guided API lab, expected results, answer key, and stretch work.
- [`common-trainee-mistakes.md`](teaching/common-trainee-mistakes.md) — likely misconceptions and instructor corrections.
- [`video-scripts.md`](teaching/video-scripts.md) — scripts and recording checklist for the two required videos. The candidate must still record and publish both videos in their own face and voice; no video links are claimed here.

The pack teaches the current deterministic workflow accurately and calls out the missing LLM/RAG behavior. It does not present planned components as implemented.

## Run locally (Windows)
Prerequisites: .NET 9 SDK and MySQL Server 8.0 or later. MySQL Workbench is optional; the API connects directly to the server. The API applies migrations and seeds its synthetic demo policy on startup.

1. In MySQL Workbench, connect to your local server (typically host `127.0.0.1`, port `3306`). Open a SQL tab and run:

   ```sql
   CREATE DATABASE IF NOT EXISTS DomainCopilot;
   ```

2. In PowerShell, from the repository folder, set your local MySQL username and password for this terminal session. Replace the example values with the same credentials that work in Workbench:

   ```powershell
   $env:ConnectionStrings__DomainCopilot = 'Server=127.0.0.1;Port=3306;Database=DomainCopilot;User ID=root;Password=YOUR_MYSQL_PASSWORD;SslMode=None;AllowPublicKeyRetrieval=True'
   ```

   The password stays in your local terminal and is not part of the repository. `SslMode=None` is for this local loopback connection only; use TLS for remote database servers.

3. In the same PowerShell session, create separate development API keys for adjusters and reviewers. Keep them private and do not add them to Git:

   ```powershell
   $env:Authentication__AdjusterApiKey = [Convert]::ToHexString([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
   $env:Authentication__ReviewerApiKey = [Convert]::ToHexString([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
   $env:Authentication__ReviewerId = 'reviewer-asmaa'
   ```

   The reviewer ID must match the reviewer assigned to a queue item. Claim intake and adjudication require the adjuster key; review actions require the reviewer key. Queue assignment requires the adjuster key.

4. Start the API:

```powershell
dotnet run --project .\src\DomainCopilot.Api
```

The HTTP profile listens on `http://localhost:5121`; health is at `/health` and remains public. Open `src/DomainCopilot.Api/DomainCopilot.Api.http` for sample claim and review requests. The seeded demo policy number is `POL-DEMO-001`, with `WATER` coverage. Claims using the 2025 version can receive a deterministic recommendation; the 2026 version includes an exclusion and therefore refers the claim for human review. The adjudication response includes `agentSteps`, `runId` and `correlationId`.

To stream progress, call `POST /claims/{claimId}/adjudicate/stream` with the Adjuster key and `Accept: text/event-stream`. Press Ctrl+C in `curl.exe` to cancel the request; the server records a cancelled run and leaves the claim for human review. Inspect persisted run details with `GET /agent-runs/{runId}` and the Reviewer key.

Run verification with:

```powershell
dotnet restore .\DomainCopilot.sln --locked-mode
dotnet build .\DomainCopilot.sln --no-restore
dotnet test .\DomainCopilot.sln --no-build --no-restore
```

The golden-case test evaluates 25 deterministic claim scenarios. RAG retrieval hit rate and LLM groundedness are not reported until those components exist.

## Architecture rule
Domain and Application do not reference LLM SDKs, vector-store SDKs or ASP.NET Core. External providers will be introduced through interfaces and Infrastructure adapters.

## Continuous integration

Pull requests run the locked .NET restore, solution build, and unit-test suite through [GitHub Actions](.github/workflows/dotnet.yml). A green CI run verifies compilation and the existing automated tests; it does not verify connectivity to an individual developer's local MySQL instance.

## Starter attribution
This solution started from the supplied ITI starter repository/project structure. The implementation is being built independently for the assigned D2/T5 variant.

