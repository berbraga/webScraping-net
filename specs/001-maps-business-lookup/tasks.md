---
description: "Task list for maps business lookup feature"
---

# Tasks: Busca de Comércios no Google Maps

**Input**: Design documents from `/specs/001-maps-business-lookup/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: OBRIGATÓRIO (constituição III). Toda user story MUST incluir
tarefas de teste automatizado. Escrever testes que falhem antes da
implementação; só então implementar até ficarem verdes.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- Backend: `backend/WebScraping.*` e `backend/tests/`
- Frontend: `frontend/app/`, `frontend/components/`, `frontend/lib/`, `frontend/tests/`

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Inicializar solution .NET, app Next.js e tooling de testes

- [X] T001 Create backend solution and projects per plan in `backend/WebScraping.sln` (Api, Application, Domain, Infrastructure + test projects)
- [X] T002 [P] Add NuGet packages (ASP.NET Core, MongoDB.Driver, xUnit, FluentAssertions, Microsoft.AspNetCore.Mvc.Testing) to projects under `backend/`
- [X] T003 [P] Scaffold Next.js (JavaScript, App Router) app in `frontend/` with `package.json`, `app/layout.js`, `app/page.js`
- [X] T004 [P] Configure Vitest + Testing Library in `frontend/vitest.config.js` and `frontend/package.json` scripts
- [X] T005 [P] Add backend config placeholders in `backend/WebScraping.Api/appsettings.json` and `backend/WebScraping.Api/appsettings.Development.json` (Mongo URI, DB name, Google Places key, default/max results)
- [X] T006 [P] Add frontend env example in `frontend/.env.example` with `NEXT_PUBLIC_API_BASE_URL`
- [X] T007 [P] Create root `.gitignore` entries for `node_modules/`, `bin/`, `obj/`, `.env`, `.next/`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Domínio compartilhado, persistência Mongo, DI, health e CORS — bloqueia todas as user stories

**⚠️ CRITICAL**: Nenhuma user story começa antes desta fase

- [X] T008 [P] Define domain entities `Search` and `Business` plus enums `SearchStatus` and `DetailStatus` in `backend/WebScraping.Domain/Entities/`
- [X] T009 [P] Define repository and source interfaces `ISearchRepository`, `IBusinessRepository`, `IBusinessLookupSource` in `backend/WebScraping.Domain/Abstractions/`
- [X] T010 [P] Implement MongoDB document mappings and repositories in `backend/WebScraping.Infrastructure/Persistence/`
- [X] T011 [P] Implement `FakeBusinessLookupSource` for tests in `backend/WebScraping.Infrastructure/Lookup/FakeBusinessLookupSource.cs`
- [X] T012 Wire DI, Mongo client, CORS for Next.js origin, and `GET /api/health` in `backend/WebScraping.Api/Program.cs`
- [X] T013 Create API HTTP client helper in `frontend/lib/apiClient.js`
- [X] T014 Ensure indexes `{ searchId: 1 }` and unique partial `{ searchId, externalId }` are created on startup in `backend/WebScraping.Infrastructure/Persistence/MongoIndexInitializer.cs`

**Checkpoint**: API sobe, health responde, Mongo conecta, fakes registráveis nos testes

---

## Phase 3: User Story 1 - Buscar e listar comércios da região (Priority: P1) 🎯 MVP

**Goal**: Usuário informa região + termo (+ limite), inicia busca e vê lista com pelo menos o nome de cada comércio

**Independent Test**: `POST /api/searches` com fake source → `GET .../businesses` retorna nomes; UI mostra lista ou mensagem de vazio

### Tests for User Story 1 (REQUIRED) ✅

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [X] T015 [P] [US1] Unit test Search validation (empty region/query, maxResults bounds) in `backend/tests/WebScraping.Domain.Tests/SearchValidationTests.cs`
- [X] T016 [P] [US1] Application test StartSearch creates pending/running search and persists discovered businesses (fake source) in `backend/tests/WebScraping.Application.Tests/StartSearchTests.cs`
- [X] T017 [P] [US1] API test POST `/api/searches` and GET `/api/searches/{id}/businesses` in `backend/tests/WebScraping.Api.Tests/SearchesEndpointTests.cs`
- [X] T018 [P] [US1] Frontend test search form submits region/query/maxResults in `frontend/tests/SearchForm.test.jsx`
- [X] T019 [P] [US1] Frontend test business list renders names and empty state in `frontend/tests/BusinessList.test.jsx`

### Implementation for User Story 1

- [X] T020 [P] [US1] Implement `StartSearchCommand` / handler in `backend/WebScraping.Application/Searches/StartSearchHandler.cs`
- [X] T021 [P] [US1] Implement Google Places text search adapter (list by region+query) in `backend/WebScraping.Infrastructure/Lookup/GooglePlacesBusinessLookupSource.cs` (name + externalId at minimum)
- [X] T022 [US1] Map POST `/api/searches` and GET `/api/searches/{id}` and GET `/api/searches/{id}/businesses` in `backend/WebScraping.Api/Endpoints/SearchesEndpoints.cs`
- [X] T023 [P] [US1] Add API functions `createSearch`, `getSearch`, `listBusinesses` in `frontend/lib/searchesApi.js`
- [X] T024 [US1] Build search form UI in `frontend/components/SearchForm.jsx`
- [X] T025 [US1] Build business list UI (name-first) in `frontend/components/BusinessList.jsx`
- [X] T026 [US1] Wire page flow create → poll status → show list/empty message in `frontend/app/page.js`

**Checkpoint**: MVP — busca + listagem de nomes funciona com fake ou Places key

---

## Phase 4: User Story 2 - Extrair detalhes de cada comércio (Priority: P1)

**Goal**: Percorrer cada comércio e preencher Telefone, Site e Avaliação; mostrar progresso; permitir cancelar

**Independent Test**: Com busca seedada e fake source, worker enriquece itens; progresso sobe; cancel preserva parciais; campos null mostram ausência

### Tests for User Story 2 (REQUIRED) ✅

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [X] T027 [P] [US2] Unit/application test enrichment updates phone/website/rating and nulls for missing fields in `backend/tests/WebScraping.Application.Tests/EnrichBusinessesTests.cs`
- [X] T028 [P] [US2] Application test cancel sets status cancelled and skips remaining pending items in `backend/tests/WebScraping.Application.Tests/CancelSearchTests.cs`
- [X] T029 [P] [US2] API test POST `/api/searches/{id}/cancel` and progress fields on GET `/api/searches/{id}` in `backend/tests/WebScraping.Api.Tests/SearchProgressCancelTests.cs`
- [X] T030 [P] [US2] Frontend test progress indicator (processed/total) in `frontend/tests/SearchProgress.test.jsx`
- [X] T031 [P] [US2] Frontend test missing fields render explicit unavailable marker in `frontend/tests/BusinessList.missingFields.test.jsx`

### Implementation for User Story 2

- [X] T032 [P] [US2] Extend Places adapter with Place Details (phone, website, rating) in `backend/WebScraping.Infrastructure/Lookup/GooglePlacesBusinessLookupSource.cs`
- [X] T033 [US2] Implement background worker + in-process queue to enrich businesses and update counters in `backend/WebScraping.Infrastructure/Workers/SearchEnrichmentWorker.cs`
- [X] T034 [P] [US2] Implement `CancelSearchHandler` in `backend/WebScraping.Application/Searches/CancelSearchHandler.cs`
- [X] T035 [US2] Map POST `/api/searches/{id}/cancel` in `backend/WebScraping.Api/Endpoints/SearchesEndpoints.cs`
- [X] T036 [P] [US2] Add `cancelSearch` in `frontend/lib/searchesApi.js`
- [X] T037 [US2] Build progress + cancel controls in `frontend/components/SearchProgress.jsx`
- [X] T038 [US2] Show phone/website/rating with X/empty for nulls in `frontend/components/BusinessList.jsx`
- [X] T039 [US2] Integrate polling (~2s) and cancel into `frontend/app/page.js`

**Checkpoint**: US1 + US2 — detalhes, progresso e cancelamento funcionando

---

## Phase 5: User Story 3 - Exportar resultados (Priority: P2)

**Goal**: Exportar CSV com colunas Nome, Telefone, Site, Avaliação

**Independent Test**: Após coleta com ≥1 item, `GET .../export` baixa CSV válido; UI dispara download

### Tests for User Story 3 (REQUIRED) ✅

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [X] T040 [P] [US3] Unit test CSV formatter escapes quotes/commas and empty optional fields in `backend/tests/WebScraping.Application.Tests/CsvExportTests.cs`
- [X] T041 [P] [US3] API test GET `/api/searches/{id}/export` returns `text/csv` with correct header in `backend/tests/WebScraping.Api.Tests/ExportEndpointTests.cs`
- [X] T042 [P] [US3] Frontend test export button triggers download URL in `frontend/tests/ExportButton.test.jsx`

### Implementation for User Story 3

- [X] T043 [P] [US3] Implement CSV export service in `backend/WebScraping.Application/Searches/ExportSearchCsvHandler.cs`
- [X] T044 [US3] Map GET `/api/searches/{id}/export` in `backend/WebScraping.Api/Endpoints/SearchesEndpoints.cs`
- [X] T045 [P] [US3] Add `exportSearchCsv` helper in `frontend/lib/searchesApi.js`
- [X] T046 [US3] Add export button component in `frontend/components/ExportButton.jsx` and wire in `frontend/app/page.js`

**Checkpoint**: US1–US3 — fluxo completo incluindo exportação

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Hardening, docs e validação quickstart

- [X] T047 [P] Add problem-details error responses consistently in `backend/WebScraping.Api/Endpoints/SearchesEndpoints.cs`
- [X] T048 [P] Document run steps aligning with `specs/001-maps-business-lookup/quickstart.md` in root `README.md`
- [X] T049 [P] Add docker-compose for Mongo only in `docker-compose.yml`
- [X] T050 Run full `dotnet test` under `backend/` and `npm test` under `frontend/`; fix failures
- [X] T051 Manually validate quickstart scenarios 1–4 from `specs/001-maps-business-lookup/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: sem dependências
- **Foundational (Phase 2)**: depende do Setup — BLOQUEIA todas as stories
- **US1 (Phase 3)**: após Foundational — MVP
- **US2 (Phase 4)**: após Foundational; integra com busca da US1 (pode usar seeds nos testes)
- **US3 (Phase 5)**: após Foundational; na prática após haver businesses (US1/US2)
- **Polish (Phase 6)**: após stories desejadas

