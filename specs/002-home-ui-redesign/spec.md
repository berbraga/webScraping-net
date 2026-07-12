# Feature Specification: Redesign da Home de Busca

**Feature Branch**: `002-home-ui-redesign`

**Created**: 2026-07-10

**Status**: Draft

**Input**: User description: "deixar a home igual a estes prints; manter a mesma API"

**Visual references**: `references/01-form-idle.png`, `references/02-processing.png`, `references/03-completed-results.png`

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Formulário inicial alinhado ao print (Priority: P1)

Como operador, quero abrir a home e ver o formulário de busca no layout dos prints (título, subtítulo, card com Região / Termo / Limite e botão principal), para iniciar uma coleta com a mesma experiência visual definida no design.

**Why this priority**: É o primeiro contato e a base de todas as outras telas; sem isso o redesign não entrega o pedido.

**Independent Test**: Abrir a home sem busca ativa e comparar com `references/01-form-idle.png` (estrutura, hierarquia e estados do formulário).

**Acceptance Scenarios**:

1. **Given** o usuário abre a home sem busca em andamento, **When** a página carrega, **Then** vê o título "Busca de Comércios no Google Maps", o subtítulo de orientação e um card de formulário centralizado.
2. **Given** o formulário visível, **When** o usuário observa os campos, **Then** existem três campos no mesmo bloco: Região, Termo / categoria e Limite máximo, com o botão "Buscar comércios" ocupando a largura do card.
3. **Given** o formulário preenchido, **When** o usuário inicia a busca, **Then** a coleta usa o mesmo fluxo de negócio já existente (sem mudança de contrato de API).

---

### User Story 2 - Progresso durante a coleta (Priority: P1)

Como operador, quero ver o estado "processando", a contagem processados/total e uma barra de progresso enquanto a busca roda, e o botão principal em estado "Buscando...", conforme o print de processamento.

**Why this priority**: O feedback visual durante a coleta é parte central dos prints e evita a sensação de tela travada.

**Independent Test**: Iniciar uma busca e, enquanto o status não for terminal, validar contra `references/02-processing.png`.

**Acceptance Scenarios**:

1. **Given** uma busca em andamento, **When** o usuário olha o formulário, **Then** o botão principal exibe "Buscando..." e permanece claramente em estado de ocupado.
2. **Given** uma busca em andamento com totais conhecidos, **When** o progresso atualiza, **Then** aparece o texto de status com a palavra de estado destacada (ex.: processando) e a fração processados/total (ex.: 3/8).
3. **Given** progresso parcial, **When** a barra é exibida, **Then** o preenchimento visual corresponde à proporção processados/total.

---

### User Story 3 - Resultados, exportação e filtro por nome (Priority: P1)

Como operador, quero ver a tabela de resultados no layout do print (Nome, Telefone, Site, Avaliação), com marcador de ausência, exportar CSV e filtrar por nome na própria tela, após a coleta concluir.

**Why this priority**: É o valor final da home redesenhada e inclui o filtro por nome presente no print de conclusão.

**Independent Test**: Com uma busca concluída e lista preenchida, validar contra `references/03-completed-results.png` (status completed, exportar, filtro, tabela e rodapé de contagem).

**Acceptance Scenarios**:

1. **Given** uma busca concluída com itens, **When** o usuário visualiza a área de resultados, **Then** vê status completed com processados/total, botão "Exportar CSV", campo "Filtrar por nome..." e tabela com colunas Nome, Telefone, Site e Avaliação.
2. **Given** um comércio sem telefone (ou outro campo opcional ausente), **When** a linha é renderizada, **Then** o campo ausente mostra um marcador visual de indisponibilidade (X), sem omitir a linha.
3. **Given** sites disponíveis, **When** o usuário clica no link do site, **Then** o destino abre como hiperlink utilizável.
4. **Given** vários resultados na tabela, **When** o usuário digita no filtro por nome, **Then** a tabela mostra apenas linhas cujo nome contém o texto informado (sem distinguir maiúsculas/minúsculas) e o rodapé reflete a contagem filtrada (ex.: "3 de 8 resultados").
5. **Given** resultados disponíveis, **When** o usuário aciona "Exportar CSV", **Then** obtém o arquivo de exportação pelo mesmo comportamento de API já existente.

