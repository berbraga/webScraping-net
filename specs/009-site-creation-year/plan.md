# Implementation Plan: Ano de Criação via Copyright do Site

**Branch**: `009-site-creation-year` | **Date**: 2026-07-13 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/009-site-creation-year/spec.md`

**Note**: This template is filled in by the `/speckit-plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Após o enrich Places de **todos** os comércios, executar uma **segunda fase** que baixa o HTML de cada URL de site distinta (paralelismo ≤ ~10, timeout ~10s), extrai o **menor ano 19xx/20xx** do rodapé/final da página (marcador © opcional), grava `SiteCreationYear` / `siteCreationYear`, e só então marca a busca `Completed`. Falhas de acesso → `null` sem abortar. Expor coluna **Criação do site** (à direita de Site) e no CSV. Sem WHOIS/RDAP.

## Technical Context

**Language/Version**: C# / .NET 8 (backend); JavaScript (ES2022+) / Node 20+ (frontend)

**Primary Dependencies**:
- Backend: ASP.NET Core, MongoDB.Driver, Google Places (existente), `HttpClient` para GET do site do comércio
- Frontend: Next.js / React (`BusinessList`)
- Sem pacote WHOIS/RDAP; parser de ano em domínio puro (regex sobre HTML/texto)

**Storage**: MongoDB — campo opcional `siteCreationYear` (`int?`) em `businesses`

**Testing**:
- Domain: extractor de anos (intervalos, footer, inválidos)
- Application: fase pós-Places (cache URL, paralelismo, falha contida, Completed só depois)
- Api: `siteCreationYear` + CSV
- Frontend: coluna **Criação do site**

**Target Platform**: Web local/dev (Windows/Linux)

**Project Type**: Web application (frontend + backend API)

**Performance Goals**:
- ≤ ~10s por URL distinta; ≤ ~10 leituras simultâneas; cache por URL na busca (SC-003, FR-007/012/014)

**Constraints**:
- Fonte = HTML do site; sem registro de domínio (FR-011)
- Completed somente após fase de anos (FR-001)
- Falha de site ≠ `FailedCount` / `DetailStatus.Failed` de Places (FR-006)

**Scale/Scope**: Operador local; dezenas–centenas de URLs por busca

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Clareza**: Port `IWebsiteCopyrightYearLookup`, extractor puro, handler de fase — PASS
- **Simplicidade enxuta**: HttpClient + regex/helper; sem browser; sem camada nova de projeto — PASS
- **Testes automatizados**: Domain / Application / Api / Frontend por user story — PASS
- **Responsabilidade única**: Places ≠ copyright fetch; parser ≠ HTTP — PASS
- **Design testável**: Port + Fake; options de timeout/concurrency — PASS

**Post-design re-check**: PASS — contratos e modelo alinhados; Complexity Tracking vazio de violações.

## Project Structure

### Documentation (this feature)

```text
specs/009-site-creation-year/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── rest-site-creation-year.md
│   └── ui-site-creation-year.md
└── tasks.md              # (/speckit-tasks — não criado aqui)
```

### Source Code (repository root)

```text
backend/
├── WebScraping.Domain/
│   ├── Entities/Business.cs                 # + SiteCreationYear
│   ├── Abstractions/Abstractions.cs              # + IWebsiteCopyrightYearLookup
│   └── Services/CopyrightYearExtractor.cs   # HTML/texto → int?
├── WebScraping.Application/
│   ├── Searches/DiscoverSearchHandler.cs    # fase anos antes de Completed
│   ├── Searches/EnrichSiteCreationYearsHandler.cs
│   ├── Searches/EnrichAndExport.cs          # CSV
│   └── Options/Options.cs                   # WebsiteCopyrightOptions
├── WebScraping.Infrastructure/
│   ├── Lookup/HttpWebsiteCopyrightYearLookup.cs
│   ├── Lookup/FakeWebsiteCopyrightYearLookup.cs
│   ├── Persistence/MongoRepositories.cs
│   └── DependencyInjection/ServiceCollectionExtensions.cs
├── WebScraping.Api/Endpoints/SearchesEndpoints.cs
└── tests/...

frontend/
├── components/BusinessList.jsx
└── tests/
```

**Structure Decision**: Estender Api/Application/Domain/Infrastructure + Next.js existentes.

## Complexity Tracking

> Sem violações. `IWebsiteCopyrightYearLookup` segue o mesmo padrão de `IBusinessLookupSource`.
