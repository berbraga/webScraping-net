# Implementation Plan: Resultados Só Após Busca Completa

**Branch**: `005-results-after-complete` | **Date**: 2026-07-13 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/005-results-after-complete/spec.md`

## Summary

Durante `pending`/`running` (e loading inicial), a home deve mostrar **apenas** progresso/cancelar — **sem** tabela nem nomes. Ao entrar em estado terminal (`completed` / `cancelled` / `failed`), exibir a área de resultados completa (toolbar + tabela com nome/telefone/site/avaliação).

**Abordagem**: corrigir o gate de UI em `frontend/app/page.js` (hoje `showBusinessList` revela itens durante o processamento se `businesses.length > 0`) e extrair helper testável em `homeView.js`. Frontend-only; API inalterada.

## Technical Context

**Language/Version**: JavaScript (Next.js 14, React 18)

**Primary Dependencies**: React, Vitest + Testing Library

**Storage**: N/A (estado de UI)

**Testing**: Vitest — unitários do helper de visibilidade; teste de página/componente se já houver padrão (senão helper + smoke manual)

**Target Platform**: Browser (`localhost:3000`)

**Project Type**: Web app — **somente frontend**

**Performance Goals**: N/A (mudança de visibilidade)

**Constraints**: Não quebrar ordenação (004), filtro, export; progresso continua visível no processing

**Scale/Scope**: 1 helper + ajuste em `page.js` + testes

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Clareza**: Regra de visibilidade em um helper nomeado. ✅
- **Simplicidade enxuta**: Remover condição `|| businesses.length > 0`; sem novas libs. ✅
- **Testes automatizados**: Unit tests do helper cobrindo processing vs terminais. ✅
- **Responsabilidade única**: Decisão de “mostrar resultados” fora do JSX denso. ✅
- **Design testável**: Função pura `shouldShowResultsArea`. ✅

*Post-design*: sem violações.

## Project Structure

### Documentation (this feature)

```text
specs/005-results-after-complete/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── ui-results-visibility.md
└── tasks.md              # /speckit-tasks
```

### Source Code (repository root)

```text
frontend/
├── lib/homeView.js          # + shouldShowResultsArea (ou equivalente)
├── app/page.js              # gate único: resultados só se !processing && search
├── components/              # BusinessList / SearchProgress inalterados na lógica interna
└── tests/
    └── homeView.test.js     # casos processing vs terminal
```

**Structure Decision**: Reverter/ajustar o gate introduzido na 004 que mostrava a lista durante o processing; manter polling de API se útil, mas **não renderizar** a tabela até terminal.

## Complexity Tracking

> Nenhuma violação.
