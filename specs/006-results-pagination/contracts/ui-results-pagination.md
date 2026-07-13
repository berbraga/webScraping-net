# UI Contract: Paginação da Lista de Resultados

**Escopo**: Frontend only. Sem novos endpoints. Uso opcional de `take` já existente em `GET /api/searches/{id}/businesses`.

## Carga de dados

| Ação | Contrato |
|------|----------|
| Listar comércios para a home | `GET .../businesses?take=200` (ou equivalente) para cobrir o teto do produto |
| Mudança de página (Anterior/Próxima) | **Sem** nova request |

## Sinais de UI

| Sinal | Origem | Comportamento |
|-------|--------|----------------|
| `items` | lista já filtrada (page) | entrada; sort depois paginação |
| `totalCount` | length pré-filtro ou filtrado conforme uso atual | rodapé sem paginação / coerência |
| `searchId` | busca atual | reset página → 1 |
| Controles | só se `sortedOrFiltered.length > 60` | Anterior + Próxima |

## Controles

| Situação | Anterior | Próxima |
|----------|----------|---------|
| Sem paginação (≤ 60) | não renderizados | não renderizados |
| Página 1 (de N>1) | visível, **disabled** | visível, enabled |
| Página intermediária | visível, enabled | visível, enabled |
| Última página | visível, enabled | visível, **disabled** |

## Rodapé

| Situação | Texto (equivalente) |
|----------|---------------------|
| Paginação ativa | `Mostrando {start}–{end} de {total}` |
| Sem paginação | `{n} de {total} resultados` |

## Pipeline observável

```text
businesses → filterByName → [sortByRating?] → [slice page se > 60] → tabela
```

## Não-objetivos

- Furar teto do provedor de mapas.
- Paginação server-driven por clique.
- Alterar formulário de busca / CSV / gate da feature 005.
