# Data Model: Paginação da Lista de Resultados (UI)

Sem persistência. Estado e regras de visualização:

## Conjunto exibível

Lista após:

1. `filterByName` (page)
2. `sortByRating` (se ativo, BusinessList)

| Campo derivado     | Significado                                      |
|--------------------|--------------------------------------------------|
| `total`            | `length` do conjunto exibível                    |
| `paginationActive` | `total > 60`                                     |

## Página (estado de UI)

| Campo          | Tipo    | Regras                                      |
|----------------|---------|---------------------------------------------|
| `currentPage`  | number  | 1-based; mínimo 1; máximo `ceil(total/60)`  |
| `pageSize`     | const   | **60**                                      |

### Transições

```text
* --(searchId muda)--> page = 1
* --(filtro muda / items filtrados mudam por filtro)--> page = 1
* --(sentido de ordenação muda)--> page = 1
page --(próxima, se page < last)--> page + 1
page --(anterior, se page > 1)--> page - 1
paginationActive false --> sem controles; renderiza lista inteira
```

## Fatia observável

Para `page` e `total`:

- `startIndex = (page - 1) * 60` (0-based)
- `endIndexExclusive = min(startIndex + 60, total)`
- Itens renderizados: `slice(startIndex, endIndexExclusive)`
- Rodapé (paginação ativa): início = `startIndex + 1`, fim = `endIndexExclusive`, total = `total`

## Relação com exportação

CSV continua via endpoint de export da busca completa — independente da página visível.
