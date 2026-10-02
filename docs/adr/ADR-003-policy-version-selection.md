# ADR-003: Date-Aware Policy Version Selection

## Decision
A claim selects the policy version whose effective period contains the claim loss date. If multiple versions overlap, the highest version is selected by the current domain rule.

## Why
The D2 requirement specifically identifies wrong policy version as a major risk. The selection rule is explicit and testable.
