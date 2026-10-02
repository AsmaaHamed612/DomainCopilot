# DomainCopilot project instructions

## Architecture
- Use Clean Architecture.
- Domain and Application must not reference ASP.NET Core, LLM SDKs, vector-store SDKs or provider-specific SDKs.
- Put external integrations behind Application contracts and Infrastructure adapters.

## D2 insurance rules
- Policy selection must be date/version aware.
- Financial calculations must be deterministic C# code, never LLM arithmetic.
- Do not invent policy language or evidence.
- If evidence is insufficient, the eventual system must abstain.

## T5 human review rules
- Consequential decisions require human adjuster approval.
- Review items need assignment, priority, SLA, escalation and auditable decisions.
- Approval/rejection requires a reviewer comment.

## Engineering
- Prefer small, atomic changes.
- Add focused tests for domain rules.
- Use Conventional Commits.
- Do not commit secrets, generated build artifacts, .vs, bin or obj.
