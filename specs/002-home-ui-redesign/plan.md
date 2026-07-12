# Implementation Plan: Redesign da Home de Busca

**Branch**: `002-home-ui-redesign` | **Date**: 2026-07-10 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/002-home-ui-redesign/spec.md`

## Summary

Redesenhar apenas a **home do frontend Next.js** para ficar visualmente alinhada
aos prints em `references/`, mantendo 100% da API atual. Inclui card de
formulário (campos em linha), estados idle/processando/concluído, barra de
progresso, tabela de resultados, export CSV existente e **filtro local por nome**.

## Technical Context

**Language/Version**: JavaScript (ES2022+) / Node 20+; Next.js 14 (App Router)

**Primary Dependencies**: React 18, Next.js 14, Vitest + Testing Library (já no projeto)

**Storage**: N/A (sem mudanças de persistência; filtro só em memória na UI)

**Testing**: Vitest + Testing Library nos componentes da home

**Target Platform**: Browser desktop-first; empilhar campos em viewport estreita

**Project Type**: Frontend-only change within existing web app

**Performance Goals**: Filtro por nome responsivo ao digitar (<100 ms percebido em listas típicas ≤200 itens)

**Constraints**:
- NÃO alterar backend, contratos REST nem `frontend/lib/searchesApi.js` signatures
- Fonte de verdade visual: `references/01-form-idle.png`, `02-processing.png`, `03-completed-results.png`
- Manter simplicidade: CSS próprio (variáveis), sem novo design system/framework

**Scale/Scope**: Uma página (`frontend/app/page.js`) + componentes existentes

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Clareza**: Componentes com nomes de UI óbvios (`SearchForm`, `SearchProgress`,
  `BusinessList`, `ExportButton`, novo filtro) — PASS
- **Simplicidade enxuta**: Só frontend; CSS variables; sem Tailwind/UI kit novo;
  filtro client-side — PASS
- **Testes automatizados**: Atualizar/estender testes Vitest para layout states,
  progresso, filtro e marcador X — PASS
- **Responsabilidade única**: Apresentação na UI; `lib/searchesApi` permanece o
  único cliente HTTP — PASS
- **Design testável**: Filtro e mapeamento de status em funções puras testáveis;
  props claras nos componentes — PASS

**Post-design re-check**: PASS — sem backend; contrato UI documentado; API intacta.

## Project Structure

### Documentation (this feature)

```text
specs/002-home-ui-redesign/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── ui-home.md
├── references/
│   ├── 01-form-idle.png
│   ├── 02-processing.png
│   └── 03-completed-results.png
└── tasks.md             # /speckit-tasks
```

### Source Code (repository root)

```text
frontend/
├── app/
│   ├── page.js              # Orquestra estados idle/running/completed
│   ├── layout.js
│   └── globals.css          # Tokens visuais do redesign
├── components/
│   ├── SearchForm.jsx       # Card + grid de campos + botão
│   ├── SearchProgress.jsx   # Status colorido + barra
│   ├── BusinessList.jsx     # Tabela + X + rodapé contagem
│   ├── ExportButton.jsx
│   └── NameFilter.jsx       # Novo: "Filtrar por nome..."
├── lib/
│   ├── searchesApi.js       # INTACTO (sem mudança de contrato)
│   ├── apiClient.js         # INTACTO
│   └── homeView.js          # Novo: filterByName, statusLabel, progressRatio
└── tests/
    ├── SearchForm.test.jsx
    ├── SearchProgress.test.jsx
    ├── BusinessList.test.jsx
    ├── BusinessList.missingFields.test.jsx
    ├── ExportButton.test.jsx
    ├── NameFilter.test.jsx
    └── homeView.test.js
```

**Structure Decision**: Alterar somente `frontend/`. Extrair helpers de vista em
`lib/homeView.js` para manter `page.js` enxuto e testável. Sem mudanças em `backend/`.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| (nenhuma) | — | — |
