# Research: Redesign da Home de Busca

**Feature**: `002-home-ui-redesign`  
**Date**: 2026-07-10

## 1. Escopo frontend-only

**Decision**: Alterar apenas `frontend/`; zero mudanças em backend/contratos REST.

**Rationale**: Spec FR-007/FR-008 e pedido explícito “manter a mesma API”.

**Alternatives considered**:
- Novo endpoint de filtro — rejeitado (filtro local basta)
- Redesign via Blazor/outro host — rejeitado (stack já é Next.js)

## 2. Estilo visual

**Decision**: CSS com variáveis em `globals.css` (fundo creme, card branco,
verde-escuro do botão/barra, âmbar para “processando”, verde para “completed”).
Sem Tailwind/UI kit novo.

**Rationale**: Constituição II (YAGNI); projeto já usa CSS próprio; prints são
simples o suficiente para tokens manuais.

**Alternatives considered**:
- Tailwind — overhead de setup sem ganho claro nesta tela única
- Biblioteca de componentes — overkill e desvia do visual dos prints

## 3. Layout do formulário

**Decision**: Card com grid de 3 colunas no desktop (`Região` | `Termo` |
`Limite`); empilhar em breakpoint estreito. Botão full-width abaixo.

**Rationale**: Replica `references/01-form-idle.png` e `02-processing.png`.

**Alternatives considered**:
- Campos sempre empilhados — diverge dos prints desktop

## 4. Progresso

**Decision**: Componente `SearchProgress` com label de status colorido +
`<progress>` ou barra CSS proporcional a `processedCount / totalFound`.
Botão do form mostra “Buscando...” quando `loading` ou status running/pending.

**Rationale**: Print `02-processing.png`.

**Alternatives considered**:
- Spinner só no botão — insuficiente vs print (falta barra e fração)

## 5. Filtro por nome

**Decision**: Estado local `nameFilter` em `page.js`; função pura
`filterByName(items, query)` em `lib/homeView.js`; contagem do rodapé =
filtrados / total carregado.

**Rationale**: Spec FR-006; sem API; fácil de testar.

**Alternatives considered**:
- Debounce server-side — desnecessário

## 6. Status labels

**Decision**: Mapear status da API (`running`/`pending` → destaque “processando”;
`completed` → “completed” em verde; demais conforme backend) sem traduzir à força
todos os valores — seguir prints (mistura PT/EN aceita na Assumptions).

**Rationale**: Assumptions da spec.

**Alternatives considered**:
- Traduzir tudo para PT — ok depois; não bloqueia o redesign

## 7. Cancelar

**Decision**: Manter cancelamento se já existir, de forma discreta (não está nos
prints), sem quebrar o layout.

**Rationale**: Spec edge case / assumptions.
