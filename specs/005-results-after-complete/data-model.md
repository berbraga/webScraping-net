# Data Model: Visibilidade da Área de Resultados (UI)

Sem persistência. Modelo de estado de visualização:

## Search status (já existente)

| Status | Processing? | Mostra área de resultados? |
|--------|-------------|----------------------------|
| `pending` | sim | não |
| `running` | sim | não |
| `completed` | não | sim |
| `cancelled` | não | sim |
| `failed` | não | sim |
| (sem busca) | — | não |
| submit `loading` | sim (UI) | não |

## UI regions

| Região | Durante processing | Em terminal |
|--------|--------------------|-------------|
| Formulário | sim | sim |
| `SearchProgress` | sim | sim (resumo) |
| Toolbar (export/filtro) | não | sim |
| `BusinessList` / nomes | **não** | sim |

## Transições

```text
idle → submit → processing (sem tabela)
processing → completed|cancelled|failed → tabela/controles visíveis
terminal → nova busca → processing (tabela some de novo)
```
