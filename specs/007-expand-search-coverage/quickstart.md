# Quickstart: Validar cobertura ampliada

## Pré-requisitos

- Backend com Fake (sem chave Google) ou Google Places configurado
- `cd backend && dotnet test` / API em `http://localhost:5080`
- Frontend opcional para poll visual

## Testes automatizados

```bash
cd backend
dotnet test
```

Esperado após implementação:

- Planner gera fatia base + setores
- Fake: uma call ≤60; multi-fatia acumula até 200
- Discover: para em L; para em lote sem novos; falha de fatia → `failed` com itens
- Enrich intercalado: `processedCount` sobe antes do fim da descoberta (teste de orquestração)
- ApiTest: POST maxResults=100 → poll → `totalFound=100` com Fake

## Validação manual (Fake)

1. Subir API sem `GooglePlaces:ApiKey` (usa Fake).
2. POST busca `maxResults=100` (região/termo quaisquer; evitar `__empty__`).
3. Poll GET status: `totalFound` sobe (ex.: 60 → 100); status `running` depois `completed`.
4. Listar businesses: 100 distintos; CSV com 100 linhas de dados.
5. POST `maxResults=200` → `totalFound=200` se catálogo Fake permitir.
6. (Opcional) Google real: `restaurante` + cidade grande + 200 — esperar >60 quando setores trouxerem novos; não garantir 200.

## Critério rápido

| Cenário | Passa se |
|---------|----------|
| Fake L=100 | totalFound=100, não 60 |
| Lote sem novos | para sem erro de limite |
| Falha injetada na 2ª fatia | failed + itens da 1ª |
| Poll durante running | totalFound cresce |
| `dotnet test` | verde |

Ver: [contracts/discovery-coverage.md](./contracts/discovery-coverage.md), [data-model.md](./data-model.md).

## Status da implementação

Implementado (2026-07-13): discovery multi-fatia assíncrona, Fake capado em 60/call, stop em L ou lote sem novos, falha parcial com itens, enrich intercalado. `dotnet test` em `backend/` verde.
