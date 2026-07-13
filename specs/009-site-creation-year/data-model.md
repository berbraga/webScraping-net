# Data Model: Ano de Criação via Copyright do Site

## Business (estendido)

| Campo | Tipo | Obrigatório | Notas |
|-------|------|-------------|--------|
| *(existentes)* | | | Name, Phone, Website, Rating, DetailStatus, … |
| `SiteCreationYear` | `int?` | não | Menor ano 19xx/20xx do rodapé/final; `null` se sem site, falha de acesso ou sem padrão |

### Regras

- Preenchido **somente** na fase pós-Places (`EnrichSiteCreationYearsHandler`).
- Valor ∈ [1900, 2099] quando presente; senão `null`.
- Falha de leitura **não** muda `DetailStatus` / `FailedCount`.
- Documentos legados sem campo → `null`.

### Persistência

| Persistência | Campo | Tipo |
|--------------|-------|------|
| Mongo `BusinessDocument` | `siteCreationYear` | `int?` |
| InMemory `Clone` | inclui `SiteCreationYear` | |

## Fase de orquestração (não persistida)

| Conceito | Significado |
|----------|-------------|
| URL distinta | Chave de cache = `Website` normalizado (trim; comparação case-insensitive) |
| Cache da execução | `url → int?` (inclui miss) |
| Slot paralelo | no máx. `MaxDegreeOfParallelism` (default 10) GETs ativos |

### Transição de status (discovery)

```text
Running --(Places enrich completo)--> Running (fase copyright)
Running --(fase copyright done)--> Completed
Running --(cancel/fail Places)--> Cancelled/Failed  (sem exigir fase copyright)
```

## Options (`WebsiteCopyright`)

| Opção | Default | Uso |
|-------|---------|-----|
| `TimeoutSeconds` | `10` | Por GET |
| `MaxDegreeOfParallelism` | `10` | Semáforo |
| `UseFakeLookup` | `false` / testing | Fake determinístico |
