---
description: "Task list for home UI redesign"
---

# Tasks: Redesign da Home de Busca

**Input**: Design documents from `/specs/002-home-ui-redesign/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/ui-home.md

**Tests**: OBRIGATÓRIO (constituição III). Toda user story MUST incluir
tarefas de teste automatizado. Escrever testes que falhem antes da
implementação; só então implementar até ficarem verdes.

**Organization**: Tasks grouped by user story. Frontend-only — NÃO alterar backend nem `frontend/lib/searchesApi.js`.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: US1 / US2 / US3
- Include exact file paths in descriptions

## Path Conventions

- Frontend: `frontend/app/`, `frontend/components/`, `frontend/lib/`, `frontend/tests/`

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Confirmar baseline e tokens visuais compartilhados

- [X] T001 Verify existing frontend scripts/tests still run via `frontend/package.json` (`npm test`)
- [X] T002 [P] Add design tokens (creme, card, verde, âmbar, tipografia) in `frontend/app/globals.css`
- [X] T003 [P] Confirm `frontend/lib/searchesApi.js` and `frontend/lib/apiClient.js` remain unchanged (no signature edits)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Helpers de vista compartilhados por todas as stories

**⚠️ CRITICAL**: User stories dependem destes helpers

- [X] T004 Create pure helpers `filterByName`, `statusLabel`, `progressRatio`, `isProcessingStatus` in `frontend/lib/homeView.js`
- [X] T005 [P] Unit tests for helpers in `frontend/tests/homeView.test.js`
- [X] T006 Ensure `frontend/app/layout.js` loads global styles (no structural change beyond title if needed)

**Checkpoint**: Helpers testados; CSS tokens disponíveis

---

## Phase 3: User Story 1 - Formulário inicial alinhado ao print (Priority: P1) 🎯 MVP

**Goal**: Home idle com título, subtítulo, card e 3 campos em linha + botão full-width

**Independent Test**: Abrir home sem busca e comparar com `references/01-form-idle.png`

### Tests for User Story 1 (REQUIRED) ✅

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [X] T007 [P] [US1] Update/extend SearchForm test for three labeled fields + submit button text in `frontend/tests/SearchForm.test.jsx`
- [X] T008 [P] [US1] Test busy button shows "Buscando..." when `busy` prop is true in `frontend/tests/SearchForm.test.jsx`

### Implementation for User Story 1

- [X] T009 [US1] Redesign `SearchForm` as white card with desktop 3-column field grid in `frontend/components/SearchForm.jsx`
- [X] T010 [US1] Style page shell (centered title/subtitle + form) in `frontend/app/page.js` and `frontend/app/globals.css`
- [X] T011 [US1] Wire `busy`/`disabled` props from page loading/running state into `frontend/components/SearchForm.jsx` via `frontend/app/page.js`

**Checkpoint**: Idle visual alinhado ao print 01; submit ainda chama API existente

---

## Phase 4: User Story 2 - Progresso durante a coleta (Priority: P1)

**Goal**: Status colorido, fração processados/total e barra de progresso; botão "Buscando..."

**Independent Test**: Durante busca ativa, comparar com `references/02-processing.png`

### Tests for User Story 2 (REQUIRED) ✅

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [X] T012 [P] [US2] Test SearchProgress renders status label + processed/total in `frontend/tests/SearchProgress.test.jsx`
- [X] T013 [P] [US2] Test progress bar reflects ratio in `frontend/tests/SearchProgress.test.jsx`

### Implementation for User Story 2

- [X] T014 [US2] Implement colored status + progress bar UI in `frontend/components/SearchProgress.jsx`
- [X] T015 [US2] Show SearchProgress under the form while processing in `frontend/app/page.js`
- [X] T016 [US2] Keep optional cancel control discreet (if present) without breaking print layout in `frontend/components/SearchProgress.jsx`

**Checkpoint**: Estado processando alinhado ao print 02

---

## Phase 5: User Story 3 - Resultados, exportação e filtro por nome (Priority: P1)

**Goal**: Tabela print-like, X em ausências, Exportar CSV, Filtrar por nome, rodapé de contagem

**Independent Test**: Busca completed + lista; comparar com `references/03-completed-results.png`

### Tests for User Story 3 (REQUIRED) ✅

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [X] T017 [P] [US3] Test NameFilter input placeholder and onChange in `frontend/tests/NameFilter.test.jsx`
- [X] T018 [P] [US3] Test BusinessList footer count and filtered items in `frontend/tests/BusinessList.test.jsx`
- [X] T019 [P] [US3] Confirm missing-field X marker still covered in `frontend/tests/BusinessList.missingFields.test.jsx`
- [X] T020 [P] [US3] Confirm ExportButton still sets export URL in `frontend/tests/ExportButton.test.jsx`

### Implementation for User Story 3

- [X] T021 [P] [US3] Create `NameFilter` component in `frontend/components/NameFilter.jsx`
- [X] T022 [US3] Restyle results table + footer `{filtrados} de {total} resultados` in `frontend/components/BusinessList.jsx`
- [X] T023 [US3] Compose completed toolbar (status, Exportar CSV, filtro) in `frontend/app/page.js`
- [X] T024 [US3] Apply `filterByName` from `frontend/lib/homeView.js` to list rendering in `frontend/app/page.js`
- [X] T025 [US3] Ensure export still uses existing `exportSearchCsv` from `frontend/lib/searchesApi.js` via `frontend/components/ExportButton.jsx`

**Checkpoint**: Print 03 coberto; filtro local funciona; API intacta

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Responsividade, regressão e validação visual

- [X] T026 [P] Add narrow-viewport stacking rules for form fields in `frontend/app/globals.css`
- [X] T027 [P] Empty/error message styles consistent with new layout in `frontend/app/page.js` and `frontend/app/globals.css`
- [X] T028 Run `npm test` in `frontend/` and fix failures
- [X] T029 Manually validate quickstart scenarios 1–3 against `specs/002-home-ui-redesign/references/*.png` per `specs/002-home-ui-redesign/quickstart.md`
- [X] T030 Confirm no backend file changes and no public API signature changes in `frontend/lib/searchesApi.js`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: sem dependências
- **Foundational (Phase 2)**: após Setup — bloqueia stories
- **US1 (Phase 3)**: após Foundational — MVP visual idle
- **US2 (Phase 4)**: após Foundational; usa form da US1 na prática
- **US3 (Phase 5)**: após Foundational; ideal após US1/US2 para fluxo completo
- **Polish (Phase 6)**: após stories desejadas

### User Story Dependencies

- **US1**: independente após helpers/CSS
- **US2**: independente com search mock nos testes; na app depende do fluxo de busca
- **US3**: independente com items mock; filtro não depende do backend

### Within Each User Story

- Testes MUST falhar antes da implementação
- Componentes antes da orquestração em `page.js`
- Não editar contratos de `searchesApi.js`

### Parallel Opportunities

- T002–T003 no Setup
- T007–T008 testes US1
- T012–T013 testes US2
- T017–T020 testes US3; T021 em paralelo com testes após helpers

---

## Parallel Example: User Story 3

```bash
Task: "NameFilter test in frontend/tests/NameFilter.test.jsx"
Task: "BusinessList footer test in frontend/tests/BusinessList.test.jsx"
Task: "Missing fields X test in frontend/tests/BusinessList.missingFields.test.jsx"
Task: "ExportButton test in frontend/tests/ExportButton.test.jsx"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Setup + Foundational  
2. US1 form card  
3. Validar vs print 01  

### Incremental Delivery

1. US1 idle → 2. US2 progresso → 3. US3 tabela/filtro/export → 4. Polish  

---

## Notes

- Referências visuais: `specs/002-home-ui-redesign/references/`
- Contrato UI: `specs/002-home-ui-redesign/contracts/ui-home.md`
- Próximo comando: `/speckit-implement`
