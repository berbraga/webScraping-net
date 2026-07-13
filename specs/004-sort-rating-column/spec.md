# Feature Specification: Ordenação por Avaliação na Tabela

**Feature Branch**: `004-sort-rating-column`

**Created**: 2026-07-13

**Status**: Draft

**Input**: User description: "Implementar ordenação clicável no cabeçalho da tabela de resultados, focada inicialmente na coluna Avaliação — primeiro clique decrescente, segundo crescente (toggle), nulos no final, ícone visual de sentido, ordenação no client sobre os dados já exibidos sem nova requisição."

## Clarifications

### Session 2026-07-13

- Q: A implementação desta feature envolve backend/API ou apenas a interface? → A: Somente frontend (sem alterações de backend/API).
- Q: Se a lista ainda atualiza durante o processamento e o usuário já ordenou, o que acontece? → A: A ordenação só altera a lista quando a busca estiver completa.
- Q: Como o cabeçalho “Avaliação” se comporta durante o processamento? → A: Parece normal, mas cliques são ignorados até a busca completar.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Ordenar por avaliação (melhor → pior) (Priority: P1)

Como usuário, com a busca já concluída e a lista de comércios na tela, quero clicar em “Avaliação” e ver os estabelecimentos ordenados da maior nota para a menor, para identificar rapidamente os melhores.

**Why this priority**: Entrega o valor principal da feature (rankear por nota) com um único clique.

**Independent Test**: Com busca completa e uma lista contendo notas variadas, clicar uma vez em “Avaliação” e verificar a ordem decrescente das notas numéricas.

**Acceptance Scenarios**:

1. **Given** uma busca **completa** e a tabela de resultados visível com pelo menos três comércios com avaliações distintas, **When** o usuário clica pela primeira vez no cabeçalho “Avaliação”, **Then** as linhas são reordenadas da maior nota para a menor.
2. **Given** a busca completa e o cabeçalho “Avaliação” ainda não foi usado para ordenar nesta visualização, **When** o usuário passa o mouse sobre o texto “Avaliação”, **Then** o cursor indica área clicável (pointer).
3. **Given** o primeiro clique efetivo (com busca completa) acabou de aplicar ordem decrescente, **When** o usuário observa o cabeçalho, **Then** um indicador visual (ex.: seta para baixo) deixa claro que a ordenação ativa é decrescente.
4. **Given** a busca ainda em andamento e a tabela já com algumas linhas, **When** o usuário clica em “Avaliação”, **Then** a ordem das linhas não muda e nenhum sentido de ordenação fica ativo.

---

### User Story 2 - Alternar sentido da ordenação (Priority: P2)

Como usuário, quero clicar de novo no mesmo cabeçalho para inverter entre decrescente e crescente, inclusive para achar as piores notas.

**Why this priority**: Completa o ciclo de exploração (melhores ↔ piores) sem controles extras.

**Independent Test**: Após o primeiro clique (desc), clicar novamente e verificar ordem crescente; um terceiro clique volta ao decrescente.

**Acceptance Scenarios**:

1. **Given** a tabela já está ordenada de forma decrescente por avaliação, **When** o usuário clica novamente em “Avaliação”, **Then** a ordem passa a ser crescente (menor nota para a maior) e o ícone muda para indicar crescente.
2. **Given** a tabela está ordenada de forma crescente, **When** o usuário clica novamente em “Avaliação”, **Then** a ordem volta a ser decrescente e o ícone correspondente é exibido.
3. **Given** cliques sucessivos no mesmo cabeçalho, **When** o usuário continua clicando, **Then** o sentido continua alternando apenas entre crescente e decrescente.

---

### User Story 3 - Notas ausentes sempre no final (Priority: P3)

Como usuário, quero que comércios sem avaliação (nulo, vazio ou equivalente a indisponível) não “sujem” o topo da lista — devem ficar no fim, seja na ordem crescente ou decrescente.

**Why this priority**: Evita que estabelecimentos sem nota pareçam “melhores” ou “piores” indevidamente.

**Independent Test**: Lista mista com notas e itens sem nota; em ambos os sentidos, os sem nota ficam após todos os que têm nota.

**Acceptance Scenarios**:

1. **Given** a lista mistura comércios com nota e sem nota, **When** a ordenação está decrescente, **Then** todos com nota numérica vêm primeiro (maior→menor) e os sem nota ficam no final.
2. **Given** a mesma lista mista, **When** a ordenação está crescente, **Then** todos com nota vêm primeiro (menor→maior) e os sem nota ficam no final.
3. **Given** um comércio cuja avaliação é exibida como indisponível (vazio / N/A / equivalente), **When** qualquer sentido de ordenação por avaliação está ativo, **Then** esse comércio é tratado como “sem nota” e posicionado no final junto aos demais sem nota.

---

### Edge Cases

- Lista só com itens sem avaliação: ordenar por Avaliação não altera a utilidade da lista; todos permanecem juntos (já estão “no final”).
- Empates de nota iguais: ordem relativa entre empates pode permanecer estável o suficiente para não confundir (não é obrigatório desempate por nome nesta versão).
- Tabela vazia ou busca ainda em andamento: cliques em “Avaliação” são ignorados; a ordem das linhas **não** muda até a busca estar completa.
- Filtro por nome ativo: a ordenação aplica-se ao conjunto atualmente exibido (já filtrado), sem nova busca, **somente** com busca completa.
- Troca de busca / novos resultados: a ordenação volta ao estado inicial (sem sentido ativo) até o usuário clicar de novo em “Avaliação” após a nova busca completar.

