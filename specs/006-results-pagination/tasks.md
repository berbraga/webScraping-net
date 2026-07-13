# Tasks: Paginação da Lista de Resultados

**Input**: Design documents from `/specs/006-results-pagination/`

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

- Frontend only: `frontend/lib/`, `frontend/components/`, `frontend/app/`, `frontend/tests/`
- Backend: fora de escopo (apenas uso de `take` já existente na API)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Confirmar baseline de testes do frontend (sem novos pacotes)

- [x] T001 Verify Vitest suite runs in `frontend/` with `npm test` (baseline verde antes das mudanças)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Helpers puros de paginação + carga completa da lista — bloqueiam todas as stories

**⚠️ CRITICAL**: No user story UI work until this phase is complete

- [x] T002 Create `frontend/lib/paginateResults.js` exporting `PAGE_SIZE` (= 60), `shouldShowPagination(total)`, `slicePage(items, page, pageSize)`, `clampPage(page, total, pageSize)`, `formatResultsFooter({ total, page, pageSize, paginationActive })`
- [x] T003 [P] Update `listBusinesses` in `frontend/lib/searchesApi.js` to request `take=200` (AbsoluteMax do produto) via query string
- [x] T004 [P] Add failing unit tests for `shouldShowPagination`, `slicePage`, `clampPage`, and `formatResultsFooter` (≤60 vs >60; fatias; rodapé simples vs “Mostrando X–Y de Z”) in `frontend/tests/paginateResults.test.js`
- [x] T005 Implement helpers in `frontend/lib/paginateResults.js` until T004 passes
- [x] T006 Ensure all `listBusinesses` call sites in `frontend/app/page.js` use the updated API (take=200) without changing polling/cancel semantics

**Checkpoint**: Foundation ready — story work can begin

---

## Phase 3: User Story 1 - Próxima página quando > 60 (Priority: P1) 🎯 MVP

**Goal**: Com conjunto exibível > 60, mostrar no máx. 60 linhas + botão Próxima; ≤ 60 sem controles; na última página Próxima visível e desabilitada; rodapé “Mostrando…” com paginação e “N de Y” sem.

**Independent Test**: `BusinessList` com 60 itens → sem paginação; com 125 → página 1 tem 60 + Próxima; ir até a última → Próxima disabled; rodapé correto em cada caso.

### Tests for User Story 1 (REQUIRED) ✅

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T007 [P] [US1] Add failing component tests in `frontend/tests/BusinessList.test.jsx`: 60 items → no Anterior/Próxima + footer `N de Y`; 125 items → first page 60 rows, footer `Mostrando 1–60 de 125`, Próxima enabled / Anterior disabled; after next to last page → Próxima disabled
- [x] T008 [P] [US1] Extend `frontend/tests/paginateResults.test.js` for 125-item three-page slice + footer intervals `1–60`, `61–120`, `121–125`

### Implementation for User Story 1

- [x] T009 [US1] Add `currentPage` state (1-based) in `frontend/components/BusinessList.jsx`; after sort, apply `slicePage` only when `shouldShowPagination`; render only page rows in the table
- [x] T010 [US1] Render pagination controls (Anterior + Próxima) in `frontend/components/BusinessList.jsx` only when pagination active; wire Próxima; disable Próxima on last page (keep visible); disable Anterior on page 1
- [x] T011 [US1] Wire `formatResultsFooter` into results footer in `frontend/components/BusinessList.jsx`
- [x] T012 [US1] Add minimal CSS for pagination controls / disabled state in `frontend/app/globals.css`
- [x] T013 [US1] Run `npm test` in `frontend/` until T007–T008 pass

**Checkpoint**: MVP — próxima página e limiar 60 funcionando

---

## Phase 4: User Story 2 - Página anterior (Priority: P2)

**Goal**: Com paginação ativa, Anterior volta à página imediatamente anterior; na página 1 permanece visível e desabilitado.

**Independent Test**: Ir à página 2 → Anterior → página 1 com os mesmos itens; na página 1 Anterior disabled.

### Tests for User Story 2 (REQUIRED) ✅

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T014 [P] [US2] Extend `frontend/tests/BusinessList.test.jsx`: from page 2, click Anterior restores page-1 row set; on page 1 Anterior is visible and disabled

### Implementation for User Story 2

- [x] T015 [US2] Wire Anterior button to decrement `currentPage` (when > 1) in `frontend/components/BusinessList.jsx`
- [x] T016 [US2] Run `npm test` in `frontend/` until T014 passes

**Checkpoint**: Navegação bidirecional completa

---

## Phase 5: User Story 3 - Filtro e ordenação preservados (Priority: P3)

**Goal**: Paginação fatia o conjunto já filtrado/ordenado; reset para página 1 ao mudar filtro, sort ou `searchId`; filtro ≤ 60 remove controles.

