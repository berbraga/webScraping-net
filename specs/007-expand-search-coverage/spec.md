# Feature Specification: Expandir Cobertura da Busca Além do Teto do Provedor

**Feature Branch**: `007-expand-search-coverage`

**Created**: 2026-07-13

**Status**: Draft

**Input**: User description: "pedi 200 lugares e só vieram 60 por causa do teto da Places API; quero coleta multi-área/cobertura ampliada para chegar perto do limite máximo quando houver oferta suficiente na região"

## Clarifications

### Session 2026-07-13

- Q: Se uma “fatia” da cobertura falhar no meio, o que fazer? → A: Falhar com mensagem, **mantendo** itens já coletados.
- Q: Durante a cobertura ampliada, como o progresso deve evoluir? → A: Atualizar total encontrado a cada lote de descoberta; enriquecer em seguida.
- Q: Quando enriquecer (telefone/site/nota)? → A: Enriquecer cada lote assim que for descoberto (descoberta e detalhe intercalados).
- Q: Quando considerar a oferta “esgotada” e parar de expandir? → A: Parar em L ou no primeiro lote sem itens novos.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Atingir o limite pedido quando a região tem oferta (Priority: P1)

Como usuário, informo região, termo e limite máximo alto (ex.: 200). Se existirem muitos comércios na região para aquele termo, espero que a busca complete com total encontrado igual (ou muito próximo) ao limite pedido — não travar em torno de 60 por causa de um teto silencioso de uma única consulta ao provedor.

**Why this priority**: Corrige a expectativa quebrada do campo “Limite máximo” quando a oferta real é maior que o teto de uma única busca no provedor.

**Independent Test**: Em cenário de teste com oferta simulada ampla (ou região/termo conhecidamente denso), iniciar busca com limite 200 e verificar que `totalFound` chega a 200 (ou ao máximo configurado do cenário), não ~60.

**Acceptance Scenarios**:

1. **Given** uma região/termo com oferta suficiente no ambiente de teste, **When** o usuário inicia busca com limite máximo 200, **Then** a busca completa com total encontrado = 200.
2. **Given** o usuário pediu limite 100 e a oferta no teste permite 100, **When** a coleta termina com sucesso, **Then** o total encontrado é 100 (não ~60).
3. **Given** o usuário pediu limite L e a coleta atinge L itens distintos, **When** o status é concluído, **Then** o progresso processados/total é coerente com L (não com um teto artificial menor).

---

### User Story 2 - Parar quando não há mais comércios distintos (Priority: P2)

Como usuário, se a região/termo não tiver tantos comércios quanto o limite, a busca deve completar com o total real disponível (sem inventar linhas e sem erro só porque não bateu o número). A cobertura ampliada para ao atingir o limite **ou** ao receber um lote sem nenhum item novo.

**Why this priority**: Evita loops infinitos e resultados falsos quando a oferta é menor que o limite.

**Independent Test**: Cenário com oferta limitada a N &lt; L → conclusão com total = N, status concluído, sem erro de “limite”; cenário em que um lote intermediário só devolve duplicatas → para sem continuar expandindo.

**Acceptance Scenarios**:

1. **Given** oferta de teste com apenas N comércios distintos (N &lt; limite), **When** a busca termina, **Then** total encontrado = N e status concluído.
2. **Given** zero resultados no provedor para a região/termo, **When** a busca termina, **Then** o usuário vê conclusão sem resultados (comportamento vazio já existente preservado).
3. **Given** um lote de cobertura que não adiciona nenhum comércio distinto novo e o limite ainda não foi atingido, **When** esse lote é processado, **Then** a descoberta ampliada encerra e a busca segue o fluxo normal de conclusão (ou falha, se aplicável) com o total já acumulado.

---

### User Story 3 - Transparência e deduplicação (Priority: P3)

Como usuário, quero que itens repetidos (mesmo comércio encontrado em “fatias” diferentes da cobertura) não apareçam duas vezes, e que o limite que pedi continue refletido no resumo da busca mesmo quando o total coletado for menor por falta de oferta.

