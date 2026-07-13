# Implementation Plan: Paginação da Lista de Resultados

**Branch**: `006-results-pagination` | **Date**: 2026-07-13 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/006-results-pagination/spec.md`

## Summary

Paginar a tabela de resultados **somente no frontend**: com **mais de 60** itens no conjunto exibível (após filtro/ordenação), mostrar 60 por página com botões Anterior/Próxima (extremos visíveis e desabilitados); com **≤ 60**, lista completa sem controles. Rodapé “Mostrando X–Y de Z” com paginação ativa; “N de Y resultados” sem paginação. Filtro e ordenação por avaliação permanecem; paginar é o último passo do pipeline. Sem paginação página-a-página na API.

**Abordagem**: helpers puros em `frontend/lib/` (limiar, fatia, rodapé) + estado de página em `BusinessList` + garantir carga completa da lista (`take` até o teto do produto) em `listBusinesses` / `page.js`.

## Technical Context

**Language/Version**: JavaScript (Next.js 14 App Router, React 18)

**Primary Dependencies**: React, Vitest + Testing Library (já no frontend)

**Storage**: N/A (estado de UI em memória)

**Testing**: Vitest — unitários dos helpers de paginação; testes de componente (controles, extremos, rodapé, reset com filtro/sort)

**Target Platform**: Browser (`localhost:3000`)

**Project Type**: Web application — **somente frontend** nesta feature

**Performance Goals**: Fatia client-side em listas ≤ 200 instantânea na UI

**Constraints**: Sem nova request ao mudar de página; limiar/tamanho = 60; CSV e coleta inalterados; respeitar gate de resultados (005) e sort (004)

**Scale/Scope**: 1 módulo de paginação, ajuste em `BusinessList` (+ CSS), ajuste mínimo em `searchesApi`/`page.js` para `take`, testes

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Clareza**: Helpers nomeados (`paginate`, `shouldShowPagination`, `formatResultsFooter`) + controles no `BusinessList`. ✅
- **Simplicidade enxuta**: Sem lib de tabela/paginação; sem mudanças de contrato REST. ✅
- **Testes automatizados**: Unit + component tests mapeados a P1–P3. ✅
- **Responsabilidade única**: Cálculo de fatia/rodapé fora do JSX denso. ✅
- **Design testável**: Funções puras; constante `PAGE_SIZE = 60` em um só lugar. ✅

*Post-design*: sem violações; Complexity Tracking vazio.

## Project Structure

### Documentation (this feature)

```text
specs/006-results-pagination/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── ui-results-pagination.md
└── tasks.md              # /speckit-tasks (ainda não)
```

### Source Code (repository root)

```text
frontend/
├── lib/
│   ├── homeView.js              # filterByName, shouldShowResultsArea (inalterado na regra)
│   ├── sortByRating.js          # inalterado
│   ├── searchesApi.js           # listBusinesses com take até AbsoluteMax (200)
│   └── paginateResults.js       # NOVO: PAGE_SIZE, shouldShowPagination, slicePage, formatFooter
├── components/
│   └── BusinessList.jsx         # estado de página, controles, aplica pipeline sort→paginate
├── app/
│   ├── page.js                  # listBusinesses com take adequado; props inalteradas em espírito
│   └── globals.css              # estilos mínimos dos controles de paginação
└── tests/
    ├── paginateResults.test.js  # NOVO
    ├── BusinessList.test.jsx    # estender (paginação + rodapé + reset)
    └── searchesApi.test.js      # opcional se já houver padrão de mock de URL
```

**Structure Decision**: Frontend-only; paginação vive depois do sort em `BusinessList`; carga completa da lista via query `take` já suportada pela API existente (sem novo endpoint).

## Complexity Tracking

> Nenhuma violação da constituição.
