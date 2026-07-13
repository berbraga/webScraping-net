# Research: Paginação da Lista de Resultados

## R1 — Onde viver a paginação

**Decision**: Funções puras em `frontend/lib/paginateResults.js`; estado `currentPage` (1-based) em `BusinessList`, após `sortByRating` sobre os `items` já filtrados.

**Rationale**: Spec (FR-012) + padrão das features 004/005; pipeline observável: filtrar (page) → ordenar (BusinessList) → paginar (BusinessList).

**Alternatives considered**:
- Paginar em `page.js` — rejeitado (duplica conhecimento de sort ou força lift do sort).
- Lib (TanStack Table) — rejeitado (YAGNI).

## R2 — Limiar e tamanho de página

**Decision**: Constante única `PAGE_SIZE = 60`. Controles só se `displayableCount > PAGE_SIZE`. Cada página: no máximo 60 itens.

**Rationale**: Clarifications da spec (limiar = tamanho = 60).

**Alternatives considered**:
- Limiar 60 com page size 20 — rejeitado (usuário pediu paginação só acima de 60).

## R3 — Pipeline e reset de página

**Decision**:
1. `filtered = filterByName(businesses, nameFilter)` em `page.js`
2. `sorted = sortByRating(filtered, direction)` se sort ativo
3. Se `sorted.length > 60` → `pageSlice = slicePage(sorted, currentPage, 60)`; senão renderizar `sorted` inteiro sem chrome de paginação
4. Reset `currentPage = 1` quando: `searchId` muda; `nameFilter` muda (detectar via mudança dos `items` / prop ou `key`); sentido de ordenação muda

**Rationale**: FR-006–008; SC-006.

**Alternatives considered**:
- Manter página ao filtrar se ainda válida — rejeitado (spec exige página 1).

## R4 — Carga completa da lista (take)

**Decision**: `listBusinesses(id)` passa `take=200` (teto absoluto do produto já usado no backend). Sem novo contrato; a API já aceita `skip`/`take`.

**Rationale**: Spec assume lista carregada cobrindo o coletado; default atual da API (`take=100`) truncaria buscas > 100 e impediria paginar até 200.

**Alternatives considered**:
- Paginação via API a cada clique — rejeitado (clarification A / FR-012); quebraria filtro/sort client-side sem lógica no servidor.
- `take=100` — rejeitado ( AbsoluteMaxResults = 200 ).

## R5 — Controles e rodapé

**Decision**:
- Botões “Anterior” / “Próxima” sempre ambos visíveis quando a paginação está ativa; `disabled` nos extremos.
- Rodapé: se paginação ativa → `Mostrando {start}–{end} de {total}`; senão → `{n} de {total} resultados` (comportamento atual).
- Sem label obrigatório “Página X de Y” além do rodapé.

**Rationale**: Clarifications B + A (rodapé) + B (≤60).

**Alternatives considered**:
- Ocultar botões nos extremos — rejeitado (clarification).
- Só “Página X de Y” — rejeitado (clarification do rodapé).

## R6 — Backend

**Decision**: Nenhuma alteração em `backend/` (código). Apenas uso do `take` já existente a partir do frontend.

**Rationale**: FR-012; YAGNI.
