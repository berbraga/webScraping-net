# Quickstart: Busca de Comércios no Google Maps

**Feature**: `001-maps-business-lookup`  
**Goal**: Validar ponta a ponta busca → progresso → lista → export CSV.

## Prerequisites

- Node.js 20+
- .NET 8 SDK
- Docker (MongoDB local) **ou** MongoDB instalado
- Chave da Google Places API (New) com Places habilitado
- Variáveis (exemplo):

```bash
export MONGODB_URI="mongodb://localhost:27017"
export MONGODB_DATABASE="webscraping"
export GOOGLE_PLACES_API_KEY="your-key"
export NEXT_PUBLIC_API_BASE_URL="http://localhost:5080"
```

## Setup

```bash
# Mongo
docker run -d --name webscraping-mongo -p 27017:27017 mongo:7

# Backend
cd backend
dotnet restore
dotnet build
dotnet run --project WebScraping.Api

# Frontend (outro terminal)
cd frontend
npm install
npm run dev
```

UI esperada: `http://localhost:3000`  
API health: `http://localhost:5080/api/health`

## Validation scenarios

### 1. Busca com resultados (US1 + US2)

1. Abrir a UI; informar região (ex.: `Pinheiros, São Paulo`) e termo (`cafeterias`).
2. Definir limite (ex.: 10) e iniciar.
3. **Esperado**: status `running` → lista com nomes; progresso sobe
   (`processedCount` / `totalFound`); ao fim `completed`.
4. Itens sem telefone/site/avaliação mostram ausência explícita (ícone X / vazio).

### 2. Busca sem resultados

1. Região + termo improvável (ex.: termo nonsense em localidade válida).
2. **Esperado**: mensagem clara de lista vazia; status `completed` (não erro silencioso).

### 3. Cancelamento (FR-011)

1. Iniciar busca com limite alto.
2. Cancelar durante `running`.
3. **Esperado**: status `cancelled`; itens já enriquecidos permanecem listados.

### 4. Export CSV (US3)

1. Com busca `completed` ou `cancelled` com itens.
2. Clicar exportar / chamar `GET /api/searches/{id}/export`.
3. **Esperado**: arquivo com header `Nome,Telefone,Site,Avaliacao` e uma linha por comércio.

### 5. Testes automatizados

```bash
# Backend
cd backend
dotnet test

# Frontend
cd frontend
npm test
```

**Esperado**: suite verde sem chamar Google real (fakes/mocks da fonte).

## References

- Modelo: [data-model.md](./data-model.md)
- Contrato: [contracts/rest-api.md](./contracts/rest-api.md)
- Decisões: [research.md](./research.md)
