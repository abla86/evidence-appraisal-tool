# Evidence Appraisal Tool

Full-stack research prototype for structured and transparent AMSTAR 2 appraisal workflows.

## Features

- Registration of all 16 AMSTAR 2 items
- Prespecified critical domains with required rationale
- Required response, evidence location and researcher rationale
- Separate weakness and critical-flaw judgements
- Researcher-selected overall confidence
- ASP.NET Core API validation
- Word, PDF, Excel and JSON export
- Shared report model with assessment ID and SHA-256 verification
- Responsive React interface
- Automated backend and frontend tests
- GitHub Actions continuous integration

## Methodological safeguards

The application does not calculate a numerical AMSTAR 2 score, infer item responses, determine weaknesses or automatically select overall confidence.

Every judgement must be entered and verified by the researcher against the assessed systematic review, the authorised AMSTAR 2 instrument and relevant guidance.

The exact AMSTAR 2 item wording is not reproduced in the application.

## Export

Validated assessments can be downloaded as:

- Word for further academic work
- PDF for sharing and archiving
- Excel for structured review
- JSON for machine-readable backup

All formats are generated from the same validated assessment.

## Limitations

This research and portfolio prototype currently has no database persistence, authentication, PDF article analysis, automatic article interpretation, multi-reviewer reconciliation or clinical recommendation functionality.

Do not enter personal health information, confidential research data or other sensitive information.

## Technology

Frontend: React, Vite, JavaScript, Vitest, Testing Library and ESLint.

Backend: ASP.NET Core, .NET 9, C#, xUnit, Open XML SDK, PDFsharp and MigraDoc.

## Run locally

Start API from the repository root:

    dotnet run --project .\backend\EvidenceAppraisal.Api\EvidenceAppraisal.Api.csproj

Start the frontend in another PowerShell window:

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

## Reference

Shea, B. J., Reeves, B. C., Wells, G., Thuku, M., Hamel, C., Moran, J., Moher, D., Tugwell, P., Welch, V., Kristjansson, E., & Henry, D. A. (2017). AMSTAR 2: A critical appraisal tool for systematic reviews that include randomised or non-randomised studies of healthcare interventions, or both. BMJ, 358, j4008. https://doi.org/10.1136/bmj.j4008

## Licence

The source code is licensed under the MIT License. This does not grant rights to reproduce third-party appraisal instruments, trademarks or copyrighted methodological content.