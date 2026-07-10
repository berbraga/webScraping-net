# REST API Contract: Maps Business Lookup

**Base URL (dev)**: `http://localhost:5080`  
**Content-Type**: `application/json` (exceto export CSV)

Autenticação: nenhuma na v1.

## Endpoints

### POST `/api/searches`

Inicia uma busca/coleta.

**Request**:

```json
{
  "region": "Centro, São Paulo",
  "query": "padarias",
  "maxResults": 50
}
```

| Field | Required | Notes |
|-------|----------|--------|
| `region` | yes | Localidade textual |
| `query` | yes | Termo/categoria |
| `maxResults` | no | Default do servidor se omitido |

**Responses**:
- `201 Created` — body: Search summary; `Location: /api/searches/{id}`
- `400 Bad Request` — validação (`region`/`query` vazios, `maxResults` inválido)

### GET `/api/searches/{id}`

Obtém status e progresso da busca.

**Responses**:
- `200 OK` — Search summary
- `404 Not Found`

**Search summary (exemplo)**:

```json
{
  "id": "665f1a...",
  "region": "Centro, São Paulo",
  "query": "padarias",
  "maxResults": 50,
  "status": "running",
  "totalFound": 40,
  "processedCount": 12,
  "failedCount": 1,
  "errorMessage": null,
  "createdAt": "2026-07-10T20:00:00Z",
  "updatedAt": "2026-07-10T20:01:10Z",
  "completedAt": null
}
```

### GET `/api/searches/{id}/businesses`

Lista comércios da busca (para UI).

**Query**:
- `skip` (default 0), `take` (default 100, max 200)

**Responses**:
- `200 OK`:

```json
{
  "items": [
    {
      "id": "665f2b...",
      "name": "Padaria Exemplo",
      "phone": "+55 11 3000-0000",
      "website": "https://exemplo.com",
      "rating": 4.5,
      "detailStatus": "enriched",
      "detailError": null
    }
  ],
  "total": 40
}
```

- `404` se a busca não existir

Campos `phone`, `website`, `rating` podem ser `null` (indisponível).

### POST `/api/searches/{id}/cancel`

Solicita cancelamento cooperativo.

**Responses**:
- `200 OK` — summary com `status: cancelled` ou ainda `running` até o worker parar
- `404 Not Found`
- `409 Conflict` — busca já em estado terminal (`completed` / `failed`)

### GET `/api/searches/{id}/export`

Exporta CSV da coleta atual (inclui parciais se cancelada).

**Responses**:
- `200 OK` — `Content-Type: text/csv; charset=utf-8`  
  `Content-Disposition: attachment; filename="search-{id}.csv"`
- `404 Not Found`
- `409 Conflict` — ainda `pending` sem itens (opcional: permitir CSV vazio só com header)

**CSV columns (ordem fixa)**:

```text
Nome,Telefone,Site,Avaliacao
```

Valores ausentes → célula vazia. Aspas/escapes conforme RFC 4180.

### GET `/api/health`

**Responses**: `200 OK` `{ "status": "ok" }`

## Error body (padrão)

```json
{
  "title": "Validation failed",
  "detail": "Region is required",
  "status": 400
}
```

## Frontend mapping (user stories)

| Story | API |
|-------|-----|
| US1 listar | `POST /searches` + poll `GET /searches/{id}` + `GET .../businesses` |
| US2 detalhes/progresso | mesmo poll; `detailStatus` / campos preenchidos |
| US3 exportar | `GET .../export` |
| Cancelar | `POST .../cancel` |
