# Feature Specification: Paginação da Lista de Resultados

**Feature Branch**: `006-results-pagination`

**Created**: 2026-07-13

**Status**: Draft

**Input**: User description: "entao quando colocamos um total maior de que 60 na busca a api do google places ela pagina a busca, fazendo com que somente apareça 60, gostaria de que tenha uma paginação tambem no site para que eu consiga ver todos os itens, o usuario deve apertar um botão e ir para a nova pagina mostrando os resultados da proxima pagina, lembrar de manter o comportamento de filtros e buscas"

## Clarifications

### Session 2026-07-13

- Q: Quando a paginação deve aparecer? → A: Somente quando o conjunto exibível tiver **mais de 60** resultados. Com **60 ou menos**, a lista mostra todos os itens **sem** controles de paginação.
- Q: Onde a paginação “mora”? → A: Somente frontend: fatia a lista já carregada (após filtro/ordenação); sem paginação página-a-página via API.
- Q: Nos extremos, o que fazer com Anterior/Próxima? → A: Desabilitar o botão (visível, não clicável).
- Q: O que o rodapé deve mostrar com paginação ativa? → A: Intervalo da página + total filtrado (ex.: “Mostrando 1–60 de 125”).
- Q: Sem paginação (≤ 60 itens), qual rodapé? → A: Formato atual simples (“N de Y resultados”).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Navegar para a próxima página quando há mais de 60 (Priority: P1)

Como usuário, com a busca concluída e **mais de 60** comércios no conjunto exibível, quero um botão para ir à próxima página e ver o próximo bloco de resultados (além dos primeiros 60).

**Why this priority**: Entrega o valor principal — paginação só quando o volume exige; listas menores ficam simples.

**Independent Test**: Com busca completa e 61+ itens no conjunto exibível, confirmar controles de paginação e que “próxima” mostra o bloco seguinte; com ≤ 60, nenhum controle de paginação.

**Acceptance Scenarios**:

1. **Given** uma busca completa com **mais de 60** comércios no conjunto exibível, **When** o usuário vê a área de resultados, **Then** a tabela exibe no máximo os primeiros 60 itens e há um controle acionável para ir à próxima página.
2. **Given** o usuário está na primeira página com mais páginas disponíveis, **When** ele aciona o botão de próxima página, **Then** a tabela passa a mostrar os itens da página seguinte (sem misturar linhas da página anterior).
3. **Given** o usuário está na última página, **When** ele observa os controles de paginação, **Then** o botão “próxima” permanece **visível e desabilitado**, e os itens da última página permanecem visíveis.
4. **Given** uma busca completa com **60 ou menos** comércios no conjunto exibível, **When** o usuário vê a área de resultados, **Then** todos os itens aparecem de uma vez e **não** há controles de paginação (nem “próxima” nem “anterior”).

---

### User Story 2 - Voltar à página anterior (Priority: P2)

Como usuário, quando a paginação estiver ativa (> 60 itens), quero poder voltar à página anterior para revisar itens já vistos, sem reiniciar a busca.

**Why this priority**: Completa a navegação bidirecional quando a paginação existe.

**Independent Test**: Com > 60 itens, ir para a página 2 e acionar “anterior”; a página 1 reaparece com os mesmos itens da primeira visualização.

**Acceptance Scenarios**:

1. **Given** o usuário está na página 2 (ou superior) com paginação ativa, **When** ele aciona o controle de página anterior, **Then** a tabela mostra novamente os itens da página imediatamente anterior.
2. **Given** o usuário está na primeira página com paginação ativa, **When** ele observa os controles, **Then** o botão “anterior” permanece **visível e desabilitado**.

---

### User Story 3 - Filtro e ordenação preservados na paginação (Priority: P3)

Como usuário, quero que o filtro por nome e a ordenação por avaliação continuem valendo sobre o conjunto completo filtrado/ordenado, e que a paginação (quando existir) apenas “fatie” esse conjunto — sem perder o filtro nem a ordem ao mudar de página.

**Why this priority**: O pedido explícito de manter filtros e buscas evita regressão nas features já existentes da área de resultados.

