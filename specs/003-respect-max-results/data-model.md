# Data Model: Respeitar Limite Máximo de Resultados

Nenhuma mudança de schema. Comportamento esperado das entidades existentes:

## Search

| Campo | Papel nesta feature |
|-------|---------------------|
| `MaxResults` | Teto solicitado pelo usuário (1..AbsoluteMax). **Não** é alterado pelo adapter. |
| `TotalFound` | Quantidade de comércios efetivamente descobertos após lookup (≤ `MaxResults`). Pode ser < `MaxResults` se o provedor esgotar. **Não** deve travar em 20 se o limite for maior e houver mais páginas. |
| `ProcessedCount` / `FailedCount` | Enriquecimento posterior; denominador de progresso usa `TotalFound`. |
| `Status` | Fluxo existente (`Pending` → `Running` → `Completed` / `Cancelled` / `Failed`). |

### Regras de validação (inalteradas)

- `MaxResults` > 0
- `MaxResults` ≤ AbsoluteMax (200)
- Default se omitido: 50

### Invariante novo (comportamental)

- Após lookup bem-sucedido com oferta suficiente no provedor **e** páginas disponíveis, `TotalFound` MUST aproximar-se de `MaxResults` (igual quando o provedor fornecer itens o bastante), sem teto silencioso de uma página (20).

## Business

Inalterado. Cardinalidade: 0..N por `SearchId`, com N ≤ `Search.MaxResults` (após dedupe).

## Lookup (conceitual)

`IBusinessLookupSource.SearchAsync(region, query, maxResults)`:

- **Contrato**: retornar até `maxResults` listings distintos, ou menos se a fonte esgotar.
- **Google**: múltiplas páginas de até 20; acumular até `maxResults`.
- **Fake**: já implementa o contrato; manter como referência de comportamento.
