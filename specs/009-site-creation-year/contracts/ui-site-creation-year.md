# Contract: UI — Coluna Criação do site

Componente: `frontend/components/BusinessList.jsx`.

## Layout

| # | Cabeçalho | Fonte |
|---|-----------|--------|
| 1 | Nome | `item.name` |
| 2 | Telefone | `item.phone` |
| 3 | Site | `item.website` |
| 4 | **Criação do site** | `item.siteCreationYear` |
| 5 | Avaliação | `item.rating` (sort existente) |

## Célula

- Ano presente: texto do inteiro (ex. `2016`).
- Ausente: `FieldValue` → ✕ (FR-009).
- Sem ordenação nesta feature.

## Aceite

1. Cabeçalho exatamente **Criação do site**.
2. Coluna imediatamente após Site.
3. ✕ quando `siteCreationYear` null/undefined.
