# Quickstart: Redesign da Home

**Feature**: `002-home-ui-redesign`  
**Goal**: Validar home vs prints sem alterar a API.

## Prerequisites

- Backend e frontend da feature 001 já rodando (API em `http://localhost:5080`)
- `NEXT_PUBLIC_API_BASE_URL` apontando para a API
- Opcional: `GooglePlaces:UseFakeSource=true` para dados estáveis

## Setup

```bash
# Terminal 1 — API
cd backend
dotnet run --project WebScraping.Api

# Terminal 2 — UI
cd frontend
npm install
npm run dev
```

Abrir `http://localhost:3000`.

## Validation vs prints

### 1. Idle (`references/01-form-idle.png`)

1. Abrir a home sem busca.  
2. **Esperado**: título, subtítulo, card com 3 campos em linha, botão
   "Buscar comércios" full-width, visual próximo ao print.

### 2. Processando (`references/02-processing.png`)

1. Buscar com região/termo que retorne vários itens (ou fake).  
2. **Esperado**: botão "Buscando...", status processando destacado, fração
   processados/total, barra avançando.

### 3. Concluído (`references/03-completed-results.png`)

1. Aguardar `completed`.  
2. **Esperado**: status completed, Exportar CSV, Filtrar por nome, tabela com
   4 colunas, X em ausências, rodapé "N de M resultados".  
3. Filtrar por parte de um nome → tabela e rodapé atualizam.  
4. Exportar CSV → download pelo endpoint existente.

### 4. Regressão API

- Nenhuma mudança em `specs/001-maps-business-lookup/contracts/rest-api.md`
- `frontend/lib/searchesApi.js` mantém as mesmas funções públicas

## Automated tests

```bash
cd frontend
npm test
```

**Esperado**: testes de form, progresso, lista/X, filtro e export verdes.

## References

- Spec: [spec.md](./spec.md)
- UI contract: [contracts/ui-home.md](./contracts/ui-home.md)
- View model: [data-model.md](./data-model.md)
- Decisions: [research.md](./research.md)
