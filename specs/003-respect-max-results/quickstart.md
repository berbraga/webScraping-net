# Quickstart: Validar limite máximo

## Pré-requisitos

- .NET 8 SDK
- Backend configurável (Mongo ou `Testing:UseInMemoryStores=true`)
- Para validação real Google: `GooglePlaces:ApiKey` + Places API (New) habilitada
- Para CI/local sem chave: fonte Fake (`UseFakeSource=true` ou sem chave)

## Testes automatizados

```bash
cd backend
dotnet test
```

Esperado após implementação:

- Teste do lookup Google (HTTP fake) com **múltiplas páginas** → `maxResults=50` retorna 50.
- Teste de esgotamento → menos que o limite sem erro.
- Fake / Application: limite alto respeitado.

## Validação manual (Fake)

1. Em `appsettings.Development.json` (ou env): `GooglePlaces__UseFakeSource=true` e/ou in-memory.
2. Subir API: `dotnet run --project WebScraping.Api` (ou `./dev.sh --in-memory`).
3. `POST /api/searches` com `maxResults: 100`.
4. Conferir `totalFound === 100` no summary (o Fake default agora tem catálogo de 200 itens).

## Validação manual (Google real)

1. Garantir chave válida e `UseFakeSource=false`.
2. Buscar região/termo com muitos resultados, ex.: Florianópolis / restaurantes, `maxResults: 100`.
3. Esperado:
   - **Não** completar discovery em exatamente 20 se ainda houver páginas (paginação via `nextPageToken`).
   - `totalFound` > 20 quando o provedor fornecer mais páginas.
   - Se o Google esgotar páginas antes de 100 (Text Search costuma parar ~60), `totalFound` pode ser ~40–60 — isso é aceitável (FR-003), desde que **não** pare em 20 com mais páginas disponíveis.
4. UI: status deve mostrar progresso coerente com `totalFound`; lista/export com o volume coletado.
5. Reinicie a API após o deploy desta feature para carregar o adapter atualizado.

## Critério de aceite rápido

| Cenário | Passa se |
|---------|----------|
| Fake limite 100 | `totalFound = 100` |
| Google + oferta ampla | `totalFound > 20` (idealmente até min(pedido, teto do provedor)) |
| Regressão validação | `maxResults: 201` → 400 |

Ver também: [contracts/rest-behavior.md](./contracts/rest-behavior.md), [data-model.md](./data-model.md).
