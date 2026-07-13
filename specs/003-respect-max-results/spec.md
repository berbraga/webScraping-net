# Feature Specification: Respeitar Limite Máximo de Resultados

**Feature Branch**: `003-respect-max-results`

**Created**: 2026-07-12

**Status**: Draft

**Input**: User description: "gostaria de como usuario eu colocar um limite maximo de buscas e ele realmente aparecer o limite, coisa que hoje não está acontecendo, eu coloquei 100 e apareceu somente 20 pesquisas"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Limite máximo honrado na coleta (Priority: P1)

Como usuário, informo uma região, um termo e um limite máximo (ex.: 100). Ao concluir a busca, espero receber até esse número de comércios — não um teto menor e silencioso (ex.: sempre 20).

**Why this priority**: Sem isso, o campo “Limite máximo” engana o usuário e reduz o valor da ferramenta; é o problema reportado.

**Independent Test**: Em uma região/termo com oferta ampla, iniciar busca com limite 100 e verificar que o total encontrado/processado chega próximo ou igual a 100 (quando houver disponibilidade), e não para em 20.

**Acceptance Scenarios**:

1. **Given** uma região e termo com muitos comércios disponíveis, **When** o usuário inicia a busca com limite máximo 100, **Then** a busca completa com total encontrado igual a 100 (ou o máximo disponível se ainda houver paginação necessária até completar o limite).
2. **Given** o usuário informa limite 100, **When** a busca termina com sucesso, **Then** o status de progresso não deve indicar conclusão “cheia” em apenas 20 itens se ainda houver resultados disponíveis abaixo do limite pedido.
3. **Given** o usuário informa limite 50, **When** a busca completa e há pelo menos 50 resultados disponíveis, **Then** o total encontrado é 50.

---

### User Story 2 - Menos resultados do que o limite (Priority: P2)

Como usuário, se a região/termo tiver menos comércios do que o limite pedido, a busca deve completar normalmente com o total real encontrado, sem erro e sem “inventar” resultados.

**Why this priority**: Comportamento esperado e evita confusão quando a oferta é pequena.

**Independent Test**: Buscar com limite alto em um termo muito específico com poucos resultados e confirmar conclusão com total < limite.

**Acceptance Scenarios**:

1. **Given** uma região/termo com apenas N comércios (N < limite), **When** o usuário busca com limite maior que N, **Then** a busca completa com total encontrado = N e status concluído.
2. **Given** zero comércios encontrados, **When** a busca termina, **Then** o usuário vê conclusão sem resultados (comportamento já existente preservado).

---

### User Story 3 - Transparência do limite na experiência (Priority: P3)

Como usuário, quero que o limite que enviei continue refletido no resumo da busca (valor solicitado) e que o progresso (processados / total encontrado) seja coerente com o que realmente foi coletado.

**Why this priority**: Reforça confiança; o bug atual mistura “pedi 100” com “fechei em 20”.

**Independent Test**: Após busca com limite 100 e coleta correta, conferir que o resumo exibe o limite solicitado e que o progresso usa o total realmente encontrado.

**Acceptance Scenarios**:

1. **Given** uma busca iniciada com limite 100, **When** o usuário consulta o status, **Then** o limite máximo registrado da busca é 100.
2. **Given** uma busca que coletou 100 itens, **When** o progresso é exibido, **Then** o denominador do progresso corresponde a 100 (total encontrado), não a um teto artificial menor.

---

### Edge Cases

- Limite no valor mínimo permitido (1): deve retornar no máximo 1 comércio.
- Limite no teto absoluto do produto (hoje 200): deve aceitar e coletar até esse valor quando disponível.
- Limite acima do teto absoluto: continua rejeitado com mensagem clara (comportamento de validação já existente).
- Limite omitido: continua usando o padrão do produto (hoje 50).
- Cancelamento no meio da coleta: não deve forçar preenchimento até o limite; cancela conforme regra já existente.
- Fonte de dados indisponível ou erro parcial: não deve marcar como “atingiu o limite” se na verdade falhou antes; erros devem permanecer visíveis.

## Requirements *(mandatory)*

### Quality & Testing Constraints *(from constitution)*

- Cada user story MUST ter cenários de aceitação mapeáveis a testes
  automatizados (unitários e/ou integração).
- A solução MUST permanecer enxuta: evitar requisitos que forcem
  complexidade sem valor claro ao usuário.
- Comportamentos observáveis MUST ser descritos de forma testável
  (Given/When/Then), sem ambiguidade.

### Functional Requirements

- **FR-001**: O sistema MUST tratar o “Limite máximo” informado pelo usuário como o teto real de comércios a coletar naquela busca (sujeito ao teto absoluto do produto e à disponibilidade real de resultados).
- **FR-002**: O sistema MUST NÃO aplicar um teto silencioso menor que o limite solicitado (ex.: parar em 20 quando o usuário pediu 100 e ainda há resultados disponíveis).
- **FR-003**: Quando existirem menos resultados disponíveis que o limite, o sistema MUST completar a busca com o total real encontrado, sem falha.
- **FR-004**: O sistema MUST persistir e expor no status da busca o limite máximo solicitado pelo usuário.
- **FR-005**: O progresso (processados / total encontrado) MUST refletir a quantidade realmente coletada para aquela busca, coerente com FR-001–FR-003.
- **FR-006**: A validação existente de limite (maior que zero e não acima do teto absoluto) MUST permanecer; esta feature não altera os limites absolutos do produto salvo se documentado em Assumptions.
- **FR-007**: Exportação CSV e listagem de resultados MUST refletir o conjunto coletado após a busca (incluindo volumes acima de 20 quando o limite e a disponibilidade permitirem).

### Key Entities

- **Busca**: inclui região, termo, limite máximo solicitado, status, total encontrado, processados e falhas.
- **Comércio**: item coletado vinculado a uma busca; a quantidade de comércios de uma busca NÃO deve ser artificialmente limitada abaixo do limite solicitado quando houver disponibilidade.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Em cenários de teste com oferta suficiente, uma busca com limite 100 completa com total encontrado = 100 em ≥ 95% das execuções bem-sucedidas (sem erro de provedor).
- **SC-002**: Em cenários de teste com oferta suficiente, uma busca com limite 50 nunca completa com total encontrado artificialmente travado em 20.
- **SC-003**: Usuários conseguem verificar, no status da busca, que o limite solicitado permanece o valor que informaram (ex.: 100).
- **SC-004**: Quando há menos de L resultados disponíveis, a busca completa com total < L sem mensagem de erro relacionada a “limite”.
- **SC-005**: Os cenários de aceitação P1 e P2 possuem testes automatizados passando antes de considerar a feature concluída.

## Assumptions

- O teto absoluto do produto permanece 200 (já existente); o problema reportado é o teto silencioso em torno de 20, não o teto absoluto.
- “Aparecer o limite” significa coletar até o número pedido quando houver resultados suficientes — não inventar linhas vazias nem repetir registros para “bater” o número.
- A UI do campo “Limite máximo” já envia o valor corretamente; o foco desta feature é o comportamento de coleta/resultado respeitar esse valor.
- Se a fonte externa de dados tiver restrições de página/lote, o produto ainda assim deve continuar buscando até atingir o limite do usuário ou esgotar os resultados — do ponto de vista do usuário, o limite é o contrato.
- Cancelamento, exportação e redesign visual da home permanecem fora do escopo desta feature, exceto na medida em que o volume de resultados muda.
- Fonte fake de desenvolvimento, se usada, já respeita o limite; a correção deve ser validada também no fluxo real de coleta usado em produção/desenvolvimento com provedor real.
