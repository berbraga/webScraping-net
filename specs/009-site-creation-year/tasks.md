# Tasks: Ano de Criação via Copyright do Site

**Input**: Design documents from `/specs/009-site-creation-year/`

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

- Backend: `backend/WebScraping.{Domain,Application,Infrastructure,Api}/`, `backend/tests/`
- Frontend: `frontend/components/`, `frontend/tests/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Baseline verde antes das mudanças

- [x] T001 Verify `dotnet test` in `backend/` and `npm test` in `frontend/` pass on current branch baseline

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Campo, port, options, fake, persistência e projeção API — sem fase de leitura ainda

**⚠️ CRITICAL**: No user story work until this phase is complete

- [x] T002 Add `int? SiteCreationYear` to `backend/WebScraping.Domain/Entities/Business.cs`
- [x] T003 [P] Add `IWebsiteCopyrightYearLookup` (`Task<int?> GetYearAsync(string websiteUrl, CancellationToken)`) in `backend/WebScraping.Domain/Abstractions/Abstractions.cs`
- [x] T004 [P] Add `WebsiteCopyrightOptions` (`TimeoutSeconds=10`, `MaxDegreeOfParallelism=10`, `UseFakeLookup`) in `backend/WebScraping.Application/Options/Options.cs` and sample keys in `backend/WebScraping.Api/appsettings.json`
- [x] T005 [P] Implement stub/fake `FakeWebsiteCopyrightYearLookup` in `backend/WebScraping.Infrastructure/Lookup/FakeWebsiteCopyrightYearLookup.cs`
- [x] T006 Map `SiteCreationYear` in `BusinessDocument` / ToDocument / FromDocument in `backend/WebScraping.Infrastructure/Persistence/MongoRepositories.cs` and `Clone` in `backend/WebScraping.Infrastructure/Persistence/InMemoryStores.cs`
- [x] T007 Bind options + register fake/real lookup DI in `backend/WebScraping.Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs`
- [x] T008 Expose `siteCreationYear` (int or null) after `website` in `GET .../businesses` in `backend/WebScraping.Api/Endpoints/SearchesEndpoints.cs`

**Checkpoint**: Foundation ready — API já devolve `siteCreationYear: null`

---

## Phase 3: User Story 1 - Extrair ano de copyright (Priority: P1) 🎯 MVP

**Goal**: Após Places, fase paralela lê HTML, extrai menor ano 19xx/20xx (footer/final), grava `SiteCreationYear`; busca só `Completed` depois.

**Independent Test**: Fake HTML com intervalo/ano único → `siteCreationYear` esperado; mesma URL 1 GET; Completed só após a fase.

### Tests for User Story 1 (REQUIRED) ✅

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T009 [P] [US1] Add failing unit tests for `CopyrightYearExtractor` in `backend/tests/WebScraping.Domain.Tests/` (© 2016-2026 → 2016; Copyright 2015; 2018 - 2024; footer vs tail; no match → null; ignore non-19/20xx)
- [x] T010 [P] [US1] Add failing Application tests in `backend/tests/WebScraping.Application.Tests/ApplicationTests.cs`: after Places, years filled from fake lookup; Completed only after year phase; duplicate Website → one lookup call
- [x] T011 [P] [US1] Add failing API assertion for `siteCreationYear` in `backend/tests/WebScraping.Api.Tests/ApiTests.cs` (inject fake lookup)

### Implementation for User Story 1

- [x] T012 [US1] Implement `CopyrightYearExtractor.TryExtractOldestYear` in `backend/WebScraping.Domain/Services/CopyrightYearExtractor.cs`
- [x] T013 [US1] Implement `HttpWebsiteCopyrightYearLookup` (GET + timeout + call extractor; failures → null) in `backend/WebScraping.Infrastructure/Lookup/HttpWebsiteCopyrightYearLookup.cs` and finish Fake with deterministic HTML/years
- [x] T014 [US1] Create `EnrichSiteCreationYearsHandler` in `backend/WebScraping.Application/Searches/EnrichSiteCreationYearsHandler.cs` (URL cache, `SemaphoreSlim` MaxDegreeOfParallelism, update businesses; never touch FailedCount/DetailStatus for copyright miss)
- [x] T015 [US1] Wire handler after last Places enrich and **before** Completed in `backend/WebScraping.Application/Searches/DiscoverSearchHandler.cs` (and mirror in `EnrichSearchAsync` path in `backend/WebScraping.Application/Searches/EnrichAndExport.cs` if still used); register handler in DI
- [x] T016 [US1] Ensure test hosts inject `IWebsiteCopyrightYearLookup` fake in `backend/tests/WebScraping.Application.Tests/ApplicationTests.cs` and `backend/tests/WebScraping.Api.Tests/ApiTests.cs`
- [x] T017 [US1] Run `dotnet test` in `backend/` until T009–T011 pass

**Checkpoint**: MVP de dados — ano no resultado/API após busca completa

---

## Phase 4: User Story 2 - Falha contida na leitura do site (Priority: P1)

**Goal**: Timeout (~10s), SSL, 404, HTML sem ano → `null`; fase conclui; Places intacto.

**Independent Test**: Lookup que lança/demora/null → item com Places ok e `siteCreationYear` null; demais URLs seguem; cache reutiliza null.

### Tests for User Story 2 (REQUIRED) ✅

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T018 [P] [US2] Add failing tests in `backend/tests/WebScraping.Application.Tests/ApplicationTests.cs`: throwing lookup → null year + search still Completed; slow call respects TimeoutSeconds; failed URL cached and not re-fetched; FailedCount unchanged for copyright-only failure

### Implementation for User Story 2

- [x] T019 [US2] Harden per-URL CTS/`CancelAfter(TimeoutSeconds)` and catch-all → null in `backend/WebScraping.Application/Searches/EnrichSiteCreationYearsHandler.cs` and/or `backend/WebScraping.Infrastructure/Lookup/HttpWebsiteCopyrightYearLookup.cs`
- [x] T020 [US2] Confirm concurrency cap (~10) and that one failure does not cancel sibling tasks incorrectly in `backend/WebScraping.Application/Searches/EnrichSiteCreationYearsHandler.cs`
- [x] T021 [US2] Run `dotnet test` in `backend/` until T018 passes

**Checkpoint**: Resiliência FR-006/007/012/014

---

## Phase 5: User Story 3 - Coluna UI e exportação (Priority: P1)

**Goal**: Coluna **Criação do site** à direita de Site; CSV com mesma coluna.

**Independent Test**: BusinessList mostra ano ou ✕; CSV header `...,Site,Criação do site,Avaliacao`.

### Tests for User Story 3 (REQUIRED) ✅

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T022 [P] [US3] Add failing tests in `frontend/tests/BusinessList.test.jsx` (and/or `frontend/tests/BusinessList.missingFields.test.jsx`): header **Criação do site** after Site; shows year; missing → ✕
- [x] T023 [P] [US3] Add failing CSV tests in `backend/tests/WebScraping.Application.Tests/ApplicationTests.cs`: header includes **Criação do site** after Site; cell year or empty

### Implementation for User Story 3

- [x] T024 [US3] Add column **Criação do site** (`item.siteCreationYear`) after Site in `frontend/components/BusinessList.jsx`
- [x] T025 [US3] Update `ExportSearchCsvHandler.BuildCsv` in `backend/WebScraping.Application/Searches/EnrichAndExport.cs` for **Criação do site** after Site
- [x] T026 [US3] Run `npm test` in `frontend/` and `dotnet test` in `backend/` until T022–T023 pass

**Checkpoint**: Superfície completa (SC-004/SC-005)

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Validação transversal

- [x] T027 [P] Review `WebsiteCopyright` config in `backend/WebScraping.Api/appsettings.json` / Development
- [x] T028 Run full `dotnet test` in `backend/` + `npm test` in `frontend/`
- [x] T029 Manual smoke from `specs/009-site-creation-year/quickstart.md` (fake path)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Sem dependências
- **Foundational (Phase 2)**: Depende do Setup — **bloqueia** stories
- **US1 (Phase 3)**: Após Foundational — MVP de extração
- **US2 (Phase 4)**: Após US1 (mesmo handler)
- **US3 (Phase 5)**: Após Foundational para UI (campo na API); idealmente após US1 para dados reais; CSV pode seguir US1
- **Polish (Phase 6)**: Após stories desejadas

### User Story Dependencies

- **US1 (P1)**: Foundational — extractor + fase pós-Places
- **US2 (P1)**: Depende de US1
- **US3 (P1)**: UI pode começar após T008; CSV após T002; melhor com US1 verde

### Parallel Opportunities

- T003 / T004 / T005 em paralelo (Phase 2)
- T009 / T010 / T011 em paralelo (testes US1)
- Após T008: T022 (frontend) pode avançar em paralelo ao backend US1
- T022 e T023 em paralelo na US3

---

## Parallel Example: User Story 1

```bash
Task: "CopyrightYearExtractor tests in backend/tests/WebScraping.Domain.Tests/"
Task: "Year phase tests in backend/tests/WebScraping.Application.Tests/ApplicationTests.cs"
Task: "API siteCreationYear in backend/tests/WebScraping.Api.Tests/ApiTests.cs"
```

---

## Parallel Example: User Story 3

```bash
Task: "BusinessList column in frontend/components/BusinessList.jsx"
Task: "CSV BuildCsv in backend/WebScraping.Application/Searches/EnrichAndExport.cs"
```

---

## Implementation Strategy

### MVP First (US1 + Foundational)

1. Phase 1–2
2. Phase 3 US1 (dados + Completed ordering)
3. **STOP** e validar API `siteCreationYear`
4. US2 resiliência → US3 UI/CSV

### Incremental Delivery

1. Setup + Foundational  
2. US1 → ano no backend  
3. US2 → timeouts/falhas  
4. US3 → coluna + CSV  
5. Polish / quickstart  

### Parallel Team Strategy

1. Time fecha Phase 1–2  
2. Dev A: US1/US2 backend | Dev B: US3 frontend (após T008) | Dev C: CSV (T023/T025)

---

## Notes

- [P] = arquivos distintos sem dependência incompleta
- Rótulo UI/CSV: **Criação do site**; campo: `SiteCreationYear` / `siteCreationYear`
- Sem WHOIS/RDAP (FR-011)
- Fase copyright **não** roda em early-complete vazio nem em Failed/Cancelled de Places
- Cache de URL só na execução da busca
