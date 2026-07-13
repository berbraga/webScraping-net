# Feature Specification: Resultados Só Após Busca Completa

**Feature Branch**: `005-results-after-complete`

**Created**: 2026-07-13

**Status**: Draft

**Input**: User description: "No início da pesquisa, não mostrar ao usuário o nome dos itens; somente quando a busca for completa mostrar os dados da tabela com todas as informações necessárias."

## Clarifications

### Session 2026-07-13

- Q: Em busca cancelada ou falha, a tabela/área de resultados deve aparecer? → A: Sim — mostrar área de resultados (itens coletados e/ou mensagem), sem permanecer no modo “só progresso”.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Ocultar lista durante o processamento (Priority: P1)

Como usuário que acabou de iniciar uma busca, quero ver apenas o progresso (e poder cancelar), sem uma tabela parcial de nomes/comércios, para não confundir resultados incompletos com o resultado final.

**Why this priority**: Corrige a expectativa principal — durante a coleta a lista não deve aparecer.

**Independent Test**: Iniciar uma busca e, enquanto o status for processando, confirmar que nenhum nome de comércio nem tabela de resultados está visível; o progresso permanece visível.

**Acceptance Scenarios**:

1. **Given** o usuário enviou uma busca válida e a coleta está em andamento, **When** a tela de progresso é exibida, **Then** a tabela de resultados (incluindo nomes e demais colunas) **não** está visível.
2. **Given** a busca ainda processa e já existem itens coletados no sistema, **When** o usuário olha a home, **Then** esses itens **não** são mostrados na interface até a busca completar.
3. **Given** a busca está em andamento, **When** o usuário observa a área abaixo do formulário, **Then** vê o acompanhamento de progresso (e cancelamento, se aplicável), sem lista de comércios.

---

### User Story 2 - Exibir tabela completa ao concluir (Priority: P2)

Como usuário, quando a busca termina com sucesso, quero ver de uma vez a tabela com todas as informações necessárias (nome, telefone, site, avaliação e demais elementos já existentes da área de resultados), para analisar e ordenar/filtrar/exportar o resultado final.

**Why this priority**: Entrega o valor da coleta — o resultado só faz sentido completo.

**Independent Test**: Após status concluído, a tabela aparece com as colunas esperadas e os itens da busca.

**Acceptance Scenarios**:

1. **Given** a busca acabou de ficar **completa**, **When** a interface atualiza, **Then** a tabela de resultados fica visível com as colunas de informações do comércio (nome, telefone, site, avaliação).
2. **Given** a busca completa com N comércios, **When** o usuário vê a tabela, **Then** os nomes e demais campos disponíveis são exibidos conforme o conjunto final (incluindo marcação de campos ausentes, se já existir na UI).
3. **Given** a busca completa, **When** a área de resultados aparece, **Then** os controles já existentes dessa área (ex.: filtro por nome, exportação, ordenação por avaliação) ficam disponíveis junto com a tabela, sem exigir nova busca.

---

### User Story 3 - Transição limpa processamento → resultado (Priority: P3)

Como usuário, quero que a passagem de “só progresso” para “tabela completa” seja clara: ao completar, a lista surge; se eu iniciar outra busca, a tabela some de novo até a nova conclusão.

**Why this priority**: Evita estados misturados entre buscas.

**Independent Test**: Completar uma busca (tabela visível) → iniciar outra → tabela some enquanto processa → ao completar, tabela volta com os novos dados.

**Acceptance Scenarios**:

1. **Given** uma busca completa com tabela visível, **When** o usuário inicia uma nova busca, **Then** a tabela da busca anterior deixa de ser exibida enquanto a nova busca processa.
2. **Given** a nova busca completa, **When** o status deixa de ser processando, **Then** a tabela reaparece com os dados da nova busca.
3. **Given** a busca completa sem nenhum comércio encontrado, **When** a área de resultados é mostrada, **Then** o usuário vê o estado vazio apropriado (mensagem de nenhum resultado), não uma tabela parcial de outra busca.

---

### Edge Cases

