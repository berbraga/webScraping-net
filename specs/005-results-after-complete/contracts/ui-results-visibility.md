# UI Contract: Visibilidade dos Resultados

**Escopo**: Frontend only. Sem novos endpoints.

## Regra

```text
showResultsArea = search != null && !isProcessingStatus(search.status) && !loadingSubmit
```

Durante `showResultsArea === false` e busca ativa: apenas progresso (+ cancel).

## Observável

| Situação | Tabela / nomes | Progresso |
|----------|----------------|-----------|
| `running` / `pending` | oculto | visível |
| `completed` com itens | visível (colunas nome, telefone, site, avaliação) | pode permanecer como status |
| `cancelled` / `failed` | área de resultados visível (itens e/ou vazio) | status terminal |
| Nova busca após terminal | oculta de novo até terminal | progresso da nova busca |

## Não-objetivos

- Alterar API de listagem.
- Remover ordenação/filtro/export após revelar.
- Esconder contadores de progresso durante a coleta.
