# Evidence Appraisal Tool

[![CI](https://github.com/abla86/evidence-appraisal-tool/actions/workflows/ci.yml/badge.svg)](https://github.com/abla86/evidence-appraisal-tool/actions/workflows/ci.yml)
[![CodeQL](https://github.com/abla86/evidence-appraisal-tool/actions/workflows/codeql.yml/badge.svg)](https://github.com/abla86/evidence-appraisal-tool/actions/workflows/codeql.yml)

## Live demo

[Open Evidence Appraisal Tool](https://evidence-appraisal-tool.onrender.com)

The free Render service may require approximately 50 seconds to start after inactivity.

A full-stack research application for transparent, structured and traceable critical appraisal. The application supports separate workflows for AMSTAR 2, CASP, AGREE II and GRADE.

## Research modules

### AMSTAR 2

- registration of all 16 items
- prespecified critical domains with required rationale
- required response, evidence location and researcher rationale
- separate weakness and critical-flaw judgements
- researcher-confirmed overall confidence
- Word, PDF, Excel and JSON export with SHA-256 verification
- no prohibited numerical total score

### CASP

- works with a researcher-selected, authorised, design-specific CASP checklist
- records checklist title, version and official source URL
- records Yes, No or Cannot tell for every checklist item
- requires rationale and evidence location for every judgement
- validates checklist completeness without inventing a numerical quality score

### AGREE II

- records all 23 item ratings on the official 1–7 scale
- requires item-level rationale and evidence location
- calculates the six standardised domain scores using the AGREE II formula
- records overall quality and recommendation separately
- supports pseudonymous appraiser identifiers
- flags whether the minimum of two independent appraisals has been reached
- does not create an unsupported aggregate quality score

### GRADE

- evaluates certainty separately for each important or critical outcome
- records PICO, effect measure, relative and absolute effects, studies and participants
- documents the five downgrade domains
- documents the three upgrade domains
- requires rationale and evidence location for every domain judgement
- calculates a bounded provisional certainty category
- keeps provisional calculation separate from researcher-confirmed certainty

## Methodological safeguards

The application validates structure and documentation. It does not read articles, infer answers, determine research quality automatically or replace the researcher’s methodological judgement.

Official appraisal wording is not reproduced. Researchers must use the authorised instrument and current official guidance alongside the application.

GRADE certainty of evidence is distinct from the strength of a recommendation. Evidence-to-Decision and recommendation development are not implemented in this release.

## Export

AMSTAR 2 assessments can currently be exported as Word, PDF, Excel and JSON. Export for CASP, AGREE II and GRADE is the next persistence/export layer and is not claimed as available in this release.

## Security and DevSecOps

- CodeQL analysis for C# and JavaScript/TypeScript
- Dependabot monitoring for NuGet, npm and GitHub Actions
- automated backend build and tests
- automated frontend tests, lint and production build
- least-privilege GitHub Actions permissions
- local secrets excluded through `.gitignore`
- published security policy
- SHA-256 verification for exported AMSTAR 2 reports

See [SECURITY.md](SECURITY.md) for responsible vulnerability reporting.

## Current limitations

The application currently has no database persistence, authentication, encrypted research workspace, PDF article analysis, automatic article interpretation, multi-reviewer reconciliation interface or clinical recommendation functionality.

Do not enter personal health information, confidential research data or directly identifying information. Use pseudonymous reviewer codes.

## Technology

Frontend: React, Vite, JavaScript, Vitest, Testing Library and ESLint.

Backend: ASP.NET Core, .NET 9, C#, xUnit, Open XML SDK, PDFsharp and MigraDoc.

## Run locally

Backend:

    dotnet run --project .\backend\EvidenceAppraisal.Api\EvidenceAppraisal.Api.csproj

Frontend:

    Set-Location .\frontend
    npm install
    npm run dev

Frontend: http://localhost:5173

API: http://localhost:5237

## Verification

Backend:

    dotnet build EvidenceAppraisalTool.sln
    dotnet test EvidenceAppraisalTool.sln --no-build

Frontend:

    Set-Location .\frontend
    npm ci
    npm test
    npm run lint
    npm run build

## Methodological sources

AGREE Next Steps Consortium. (2017). *The AGREE II instrument* (electronic version). https://www.agreetrust.org/resource-centre/agree-ii/

Critical Appraisal Skills Programme. (2024). *CASP checklists*. https://casp-uk.net/casp-tools-checklists/

GRADE Working Group. (n.d.). *GRADE*. https://www.gradeworkinggroup.org/

Shea, B. J., Reeves, B. C., Wells, G., Thuku, M., Hamel, C., Moran, J., Moher, D., Tugwell, P., Welch, V., Kristjansson, E., & Henry, D. A. (2017). AMSTAR 2: A critical appraisal tool for systematic reviews that include randomised or non-randomised studies of healthcare interventions, or both. *BMJ, 358*, j4008. https://doi.org/10.1136/bmj.j4008

## Licence

The source code is licensed under the MIT License. This does not grant rights to reproduce third-party appraisal instruments, trademarks or copyrighted methodological content.
