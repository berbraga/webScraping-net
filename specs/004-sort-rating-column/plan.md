# Implementation Plan: Ordenação por Avaliação na Tabela

**Branch**: `004-sort-rating-column` | **Date**: 2026-07-13 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/004-sort-rating-column/spec.md`

## Summary

Permitir ordenar a tabela de resultados pela coluna **Avaliação** no frontend: 1º clique efetivo → decrescente; cliques seguintes → toggle crescente/decrescente; notas ausentes sempre no final; ícone de sentido no cabeçalho. Ordenação **só altera a lista com busca completa**; durante processamento o cabeçalho parece normal mas cliques são ignorados. Sem mudanças de backend/API.

**Abordagem**: função pura de sort em `frontend/lib/` + estado de sentido em `BusinessList` (ou page) + testes Vitest; CSS mínimo para cursor/ícone.

## Technical Context

**Language/Version**: JavaScript (Next.js 14 App Router, React 18)

**Primary Dependencies**: React, Vitest + Testing Library (já no frontend)

**Storage**: N/A (estado de UI em memória)

**Testing**: Vitest — unitários da função de sort; testes de componente do cabeçalho/toggle/ignore enquanto processa

**Target Platform**: Browser (UI em `localhost:3000`)

**Project Type**: Web application — **somente frontend** nesta feature

**Performance Goals**: Sort client-side em listas típicas (≤200 linhas) instantâneo na UI

**Constraints**: Sem novas requests; só coluna Avaliação; reset ao trocar busca; ignorar cliques se status ≠ completed (e similares terminais? Spec diz “completa” — tratar `completed` como elegível; `cancelled`/`failed` com lista: decidir em research — recomendado permitir sort se houver itens e status não for processing)

**Scale/Scope**: 1 util de sort, ajuste em `BusinessList` (+ props de `page.js`), CSS, testes

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Clareza**: Sort puro + cabeçalho com responsabilidade única. ✅
- **Simplicidade enxuta**: Sem libs de table/sort; sem backend. ✅
- **Testes automatizados**: Unit + component tests por user story. ✅
- **Responsabilidade única**: Comparação/ordenação fora do JSX de render. ✅
- **Design testável**: Função pura `sortByRating` mockável/testável sem DOM. ✅

*Post-design*: sem violações; Complexity Tracking vazio.

## Project Structure

### Documentation (this feature)

```text
specs/004-sort-rating-column/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── ui-sort-rating.md
└── tasks.md              # /speckit-tasks (ainda não)
```

### Source Code (repository root)

```text
frontend/
├── lib/
│   ├── homeView.js              # filterByName, isProcessingStatus (reutilizar)
│   └── sortByRating.js          # NOVO: sort puro + nextSortDirection
├── components/
│   └── BusinessList.jsx         # cabeçalho clicável, ícone, aplica sort
├── app/
│   ├── page.js                  # passar searchStatus / canSort; reset filter já existe
│   └── globals.css              # .th-sortable, cursor, ícone
└── tests/
    ├── sortByRating.test.js     # NOVO
    ├── BusinessList.test.jsx    # estender
    └── homeView.test.js         # inalterado salvo helpers extras
```

**Structure Decision**: Frontend-only; reutilizar `isProcessingStatus` / status da busca já existentes em `page.js` e `homeView.js`.

## Complexity Tracking

> Nenhuma violação da constituição.
