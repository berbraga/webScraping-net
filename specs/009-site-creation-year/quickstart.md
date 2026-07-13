# Quickstart: Ano de Criação via Copyright do Site

Validação E2E da feature 009. Contratos: [rest-site-creation-year.md](./contracts/rest-site-creation-year.md), [ui-site-creation-year.md](./contracts/ui-site-creation-year.md). Modelo: [data-model.md](./data-model.md).

## Pré-requisitos

- .NET 8, Node 20+, Mongo local (ou stack do projeto)
- Fakes: Places + `IWebsiteCopyrightYearLookup` com HTML/anos conhecidos
- Branch `009-site-creation-year`

## Setup

```bash
cd backend && dotnet restore && dotnet build
cd ../frontend && npm install
```

Config (`WebsiteCopyright`):

- `TimeoutSeconds=10`
- `MaxDegreeOfParallelism=10`
- `UseFakeLookup=true` para CI

## Testes

```bash
cd backend && dotnet test
cd ../frontend && npm test
```

Esperado: extractor (intervalos/footer); enrich fase (cache, paralelismo, null em falha, Completed após); API/CSV; coluna UI.

## Manual (fake)

1. Subir API + frontend.
2. Busca com itens com/sem website; 2 itens mesma URL se o fake permitir.
3. Durante `running`, Places pode completar antes do ano; só `completed` após fase copyright.
4. Tabela: **Criação do site** à direita de Site; ano ou ✕.
5. Export: header com **Criação do site** após Site.

## Critérios rápidos

| Critério | Verificação |
|----------|-------------|
| SC-002 / SC-003 | Timeout/null sem abortar; ≤~10s/URL |
| FR-012 / FR-014 | ≤10 paralelos; 1 GET por URL distinta |
| SC-004 / SC-005 | Coluna + CSV **Criação do site** |
| FR-011 | Nenhum WHOIS/RDAP |
