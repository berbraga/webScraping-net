# Quickstart: Validar resultados só após conclusão

## Pré-requisitos

```bash
# API + UI (ex.)
./dev.sh
# ou frontend isolado com API já no ar
cd frontend && npm run dev
```

## Testes automatizados

```bash
cd frontend
npm test
```

Esperado: testes de `shouldShowResultsArea` cobrindo processing = oculto; completed/cancelled/failed = visível.

**Implementado 2026-07-13**: `npm test` — 32 testes passando.

## Validação manual

1. Iniciar busca com vários resultados potenciais.
2. Enquanto **processando**: confirmar **nenhum** nome/tabela na tela; progresso (e cancel) OK.
3. Ao **completed**: tabela aparece com nome, telefone, site, avaliação; filtro/export/ordenação disponíveis.
4. **Cancelar** no meio: área de resultados aparece com o que houver (não fica só no progresso eterno).
5. Nova busca: tabela some até o novo terminal.

## Critério rápido

| Cenário | Passa se |
|---------|----------|
| Running | 0 nomes na UI |
| Completed | tabela completa visível |
| Nova busca | tabela some no processing |

Ver: [contracts/ui-results-visibility.md](./contracts/ui-results-visibility.md).