**Independent Test**: Aplicar filtro e/ou ordenação; se o conjunto filtrado tiver > 60, paginar; se ≤ 60, sem paginação. Ao mudar filtro/ordenação, voltar à página 1 quando a paginação existir.

**Acceptance Scenarios**:

1. **Given** um filtro por nome ativo cujo conjunto filtrado tem **mais de 60** itens, **When** o usuário navega entre páginas, **Then** todas as páginas mostram somente itens que batem com o filtro.
2. **Given** a ordenação por avaliação ativa e conjunto exibível com **mais de 60** itens, **When** o usuário muda de página, **Then** a ordem global permanece a ordenada; cada página é um segmento contíguo dessa ordem.
3. **Given** o usuário está na página 2 ou superior, **When** ele altera o filtro por nome ou o sentido da ordenação por avaliação, **Then** a visualização volta para a página 1 do novo conjunto filtrado/ordenado (e a paginação só reaparece se o novo conjunto ainda tiver mais de 60).
4. **Given** um filtro que reduz o conjunto a **60 ou menos** itens, **When** o usuário observa a área de resultados, **Then** todos os itens filtrados aparecem sem controles de paginação.

---

### Edge Cases

- Busca completa com zero itens: estado vazio usual; sem controles de paginação.
- Exatamente 60 itens no conjunto exibível: lista completa sem paginação.
- Exatamente 61 itens: paginação ativa; página 1 com 60, página 2 com 1; “próxima” habilitada na página 1 e desabilitada na página 2; “anterior” desabilitada na página 1.
- Filtro que zera resultados: mensagem de “nenhum corresponde ao filtro”; sem paginação.
- Nova busca iniciada: estado de página reinicia (página 1) quando a área de resultados da nova busca aparecer.
- Exportação CSV: continua exportando o conjunto completo da busca (não só a página visível).
- Limite do provedor externo de mapas: a paginação da UI percorre os itens **já coletados**; não inventa comércios além do que a coleta retornou.

## Requirements *(mandatory)*

### Quality & Testing Constraints *(from constitution)*

- Cada user story MUST ter cenários de aceitação mapeáveis a testes
  automatizados (unitários e/ou integração).
- A solução MUST permanecer enxuta: evitar requisitos que forcem
  complexidade sem valor claro ao usuário.
- Comportamentos observáveis MUST ser descritos de forma testável
  (Given/When/Then), sem ambiguidade.

### Functional Requirements

- **FR-001**: A interface MUST exibir controles de paginação **somente** quando o conjunto exibível tiver **mais de 60** itens.
- **FR-002**: Quando o conjunto exibível tiver **60 ou menos** itens, a interface MUST mostrar todos os itens de uma vez e MUST NOT exibir controles de paginação (próxima/anterior).
- **FR-003**: Quando a paginação estiver ativa, cada página MUST exibir no máximo **60** linhas; itens além disso MUST ser acessíveis apenas mudando de página; a interface MUST oferecer ação explícita de **próxima página**.
- **FR-004**: Com paginação ativa e o usuário fora da primeira página, a interface MUST oferecer ação de **página anterior**.
- **FR-005**: Com paginação ativa, o rodapé/resumo MUST exibir o **intervalo da página atual** e o **total do conjunto filtrado** no formato equivalente a “Mostrando {início}–{fim} de {total}” (ex.: “Mostrando 1–60 de 125”). Sem paginação (conjunto exibível com **60 ou menos** itens), o rodapé MUST manter o formato simples existente equivalente a “{N} de {Y} resultados”.
- **FR-006**: Filtro por nome existente MUST continuar aplicando-se ao conjunto completo de resultados carregados da busca; a regra de “mais de 60” e a paginação MUST atuar sobre o conjunto **já filtrado**.
- **FR-007**: Ordenação por avaliação existente MUST continuar aplicando-se ao conjunto filtrado; a paginação MUST atuar sobre o conjunto **já ordenado** quando a ordenação estiver ativa.
- **FR-008**: Ao alterar filtro por nome, ordenação por avaliação, ou ao trocar de busca, a página atual MUST voltar para a **primeira** página (quando a paginação ainda se aplicar).
- **FR-009**: Com paginação ativa, nos extremos (primeira/última página) os botões indisponíveis MUST permanecer **visíveis e desabilitados** (não ocultos).
- **FR-010**: A exportação CSV existente MUST continuar considerando o conjunto completo da busca, não apenas a página visível.
- **FR-011**: Esta feature MUST NÃO prometer ou simular coleta de comércios além do total já retornado pela busca; paginação é da **visualização** dos resultados coletados.
- **FR-012**: A paginação MUST ser implementada **somente no frontend**, fatiando a lista já carregada após filtro/ordenação; MUST NOT exigir nova requisição ao servidor a cada mudança de página.

