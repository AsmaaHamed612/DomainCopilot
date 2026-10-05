# Video Recording Scripts

The assignment requires unlisted videos featuring the candidate's own face and voice. These scripts prepare the recording; they are not substitutes for recording and publishing the videos.

## Product demo — target 6 minutes

### 0:00–0:35 — Introduce the workflow

“This is DomainCopilot, an insurance claims workflow for the D2/T5 assignment. The current version uses deterministic policy and payout logic, a reviewer queue, API-key roles, live progress, and persisted run traces. It does not yet call an LLM or retrieve policy documents.”

### 0:35–1:10 — Health and synthetic data

Show `GET /health` and the synthetic `POL-DEMO-001` seed policy. Point out that the demo data is fictional and no claimant personal data is used.

### 1:10–2:20 — Claim intake and grounded boundary

Submit a 2025 water-damage claim. Explain that the loss date selects the effective policy version and that limit/deductible arithmetic is performed in domain code. Show the expected 37,000 EGP calculation for the 42,000 EGP claim.

### 2:20–3:20 — Agent-stage progress

Start streamed adjudication and show Coverage Matcher, Exclusion Analyst, and Adjudication Drafter progress. Explain that these are deterministic typed stages today; no model tokens or retrieval chunks are claimed.

### 3:20–4:20 — Human approval gate

Show the new queue item, assign it, start review with the Reviewer key, and record a decision with a comment. Explain that the adjuster and reviewer roles have different server-side permissions.

### 4:20–5:15 — Trace and cancellation

Open `GET /agent-runs/{runId}` and point to correlation ID, ordered steps, status, and duration. Explain which fields are null because there is no model/RAG. If time allows, demonstrate client cancellation and the fallback to human review.

### 5:15–6:00 — Honest limits

“The current MVP has no document ingestion, vector search, citation-backed answers, or model provider. The security and evaluation reports list those gaps and the next verification steps. The human gate and deterministic financial logic are implemented now.”

## Teaching sample — target 10 minutes

### 0:00–0:45 — Learning goal

“In this short lesson, we will trace a claim through typed workflow stages and show where a human approval gate protects a consequential decision.”

### 0:45–2:00 — Start from the domain risk

Ask trainees: “What could go wrong if the wrong policy version is selected or the payout is guessed?” Take two responses. Explain loss-date version selection and deterministic arithmetic.

### 2:00–4:00 — Walk the pipeline

Show the actual flow: API → Coverage Matcher → Exclusion Analyst → Adjudication Drafter → review queue. For each stage, name the input, output, and conservative stop/referral condition. Be clear that these current stages are application components, not calls to autonomous LLM agents.

### 4:00–6:00 — Show the approval boundary

Demonstrate the 2025 example's 37,000 EGP recommendation. Ask: “Is this a payment decision?” Explain why no: it is still pending human review. Show assignment, reviewer identity from the authenticated Reviewer key, and the audited decision.

### 6:00–8:00 — Read the run trace

Show `runId`, `correlationId`, stage statuses, and durations. Ask trainees which evidence they would need to audit an LLM-backed version. Identify missing chunk IDs, tokens, model cost, citations, and model traces.

### 8:00–9:15 — Discuss the adversarial set

Show one malicious exclusion string. Ask what the deterministic implementation does. Explain that it does not interpret the string and conservatively routes any structured exclusion to review; this does not prove prompt-injection resistance for a future LLM.

### 9:15–10:00 — Check learning

Ask trainees to state the selected 2025 policy version, calculate the payout, and identify who can make the final decision. Close by naming the next engineering step: ingest versioned policy evidence and evaluate citations/refusals before adding model authority.

## Recording checklist

- Record face and voice; speak to trainees rather than reading slide text aloud.
- Use only synthetic claims and private development API keys. Keep keys out of screen recordings; regenerate any key that appears.
- Record the product demo and teaching sample as separate videos; test both unlisted links in a private browser window before adding them to the README.
