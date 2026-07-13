# Research: Respeitar Limite Máximo de Resultados

## R1 — Causa raiz do teto silencioso em 20

**Decision**: Tratar o bug como falha de paginação no adapter Google Places, não como bug de validação/UI.

**Rationale**:
- `StartSearchHandler` já passa `search.MaxResults` para `IBusinessLookupSource.SearchAsync`.
- `FakeBusinessLookupSource` já respeita `maxResults`.
- `GooglePlacesBusinessLookupSource` faz **uma** chamada com `pageSize = Math.Clamp(maxResults, 1, 20)` e field mask só `places.id,places.displayName` — **sem** `nextPageToken` e sem loop de páginas.
- Sintoma do usuário (limite 100 → 20/20 completed) casa exatamente com uma página máxima da API.

**Alternatives considered**:
- Alterar default/UI para 20 — rejeitado (esconde o problema; quebra a expectativa do campo).
- Mudar AbsoluteMaxResults — rejeitado (não é a causa).

## R2 — Paginação Places API (New) Text Search

**Decision**: Em `SearchAsync`, loopar páginas até `results.Count >= maxResults` ou ausência de `nextPageToken`.

**Rationale** (documentação Google Places API — Text Search New):
- `pageSize` máximo = **20** (valores maiores são coagidos a 20).
- Resposta pode incluir `nextPageToken`; próxima request usa `pageToken`.
- Field mask **deve** incluir `nextPageToken` (senão o token não vem na resposta).
- Demais parâmetros da request (ex.: `textQuery`) devem permanecer iguais entre páginas.
- Deduplicar por `externalId`/`id` ao acumular páginas (defensivo).

**Alternatives considered**:
- Várias queries com variações de texto para “furar” o teto — rejeitado (frágil, fora do escopo, complexidade alta).
- Nearby Search em paralelo — rejeitado (mudança de modelo de busca; YAGNI).

## R3 — Teto do provedor vs teto do produto (200)

**Decision**: Continuar paginando até o limite do usuário **ou** o provedor parar de retornar `nextPageToken`. Se o Google esgotar páginas antes (comunidade/docs frequentemente citam ~60 resultados por text search), `totalFound < maxResults` é válido (FR-003 / SC-004), **não** um bug — desde que não haja teto artificial em 20 com token ainda disponível.

**Rationale**: O produto não controla o inventário do provedor. O contrato do usuário é “até N, se disponível”.

**Alternatives considered**:
- Baixar AbsoluteMaxResults para 60 — possível melhoria futura de UX; fora do escopo desta correção.
- Mensagem explícita “provedor limitou a N” — nice-to-have futuro; não bloqueia P1.

## R4 — Estratégia de testes

**Decision**:
1. Teste unitário/integração do `GooglePlacesBusinessLookupSource` com `HttpMessageHandler` fake: 1ª página 20 + token → 2ª página 20 + token → 3ª página 10 sem token; `maxResults=50` → 50 itens.
2. Mesmo setup: `maxResults=100` mas só 2 páginas (40) sem token final → retorna 40 (sem erro).
3. Regressão Fake: `maxResults=100` continua gerando 100.
4. Teste de Application existente com Fake permanece verde; opcional ApiTest com Fake e limite alto.

**Rationale**: Constitution exige testes mapeáveis às user stories P1/P2 sem depender de chave Google real no CI.

**Alternatives considered**:
- Só teste manual com API real — insuficiente para gate de qualidade.

## R5 — Frontend / contrato REST

**Decision**: Nenhuma mudança de contrato ou UI nesta feature.

**Rationale**: Spec assume que a UI já envia o limite; status já expõe `maxResults` e `totalFound`. Após o fix, o progresso passa a refletir o total real coletado.

**Alternatives considered**:
- Badge “mostrando X de Y pedidos” — polish futuro, fora de escopo.