## Requirements *(mandatory)*

### Quality & Testing Constraints *(from constitution)*

- Cada user story MUST ter cenários de aceitação mapeáveis a testes
  automatizados (unitários e/ou integração).
- A solução MUST permanecer enxuta: evitar requisitos que forcem
  complexidade sem valor claro ao usuário.
- Comportamentos observáveis MUST ser descritos de forma testável
  (Given/When/Then), sem ambiguidade.

### Functional Requirements

- **FR-001**: O cabeçalho da coluna “Avaliação” MUST permanecer visualmente presente na tabela quando houver resultados; a aparência de cabeçalho ordenável (incluindo cursor pointer no hover) MAY existir também durante o processamento.
- **FR-002**: Com a busca **completa**, ao passar o ponteiro sobre o cabeçalho “Avaliação”, o cursor MUST indicar área clicável e o clique MUST aplicar/alternar a ordenação.
- **FR-003**: O primeiro clique efetivo em “Avaliação” (busca completa, estado sem ordenação ativa) MUST ordenar as linhas por nota em ordem **decrescente**.
- **FR-004**: Cliques consecutivos efetivos no mesmo cabeçalho MUST alternar entre ordenação **decrescente** e **crescente**.
- **FR-005**: Itens sem avaliação válida (nulo, vazio, ou valor tratado como indisponível / “N/A”) MUST ser posicionados **após** todos os itens com nota numérica, em ambos os sentidos de ordenação.
- **FR-006**: Enquanto a ordenação por Avaliação estiver ativa, o cabeçalho MUST exibir um indicador visual dinâmico do sentido (ex.: seta para baixo = decrescente; seta para cima = crescente).
- **FR-007**: A reordenação MUST ocorrer sobre os dados já disponíveis na tela (lista já carregada/filtrada), **sem** disparar nova coleta ou nova requisição de busca ao servidor.
- **FR-008**: Nesta versão, apenas a coluna “Avaliação” MUST ser ordenável; as demais colunas permanecem cabeçalhos não ordenáveis.
- **FR-009**: A exportação CSV existente MUST permanecer funcional; esta feature **não** exige que o arquivo exportado reflita a ordem visual da tabela (ordenação é da visualização).
- **FR-010**: A feature MUST ser implementada **somente no frontend**; MUST NOT exigir mudanças de contrato, endpoints ou lógica de servidor.
- **FR-011**: Enquanto a busca **não** estiver completa, a interface MUST **não** alterar a ordem das linhas por causa da ordenação por Avaliação (a lista permanece na ordem original da coleta/exibição).
- **FR-012**: Enquanto a busca **não** estiver completa, cliques no cabeçalho “Avaliação” MUST ser ignorados (sem mudança de ordem e sem ativar ícone de sentido); o cabeçalho MUST NÃO precisar parecer desabilitado.

### Key Entities

- **Linha de resultado (comércio)**: inclui avaliação opcional (número ou ausente) usada como chave de ordenação.
- **Estado de ordenação da visualização**: sentido ativo (nenhum → após primeiro clique decrescente → alterna com crescente) e indicador visual associado ao cabeçalho Avaliação.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Em uma lista de teste com busca completa e notas 1.0, 3.0 e 5.0, após o primeiro clique em “Avaliação”, a sequência visível das notas é 5.0 → 3.0 → 1.0.
- **SC-002**: Na mesma lista, o segundo clique produz 1.0 → 3.0 → 5.0; o terceiro clique restaura 5.0 → 3.0 → 1.0.
- **SC-003**: Em uma lista com pelo menos um item sem nota e três com nota, 100% dos itens sem nota aparecem depois de todos os com nota, nos dois sentidos.
- **SC-004**: 100% dos cliques de ordenação na Avaliação, em condições normais de uso da lista já carregada, concluém sem nova busca de comércios (sem recarregar a coleta).
- **SC-005**: Em teste com usuários ou checklist de UX, ≥ 90% identificam corretamente o sentido ativo pelo ícone no cabeçalho após um clique.
- **SC-006**: Cenários P1–P3 possuem testes automatizados passando antes de considerar a feature concluída.
- **SC-007**: Durante uma busca em andamento, cliques em “Avaliação” não alteram a ordem nem ativam o ícone de sentido; após a conclusão, o usuário consegue ordenar conforme SC-001.

## Assumptions

- Escopo v1 limitado à coluna “Avaliação”; outras colunas ordenáveis ficam para uma feature futura.
- “Dados já renderizados” inclui o conjunto após filtro por nome, quando o filtro estiver ativo.
- Valores de avaliação exibidos como indisponíveis (marcador visual atual ou texto vazio/N/A) contam como “sem nota” para o edge case.
- Antes do primeiro clique, a ordem permanece a ordem original fornecida pela busca (sem ícone de sentido obrigatório).
- Ao iniciar uma nova busca ou substituir o conjunto de resultados, o estado de ordenação reinicia (sem sentido ativo).
- Acessibilidade mínima: o controle no cabeçalho deve ser acionável também por teclado (foco + ativação), além do clique do mouse.
- Ordenação só altera a lista quando a busca está completa (confirmado em Clarifications); durante o processamento a ordem original é preservada e cliques no cabeçalho são ignorados sem exigir aparência de desabilitado.
- Escopo exclusivo de frontend: nenhum trabalho de backend/API nesta feature (confirmado em Clarifications).
