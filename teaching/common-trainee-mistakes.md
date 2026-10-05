# Five Common Trainee Mistakes

## 1. Using the newest policy instead of the loss-date version

**Misconception:** The newest policy always applies.  
**Correction:** Select the version effective on the loss date. Walk through the 2025 and 2026 demo policy dates and show the different exclusion outcome.

## 2. Letting the model calculate the payout

**Misconception:** The language model can do limit, deductible, and payout arithmetic reliably enough.  
**Correction:** Keep financial calculations in deterministic domain code. The model may help interpret evidence, but it must not own authoritative financial state.

## 3. Treating a recommendation as a final decision

**Misconception:** An `Approve` recommendation means the claim is paid.  
**Correction:** The recommendation creates a review queue item. An assigned, authenticated reviewer must make and audit the consequential decision.

## 4. Calling deterministic stages autonomous AI agents

**Misconception:** Three named classes automatically satisfy the multi-agent requirement.  
**Correction:** The current stages are typed deterministic application components. A production agent design also needs explicit tool allow-lists, LLM/provider contracts, termination conditions, timeouts, retries, and measured grounded evidence. State the implementation gap plainly.

## 5. Treating a passing golden set as proof of general accuracy or injection safety

**Misconception:** 25/25 fixtures means the system is accurate and resists prompt injection.  
**Correction:** The result only shows agreement with 25 hand-authored deterministic examples. Exclusion text is not interpreted by an LLM in this implementation. Add a retrieval Q/A benchmark and actual indirect-injection tests before making broader claims.
