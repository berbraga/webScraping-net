# UI Contract: Home Redesign

**Feature**: `002-home-ui-redesign`  
**Scope**: Contrato de interface da home. **API REST inalterada** — ver
`specs/001-maps-business-lookup/contracts/rest-api.md`.

## Visual structure (must match references)

### Idle — `references/01-form-idle.png`

1. Título + subtítulo centralizados  
2. Card branco com sombra suave  
3. Três campos em linha: Região | Termo / categoria | Limite máximo  
4. Botão full-width: "Buscar comércios"

### Processing — `references/02-processing.png`

1. Mesmo card; botão: "Buscando..."  
2. Abaixo: `Status: {label} — {processed}/{total} processados`  
3. Barra de progresso proporcional

### Completed — `references/03-completed-results.png`

1. Status completed + fração  
2. "Exportar CSV" (esquerda) + "Filtrar por nome..." (direita)  
3. Tabela: Nome | Telefone | Site | Avaliação  
4. Rodapé: `{filtrados} de {total} resultados`  
5. Ausências: marcador X

## Interaction contract

| Ação do usuário | Efeito | API |
|-----------------|--------|-----|
| Submit formulário | Cria busca + inicia polling | `POST /api/searches` (existente) |
| Polling ~2s | Atualiza summary + lista | `GET /searches/{id}`, `GET .../businesses` |
| Exportar CSV | Download | `GET .../export` (existente) |
| Digitar filtro | Filtra tabela localmente | nenhuma |
| Cancelar (se visível) | Cancela busca | `POST .../cancel` (existente) |

## Non-goals

- Novos endpoints  
- Mudança de campos/payloads da API  
- Autenticação / multi-página

## Component surface (frontend)

| Component | Props essenciais |
|-----------|------------------|
| `SearchForm` | `onSubmit`, `disabled`, `busy` (texto do botão) |
| `SearchProgress` | `search`, opcional `onCancel` |
| `NameFilter` | `value`, `onChange` |
| `BusinessList` | `items`, `totalCount`, `emptyMessage` |
| `ExportButton` | `searchId`, `disabled` |
