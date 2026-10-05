# Security Assessment — Current MVP

**Assessment date:** 2026-10-05

**Scope:** Source and configuration currently present in this repository. This is a code-based assessment, not a penetration test or a deployment certification.

## Data and trust boundaries

- The demo policy and golden-set claims are synthetic. Do not put real claimant personal data in this repository.
- The API accepts claim descriptions and stores them in the configured MySQL database. Those values remain in the database host; no LLM or external document service is integrated, so the current workflow sends no claim data to an external model provider.
- The API trusts the `X-API-Key` header only to establish one of two application roles. It does not provide user accounts, per-user keys, tenant isolation, or key rotation.
- Database credentials are supplied through .NET configuration and should be overridden with the `ConnectionStrings__DomainCopilot` environment variable. The checked-in `appsettings.json` currently contains a local `root` account with an empty password; treat that as a development-only default and replace it before running against any non-disposable database. A least-privilege database account is not yet provisioned by the project.

## Implemented controls

| Threat | Current control | Limit |
|---|---|---|
| Broken access control | Server-side `Adjuster` and `Reviewer` policies protect claim, queue, audit, and run routes. Reviewer identity for start/decision actions comes from the authenticated API key. The review service checks assignment before a reviewer can act. | Two static shared keys are the only identities. Review-queue listing and audit/run reads are role-gated but do not implement tenant or object-owner scoping. No HTTP-level authorization test suite is present. |
| API-key comparison | Configured keys are compared using `CryptographicOperations.FixedTimeEquals`. | This does not provide key rotation, revocation, per-user accountability, or secure secret storage by itself. |
| Financial manipulation | Coverage limits, deductibles, and payout are handled by deterministic domain code; recommendations are routed through the human approval queue. | No LLM is connected. This control has only unit/golden-case coverage for the current deterministic workflow. |
| SQL injection | Data access uses Entity Framework Core repositories and LINQ. No raw SQL execution was found in the reviewed source. | Upload validation and document parsing do not exist yet. |
| Sensitive error disclosure | API exception handling returns a generic problem response instead of exception details. | Console logging is enabled; a deployment logging policy, retention policy, and secret redaction tests are not configured. |
| Request correlation | A validated or generated correlation ID is returned in `X-Correlation-ID` and persisted with an agent run. | The ID is not yet attached to a structured logging scope across every dependency. |
| Prompt injection / excessive agency | No model, prompt execution, or external tool execution exists in this MVP. Exclusion text is not interpreted; any structured exclusion causes human referral. | The adversarial cases exercise deterministic exclusion handling only. They do not prove resistance to direct or indirect prompt injection in an LLM system. |

## Required before an internet-facing deployment

These controls are **not implemented or verified** and must be addressed before public deployment:

1. Replace the shared API keys with a managed identity or per-user credentials, add rotation/revocation, and enforce object ownership for every read and write.
2. Remove the empty-password `root` database default; provision a least-privilege account and keep credentials in a secrets manager or environment configuration.
3. Add rate limits, payload/request-size limits, abuse controls, HTTPS/HSTS policy, secure headers, and an explicit CORS policy.
4. Add audit/security events for authentication failures and reviewer actions, and prove logs exclude API keys, database credentials, and claimant-sensitive fields.
5. Add dependency and full-history secret scanning in CI. Neither scanner is configured in the current workflow, and no full-history scan is claimed here.
6. Add API integration tests for unauthenticated access, role separation, assigned-reviewer ownership, malformed input, and secret-safe errors.
7. Before adding an LLM or document ingestion, add PII detection/redaction, document sanitization, strict separation of instructions from retrieved content, per-agent tool allow-lists, schema validation, token/iteration/time limits, and at least three measured indirect-injection cases.

## Verification record

- Source/configuration reviewed on 2026-10-05.
- The solution build completed with 0 warnings and 0 errors; 20 domain/application tests passed in the local test run before this report was written.
- MySQL was reachable on `127.0.0.1:3306`, but this verification shell had no database connection string, so live migration/startup behavior was not verified in that run.
- No penetration test, secret scan, dependency scan, or production deployment test has been performed.

