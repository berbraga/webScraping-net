# Implementation Plan: Respeitar Limite Máximo de Resultados

**Branch**: `003-respect-max-results` | **Date**: 2026-07-12 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/003-respect-max-results/spec.md`

## Summary

O campo “Limite máximo” já é validado e persistido, mas a fonte Google Places (New) Text Search faz **uma única página** com `pageSize` limitado a 20 — daí o sintoma “pedi 100, veio 20”.

**Abordagem**: implementar paginação em `GooglePlacesBusinessLookupSource.SearchAsync` (`pageToken` / `nextPageToken`, field mask incluindo `nextPageToken`) até atingir `maxResults` ou esgotar páginas do provedor. Contrato REST e frontend permanecem iguais. Testes cobrem acumulação multi-página e esgotamento antecipado.

## Technical Context

**Language/Version**: C# / .NET 8 (backend); frontend sem mudanças nesta feature

**Primary Dependencies**: ASP.NET Core Minimal APIs, `HttpClient` + Places API (New) `places:searchText`, MongoDB (inalterado)

**Storage**: MongoDB / InMemory — modelo de Search/Business inalterado

**Testing**: xUnit + FluentAssertions (Application/Infrastructure/Api); testes unitários da lógica de paginação com `HttpMessageHandler` fake ou wrapper testável

**Target Platform**: API local Windows/Linux (`localhost:5080`)

**Project Type**: Web application (backend + frontend existente)

**Performance Goals**: Coleta inicial de até ~100 listings em tempo aceitável para uso interativo (segundos, não minutos); paginação sequencial é suficiente

**Constraints**: Places API `pageSize` máximo = 20 por request; teto absoluto do produto = 200; provedor pode esgotar páginas antes do limite do usuário (comportamento esperado FR-003)

**Scale/Scope**: 1 componente principal (`GooglePlacesBusinessLookupSource`), testes associados; sem mudança de schema UI/API

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Clareza**: Paginação fica encapsulada no lookup Google; handlers/API não mudam de responsabilidade. ✅
- **Simplicidade enxuta**: Correção local no adapter existente; sem novas camadas/projetos. ✅
- **Testes automatizados**: Testes para multi-página até `maxResults`, parada sem `nextPageToken`, e Fake já respeitando limite (regressão). ✅
- **Responsabilidade única**: I/O HTTP + paginação no Infrastructure; Domain/Application intactos. ✅
- **Design testável**: Lookup continua atrás de `IBusinessLookupSource`; HTTP mockável. ✅

*Post-design re-check*: sem violações; Complexity Tracking vazio.

## Project Structure

### Documentation (this feature)

```text
specs/003-respect-max-results/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── rest-behavior.md
└── tasks.md              # /speckit-tasks (ainda não)
```

### Source Code (repository root)

```text
backend/
├── WebScraping.Domain/           # inalterado (contratos já passam maxResults)
├── WebScraping.Application/      # inalterado (StartSearchHandler já propaga maxResults)
├── WebScraping.Infrastructure/
│   └── Lookup/
│       ├── GooglePlacesBusinessLookupSource.cs   # FIX: paginação
│       └── FakeBusinessLookupSource.cs           # já respeita maxResults (regressão)
├── WebScraping.Api/              # inalterado
└── tests/
    ├── WebScraping.Application.Tests/
    ├── WebScraping.Api.Tests/
    └── (novo ou estendido) testes do lookup Google com HTTP fake

frontend/                         # sem mudanças (já envia maxResults)
```

**Structure Decision**: Manter a solução multi-projeto existente; alterar apenas o adapter Google Places e testes. Sem mudança de frontend/contrato HTTP.

## Complexity Tracking

> Nenhuma violação da constituição nesta feature.