### User Story Dependencies

- **US1 (P1)**: independente após Foundational
- **US2 (P1)**: usa `searchId` + businesses; testes isolados com repositório fake/seed
- **US3 (P2)**: exporta businesses existentes; testes com dados seedados

### Within Each User Story

- Testes MUST falhar antes da implementação
- Handlers/domínio antes de endpoints
- Endpoints antes da UI que os consome
- Story completa antes de subir prioridade seguinte (recomendado)

### Parallel Opportunities

- T002–T007 em paralelo no Setup
- T008–T011 em paralelo no Foundational
- Todos os testes `[P]` de uma story em paralelo
- T020/T021, T023–T025 (após contratos estáveis) em paralelo dentro da US1
- T032/T034/T036 em paralelo na US2
- T040–T042 e T043/T045 em paralelo na US3

---

## Parallel Example: User Story 1

```bash
# Testes US1 em paralelo:
Task: "Unit test Search validation in backend/tests/WebScraping.Domain.Tests/SearchValidationTests.cs"
Task: "Application test StartSearch in backend/tests/WebScraping.Application.Tests/StartSearchTests.cs"
Task: "API test POST/GET searches in backend/tests/WebScraping.Api.Tests/SearchesEndpointTests.cs"
Task: "Frontend SearchForm test in frontend/tests/SearchForm.test.jsx"
Task: "Frontend BusinessList test in frontend/tests/BusinessList.test.jsx"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Phase 1: Setup
2. Phase 2: Foundational
3. Phase 3: US1 (testes → API → UI)
4. **STOP and VALIDATE**: listagem de nomes / empty state
5. Demo local

### Incremental Delivery

1. Setup + Foundational
2. US1 → validar lista
3. US2 → detalhes, progresso, cancel
4. US3 → export CSV
5. Polish + quickstart

### Parallel Team Strategy

1. Time fecha Setup + Foundational juntos
2. Depois: Dev A US1 backend, Dev B US1 frontend; em seguida US2/US3

---

## Notes

- `[P]` = arquivos diferentes, sem dependência de tarefa incompleta
- `[USn]` mapeia à user story da spec
- Google Places real só no adapter de Infrastructure; testes usam `FakeBusinessLookupSource`
- Não commitar secrets; usar env / user-secrets
- Próximo comando sugerido: `/speckit-implement`
