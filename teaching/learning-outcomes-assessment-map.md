# Learning Outcomes and Assessment Map

**Session:** Governed Multi-Agent Workflows: From Typed Stages to Human Approval  
**Duration:** 90 minutes  
**Audience:** Post-graduate software trainees

## Learning outcomes

By the end of the session, a trainee can:

1. **Explain** why insurance policy selection and payout arithmetic belong in deterministic code rather than an LLM.
2. **Trace** a claim through Coverage Matcher, Exclusion Analyst, Adjudication Drafter, persistence, and the reviewer gate using the actual repository.
3. **Design** a typed stage contract with explicit inputs, outputs, failure states, and a termination condition.
4. **Distinguish** recommendations from authorized side effects and identify where human approval must block state changes.
5. **Interpret** run ID, correlation ID, stage status, duration, SSE progress, cancellation, and the fields that remain unavailable without LLM/RAG.
6. **Evaluate** a golden-set result without presenting fixture agreement as real-world accuracy or prompt-injection resistance.

## Session plan

| Minutes | Activity | Evidence |
|---:|---|---|
| 0–8 | Claims scenario, risks, and session outcomes | Trainees identify wrong policy version and payout arithmetic as safety risks. |
| 8–20 | Typed workflow stages and orchestration patterns | Trainees label each stage's input, output, failure/referral state, and stop condition. |
| 20–32 | Architecture and trust boundaries walkthrough | Trainees locate the API, application orchestrator, domain calculation, persistence, and review queue in the repository. |
| 32–42 | Human approval and side-effect boundary | Trainees distinguish a recommendation from a reviewer-authorized decision. |
| 42–52 | Run telemetry, SSE, and cancellation | Trainees map progress events to persisted steps and explain remaining observability gaps. |
| 52–77 | Hands-on lab | Trainees submit, adjudicate, inspect, assign, and decide a synthetic claim. |
| 77–85 | Stretch challenge and adversarial discussion | Trainees test exclusion handling or role boundaries and state what the tests do not prove. |
| 85–90 | Exit ticket | Each trainee answers the checks below. |

## Assessment checks

Score each check 0 or 1. A trainee meets the outcomes with at least 5 of 6 points and must pass checks 1 and 4 because they cover financial safety and human approval.

1. Given the 2025 demo policy and a 42,000 EGP claim, calculate the 37,000 EGP payout and identify the policy version source.
2. Name the three typed workflow stages in execution order.
3. Identify one failure/referral case that terminates the automatic recommendation path.
4. Explain why `Approve` from adjudication is not a final decision and who can authorize the decision.
5. Locate `runId` and `correlationId` and name one telemetry field that is null or absent today.
6. Explain why the five adversarial golden cases are not proof of LLM prompt-injection resistance.

## Evidence and pass criteria

- Lab evidence: HTTP 201 claim receipt, a deterministic adjudication result, a retrieved run trace, and a reviewer decision audit.
- Exit-ticket evidence: concise answers tied to the code or observed API response.
- Critical pass conditions: correct payout calculation and correct description of the human approval gate.
- If a trainee misses a critical condition, repeat the corresponding lab section before marking the outcome achieved.
