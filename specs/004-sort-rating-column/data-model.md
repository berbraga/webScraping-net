# Data Model: Ordenação por Avaliação (UI)

Sem persistência. Estado e regras de visualização:

## Business row (já existente na UI)

| Campo   | Uso na ordenação                                      |
|---------|--------------------------------------------------------|
| `rating`| Número ou ausente; chave de comparação                |
| `id`    | Estabilidade de React key; não obrigatório no desempate |
| `name`  | Não usado no sort v1                                  |

## Sort direction (estado de UI)

| Valor    | Significado                                      |
|----------|--------------------------------------------------|
| `null`   | Sem ordenação ativa (ordem original dos `items`) |
| `desc`   | Maior → menor; sem nota no final                 |
| `asc`    | Menor → maior; sem nota no final                 |

### Transições

```text
null  --(clique efetivo)--> desc
desc  --(clique efetivo)--> asc
asc   --(clique efetivo)--> desc
*     --(nova busca / searchId muda)--> null
*     --(clique enquanto processing)--> *  (sem mudança)
```

## Regras de “sem nota”

Tratar como ausente: `null`, `undefined`, `''`, string `'N/A'` (qualquer caixa).  
Números válidos: `Number.isFinite(n)`.

## Relação com filtro

1. Filtrar por nome (`filterByName`)
2. Se `direction != null` e clique/elegibilidade OK → `sortByRating(filtered, direction)`
3. Renderizar

Export CSV: inalterado (ordem do servidor).
