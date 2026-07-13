# Tasks: Ordenação por Avaliação na Tabela

**Input**: Design documents from `/specs/004-sort-rating-column/`

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
- Backend: fora de escopo

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Confirmar baseline de testes do frontend (sem novos pacotes)

- [x] T001 Verify Vitest suite runs in `frontend/` with `npm test` (baseline verde antes das mudanças)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Util puro de ordenação + helper de elegibilidade compartilhados por todas as stories

**⚠️ CRITICAL**: No user story UI work until this phase is complete

- [x] T002 Create `sortByRating(items, direction)` and `nextRatingSortDirection(current)` stubs in `frontend/lib/sortByRating.js` (export API only; behavior filled in stories)
- [x] T003 [P] Add `isRatingSortAllowed(status)` in `frontend/lib/homeView.js` (false for `pending`/`running`; true for `completed`/`cancelled`/`failed`)
- [x] T004 [P] Add unit tests for `isRatingSortAllowed` in `frontend/tests/homeView.test.js`

**Checkpoint**: Foundation ready — story work can begin

---

## Phase 3: User Story 1 - Ordenar desc na 1ª ativação (Priority: P1) 🎯 MVP

**Goal**: Com busca não-processando, 1º clique em Avaliação ordena decrescente, mostra seta ↓ e cursor pointer; durante processing clique não altera ordem.

**Independent Test**: Render `BusinessList` com status `completed` e ratings 1/3/5 → um clique → ordem 5,3,1 + ícone; com status `running` → clique não muda ordem.

### Tests for User Story 1 (REQUIRED) ✅

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T005 [P] [US1] Add failing unit tests for `sortByRating(..., 'desc')` (incl. numeric order) in `frontend/tests/sortByRating.test.js`
- [x] T006 [P] [US1] Add failing component tests in `frontend/tests/BusinessList.test.jsx`: first click sorts desc when `searchStatus="completed"`; click ignored when `searchStatus="running"`; header has sortable/pointer affordance

### Implementation for User Story 1

- [x] T007 [US1] Implement `sortByRating` descending path in `frontend/lib/sortByRating.js`
- [x] T008 [US1] Implement `nextRatingSortDirection` (`null`→`desc`) in `frontend/lib/sortByRating.js`
- [x] T009 [US1] Wire sortable Avaliação header + local sort state + desc icon in `frontend/components/BusinessList.jsx` (accept `searchStatus` prop; ignore clicks when `!isRatingSortAllowed`)
- [x] T010 [US1] Pass `searchStatus={search?.status}` from `frontend/app/page.js` into `BusinessList`
- [x] T011 [US1] Add CSS for `.th-sortable` / cursor pointer / sort icon in `frontend/app/globals.css`
- [x] T012 [US1] Run `npm test` in `frontend/` until T005–T006 pass

**Checkpoint**: MVP — primeiro clique efetivo ordena desc

---

## Phase 4: User Story 2 - Toggle asc/desc (Priority: P2)

**Goal**: Cliques seguintes alternam crescente ↔ decrescente e atualizam o ícone (↑/↓).

**Independent Test**: Após desc, segundo clique → asc (1,3,5); terceiro → desc de novo.

### Tests for User Story 2 (REQUIRED) ✅

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T013 [P] [US2] Extend `frontend/tests/sortByRating.test.js` for `sortByRating(..., 'asc')` and `nextRatingSortDirection` cycle `null→desc→asc→desc`
- [x] T014 [P] [US2] Extend `frontend/tests/BusinessList.test.jsx` for second/third click toggle and icon change (↓ then ↑ then ↓)

### Implementation for User Story 2

- [x] T015 [US2] Complete ascending branch + direction cycling in `frontend/lib/sortByRating.js` and `frontend/components/BusinessList.jsx`
- [x] T016 [US2] Ensure `aria-sort` updates on Avaliação header in `frontend/components/BusinessList.jsx`
- [x] T017 [US2] Run `npm test` in `frontend/` until T013–T014 pass

**Checkpoint**: Toggle completo

---

## Phase 5: User Story 3 - Sem nota sempre no final (Priority: P3)

**Goal**: `null` / vazio / `N/A` ficam após todos os com nota, em asc e desc.

**Independent Test**: Lista mista → desc e asc mantêm ausentes no fim.

### Tests for User Story 3 (REQUIRED) ✅

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T018 [P] [US3] Extend `frontend/tests/sortByRating.test.js` for nulls-last on both directions and treat `'N/A'` / `''` as missing
- [x] T019 [P] [US3] Extend `frontend/tests/BusinessList.test.jsx` asserting missing-rating rows render after rated rows after sort

### Implementation for User Story 3

- [x] T020 [US3] Implement missing-rating detection + nulls-last partitioning in `frontend/lib/sortByRating.js`
- [x] T021 [US3] Run `npm test` in `frontend/` until T018–T019 pass

**Checkpoint**: Edge case de notas ausentes coberto

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Reset, a11y, regressões

- [x] T022 Reset `ratingSort` when `searchId` changes (prop `searchId` + `useEffect` or `key`) in `frontend/components/BusinessList.jsx` / `frontend/app/page.js`
- [x] T023 [P] Add keyboard activation (Enter/Space) on Avaliação control in `frontend/components/BusinessList.jsx`
- [x] T024 [P] Add test that new `searchId` clears sort state in `frontend/tests/BusinessList.test.jsx`
- [x] T025 Run full `npm test` in `frontend/` and fix regressions
- [x] T026 Manual smoke per `specs/004-sort-rating-column/quickstart.md` (completed vs running)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies
- **Foundational (Phase 2)**: Depends on Setup — BLOCKS stories
- **US1 (Phase 3)**: Depends on Foundational — MVP
- **US2 (Phase 4)**: Builds on US1 header/state
- **US3 (Phase 5)**: Builds on sort util (can start tests after T002; impl after desc/asc exist)
- **Polish (Phase 6)**: After US1–US3 desired scope

### User Story Dependencies

- **US1**: After Phase 2
- **US2**: After US1 click wiring
- **US3**: Sort util nulls-last (can land with/just after US2)

### Within Each User Story

- Tests fail first → implement → `npm test` green

### Parallel Opportunities

- T003/T004 in parallel
- T005/T006 in parallel
- T013/T014 in parallel
- T018/T019 in parallel
- T023/T024 in parallel after T022

---

## Parallel Example: User Story 1

```bash
# After T004:
Task: "T005 sortByRating desc tests in frontend/tests/sortByRating.test.js"
Task: "T006 BusinessList first-click / running-ignore tests in frontend/tests/BusinessList.test.jsx"

# Then implement T007–T011
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Phase 1–2
2. Phase 3 (desc + ignore while processing)
3. **STOP and VALIDATE** quickstart steps 1–2 e 4
4. Demo ready

### Incremental Delivery

1. Foundation utils
2. US1 desc
3. US2 toggle
4. US3 nulls-last
5. Reset + a11y + full test

### Parallel Team Strategy

- Dev A: `sortByRating.js` + unit tests
- Dev B: `BusinessList.jsx` + CSS + page wiring (after T002 API exists)

---

## Notes

- Backend untouched
- Export CSV unchanged
- [P] = different files / no incomplete dependency
- Commit after each story checkpoint when possible
- **Implemented 2026-07-13**: all T001–T026 done; `npm test` 28 passed
