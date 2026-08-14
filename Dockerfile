FROM node:24-alpine AS frontend-build
WORKDIR /source/frontend

COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci

COPY frontend/ ./
RUN npm run build


FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend-build
WORKDIR /source

COPY EvidenceAppraisalTool.sln ./
COPY backend/EvidenceAppraisal.Api/EvidenceAppraisal.Api.csproj backend/EvidenceAppraisal.Api/

RUN dotnet restore backend/EvidenceAppraisal.Api/EvidenceAppraisal.Api.csproj

COPY backend/EvidenceAppraisal.Api/ backend/EvidenceAppraisal.Api/

RUN dotnet publish backend/EvidenceAppraisal.Api/EvidenceAppraisal.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore


FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

RUN apt-get update \
    && apt-get install -y --no-install-recommends fonts-dejavu-core \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY --from=backend-build /app/publish ./
COPY --from=frontend-build /source/frontend/dist ./wwwroot

ENV ASPNETCORE_URLS=http://0.0.0.0:10000
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 10000

ENTRYPOINT ["dotnet", "EvidenceAppraisal.Api.dll"]