**Independent Test**: Filtro com 70 itens pagina só esses; filtro com 25 remove paginação; sort ativo + > 60 mantém ordem global entre páginas; mudar sort/filtro/`searchId` volta à página 1.

### Tests for User Story 3 (REQUIRED) ✅

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T017 [P] [US3] Extend `frontend/tests/BusinessList.test.jsx`: with > 60 filtered items, pages only contain matching names; with ≤ 60 filtered, no pagination controls
- [x] T018 [P] [US3] Extend `frontend/tests/BusinessList.test.jsx`: with rating sort + > 60 items, concatenation of pages matches global sorted order; changing sort or `searchId` resets to page 1
- [x] T019 [P] [US3] Add/extend test that page changes do not call `listBusinesses` (no network) — e.g. assert in `frontend/tests/BusinessList.test.jsx` that pagination is pure client state (no API mock invocations on next/prev)

### Implementation for User Story 3

- [x] T020 [US3] Reset `currentPage` to 1 on `searchId` change and when rating sort direction changes in `frontend/components/BusinessList.jsx`
- [x] T021 [US3] Reset `currentPage` to 1 when filtered `items` identity/length changes due to name filter (prop change from `frontend/app/page.js` / effect in `BusinessList.jsx`) without breaking terminal-results display
- [x] T022 [US3] Confirm pipeline order filter → sort → paginate in `frontend/components/BusinessList.jsx` and that ExportButton in `frontend/app/page.js` remains independent of page slice
- [x] T023 [US3] Run `npm test` in `frontend/` until T017–T019 pass

**Checkpoint**: Filtro/ordenação + reset cobertos

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Validação final e arestas

- [x] T024 [P] Cover edge cases in `frontend/tests/paginateResults.test.js` and/or `BusinessList.test.jsx`: 0 items, exactly 60, exactly 61 (page2 = 1 item)
- [x] T025 Run full `npm test` in `frontend/` and smoke checklist from `specs/006-results-pagination/quickstart.md`
- [x] T026 [P] Update footer notes in `specs/006-results-pagination/quickstart.md` if any command/outcome diverges after implementation

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Sem dependências
- **Foundational (Phase 2)**: Depende do Setup — BLOQUEIA todas as user stories
- **User Stories (Phase 3+)**: Dependem da Phase 2; preferir P1 → P2 → P3 (US2/US3 estendem os mesmos controles do US1)
- **Polish**: Após as stories desejadas

### User Story Dependencies

- **US1 (P1)**: Após Phase 2 — MVP isolado (Próxima + limiar + rodapé)
- **US2 (P2)**: Naturalmente após US1 (mesmo chrome; adiciona Anterior funcional)
- **US3 (P3)**: Após US1 (idealmente após US2); integra filtro/sort existentes

### Within Each User Story

- Testes MUST falhar antes da implementação
- Helpers/foundation antes da UI da story
- Story completa antes de avançar prioridade (recomendado)

### Parallel Opportunities

- T003 ∥ T004 (arquivos diferentes) após T002 stub existir
- T007 ∥ T008 dentro de US1
- T017 ∥ T018 ∥ T019 dentro de US3
- T024 ∥ T026 no polish

---

## Parallel Example: User Story 1

```bash
# Testes US1 em paralelo (REQUIRED):
Task: "BusinessList pagination component tests in frontend/tests/BusinessList.test.jsx"
Task: "paginateResults 125-item intervals in frontend/tests/paginateResults.test.js"

# Depois implementação sequencial no mesmo componente:
Task: "slice + Próxima + footer + CSS em BusinessList / globals.css"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Phase 1: Setup (baseline `npm test`)
2. Phase 2: Foundational (`paginateResults` + `take=200`)
3. Phase 3: US1 (Próxima + limiar 60 + rodapé)
4. **STOP and VALIDATE** via Independent Test / quickstart parcial
5. Demo se pronto

### Incremental Delivery

1. Setup + Foundational → base pronta
2. US1 → MVP paginado
3. US2 → Anterior
4. US3 → filtro/sort/reset
5. Polish → edge cases + quickstart

### Parallel Team Strategy

Com mais de um dev após Phase 2:

- Dev A: US1 (depois US2 no mesmo arquivo `BusinessList.jsx` — coordenar)
- Dev B: testes unitários extras / T003 take / CSS — evitar conflito no mesmo JSX

Na prática, US1→US2→US3 no mesmo `BusinessList.jsx` favorece execução sequencial.

---

## Notes

- [P] = arquivos diferentes, sem dependência de tarefa incompleta
- [USn] = rastreio à user story da spec
- Constante `PAGE_SIZE = 60` em um único módulo (`paginateResults.js`)
- Não alterar backend; não furar teto do provedor de mapas
- Verificar testes falhando antes de implementar
- Commit após cada tarefa ou grupo lógico
- Parar em qualquer checkpoint para validar a story isoladamente
