# Data Model: Busca de Comércios no Google Maps

**Feature**: `001-maps-business-lookup`  
**Storage**: MongoDB

## Collections

### `searches`

Representa uma execução de busca/coleta iniciada pelo usuário.

| Field | Type | Rules |
|-------|------|--------|
| `_id` | ObjectId / string | Gerado pelo sistema |
| `region` | string | Obrigatório, não vazio (trim) |
| `query` | string | Termo/categoria obrigatório, não vazio |
| `maxResults` | int | > 0; default configurável (ex.: 50) |
| `status` | string enum | Ver estados abaixo |
| `totalFound` | int | ≥ 0; tamanho da lista descoberta |
| `processedCount` | int | ≥ 0; itens já percorridos na coleta de detalhes |
| `failedCount` | int | ≥ 0 |
| `errorMessage` | string? | Preenchido se `status = failed` |
| `createdAt` | datetime (UTC) | Obrigatório |
| `updatedAt` | datetime (UTC) | Obrigatório |
| `completedAt` | datetime (UTC)? | Quando terminal |

**Status (`SearchStatus`)**:

```text
pending → running → completed
                 → cancelled
                 → failed
```

- `pending`: criada, aguardando worker
- `running`: listagem e/ou enriquecimento em andamento
- `completed`: terminou (lista vazia também pode ser `completed`)
- `cancelled`: cancelada pelo usuário; resultados parciais preservados
- `failed`: erro fatal da busca (ex.: região inválida / falha na fonte)

**Validation**:
- Não aceitar `region` ou `query` só com espaços
- `maxResults` limitado por teto de configuração (ex.: 200)

### `businesses`

Um documento por comércio vinculado a uma busca.

| Field | Type | Rules |
|-------|------|--------|
| `_id` | ObjectId / string | Gerado pelo sistema |
| `searchId` | string | Obrigatório; FK lógica → `searches._id` |
| `externalId` | string? | ID estável da fonte (Place ID), quando houver |
| `name` | string | Obrigatório após descoberta; não vazio |
| `phone` | string? | null = indisponível |
| `website` | string? | null = indisponível; se presente, URL absoluta |
| `rating` | number? | null = indisponível; tipicamente 0–5 |
| `detailStatus` | string enum | `pending` \| `enriched` \| `failed` \| `skipped` |
| `detailError` | string? | Se `detailStatus = failed` |
| `createdAt` | datetime (UTC) | Obrigatório |
| `updatedAt` | datetime (UTC) | Obrigatório |

**Notes**:
- Campos ausentes usam `null` (FR-006); UI pode mostrar ícone de X
- Deduplicação preferencial por `externalId` dentro do mesmo `searchId`
- Índice: `{ searchId: 1 }`; único parcial `{ searchId: 1, externalId: 1 }` quando `externalId` existe

## Relationships

```text
Search 1 ─── * Business
```

- Apagar ou arquivar busca (futuro) implica decidir cascade; v1 não exige delete.

## State transitions (Business.detailStatus)

```text
pending → enriched   (detalhes obtidos, campos opcionais podem ser null)
pending → failed     (erro ao enriquecer; comércio permanece na lista)
pending → skipped    (busca cancelada antes de processar o item)
```

## Derived views (não persistidos)

- Progresso: `processedCount / max(totalFound, 1)` a partir de `searches`
- Export CSV: projeção de `businesses` por `searchId` nas colunas da spec
