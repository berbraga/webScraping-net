# Research: Ordenação por Avaliação na Tabela

## R1 — Onde viver o sort

**Decision**: Função pura `sortByRating(items, direction)` em `frontend/lib/sortByRating.js`; `BusinessList` mantém estado local `ratingSort` (`null` | `'desc'` | `'asc'`) e renderiza `sortedItems`.

**Rationale**: Constitution (testabilidade); espelha o padrão de `filterByName` em `homeView.js`. Lista filtrada já chega via props — sort aplica-se ao array recebido (conjunto exibido).

**Alternatives considered**:
- Sort no `page.js` — rejeitado (mais props drilling sem ganho).
- Lib externa (TanStack Table) — rejeitado (YAGNI).

## R2 — Quando o clique é efetivo

**Decision**: Clique efetivo somente quando `!isProcessingStatus(status)` **e** status permite ver resultados estáveis. Spec: “busca completa”.

**Interpretation**:
- `pending` / `running` → cliques ignorados (FR-011/012).
- `completed` → sort ativo.
- `cancelled` / `failed` com itens na tela → **permitir** sort (mesmo comportamento que completed): usuário ainda quer rankear o que foi coletado; não é “processamento”.

**Rationale**: Clarification B = aparência normal + ignore enquanto processa. Terminais com dados não são processamento.

**Alternatives considered**:
- Só `completed` estrito — rejeitado por piorar UX em cancelado com resultados.
- Desabilitar visualmente — rejeitado (clarification B).

## R3 — Nulls last + toggle

**Decision**:
- `hasRating(value)`: número finito; `null`/`undefined`/`''`/`'N/A'` (case-insensitive) → sem nota.
- Comparador: primeiro particiona com-nota vs sem-nota; sem-nota sempre depois; com-nota compara numericamente por `direction`.
- Ciclo de estado: `null` → `desc` → `asc` → `desc` … (nunca volta a `null` só com cliques; reset só em nova busca / troca de `items` identity via `searchId` ou reset prop).

**Rationale**: Casa com FR-003–005 e edge cases.

**Alternatives considered**:
- Terceiro clique volta à ordem original — fora da spec (só toggle asc/desc).

## R4 — Reset de estado

**Decision**: Resetar `ratingSort` para `null` quando `searchId` mudar (prop) ou quando `page.js` iniciar nova busca (já zera filtro — passar `key={search?.id}` no `BusinessList` ou `useEffect` em searchId).

**Rationale**: Assumption da spec.

## R5 — Acessibilidade

**Decision**: Cabeçalho como `<button type="button">` dentro do `<th>` (ou `th` com `tabIndex={0}`, `role="button"`, Enter/Space). `aria-sort="descending"|"ascending"|"none"`.

**Rationale**: Assumption de teclado; sem mudar design system além do necessário.

## R6 — Backend

**Decision**: Nenhuma alteração em `backend/`.

**Rationale**: Clarification + FR-010.