---

### Edge Cases

- Busca sem resultados: manter formulário/status claros e mensagem de lista vazia, sem tabela “fantasma”.
- Erro na busca: mensagem de erro legível sem quebrar o layout do card.
- Filtro sem correspondências: tabela vazia + contagem "0 de N resultados".
- Campos ausentes (telefone/site/avaliação): sempre X/marcador, nunca célula quebrada.
- Tela estreita: campos do formulário podem empilhar verticalmente, preservando a mesma hierarquia visual dos prints.
- Cancelamento (se disponível na app atual): não remover o redesign; progresso/parciais continuam legíveis.

## Requirements *(mandatory)*

### Quality & Testing Constraints *(from constitution)*

- Cada user story MUST ter cenários de aceitação mapeáveis a testes
  automatizados (unitários e/ou integração).
- A solução MUST permanecer enxuta: evitar requisitos que forcem
  complexidade sem valor claro ao usuário.
- Comportamentos observáveis MUST ser descritos de forma testável
  (Given/When/Then), sem ambiguidade.

### Functional Requirements

- **FR-001**: A home MUST reproduzir a composição visual dos prints de referência (fundo claro, card branco de formulário, tipografia hierárquica, botão principal em destaque verde-escuro).
- **FR-002**: O formulário MUST exibir Região, Termo / categoria e Limite máximo no mesmo card, com botão "Buscar comércios".
- **FR-003**: Durante coleta ativa, o botão principal MUST exibir "Buscando..." e o usuário MUST ver status + contagem processados/total + barra de progresso.
- **FR-004**: Com busca concluída, a interface MUST exibir status completed, ação "Exportar CSV", filtro "Filtrar por nome..." e tabela Nome / Telefone / Site / Avaliação.
- **FR-005**: Campos opcionais ausentes MUST ser marcados visualmente (X), sem remover o comércio da lista.
- **FR-006**: O filtro por nome MUST reduzir a tabela localmente conforme o texto digitado e atualizar o rodapé de contagem ("X de Y resultados").
- **FR-007**: Exportação CSV MUST continuar usando o mesmo endpoint/comportamento de API já existente (sem novo contrato).
- **FR-008**: Esta feature MUST NÃO alterar contratos, payloads ou endpoints da API de buscas; apenas a experiência visual e interações de apresentação na home.
- **FR-009**: Estados de erro e lista vazia MUST permanecer compreensíveis dentro do novo layout.

### Key Entities

- **Vista da Home**: estados idle, processando e concluído (e erro/vazio quando aplicável).
- **Linha de Resultado**: Nome, Telefone, Site, Avaliação e indicação de ausência.
- **Filtro de Nome**: texto de busca local sobre a lista já carregada.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Em revisão lado a lado com os três prints, pelo menos 90% dos elementos estruturais (título, card, campos, botão, status, barra, tabela, filtro, exportar, rodapé) estão presentes e na mesma ordem visual.
- **SC-002**: Operador identifica o estado da coleta (idle / processando / concluído) em menos de 3 segundos sem instrução adicional.
- **SC-003**: Com 8 resultados de exemplo, filtrar por um trecho de nome reduz a tabela corretamente e o rodapé mostra a contagem filtrada em 100% dos casos de teste manuais definidos.
- **SC-004**: Exportar CSV permanece funcional após o redesign (arquivo abre com as colunas esperadas).
- **SC-005**: Nenhum endpoint novo é exigido para cumprir esta feature; a home continua operando com a API atual.

## Assumptions

- Os três prints anexados são a fonte de verdade visual da home.
- O fluxo de negócio (criar busca, acompanhar, listar, cancelar se existir, exportar) permanece o da feature `001-maps-business-lookup`.
- O filtro por nome é apenas na interface (sobre resultados já carregados), não exige endpoint novo.
- Textos de status podem aparecer em português ou inglês conforme o backend atual (`processando` / `completed`); a UI destaca o estado com cor, como nos prints.
- Cancelar coleta não aparece nos prints; se já existir na app, pode permanecer de forma discreta sem contradizer o layout.
- Responsividade: desktop primeiro (como nos prints); em telas estreitas, empilhar campos é aceitável.
