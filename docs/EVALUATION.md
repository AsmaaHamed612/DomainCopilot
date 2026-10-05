# Evaluation Report — Current MVP

**Evaluation date:** 2026-10-05

**Scope:** Deterministic policy-version, coverage, payout, and human-referral behavior. This is not an evaluation of retrieval or an LLM because neither is implemented.

## Dataset and harness

The versioned golden set is [`docs/evaluation/claims-golden-set.json`](evaluation/claims-golden-set.json). It contains 25 synthetic claim scenarios: 20 ordinary boundary/lookup cases and 5 adversarial exclusion-text cases. The application test reads this file and compares each deterministic recommendation and payout with the expected result. The test is run as part of the normal .NET test suite; there is not yet a separate command-line evaluation report generator.

Run it from the repository root:

```powershell
dotnet test .\DomainCopilot.sln --no-restore
```

The golden-case test is `ClaimAdjudicationEvaluationTests.GoldenSet_ProducesExpectedDeterministicRecommendations` in `tests/DomainCopilot.Application.Tests/ClaimAdjudicationEvaluationTests.cs`.

## Baseline results

| Measure | Result | Interpretation |
|---|---:|---|
| Golden cases matched exactly | 25 / 25 (100%) | All expected recommendation and payout pairs matched for the deterministic code and synthetic fixture. This is fixture agreement, not real-world accuracy. |
| Adversarial exclusion-text cases referred to a human | 5 / 5 (100%) | Structured exclusions always trigger review. The exclusion text itself is not interpreted by a model, so this does not establish LLM prompt-injection resistance. |
| Domain + application tests | 20 / 20 passed | Local test run on 2026-10-05. |
| Retrieval hit rate | Not measured | There is no ingestion, index, or retrieval pipeline. |
| Groundedness / citation correctness | Not measured | The API does not produce retrieved citations or LLM answers. |
| Refusal correctness | Not measured as an independent metric | The golden set checks deterministic `ReferToHuman` outcomes for missing/ambiguous structured policy data, but there is no corpus-grounded refusal behavior to score. |
| Tokens and model cost | Not applicable | No model calls exist; telemetry values remain null. |

## Failure analysis and limits

- The strongest current result is that policy-version selection and payout boundaries match the authored synthetic examples. The examples are small, hand-authored, and do not estimate production claim accuracy.
- All five adversarial strings are stored as exclusion text. `ExclusionAnalystAgent` does not parse that text: when any structured exclusion exists, it refers the claim to a human. The current tests show that the workflow remains conservative for these fixtures, but they do not test an LLM reading malicious source documents.
- There is no ≥25-pair question/answer corpus, retrieval harness, citation check, groundedness judge, or refusal-accuracy report. Those core evaluation measures remain deferred until ingestion and retrieval exist.
- The test does not evaluate conflicting document versions, OCR errors, multilingual content, ranking quality, or model nondeterminism.

## Next evaluation increment

After ingestion and hybrid retrieval are implemented, add a separate retrieval Q/A set with source-chunk IDs and expected refusal labels. Report hit-rate at a documented `k`, citation precision, groundedness, and refusal correctness; include at least five out-of-corpus, ambiguous, conflicting-source, and injection cases. Preserve incorrect results in the report and explain their causes instead of tuning them out of the set.

