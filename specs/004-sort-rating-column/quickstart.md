# Quickstart: Validar ordenação por Avaliação

## Pré-requisitos

- Frontend: `cd frontend && npm install && npm run dev`
- Backend opcional (para dados reais); Fake/API já existentes
- Testes: Node 20+

## Testes automatizados

```bash
cd frontend
npm test
```

Esperado após implementação:

- `sortByRating`: desc/asc, nulls last, N/A
- `BusinessList`: 1º clique → desc; 2º → asc; clique com status running → ordem intacta; ícone presente quando ativo

## Validação manual

1. Concluir uma busca com várias avaliações (e alguns sem nota).
2. Clicar em **Avaliação** → ordem 5→…→1; seta ↓.
3. Clicar de novo → crescente; seta ↑; sem nota no final.
4. Durante **processando**, a lista pode aparecer com itens já coletados; clicar em Avaliação → lista **não** reordena.
5. Nova busca → sort reinicia (sem ícone até novo clique).
6. Export CSV continua funcionando (ordem do arquivo não precisa espelhar a tabela).

**Implementado 2026-07-13**: `npm test` — 28 testes passando.

## Critério rápido

| Cenário | Passa se |
|---------|----------|
| Completed + 1 clique | maior→menor |
| 2º clique | menor→maior |
| Nulls | sempre no fim |
| Running + clique | sem mudança |
| `npm test` | verde |

Ver: [contracts/ui-sort-rating.md](./contracts/ui-sort-rating.md), [data-model.md](./data-model.md).