**Why this priority**: Cobertura ampliada tende a sobrepor resultados; sem deduplicação a UX e o CSV ficam ruins.

**Independent Test**: Forçar sobreposição no harness de teste → lista final sem IDs/nomes duplicados; `maxResults` no status permanece o valor pedido.

**Acceptance Scenarios**:

1. **Given** a mesma empresa poderia ser descoberta mais de uma vez na cobertura ampliada, **When** a busca completa, **Then** ela aparece **uma** vez nos resultados.
2. **Given** busca iniciada com limite 200, **When** o usuário consulta o status, **Then** o limite máximo registrado continua 200.
3. **Given** coleta parcial (ex.: 140 de 200 por esgotamento real), **When** a busca conclui, **Then** o total encontrado é 140 e não há preenchimento artificial até 200.

---

### Edge Cases

- Limite 1: no máximo 1 comércio.
- Limite no teto absoluto do produto (200): aceitar e coletar até 200 quando a cobertura e a oferta permitirem.
- Limite omitido: default do produto inalterado.
- Cancelamento no meio: não forçar preenchimento até o limite; cancela conforme regra existente.
- Erro parcial do provedor em uma “fatia” da cobertura: a busca MUST terminar em **falha** com mensagem visível, **mantendo** os comércios distintos já coletados até o ponto da falha (elegíveis a listagem/exportação conforme regras de área de resultados); MUST NOT descartar o progresso nem fingir conclusão bem-sucedida até L.
- Região muito ampla ou ambígua: a busca ainda deve tentar cobrir até o limite ou esgotar descobertas distintas; não é obrigatório garantir 200 em qualquer texto de região do mundo.

## Requirements *(mandatory)*

### Quality & Testing Constraints *(from constitution)*

- Cada user story MUST ter cenários de aceitação mapeáveis a testes
  automatizados (unitários e/ou integração).
- A solução MUST permanecer enxuta: evitar requisitos que forcem
  complexidade sem valor claro ao usuário.
- Comportamentos observáveis MUST ser descritos de forma testável
  (Given/When/Then), sem ambiguidade.

### Functional Requirements

- **FR-001**: Quando o usuário informa um limite máximo L (dentro do teto absoluto do produto), o sistema MUST continuar a descoberta de comércios distintos até atingir L **ou** esgotar a oferta disponível para aquela região/termo sob a estratégia de cobertura ampliada.
- **FR-014**: A oferta MUST ser considerada esgotada (parar de expandir a cobertura) ao atingir L **ou** quando um lote de descoberta não acrescentar **nenhum** comércio distinto novo (lote vazio ou só duplicatas). O primeiro lote da busca que retorna zero itens no total continua resultando em busca sem resultados, como hoje.
- **FR-002**: O sistema MUST NÃO encerrar a descoberta apenas porque uma única consulta ao provedor devolveu um lote típico (~dezenas de itens) se ainda não atingiu L e ainda houver cobertura a explorar com chance de novos itens.
- **FR-003**: O sistema MUST deduplicar comércios descobertos (mesmo identificador externo, ou regra equivalente já usada no produto) antes de contar para o total e antes de enfileirar enriquecimento.
- **FR-004**: Se a oferta distinta esgotar antes de L, a busca MUST completar com total &lt; L sem erro relacionado a “não atingiu o limite”.
- **FR-005**: O limite máximo solicitado MUST permanecer persistido e visível no status da busca.
- **FR-006**: Progresso (processados / total encontrado) MUST refletir o conjunto distinto realmente coletado após a cobertura ampliada.
- **FR-012**: Durante a cobertura ampliada, o sistema MUST atualizar o **total encontrado** (e o status da busca) **a cada lote** de novos comércios distintos descobertos — não apenas ao final de toda a cobertura.
- **FR-013**: O enriquecimento de detalhes (telefone, site, avaliação) MUST poder iniciar **por lote** assim que o lote for descoberto e persistido — intercalando descoberta e detalhe — em vez de esperar o fim de toda a cobertura ampliada.
- **FR-007**: Validação existente de limite (maior que zero, ≤ teto absoluto) MUST permanecer.
- **FR-008**: Exportação CSV e listagem MUST refletir o conjunto distinto coletado (incluindo volumes acima de ~60 quando L e a oferta permitirem).
- **FR-009**: A experiência de formulário (região, termo, limite) MUST permanecer a mesma do ponto de vista do usuário; a cobertura ampliada é responsabilidade da coleta, não de novos campos obrigatórios nesta versão.
- **FR-010**: Cancelamento e estados de falha existentes MUST continuar funcionando; cancelar interrompe a cobertura ampliada sem “completar” artificialmente até L.
- **FR-011**: Se uma falha do provedor interromper a cobertura ampliada, a busca MUST ficar com status de **falha** e mensagem de erro; os comércios distintos já descobertos MUST permanecer associados à busca (não descartados).

