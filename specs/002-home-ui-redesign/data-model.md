# Data Model (UI View): Redesign da Home

**Feature**: `002-home-ui-redesign`  
**Note**: Modelo de *apresentação* na UI. Persistência/API permanecem as da feature 001.

## View states

| State | Quando | UI principal |
|-------|--------|--------------|
| `idle` | Sem busca ativa | Formulário + botão "Buscar comércios" |
| `processing` | `status` ∈ {pending, running} ou `loading` | Botão "Buscando...", status + barra |
| `completed` | `status` = completed (ou cancelled com itens) | Status, export, filtro, tabela |
| `empty` | Busca terminal com 0 itens | Mensagem de vazio |
| `error` | Falha na chamada / status failed | Mensagem de erro no layout |

## HomeViewModel (client)

| Field | Source | Notes |
|-------|--------|--------|
| `region`, `query`, `maxResults` | formulário | Enviados à API existente |
| `search` | `GET /api/searches/{id}` | Summary da API |
| `businesses` | `GET .../businesses` | Lista completa carregada |
| `nameFilter` | input local | Não persiste |
| `filteredBusinesses` | derivado | `filterByName(businesses, nameFilter)` |
| `progressRatio` | derivado | `processedCount / max(totalFound, 1)` quando totalKnown |

## Result row (apresentação)

| Field | Display |
|-------|---------|
| `name` | texto |
| `phone` | texto ou marcador X se null/vazio |
| `website` | link ou X |
| `rating` | número ou X |
| `detailStatus` | não precisa de coluna extra nos prints |

## Filter rules

- Comparação case-insensitive
- Match por substring no `name`
- Rodapé: `{filteredCount} de {totalCount} resultados`
- Filtro vazio → mostra todos

## Status display mapping

| API `status` | Label visual (print) | Cor de destaque |
|--------------|----------------------|-----------------|
| `pending` / `running` | processando | âmbar/mostarda |
| `completed` | completed | verde |
| `cancelled` | cancelled | neutro |
| `failed` | failed | vermelho/erro |
