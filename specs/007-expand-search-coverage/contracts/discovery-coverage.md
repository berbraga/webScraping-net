# Contract: Discovery com Cobertura Ampliada

**Escopo**: Comportamento backend. Sem novos endpoints REST. Sem mudanças de formulário.

## POST `/api/searches`

| Antes | Depois |
|-------|--------|
| Descoberta sync até o fim; 201 com `totalFound` final da única consulta | Cria busca `running`, enfileira discovery; 201 pode ter `totalFound=0` (ou baixo); cliente faz poll |

Body inalterado: `region`, `query`, `maxResults`.

## GET `/api/searches/{id}`

Observável durante `running`:

- `totalFound` **aumenta** ao longo dos lotes de descoberta
- `processedCount` pode aumentar **antes** do fim da descoberta (enrich intercalado)
- `maxResults` permanece o valor pedido

Estados terminais:

| Status | Itens | Notas |
|--------|-------|-------|
| `completed` | 0..L | Esgotou ou atingiu L; enrich concluído |
| `failed` | 0..parcial | Erro em fatia; itens **não** apagados; `errorMessage` setado |
| `cancelled` | parcial | Como hoje |

## Lookup interno (por fatia)

Entrada lógica por fatia:

- `effectiveRegion` / `effectiveQuery` (textQuery Google = `"{query} in {region}"` ou variante com setor)
- `maxResults` = min(restante, cap do provedor por consulta)

Saída: listings; orquestrador deduplica contra acumulado.

## Fake (harness)

- Máx. **60** resultados por `SearchAsync`
- Fatias deslocam janela no catálogo (≥200) para permitir `totalFound=200` com L=200

## Não-objetivos

- Novos campos de UI
- Garantir 200 em qualquer região real do mundo
- Geocode/grid `locationRestriction` na v1 (possível v2)
