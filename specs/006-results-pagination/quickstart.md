# Quickstart: Validar paginação da lista de resultados

## Pré-requisitos

- Frontend: `cd frontend && npm install && npm run dev`
- Backend opcional (dados reais ou Fake); AbsoluteMaxResults = 200
- Testes: Node 20+

## Testes automatizados

```bash
cd frontend
npm test
```

Esperado após implementação:

- `paginateResults`: limiar > 60; fatias; rodapé com/sem paginação; página inválida clampada
- `BusinessList`: sem controles com ≤ 60; com 125 itens — 3 páginas, extremos desabilitados; Anterior/Próxima; reset ao mudar sort/`searchId`
- Listagem usa `take` suficiente para não truncar em 100

## Validação manual

1. Busca com **≤ 60** resultados → lista completa; **sem** Anterior/Próxima; rodapé `N de Y resultados`.
2. Com Fake ou dados com **> 60** (ideal ≥ 61, melhor 125) → primeira página 60 itens; rodapé `Mostrando 1–60 de …`; Próxima habilitada; Anterior desabilitado.
3. Próxima → próximo bloco; na última, Próxima desabilitada.
4. Anterior → volta à página anterior.
5. Filtro por nome que deixa ≤ 60 → controles somem; que deixa > 60 → paginação sobre o filtrado; ao filtrar, volta à página 1.
6. Ordenar por Avaliação com > 60 → páginas respeitam a ordem global; ao alternar sort, página 1.
7. Export CSV continua exportando o conjunto completo da busca.
8. Trocar de página **não** dispara nova chamada de listagem (Network).

## Critério rápido

| Cenário | Passa se |
|---------|----------|
| 60 itens | sem paginação + rodapé simples |
| 125 itens | 60 + 60 + 5; extremos disabled |
| Filtro / sort | página reseta a 1; só itens filtrados/ordenados |
| Network ao paginar | sem GET businesses extra |
| `npm test` | verde |

Ver: [contracts/ui-results-pagination.md](./contracts/ui-results-pagination.md), [data-model.md](./data-model.md).

**Implementado 2026-07-13**: `cd frontend && npm test` — 46 testes passando.
