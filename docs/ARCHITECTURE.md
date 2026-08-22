# Architecture

## System overview

Evidence Appraisal Tool is a full-stack research application designed to make critical appraisal workflows structured, traceable and auditable without replacing researcher judgement.

```text
Researcher
   |
   v
React + Vite
   |
   | REST / JSON
   v
ASP.NET Core API (.NET 9)
   |
   +--> appraisal workflow validation
   +--> AMSTAR 2 / CASP / AGREE II / GRADE domain logic
   +--> export services
   +--> verification/integrity services
   |
   +--> Word / PDF / Excel / JSON export
```

## Methodological boundary

The application validates structure and documentation. It does not read articles, infer answers, determine research quality automatically or replace methodological judgement.

Researcher rationale and evidence location are first-class data rather than optional explanatory text.

## Frontend

React and Vite provide:

- checklist workflows
- structured input
- validation
- appraisal result presentation
- export initiation

The current frontend is JavaScript-based; TypeScript is not claimed as implemented in this repository.

## Backend

ASP.NET Core provides:

- REST endpoints
- validation
- appraisal workflow logic
- export generation
- integrity verification

The backend currently has no application database persistence. Assessments are processed within the application's current workflow/export model.

## Security and integrity

The repository currently demonstrates:

- CodeQL
- Dependabot
- least-privilege GitHub Actions permissions
- local secret exclusion
- SHA-256 verification for exported AMSTAR 2 reports

The application does not currently claim:

- enterprise authentication
- encrypted research workspace
- production identity management
- persistent multi-user research storage

## AI boundary

AI-assisted development may be used to develop the software, but the application does not currently claim automatic article interpretation or autonomous appraisal judgement.

Any future AI functionality should preserve:

- human verification
- provenance
- uncertainty
- auditability
- methodological boundaries

## Current architecture gaps

Not currently implemented:

- persistent database-backed research workspace
- OAuth2/OIDC/Entra ID
- multi-reviewer reconciliation interface
- PDF/article content analysis
- production cloud deployment
- Kubernetes
- Infrastructure as Code
- OpenTelemetry
- dedicated E2E browser test suite
