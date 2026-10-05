# Hands-on Lab: Trace a Governed Claims Workflow

**Audience:** Post-graduate software trainees  
**Duration:** 25 minutes  
**Repository:** DomainCopilot, D2 Insurance + T5 Human Review Queue

## Learning goal

Trace one synthetic claim through typed workflow stages, inspect the stored run, and complete the reviewer approval gate. Distinguish implemented deterministic stages from future LLM agents.

## Requirements

- .NET 9 SDK and MySQL Server 8.0 or later.
- A local `DomainCopilot` database. The API applies migrations and inserts the synthetic demo policy at startup.
- The repository's local-run steps completed from the README. Configure the connection string and two API keys in a private PowerShell session; never commit them or paste them into the lab submission.
- API running at `http://localhost:5121`.

## Part A — Health and authorization (3 minutes)

1. Open `http://localhost:5121/health`.
2. Expected: HTTP 200 and `{"status":"healthy"}`.
3. In `src/DomainCopilot.Api/DomainCopilot.Api.http`, inspect the separate Adjuster and Reviewer key variables.
4. Expected: no key is written into the repository; protected routes use the `X-API-Key` header.

## Part B — Receive and adjudicate a claim (8 minutes)

In the `.http` file, send **Receive a synthetic claim** using the Adjuster key. Use the provided `CLM-DEMO-HTTP-001` claim number only once; choose a new synthetic claim number for a repeated run.

Expected response: HTTP 201, a claim ID, and status `Received`.

Copy the returned claim ID into `@claimId`. Send **Adjudicate the created claim**.

Expected response for the supplied 2025 claim: recommendation `Approve`, calculated payout `37000` EGP, three stage summaries, a `runId`, a `correlationId`, and a pending review queue item. The payout is deterministic: `min(42000, 100000) - 5000 = 37000` EGP.

Discuss: the `Approve` value is a recommendation. It is not a final payment decision because the queue item still requires a human decision.

## Part C — Inspect live progress and trace (5 minutes)

1. Submit a second synthetic claim and copy its ID into `@streamClaimId`.
2. From PowerShell, run the stream request using `curl.exe -N`, the Adjuster key, and `Accept: text/event-stream` as shown in the HTTP file.
3. Expected: progress events identify Coverage Matcher, Exclusion Analyst, and Adjudication Drafter, followed by a result event.
4. Copy the `runId` into `@runId`; call **Inspect an adjudication run** with the Reviewer key.
5. Expected: persisted run status, correlation ID, and ordered steps with status and duration.

The current stages are deterministic application components. No LLM call, document retrieval, token count, or model cost is present.

## Part D — Human approval gate (7 minutes)

1. Copy `reviewQueueItemId` from an adjudication response into `@reviewQueueItemId`.
2. Use the Adjuster key to assign it to `reviewer-asmaa`.
3. Use the Reviewer key to start the review.
4. Submit the approval request with a short, evidence-based comment.
5. Inspect the decision audit endpoint in the API or HTTP examples.

Expected: assignment succeeds, the assigned reviewer can start, the reviewer can record an approval, and the audit record contains the authenticated reviewer identity, action, comment, and time.

## Stretch challenges

1. **Conservative exclusion handling:** submit a claim with loss date `2026-06-15`. Predict the result before sending it. Confirm the structured exclusion causes `ReferToHuman` with zero calculated payout and a queue item.
2. **Role boundary:** call `GET /review-queue` with the Adjuster key, then with the Reviewer key. Record the HTTP statuses and explain the difference.
3. **Identity boundary:** assign a queue item to a different synthetic reviewer ID, then try starting it using the configured Reviewer key for `reviewer-asmaa`. Explain why request-body identity cannot replace the authenticated identity.
4. **Correlation:** send a valid GUID in `X-Correlation-ID`, then omit it or send invalid text. Compare the response header and persisted run.
5. **Cancellation:** start the SSE request and interrupt `curl.exe` with Ctrl+C. Verify the run is cancelled and the claim remains routed to human review.

## Answer key

- The 2025 demo policy is version 1: limit 100,000 EGP, deductible 5,000 EGP; the 42,000 EGP claim yields 37,000 EGP and an `Approve` recommendation.
- The 2026 policy is version 2 and contains a structured exclusion; the exclusion analyst does not guess from claim wording, so the recommendation is `ReferToHuman`, payout is 0, and human review is required.
- Reviewer-only routes reject the Adjuster key. The reviewer identity used to start or decide is derived from the Reviewer key configuration. Assignment is an Adjuster action.
- A valid caller correlation ID is echoed and persisted; invalid or absent input is replaced with a generated GUID.
- On client cancellation, server-side cancellation is observed, the run is marked cancelled, and the claim is sent to the queue.

## Debrief prompts

- Which stage owns policy-version selection, and why should the LLM not calculate money?
- Which workflow operations change state, and what must happen before they are allowed?
- What evidence does the current trace provide? Which token, cost, and retrieved-chunk fields are absent?
- What must change before these deterministic stages can honestly be called LLM agents?
