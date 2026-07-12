# Implementation Plan: Busca de Comércios no Google Maps

**Branch**: `001-maps-business-lookup` | **Date**: 2026-07-10 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-maps-business-lookup/spec.md`

**Note**: This template is filled in by the `/speckit-plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Aplicação web em dois projetos: **frontend Next.js (JavaScript/React)** para
iniciar buscas, acompanhar progresso, listar comércios e exportar CSV; **backend
ASP.NET Core (C#)** expondo apenas API REST e persistência. Dados de busca e
comércios ficam em **MongoDB (NoSQL)**. A coleta de Nome, Telefone, Site e
Avaliação usa a **Google Places API (New)** atrás de uma abstração injetável,
com job em background para percorrer itens, atualizar progresso e permitir
cancelamento.

## Technical Context

**Language/Version**: C# / .NET 8 (backend); JavaScript (ES2022+) / Node 20+ (frontend)

**Primary Dependencies**:
- Backend: ASP.NET Core Web API, MongoDB.Driver, Google Places API (HTTP)
- Frontend: Next.js (App Router), React

**Storage**: MongoDB (documentos `searches` e `businesses`)

**Testing**:
- Backend: xUnit + FluentAssertions + Testcontainers (Mongo) ou Mongo em memória/fake
- Frontend: Vitest + Testing Library; testes de contrato HTTP com mocks da API

**Target Platform**: Web local/dev (Windows/Linux); API + UI em processos separados

**Project Type**: Web application (frontend + backend API)

**Performance Goals**:
- Primeiros itens da listagem visíveis em até 2 minutos (SC-001)
- Progresso da coleta atualizável via polling a cada ~2s

**Constraints**:
- Backend restrito a API + integração com banco (sem UI no .NET)
- Frontend não acessa Mongo nem Google diretamente — só a API
- Sem autenticação de usuário na v1
- Limite máximo configurável de comércios por busca
- Uso responsável da Places API (quota/chave em configuração)

**Scale/Scope**: Operador único local; dezenas a poucas centenas de comércios por execução

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Clareza**: API com recursos `searches` / `businesses`; pastas Domain /
  Application / Infrastructure com nomes óbvios — PASS
- **Simplicidade enxuta**: Dois projetos (exigência do usuário); um único
  projeto .NET (sem solution com N camadas); polling em vez de SignalR;
  Places API em vez de browser scraping — PASS (ver Complexity Tracking)
- **Testes automatizados**: xUnit no backend (domínio + API com fakes);
  Vitest no frontend; contrato OpenAPI como referência — PASS
- **Responsabilidade única**: Frontend = UI; API = orquestração; Infrastructure =
  Mongo + Google Places; domínio sem I/O direto — PASS
- **Design testável**: `IBusinessLookupSource`, `ISearchRepository` injetados;
  config (connection string, API key, limites) em `appsettings` / env — PASS

**Post-design re-check**: PASS — contratos e modelo alinhados; sem camadas extras.

## Project Structure

### Documentation (this feature)

```text
specs/001-maps-business-lookup/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── rest-api.md
└── tasks.md
```

### Source Code (repository root)

```text
backend/
├── WebScraping.Api/
│   ├── Program.cs
│   ├── Endpoints/
│   ├── appsettings.json
│   └── WebScraping.Api.csproj
├── WebScraping.Application/
├── WebScraping.Domain/
├── WebScraping.Infrastructure/
└── tests/
    ├── WebScraping.Domain.Tests/
    ├── WebScraping.Application.Tests/
    └── WebScraping.Api.Tests/

frontend/
├── app/
├── components/
├── lib/
├── public/
├── package.json
└── tests/
```

**Structure Decision**: Opção web frontend + backend. Backend em poucos projetos
C# (Api / Application / Domain / Infrastructure) para DI e testes sem
sobreengenharia. Frontend Next.js consome apenas a API REST.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Dois projetos (FE Next + BE .NET) | Exigência explícita do usuário | Monólito único não atende a divisão pedida |
| 4 projetos C# (Api/App/Domain/Infra) | Separar I/O (Mongo/Places) da lógica e permitir testes com fakes (constituição IV/V) | Um único .csproj mistura persistência/HTTP externo com domínio e dificulta mocks |
| Worker em background na API | Coleta longa, progresso e cancelamento (FR-007, FR-011) sem estourar timeout HTTP | Request síncrono único bloqueia e impede cancelamento útil |
| Abstração `IBusinessLookupSource` | Trocar/mockar Google Places nos testes | Chamada direta à Google nos controllers acopla e impede testes rápidos |
