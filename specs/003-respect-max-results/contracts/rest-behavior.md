# Contract: Comportamento do limite máximo (REST inalterado)

**Base**: contrato HTTP de `specs/001-maps-business-lookup/contracts/rest-api.md` — **sem breaking changes**.

Esta feature clarifica o **comportamento observável** de `maxResults` / `totalFound`.

## POST `/api/searches`

Request (inalterado):

```json
{
  "region": "Florianopolis",
  "query": "restaurantes",
  "maxResults": 100
}
```

### Garantias de comportamento

| Condição | Resposta esperada |
|----------|-------------------|
| `maxResults` válido (ex.: 100) | `201` com `maxResults: 100` no summary |
| Oferta do provedor ≥ 100 e páginas disponíveis | Após conclusão do discovery, `totalFound` = 100 (não 20) |
| Oferta / páginas do provedor < `maxResults` | `totalFound` = quantidade real; status pode ir a `completed` sem erro de “limite” |
| `maxResults` > AbsoluteMax (200) | `400` (validação existente) |

## GET `/api/searches/{id}`

Campos relevantes (shape inalterado):

```json
{
  "maxResults": 100,
  "totalFound": 100,
  "processedCount": 100,
  "status": "completed"
}
```

**Regra**: `maxResults` reflete o pedido do usuário; `totalFound` reflete o coletado. É inválido, para oferta ampla, completar discovery com `totalFound === 20` quando `maxResults === 100` **e** o provedor ainda teria páginas.

## GET `/api/searches/{id}/businesses`

Inalterado. Com `totalFound` > 20, `items.length` (com `take` adequado) e `total` devem refletir o volume coletado.

## GET `/api/searches/{id}/export`

Inalterado. CSV deve incluir todas as linhas coletadas (incluindo > 20).

## Contrato interno: `IBusinessLookupSource`

```text
SearchAsync(region, query, maxResults) → até maxResults listings
```

Implementações:

- **Fake**: já conforme.
- **GooglePlaces**: MUST paginar (`pageSize` ≤ 20, `pageToken` / `nextPageToken`) até `maxResults` ou fim das páginas.
