# Implementation Plan: Expandir Cobertura da Busca

**Branch**: `007-expand-search-coverage` | **Date**: 2026-07-13 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/007-expand-search-coverage/spec.md`

## Summary

Honrar limites altos (ex.: 200) quando houver oferta: a Places Text Search limita ~60 por consulta; a solução é **cobertura ampliada** — várias descobertas (fatias) com deduplicação, parando em L ou no primeiro lote sem itens novos. Atualizar `totalFound` a cada lote; enriquecer cada lote assim que descoberto; em falha de fatia → status `failed` mantendo itens.

**Abordagem**: orquestrador de descoberta em **background** (fila existente); plano de fatias (v1: variantes de `textQuery` por setor configurável; Google mantém paginação ≤60/fatia); Fake capado em 60/chamada com fatias deslocadas no catálogo; enriquecer pending por lote sem marcar `completed` até o fim da orquestração.

## Technical Context

**Language/Version**: C# / .NET 8 (backend); frontend inalterado nesta feature

**Primary Dependencies**: ASP.NET Core, `IBusinessLookupSource` (Google Places Text Search + Fake), `ISearchJobQueue` / worker in-process

**Storage**: MongoDB / InMemory (entidades Search/Business existentes)

**Testing**: xUnit + FluentAssertions (Application + Infrastructure + Api harness)

**Target Platform**: Backend API (`localhost:5080`)

**Project Type**: Web application — foco **backend/application**; UI só se beneficia de totals > 60 (paginação 006)

**Performance Goals**: Aceitar maior latência/custo (múltiplas consultas); progresso observável via GET status durante Running

**Constraints**: AbsoluteMaxResults=200; parar em L ou lote sem novos; falha parcial mantém itens; sem novos campos de UI; Fake deve simular >60

**Scale/Scope**: Orquestrador + plano de cobertura + ajuste Fake/Google + testes; possível extensão mínima da abstração de lookup (parâmetro de fatia / query efetiva)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Clareza**: Orquestrador e plano de fatias com nomes explícitos. ✅
- **Simplicidade enxuta**: v1 sem Geocoding/grid pesado — variantes de texto por setor + paginação já existente; Geocode+viewport justificado só se v1 falhar. ✅ (ver Complexity se geocode entrar)
- **Testes automatizados**: Harness Fake multi-lote + unitários de parada/dedupe/falha. ✅
- **Responsabilidade única**: Plano de fatias ≠ HTTP Places ≠ enriquecimento. ✅
- **Design testável**: Fake + planner injetáveis; orquestrador testável sem Google real. ✅

*Post-design*: Complexity Tracking abaixo para mudança de ownership de `completed` e discovery assíncrona.

## Project Structure

### Documentation (this feature)

```text
specs/007-expand-search-coverage/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── discovery-coverage.md
└── tasks.md              # /speckit-tasks
```

### Source Code (repository root)

```text
backend/
├── WebScraping.Domain/Abstractions/
│   └── Abstractions.cs                    # opcional: SearchAsync com CoverageSlice / EffectiveQuery
├── WebScraping.Application/
│   ├── Options/Options.cs            # CoverageSectorSuffixes (config)
│   └── Searches/
│       ├── StartSearchHandler.cs     # cria Running + enqueue discovery (não descobre sync)
│       ├── DiscoverSearchHandler.cs  # NOVO: loop fatias → insert → totalFound → enrich lote
│       └── EnrichAndExport.cs        # EnrichPendingOnly sem completed; completed no discover
├── WebScraping.Infrastructure/
│   ├── Lookup/
│   │   ├── GooglePlacesBusinessLookupSource.cs  # textQuery por fatia (e paginação)
│   │   ├── FakeBusinessLookupSource.cs          # cap 60/call + janelas por fatia
│   │   └── TextCoveragePlanner.cs               # NOVO: gera fatias
│   └── Workers/SearchEnrichmentWorker.cs        # despacha discovery (ou dual handler)
└── tests/
    ├── WebScraping.Application.Tests/
    ├── WebScraping.Infrastructure.Tests/
    └── WebScraping.Api.Tests/          # poll até completed com Fake >60
```

**Structure Decision**: Lógica de cobertura na Application; planner + adapters na Infrastructure; frontend fora de escopo.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|--------------------------------------|
| Discovery assíncrona + ownership de `Completed` no orquestrador | FR-012/013 exigem total crescente e enrich intercalado; enrich atual marca Completed ao esvaziar pending e correria com novos lotes | Manter discovery sync no POST — status incremental não é observável; enrich early Completed quebra lotes seguintes |
| Cap Fake em 60/chamada + fatias | Sem isso Fake já devolve 200 numa call e não exercita cobertura | Só testar Google real — frágil no CI |
