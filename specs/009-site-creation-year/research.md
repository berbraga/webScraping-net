# Research: Ano de Criação via Copyright do Site

## R1 — Orquestração (fase após Places)

**Decision**: Em `DiscoverSearchHandler`, após o último `EnrichPendingAsync` (Places) e **antes** de `Status = Completed`, chamar `EnrichSiteCreationYearsHandler.HandleAsync(searchId)`. Espelhar a mesma ordem em `EnrichSearchAsync` se ainda for caminho de teste/legado. Early-complete (0 resultados) e `Failed`/`Cancelled` **não** disparam a fase de anos.

**Rationale**: Clarification B / FR-001 / FR-008 — Completed só depois da leitura paralela dos sites.

**Alternatives considered**:
- Inline no `foreach` de Places — rejeitado (usuário escolheu fase 2 paralela).
- Completed antes e preenchimento lazy — rejeitado (clarification C).

## R2 — Port e HTTP

**Decision**: `IWebsiteCopyrightYearLookup.GetYearAsync(string websiteUrl, CancellationToken)` → `Task<int?>`. Implementação `HttpWebsiteCopyrightYearLookup`: GET com `HttpClient`, `Timeout`/`CancelAfter` ~10s; qualquer falha (SSL, 404, timeout, non-success, body vazio) → `null` (sem throw que aborte a fase). Fake mapeia URL→ano para CI.

**Rationale**: Separa I/O do parser; testável; alinhado a Places.

**Alternatives considered**:
- Incluir fetch no Application com HttpClient concreto — rejeitado (constituição V).
- Headless browser — rejeitado (assumption / YAGNI).

## R3 — Extração de ano (parser)

**Decision**: Helper puro `CopyrightYearExtractor.TryExtractOldestYear(string html) → int?` no Domain:
1. Isolar candidato: conteúdo de `<footer>...</footer>` (case-insensitive); se ausente/sem anos, usar últimos ~15–20% do documento (ou últimas N linhas de texto após strip grosseiro de tags).
2. Regex de anos `19\d{2}|20\d{2}` e intervalos `YEAR \s*[-–—]\s* YEAR`; para cada match, tomar o menor ano do intervalo; entre todos os matches válidos na área, tomar o mínimo global.
3. Sem © obrigatório (FR-013). Fora de 1900–2099 → ignorar.

**Rationale**: Testável sem rede; cobre FR-002/003/004/013 sem AngleSharp (YAGNI).

**Alternatives considered**:
- AngleSharp/HtmlAgilityPack — melhor DOM; adiado até heurística falhar em produção.
- Exigir © perto do ano — rejeitado (clarification A).

## R4 — Paralelismo, timeout, cache

**Decision**:
- `WebsiteCopyrightOptions`: `TimeoutSeconds = 10`, `MaxDegreeOfParallelism = 10`.
- `SemaphoreSlim(MaxDegreeOfParallelism)` + `Task.WhenAll` sobre **URLs distintas** (normalizar trim/case leve da string Website).
- Cache `Dictionary<string, int?>` da execução: uma GET (ou falha) por URL; aplicar o mesmo ano a todos os businesses com aquela URL.
- Falha de copyright **não** incrementa `FailedCount` nem altera `DetailStatus`.

**Rationale**: FR-007/012/014, SC-002/003.

**Alternatives considered**:
- Paralelismo ilimitado — rejeitado (clarification).
- Retry — YAGNI na v1.

## R5 — Modelo / API / UI / CSV

**Decision**:
- `Business.SiteCreationYear` (`int?`).
- JSON: `siteCreationYear` (camelCase, padrão da API).
- CSV: `Nome,Telefone,Site,Criação do site,Avaliacao`.
- UI: coluna **Criação do site** após Site.

**Rationale**: FR-005/009/010/012 labels; consistência com `website`/`rating`.

## R6 — SSL e segurança

**Decision**: Validação SSL padrão do `HttpClient`; certificado inválido → falha → `null` (edge case da spec). Não desligar validação na v1.

**Rationale**: Segurança; comportamento explícito na spec.

**Alternatives considered**: Bypass SSL — rejeitado (risco sem ganho de produto garantido).
