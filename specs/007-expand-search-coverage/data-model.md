# Data Model: Cobertura Ampliada

Persistência principal inalterada (`Search`, `Business`). Novos conceitos são de **orquestração** (em memória / config).

## Search (existente — campos relevantes)

| Campo | Uso nesta feature |
|-------|-------------------|
| `MaxResults` | Limite L pedido (inalterado) |
| `TotalFound` | Atualizado **a cada lote** com distincts acumulados |
| `ProcessedCount` / `FailedCount` | Enrich por lote |
| `Status` | `Running` durante discovery+enrich; `Completed` no fim OK; `Failed` se fatia falhar (itens permanecem); `Cancelled` interrompe |
| `ErrorMessage` | Preenchido em `Failed` |

### Transições (discovery)

```text
Pending/Running --(POST cria + enqueue)--> Running
Running --(lote com novos)--> Running (TotalFound↑; enrich pending)
Running --(lote 0 novos, TotalFound>0)--> … enrich resto se preciso --> Completed
Running --(fatia 0 vazia)--> Completed (TotalFound=0)
Running --(erro de fatia)--> Failed (itens mantidos)
Running --(cancel)--> Cancelled
```

## CoverageSlice (não persistido)

| Campo | Significado |
|-------|-------------|
| `Index` | 0..N-1 |
| `EffectiveRegion` | Região passada ao lookup (pode incluir marcador de fatia no Fake) |
| `EffectiveQuery` | Termo / textQuery lógico |
| `Label` | Setor (ex.: `norte`) para logs |

## Regras de deduplicação (inalteradas em espírito)

Chave: `id:{ExternalId}` se presente; senão `name:{Name}` (case-insensitive).  
Novos do lote = não presentes no conjunto acumulado da busca.

## Parada

- `accumulated.Count >= MaxResults` → não pedir mais fatias
- lote com `newDistinct.Count == 0` → parar expansão (exceto interpretação fatia 0 vazia = sem resultados)

## Config (`SearchOptions`)

| Opção | Default sugerido |
|-------|------------------|
| `CoverageSectorSuffixes` | `centro,norte,sul,leste,oeste` |
| `ProviderPageCap` (opcional) | 60 — usado pelo planner/Fake |
