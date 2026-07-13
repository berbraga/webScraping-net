# Contract: REST — `siteCreationYear`

Estende `GET /api/searches/{id}/businesses` e `GET /api/searches/{id}/export`.

## GET `/api/searches/{id}/businesses`

### Item (estendido)

```json
{
  "id": "665f2b...",
  "name": "Padaria Exemplo",
  "phone": "+55 11 3000-0000",
  "website": "https://www.exemplo.com.br/contato",
  "siteCreationYear": 2016,
  "rating": 4.5,
  "detailStatus": "enriched",
  "detailError": null
}
```

| Field | Type | Notes |
|-------|------|--------|
| `siteCreationYear` | number \| null | Inteiro 4 dígitos (ex. `2016`); `null` se indisponível |
| demais | | Inalterados |

Ordem sugerida no objeto: após `website`, antes de `rating`.

### Observável durante `running`

- Após Places, itens podem já ter `website` com `siteCreationYear` ainda `null` até a fase de copyright terminar.
- `completed` implica fase de copyright concluída (para buscas que passaram por enrichment completo).

## GET `/api/searches/{id}/export`

### Header

```text
Nome,Telefone,Site,Criação do site,Avaliacao
```

| Coluna | Conteúdo |
|--------|----------|
| Criação do site | ano (`2016`) ou vazio |
| Site | URL completa |

Coluna **Criação do site** imediatamente após **Site**. UTF-8 BOM permanece.

## Comportamento de coleta

- Fase copyright: ≤ ~10 GETs simultâneos; timeout ~10s/URL; cache por URL na busca.
- Falha de GET/HTML **não** eleva `failedCount` nem marca `detailStatus: failed` se Places ok.
