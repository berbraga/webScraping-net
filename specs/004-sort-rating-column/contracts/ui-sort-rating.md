# UI Contract: Ordenação por Avaliação

**Escopo**: Frontend only. Sem novos endpoints.

## Props / sinais

| Sinal | Origem | Comportamento |
|-------|--------|----------------|
| `items` | lista já filtrada (page) | entrada da tabela |
| `searchStatus` (ou `sortable`) | `search.status` | se processing → ignore clicks |
| Cabeçalho “Avaliação” | `BusinessList` | hover pointer; clique efetivo só se não processing |

## Interação

| Estado da busca | Hover pointer | Clique | Ícone ↑/↓ |
|-----------------|---------------|--------|-----------|
| `pending` / `running` | permitido (aparência normal) | ignorado | nenhum |
| `completed` / `cancelled` / `failed` (com dados) | sim | aplica/alterna sort | conforme `desc`/`asc` |

## Ordem resultante (contrato observável)

- `desc`: ratings finitos decrescentes, depois ausentes.
- `asc`: ratings finitos crescentes, depois ausentes.
- Sem sort ativo: ordem de `items` inalterada.

## Acessibilidade

- Controle acionável por teclado.
- `aria-sort` no cabeçalho Avaliaçao quando ativo.

## Não-objetivos

- Ordenar outras colunas.
- Mudar API/CSV.
- Persistir preferência de sort.
