# Research: Resultados Só Após Busca Completa

## R1 — Causa do comportamento atual

**Decision**: Tratar o bug/expectativa como gate de UI incorreto em `page.js`.

**Rationale**: Hoje:
```js
const showResultsChrome = Boolean(search) && !processing;
const showBusinessList = Boolean(search) && (showResultsChrome || businesses.length > 0);
```
A segunda linha foi adicionada na feature 004 para permitir “clique ignorado durante processing” com tabela visível. Isso conflita com a expectativa do usuário (005): **não** mostrar nomes até o fim.

**Alternatives considered**:
- Manter tabela e só esconder nomes — rejeitado (ainda revela lista parcial).
- Parar de buscar `listBusinesses` no poll — opcional; não necessário se a UI não renderiza (YAGNI; pode manter poll para ter dados prontos no instante do terminal).

## R2 — Regra de visibilidade

**Decision**: `shouldShowResultsArea({ status, loading })` → `true` somente quando há busca e **não** está em processing (`pending`/`running`) e **não** está no loading inicial de submit.

Equivalente prático: reutilizar `processing = loading || isProcessingStatus(status)` e `showResults = Boolean(search) && !processing`.

Estados que **mostram** área: `completed`, `cancelled`, `failed` (clarification A).

**Rationale**: Alinha FR-001, FR-007, FR-008.

**Alternatives considered**:
- Só `completed` mostra tabela — rejeitado na clarify (opção A).

## R3 — Impacto na feature 004 (ordenação)

**Decision**: Ordenação permanece; testes de “ignore click while running” no `BusinessList` com `searchStatus="running"` continuam válidos como contrato do componente, mas a **home não monta** `BusinessList` durante running. Sem conflito de produto.

**Rationale**: Spec 005 Assumptions + 004 still valid after reveal.

## R4 — Testes

**Decision**:
1. Unit: `shouldShowResultsArea` / equivalência via `isProcessingStatus` + flags.
2. Opcional: extrair constante de render na page é difícil sem RTL de page; priorizar helper + quickstart manual.
3. Garantir que `homeView.test.js` cubra terminais vs processing.

**Rationale**: Constitution + simplicidade.

## R5 — Backend

**Decision**: Nenhuma mudança.
