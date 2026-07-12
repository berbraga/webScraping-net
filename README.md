# webScraping-net

Aplicação para buscar comércios (região + termo), coletar Nome, Telefone, Site e Avaliação, acompanhar progresso e exportar CSV.

## Stack

- **Frontend**: Next.js (JavaScript) em `frontend/`
- **Backend**: ASP.NET Core (.NET 8) em `backend/`
- **Banco**: MongoDB (NoSQL)

## Pré-requisitos

- Node.js 20+
- .NET 8 SDK
- Docker (para Mongo) ou MongoDB local
- (Opcional) chave Google Places API — sem chave, o backend usa fonte fake para desenvolvimento

## Subir Mongo

```bash
docker compose up -d
```

## Backend

```bash
cd backend
dotnet restore
dotnet run --project WebScraping.Api
```

API: `http://localhost:5080`  
Health: `http://localhost:5080/api/health`

Configure em `backend/WebScraping.Api/appsettings.Development.json` ou variáveis de ambiente:

- `Mongo__ConnectionString`
- `Mongo__DatabaseName`
- `GooglePlaces__ApiKey`
- `GooglePlaces__UseFakeSource` (`true` para fake)

## Frontend

```bash
cd frontend
cp .env.example .env.local
npm install
npm run dev
```

UI: `http://localhost:3000`

## Testes

```bash
cd backend && dotnet test
cd frontend && npm test
```

## Documentação da feature

Veja `specs/001-maps-business-lookup/quickstart.md` para cenários de validação ponta a ponta.