### Key Entities

- **Conjunto exibível**: lista de comércios da busca após filtro por nome e, se aplicável, ordenação por avaliação.
- **Limiar de paginação**: 60 itens — acima disso a paginação existe; igual ou abaixo, lista única sem controles.
- **Página de resultados**: fatia contígua do conjunto exibível (tamanho máximo 60), com índice de página (começando em 1).
- **Controles de paginação**: ações de anterior/próxima, visíveis apenas quando o limiar é ultrapassado.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Em uma lista de teste com 60 itens, 100% dos itens aparecem de uma vez, zero controles de paginação estão presentes, e o rodapé usa o formato simples “N de Y resultados”.
- **SC-002**: Em uma lista de teste com 125 itens, a primeira página mostra 60; o rodapé indica o intervalo correspondente (ex.: “Mostrando 1–60 de 125”); após “próxima”, a segunda mostra 60 com intervalo 61–120; após outra “próxima”, a terceira mostra 5 com intervalo 121–125; “próxima” fica desabilitada na terceira.
- **SC-003**: A partir da página 2 (com > 60 itens), “anterior” restaura exatamente o conjunto da página 1 em 100% dos testes automatizados do fluxo.
- **SC-004**: Com filtro ativo que deixa 25 itens, não há paginação e só esses 25 aparecem; com filtro que deixa 70 itens, a paginação cobre só esses 70.
- **SC-005**: Com ordenação por avaliação ativa e > 60 itens, a concatenação das páginas reproduz a ordem global ordenada.
- **SC-006**: Ao mudar filtro ou ordenação estando na página 2+, 100% dos casos voltam à página 1 (ou perdem a paginação se o novo conjunto ≤ 60).
- **SC-007**: Cenários P1–P3 possuem testes automatizados passando antes de considerar a feature concluída.
- **SC-008**: Mudanças de página (próxima/anterior) concluém sem nova requisição de listagem ao servidor em 100% dos testes do fluxo de paginação.

## Assumptions

- O tamanho de página e o limiar de exibição dos controles são **60** (confirmado em Clarifications).
- A contagem que decide paginar é a do **conjunto exibível** (após filtro), não necessariamente o `totalFound` bruto se o filtro reduzir a lista.
- A paginação é da **lista de resultados já coletados e carregados** na busca atual (somente frontend; confirmado em Clarifications); o teto do provedor externo de mapas permanece uma limitação de coleta — fora do escopo “furar” esse teto.
- Garantir que a lista carregada na UI cubra o conjunto coletado necessário à paginação (até o teto do produto) é responsabilidade desta feature no frontend; não há contrato novo de paginação na API.
- Controles mínimos: botões **Anterior** e **Próxima**. Nos extremos, botões indisponíveis ficam visíveis e desabilitados (confirmado em Clarifications).
- Com paginação ativa, o rodapé usa intervalo + total filtrado (ex.: “Mostrando 1–60 de 125”; confirmado em Clarifications). Sem paginação (≤ 60), o rodapé permanece no formato simples “N de Y resultados” (confirmado em Clarifications). Indicação extra “Página X de Y” ao lado dos botões não é obrigatória se o rodapé já comunicar a posição.
- Filtro = filtro por nome já existente; manter também a ordenação por avaliação ao paginar.
- Escopo centrado na home / área de resultados; não altera o formulário de região/termo/limite máximo.
- Exportação CSV e coleta assíncrona permanecem inalterados em comportamento de negócio, salvo ajustes mínimos necessários para a UI paginar.
- Visibilidade da área de resultados (feature 005) e regras de ordenação (feature 004) permanecem válidas; a paginação só aparece quando a tabela está elegível **e** o limiar de 60 é ultrapassado.