### Key Entities

- **Busca**: região, termo, limite máximo solicitado, status, total encontrado (distintos), processados, falhas.
- **Comércio**: item distinto vinculado à busca; quantidade não pode ser inflada por duplicatas da cobertura.
- **Cobertura ampliada**: conjunto de tentativas de descoberta que, em conjunto, visa ultrapassar o teto de uma única consulta ao provedor — transparente para o usuário final nesta versão (sem UI extra obrigatória).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Em harness de teste com oferta ≥ 200, busca com limite 200 completa com `totalFound = 200` em ≥ 95% das execuções bem-sucedidas (sem erro de provedor injetado).
- **SC-002**: Em harness com oferta ≥ 100, busca com limite 100 nunca completa artificialmente travada em 60 quando ainda há itens distintos disponíveis no harness.
- **SC-003**: Em harness com sobreposição forçada, 100% dos resultados finais são únicos por identificador de deduplicação.
- **SC-004**: Quando a oferta do harness é N &lt; L, a busca completa com total = N sem mensagem de erro de limite; se um lote intermediário não acrescenta itens novos, a expansão para.
- **SC-005**: O status da busca exibe o `maxResults` pedido (ex.: 200) após o início.
- **SC-007**: Em harness com cobertura em múltiplos lotes até 200, o `totalFound` observável no status aumenta ao longo da descoberta (não permanece fixo no primeiro lote até o fim da cobertura).
- **SC-008**: Em harness com ≥2 lotes de descoberta, o enriquecimento de itens do primeiro lote MUST poder avançar (`processedCount` ou status de detalhe) antes do término da descoberta do último lote.
- **SC-006**: Cenários P1–P3 possuem testes automatizados passando antes de considerar a feature concluída.

## Assumptions

- O teto absoluto do produto permanece **200**.
- O problema atual é o teto de **uma** consulta ao provedor de mapas (~60), não a validação do formulário.
- “Cobertura ampliada” significa o sistema fazer **múltiplas descobertas** (ex.: subdividir a região / variar o recorte geográfico ou equivalente) e unir resultados distintos até L — detalhes de algoritmo ficam para o plano; a spec exige o resultado observável. Parada: atingir L ou primeiro lote sem itens novos (confirmado em Clarifications).
- Não é obrigatório garantir 200 em toda região do mundo real; a garantia mensurável é no harness de teste e, em produção, “até L quando houver oferta distinta alcançável pela estratégia”.
- Fonte fake de desenvolvimento MUST poder simular oferta &gt; 60 para validar a feature sem depender só da API real.
- Paginação da UI (feature 006), ordenação (004) e ocultar resultados durante processamento (005) permanecem; com totals &gt; 60 a paginação da lista passa a ser útil de verdade.
- Custo/latência maiores (mais chamadas ao provedor) são aceitáveis para honrar o limite pedido; não há novo campo de “modo rápido” nesta versão. O progresso de descoberta atualiza o total encontrado por lote; o enriquecimento inicia por lote assim que descoberto (confirmado em Clarifications).
- Não há novo campo de UI obrigatório (ex.: lista de bairros); a região textual atual continua sendo a entrada.