- Busca cancelada pelo usuário: a coleta encerrou; a interface MUST sair do modo “só progresso” e MUST exibir a área de resultados com o que foi coletado até o cancelamento (e controles associados, se houver itens/estado vazio aplicável).
- Busca falhou: a interface MUST sair do modo “só progresso” e MUST apresentar a área de resultados e/ou a mensagem de erro conforme o estado terminal, permitindo ver itens já coletados quando existirem.
- Erro ao iniciar a busca: não mostrar tabela de resultados.
- Durante o processamento, o progresso (contadores) pode atualizar sem revelar nomes na tabela.

## Requirements *(mandatory)*

### Quality & Testing Constraints *(from constitution)*

- Cada user story MUST ter cenários de aceitação mapeáveis a testes
  automatizados (unitários e/ou integração).
- A solução MUST permanecer enxuta: evitar requisitos que forcem
  complexidade sem valor claro ao usuário.
- Comportamentos observáveis MUST ser descritos de forma testável
  (Given/When/Then), sem ambiguidade.

### Functional Requirements

- **FR-001**: Enquanto a busca estiver em processamento (ainda não concluída), a interface MUST NOT exibir a tabela de comércios nem os nomes dos itens coletados.
- **FR-002**: Enquanto a busca estiver em processamento, a interface MUST continuar exibindo o acompanhamento de progresso da busca (e ação de cancelar, quando aplicável).
- **FR-003**: Quando a busca estiver **completa**, a interface MUST exibir a tabela de resultados com as informações necessárias já previstas no produto (no mínimo: nome, telefone, site, avaliação).
- **FR-004**: Controles da área de resultados (filtro, exportação, ordenação) MUST permanecer disponíveis apenas quando a área de resultados estiver visível após o fim do processamento da busca atual (não durante o processamento).
- **FR-005**: Ao iniciar uma nova busca, a tabela/resultados da busca anterior MUST ser ocultados novamente até que a nova busca deixe de estar em processamento.
- **FR-006**: Esta feature MUST ser resolvida na experiência do usuário na home (frontend); MUST NOT exigir mudança de contrato de API só para ocultar a lista (a API pode continuar existindo; a UI é quem decide o que mostrar).
- **FR-007**: Estados terminais que não sejam “completa com sucesso” (cancelada / falha), após saírem do processamento, MUST exibir a área de resultados daquela busca (itens coletados e/ou mensagem de vazio/erro), sem manter o modo “ocultar tabela como se ainda estivesse no início”.
- **FR-008**: A tabela/área de resultados MUST ficar oculta somente enquanto o status da busca atual for de processamento (`pending` / `running` ou equivalente); qualquer status terminal (`completed`, `cancelled`, `failed`) MUST torná-la elegível à exibição.

### Key Entities

- **Busca**: possui status de processamento vs. concluída (e demais estados terminais).
- **Área de resultados**: tabela + controles; visível somente fora do processamento da busca atual.
- **Comércio (linha)**: dados exibidos na tabela apenas quando a área de resultados estiver visível.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Em 100% dos testes de fluxo “busca em andamento”, zero nomes de comércio aparecem na UI antes do fim do processamento.
- **SC-002**: Em 100% dos testes de fluxo “busca completa com itens”, a tabela fica visível com as colunas de informações necessárias após a conclusão.
- **SC-003**: Em um fluxo de duas buscas consecutivas, a tabela some durante o 2º processamento e reaparece após a 2ª conclusão, sem misturar nomes da busca anterior na tela de progresso.
- **SC-004**: Usuários conseguem acompanhar o progresso sem ver a lista parcial; após concluir, realizam filtro/exportação/ordenação na tabela final sem nova coleta.
- **SC-005**: Cenários P1–P3 possuem testes automatizados passando antes de considerar a feature concluída.

## Assumptions

- Escopo **somente frontend** (comportamento de exibição); backend/API inalterados salvo se testes exigirem harness já existente.
- “Completa” no sentido desta feature (ocultar vs mostrar lista) = busca **não** está mais em processamento; `completed`, `cancelled` e `failed` mostram a área de resultados (confirmado em Clarifications).
- O progresso durante a coleta permanece (barra/status/contadores), apenas a **tabela de itens** fica oculta.
- A ordenação por avaliação (feature 004) continua válida **depois** que a tabela aparece; durante o processamento não há tabela para ordenar.
- Mensagens de estado vazio após conclusão sem itens seguem o padrão já usado na home.
