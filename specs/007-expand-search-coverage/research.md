# Research: Expandir Cobertura da Busca

## R1 — Onde vive a cobertura ampliada

**Decision**: Orquestrador `DiscoverSearchHandler` na Application: loop de fatias → dedupe → persist → atualizar `TotalFound` → enriquecer pending do lote → próxima fatia; `completed`/`failed` só no fim (ou na falha).

**Rationale**: Spec FR-012/013/011; evita corrida do `EnrichBusinessesHandler` atual que marca `Completed` quando a lista pending esvazia.

**Alternatives considered**:
- Só expandir dentro de `GooglePlacesBusinessLookupSource.SearchAsync` — rejeitado (Fake não exercita; enrich intercalado fica opaco; status por lote difícil).
- Geocode + grid de `locationRestriction` na v1 — adiado (mais APIs/custo); ver R3.

## R2 — Discovery assíncrona

**Decision**: `StartSearchHandler` cria busca `Running`, enfileira job de **discovery** (mesma `ISearchJobQueue` ou fila com tipo); HTTP retorna 201 cedo; worker executa `DiscoverSearchHandler`.

**Rationale**: Sem background, `totalFound` por lote não é observável via GET durante a coleta (SC-007).

**Alternatives considered**:
- Discovery sync no POST — rejeitado (SC-007).
- SignalR/streaming — YAGNI.

## R3 — Estratégia de fatias (v1)

**Decision**: `TextCoveragePlanner` gera fatias ordenadas:

1. Fatia 0: query efetiva atual (`{query} in {region}`).
2. Fatias seguintes: `{query} in {sector}, {region}` para setores em config (default: `centro`, `norte`, `sul`, `leste`, `oeste` — configurável em `SearchOptions`).

Cada fatia chama o lookup pedindo até `min(60, remaining)` (ou `MaxResults` restante); Google já pagina até o teto da fatia.

**Rationale**: Sem Geocoding; YAGNI; suficiente para exercitar >60 e melhorar cobertura em regiões textuais BR; Fake espelha com janelas de catálogo.

**Alternatives considered**:
- Geocode + retângulos `locationRestriction` — melhor precisão geográfica; adiar para v2 se setores textuais forem fracos.
- Nearby Search tiling — muda modelo de busca; fora de escopo v1.

## R4 — Parada e falha

**Decision**:
- Parar expansão ao atingir L **ou** quando um lote (após dedupe contra já vistos) adiciona **0** novos.
- Fatia 0 com 0 itens → `Completed` vazio (comportamento atual).
- Exceção em fatia N>0 (ou N=0 com itens já gravados) → `Failed` + `ErrorMessage`, **não** apagar businesses; não marcar `Completed`.
- Cancelamento: respeitar status `Cancelled` entre fatias (como enrich).

**Rationale**: Clarifications A + FR-014/011/010.

## R5 — Enriquecimento intercalado

**Decision**: Extrair `EnrichPendingAsync` que processa pending **sem** setar `Completed`. Orquestrador chama após cada lote com novos itens; ao terminar todas as fatias com sucesso, seta `Completed`.

Worker deixa de tratar “só enrich” como dono do completed para buscas novas — discovery job é a entrada; enrich one-shot legado pode permanecer para testes se apontar para `EnrichPending` + complete explícito.

**Rationale**: FR-013 + evita completed prematuro.

## R6 — Fake / harness

**Decision**:
- `FakeBusinessLookupSource.SearchAsync` devolve no máximo **60** por chamada (simula teto do provedor).
- Aceita marcador de fatia no `region` (ex.: sufixo `|slice=2`) **ou** parâmetro dedicado se a abstração for estendida; retorna `Skip(slice * 60).Take(min(60, maxResults))` do catálogo (≥200 itens).
- Planner Fake/Application usa o mesmo contrato de fatias.

**Rationale**: SC-001/002/007/008 testáveis no CI.

## R7 — Contrato REST / Frontend

**Decision**: Sem novos endpoints nem campos de formulário. Comportamento: POST retorna mais cedo com `running` e `totalFound` podendo ser 0; cliente já faz poll (home). Documentar no contract.

**Rationale**: FR-009.
