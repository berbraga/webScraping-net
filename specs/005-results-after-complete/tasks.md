# Tasks: Resultados Só Após Busca Completa

**Input**: Design documents from `/specs/005-results-after-complete/`

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

- Frontend only: `frontend/lib/`, `frontend/app/`, `frontend/tests/`
- Backend: fora de escopo

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Baseline de testes do frontend

- [x] T001 Verify Vitest suite runs in `frontend/` with `npm test` (baseline verde)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Helper puro de visibilidade da área de resultados

**⚠️ CRITICAL**: No user story wiring until this phase is complete

- [x] T002 Add `shouldShowResultsArea({ status, loading })` stub/API in `frontend/lib/homeView.js` (`true` only when search status is terminal and not loading; `false` for `pending`/`running`/loading)
- [x] T003 [P] Add failing unit tests for `shouldShowResultsArea` covering `running`/`pending`/`loading` → false and `completed`/`cancelled`/`failed` → true in `frontend/tests/homeView.test.js`

**Checkpoint**: Helper contract defined

---

## Phase 3: User Story 1 - Ocultar lista durante processamento (Priority: P1) 🎯 MVP

**Goal**: Com busca `running`/`pending`, a home não renderiza tabela nem nomes; progresso continua visível.

**Independent Test**: Helper false for processing; `page.js` não monta `BusinessList`/toolbar enquanto `processing`.

### Tests for User Story 1 (REQUIRED) ✅

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T004 [P] [US1] Assert in `frontend/tests/homeView.test.js` that `shouldShowResultsArea` is false for `running` and `pending` even if conceptually “items exist”
- [x] T005 [P] [US1] Add/extend a focused UI visibility test (pure helper-driven or thin module) documenting that results chrome is gated by `shouldShowResultsArea` — prefer extending `frontend/tests/homeView.test.js`; if extracting gate from page, keep tests in `frontend/tests/homeView.test.js`

### Implementation for User Story 1

- [x] T006 [US1] Implement `shouldShowResultsArea` in `frontend/lib/homeView.js` using `isProcessingStatus` + `loading`
- [x] T007 [US1] Update `frontend/app/page.js` to remove `showBusinessList = … || businesses.length > 0` and show toolbar + `BusinessList` only when `shouldShowResultsArea` is true (keep `SearchProgress` during processing)
- [x] T008 [US1] Run `npm test` in `frontend/` until T003–T005 pass

**Checkpoint**: MVP — zero nomes na UI durante processing

---

## Phase 4: User Story 2 - Exibir tabela ao concluir (Priority: P2)

**Goal**: Em `completed` (e demais terminais cobertos pelo helper), área de resultados completa aparece com colunas existentes.

**Independent Test**: `shouldShowResultsArea({ status: 'completed' }) === true`; page monta `BusinessList` + toolbar.

### Tests for User Story 2 (REQUIRED) ✅

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T009 [P] [US2] Extend `frontend/tests/homeView.test.js` for `completed` → true (and loading false)
- [x] T010 [P] [US2] Extend `frontend/tests/homeView.test.js` for `cancelled` and `failed` → true (clarification A / FR-007)

### Implementation for User Story 2

- [x] T011 [US2] Ensure `page.js` passes `searchStatus`/`searchId` into `BusinessList` when results area is shown in `frontend/app/page.js` (preserve feature 004 props)
- [x] T012 [US2] Run `npm test` in `frontend/` until T009–T010 pass

**Checkpoint**: Tabela completa após terminal

---

## Phase 5: User Story 3 - Transição limpa entre buscas (Priority: P3)

**Goal**: Nova busca oculta resultados de novo; ao terminar, mostra dados da busca atual (sem misturar UI da anterior no processing).

**Independent Test**: Helper false again when status volta a `running`; true when completed.

### Tests for User Story 3 (REQUIRED) ✅

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T013 [P] [US3] Add sequence-style unit assertions in `frontend/tests/homeView.test.js`: completed→true, then running→false, then completed→true
- [x] T014 [P] [US3] Assert `loading: true` forces false even if status were terminal in `frontend/tests/homeView.test.js` (submit gate)

### Implementation for User Story 3

- [x] T015 [US3] Confirm `handleSubmit` in `frontend/app/page.js` sets loading/clears filter and that visibility follows helper during the new search lifecycle (fix if any stale render path remains)
- [x] T016 [US3] Run `npm test` in `frontend/` until T013–T014 pass

**Checkpoint**: Duas buscas consecutivas sem lista no meio

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Regressões e validação

- [x] T017 Run full `npm test` in `frontend/` (incl. `BusinessList` sort tests from 004) and fix regressions
- [x] T018 [P] Update `specs/005-results-after-complete/quickstart.md` if gate behavior notes need tweaking
- [x] T019 Manual smoke per quickstart: running = sem tabela; completed = tabela; cancel = área de resultados

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup** → **Foundational** → **US1 (MVP)** → **US2** → **US3** → **Polish**
- US2/US3 mostly extend the same helper + page gate after US1

### User Story Dependencies

- **US1**: After Phase 2 — core hide-while-processing
- **US2**: Depends on gate showing on terminal
- **US3**: Depends on gate flipping across search lifecycle

### Parallel Opportunities

- T004/T005 after T002
- T009/T010 in parallel
- T013/T014 in parallel
- T018 alongside T017

---

## Parallel Example: User Story 1

```bash
Task: "T004 shouldShowResultsArea false for running/pending in homeView.test.js"
Task: "T005 document results chrome gated by helper in homeView.test.js"
# Then T006–T007 page wiring
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. T001–T003
2. Implement helper + remove `businesses.length` bypass in `page.js`
3. **STOP**: validate running UI has no names

### Incremental Delivery

1. Hide during processing (US1)
2. Confirm reveal on completed/cancelled/failed (US2)
3. Confirm new-search transition (US3)
4. Full test + smoke

---

## Notes

- Backend untouched
- Feature 004 component tests for running status may remain; home simply won't mount list while processing
- [P] = different files / no incomplete dependency
- **Implemented 2026-07-13**: all T001–T019 done; `npm test` 32 passed
