# Tasks: Expandir Cobertura da Busca

**Input**: Design documents from `/specs/007-expand-search-coverage/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: OBRIGATÓRIO (constituição III). Toda user story MUST incluir
tarefas de teste automatizado. Escrever testes que falhem antes da
implementação; só então implementar até ficarem verdes.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- Backend: `backend/WebScraping.Application/`, `backend/WebScraping.Infrastructure/`, `backend/WebScraping.Domain/`, `backend/tests/`
- Frontend: fora de escopo desta feature

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Baseline de testes backend antes das mudanças

- [X] T001 Verify backend test suite is green with `dotnet test` in `backend/` (baseline)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Config, planner, Fake capado, enrich sem `Completed`, discovery assíncrona — bloqueia todas as stories

**⚠️ CRITICAL**: No user story orchestration until this phase is complete

- [X] T002 Add `CoverageSectorSuffixes` (string list/array) and optional `ProviderPageCap` (= 60) to `backend/WebScraping.Application/Options/Options.cs` with safe defaults (`centro,norte,sul,leste,oeste`)
- [X] T003 [P] Create `CoverageSlice` record + `ITextCoveragePlanner` / `TextCoveragePlanner` in `backend/WebScraping.Infrastructure/Lookup/TextCoveragePlanner.cs` (slice 0 = base region/query; following slices append sector to region/query per research R3)
- [X] T004 [P] Cap `FakeBusinessLookupSource.SearchAsync` at 60 results per call and support slice windows (e.g. region suffix `|slice=N` → `Skip(N*60).Take(...)`) in `backend/WebScraping.Infrastructure/Lookup/FakeBusinessLookupSource.cs`
- [X] T005 Refactor `EnrichBusinessesHandler` in `backend/WebScraping.Application/Searches/EnrichAndExport.cs` to expose `EnrichPendingAsync` that processes pending **without** setting `SearchStatus.Completed`; keep a thin wrapper for legacy callers if needed
- [X] T006 Create stub `DiscoverSearchHandler` in `backend/WebScraping.Application/Searches/DiscoverSearchHandler.cs` (inject repos, lookup, planner, enrich) with empty/TODO `HandleAsync` signature ready for stories
- [X] T007 Change `StartSearchHandler` in `backend/WebScraping.Application/Searches/StartSearchHandler.cs` to create `Running` search and `EnqueueAsync` discovery **without** synchronous full lookup; return 201 DTO early (`totalFound` may be 0)
- [X] T008 Wire worker in `backend/WebScraping.Infrastructure/Workers/SearchEnrichmentWorker.cs` (and DI in `backend/WebScraping.Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs`) to run `DiscoverSearchHandler` for queued search ids
- [X] T009 [P] Add unit tests for `TextCoveragePlanner` (base + sectors) in `backend/tests/WebScraping.Infrastructure.Tests/TextCoveragePlannerTests.cs`
- [X] T010 [P] Add unit tests for Fake 60-cap + slice windows in `backend/tests/WebScraping.Infrastructure.Tests/FakeBusinessLookupSourceTests.cs`

**Checkpoint**: Foundation ready — story work can begin

---

## Phase 3: User Story 1 - Atingir limite quando há oferta (Priority: P1) 🎯 MVP

**Goal**: Com Fake/oferta ampla, `maxResults` 100/200 completa com `totalFound` igual ao limite (não travar em ~60).

**Independent Test**: Application/Api harness com Fake: POST L=100 → poll → `totalFound=100`; L=200 → 200.

### Tests for User Story 1 (REQUIRED) ✅

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [X] T011 [P] [US1] Add failing Application tests for multi-slice discovery reaching L=100 and L=200 with Fake in `backend/tests/WebScraping.Application.Tests/DiscoverSearchHandlerTests.cs` (or extend `ApplicationTests.cs`)
- [X] T012 [P] [US1] Add failing ApiTest: POST `maxResults=100` then poll until terminal → `totalFound=100` and `maxResults=100` in `backend/tests/WebScraping.Api.Tests/ApiTests.cs`

### Implementation for User Story 1

- [X] T013 [US1] Implement discovery loop in `DiscoverSearchHandler.cs`: for each slice, call lookup with `min(remaining, ProviderPageCap)`, dedupe against accumulated, insert new businesses, update `TotalFound`, call `EnrichPendingAsync`, continue until L reached
- [X] T014 [US1] Ensure Google path uses planner effective query/region (textQuery variants) in `GooglePlacesBusinessLookupSource.cs` and/or by passing effective region/query from orchestrator into existing `SearchAsync`
- [X] T015 [US1] Register planner in DI (`ServiceCollectionExtensions.cs`) and ensure Fake+planner produce enough distinct items across slices for L=200
- [X] T016 [US1] Run `dotnet test` in `backend/` until T011–T012 pass

**Checkpoint**: MVP — limite alto honrado no harness Fake

---

## Phase 4: User Story 2 - Parar sem novos / oferta menor (Priority: P2)

**Goal**: Parar no primeiro lote sem itens novos; oferta N&lt;L completa com N; fatia 0 vazia → completed sem resultados.

**Independent Test**: Fake catalog truncated / slice returning only duplicates → stop; `__empty__` → total 0 completed.

### Tests for User Story 2 (REQUIRED) ✅

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [X] T017 [P] [US2] Add failing tests: stop when a mid slice adds 0 new distincts; complete with N&lt;L when catalog smaller; empty first slice → completed totalFound=0 in `backend/tests/WebScraping.Application.Tests/DiscoverSearchHandlerTests.cs`

### Implementation for User Story 2

- [X] T018 [US2] Implement stop rules (L reached OR batch with 0 new; first slice empty → Completed) in `DiscoverSearchHandler.cs`
- [X] T019 [US2] Respect `Cancelled` between slices (reload search status) in `DiscoverSearchHandler.cs`
- [X] T020 [US2] Run `dotnet test` in `backend/` until T017 passes

**Checkpoint**: Esgotamento e empty path corretos

---

## Phase 5: User Story 3 - Deduplicação, transparência e falha parcial (Priority: P3)

**Goal**: Sem duplicatas entre fatias; `maxResults` estável no status; falha de fatia → `failed` + mensagem + itens mantidos; enrich intercalado observável.

**Independent Test**: Overlapping slices → unique ids; inject failure on slice 2 → failed with slice-1 items; `processedCount` can rise before discovery finishes.

### Tests for User Story 3 (REQUIRED) ✅

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [X] T021 [P] [US3] Add failing tests for cross-slice dedupe and `maxResults` preserved on summary in `backend/tests/WebScraping.Application.Tests/DiscoverSearchHandlerTests.cs`
- [X] T022 [P] [US3] Add failing test: exception on later slice → status Failed, ErrorMessage set, businesses from earlier slices still listed in `backend/tests/WebScraping.Application.Tests/DiscoverSearchHandlerTests.cs`
- [X] T023 [P] [US3] Add failing test for interleaved enrich: after first batch, pending/enriched progress advances before last slice completes (orchestrator test) in `backend/tests/WebScraping.Application.Tests/DiscoverSearchHandlerTests.cs`

### Implementation for User Story 3

- [X] T024 [US3] Harden cumulative dedupe set across slices in `DiscoverSearchHandler.cs` (reuse StartSearch dedupe key rules)
- [X] T025 [US3] On lookup/orchestrator exception after some inserts: set `Failed` + `ErrorMessage`, persist, **do not** delete businesses; do not set `Completed`
- [X] T026 [US3] Confirm `Completed` only after all slices done successfully and pending enrich finished in `DiscoverSearchHandler.cs`
- [X] T027 [US3] Run `dotnet test` in `backend/` until T021–T023 pass

**Checkpoint**: Dedupe + falha parcial + enrich intercalado

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Regressão, docs, config

- [X] T028 [P] Update existing Application/Api tests that assumed sync discovery completion on POST in `backend/tests/WebScraping.Application.Tests/` and `backend/tests/WebScraping.Api.Tests/` (poll or await discover)
- [X] T029 [P] Document `CoverageSectorSuffixes` in `backend` appsettings sample / README if present (`backend/WebScraping.Api/appsettings.json` or project README)
- [X] T030 Run full `dotnet test` in `backend/` and smoke checklist from `specs/007-expand-search-coverage/quickstart.md`
- [X] T031 [P] Mark quickstart implemented note in `specs/007-expand-search-coverage/quickstart.md` when green

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Sem dependências
- **Foundational (Phase 2)**: Depende do Setup — BLOQUEIA todas as user stories
- **US1 → US2 → US3**: Preferir ordem (mesmo `DiscoverSearchHandler.cs`); US2/US3 estendem o loop do US1
- **Polish**: Após stories desejadas

### User Story Dependencies

- **US1 (P1)**: Após Phase 2 — MVP isolado (atingir L no Fake)
- **US2 (P2)**: Após US1 (regras de parada no mesmo handler)
- **US3 (P3)**: Após US1 (idealmente após US2); falha/dedupe/enrich

### Within Each User Story

- Testes MUST falhar antes da implementação
- Orquestração antes de polish de config
- `dotnet test` verde no checkpoint

### Parallel Opportunities

- T003 ∥ T004 após T002
- T009 ∥ T010 na foundation
- T011 ∥ T012 em US1
- T021 ∥ T022 ∥ T023 em US3
- T028 ∥ T029 ∥ T031 no polish

---

## Parallel Example: User Story 1

```bash
# Testes US1 em paralelo:
Task: "DiscoverSearchHandlerTests L=100/200 in Application.Tests"
Task: "ApiTests poll totalFound=100 in Api.Tests"

# Depois implementação sequencial no orquestrador:
Task: "DiscoverSearchHandler loop + DI + Google effective query"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Phase 1–2: Foundation (Fake 60-cap, planner, async start, enrich pending)
2. Phase 3: US1 — atingir L no harness
3. **STOP and VALIDATE** via Independent Test / quickstart parcial
4. Demo com Fake L=100/200

### Incremental Delivery

1. Foundation → pronta
2. US1 → limites altos
3. US2 → esgotamento limpo
4. US3 → dedupe/falha/enrich intercalado
5. Polish → regressão + docs

### Parallel Team Strategy

Após Phase 2:

- Dev A: DiscoverSearchHandler (US1→US2→US3 sequencial — mesmo arquivo)
- Dev B: Fake/planner tests, ApiTest poll helpers, appsettings — coordenar DI

---

## Notes

- [P] = arquivos diferentes, sem dependência de tarefa incompleta
- [USn] = rastreio à user story da spec
- Frontend fora de escopo; paginação 006 beneficia totals > 60 automaticamente
- Não apagar businesses em `Failed`
- `Completed` ownership no discover orquestrador, não no enrich early-exit
- Commit após cada tarefa ou grupo lógico
