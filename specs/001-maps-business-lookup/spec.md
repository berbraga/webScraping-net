# Feature Specification: Busca de Comércios no Google Maps

**Feature Branch**: `001-maps-business-lookup`

**Created**: 2026-07-10

**Status**: Draft

**Input**: User description: "construa uma aplicação que me ajude a buscar comércios no google maps, em uma determinada região, a aplicação tem que correr cada um item da lista de todos os comércios e pegar algumas informações como, Nome, Telefone, endereço do site e avaliação do comercio"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Buscar e listar comércios da região (Priority: P1)

Como usuário, quero informar uma região e um termo de busca (ex.: "padarias", "restaurantes") para obter a lista de comércios encontrados no Google Maps nessa área, para depois enriquecer cada item com detalhes de contato.

**Why this priority**: Sem a lista de comércios da região, não há o que percorrer nem extrair. É o MVP mínimo.

**Independent Test**: Informar região + termo de busca e verificar que a aplicação retorna uma lista de comércios com pelo menos o nome identificável de cada um.

**Acceptance Scenarios**:

1. **Given** o usuário informou uma região válida e um termo de busca, **When** inicia a busca, **Then** a aplicação apresenta uma lista de comércios encontrados nessa região relacionados ao termo.
2. **Given** a busca retornou resultados, **When** o usuário visualiza a lista, **Then** cada item exibe ao menos o nome do comércio.
3. **Given** a região ou o termo não produzem resultados, **When** a busca termina, **Then** a aplicação informa claramente que nenhum comércio foi encontrado (sem falha silenciosa).

---



### User Story 2 - Extrair detalhes de cada comércio (Priority: P1)

Como usuário, quero que a aplicação percorra cada comércio da lista e colete Nome, Telefone, endereço do site e avaliação, para ter um cadastro útil de contatos e reputação.

**Why this priority**: É o valor principal pedido — enriquecer cada item da lista com os campos definidos. Pode ser entregue junto com a US1 no MVP.

**Independent Test**: A partir de uma lista conhecida de comércios, executar a coleta e verificar que cada item possui (quando disponível) nome, telefone, URL do site e avaliação.

**Acceptance Scenarios**:

1. **Given** existe uma lista de comércios da busca, **When** a aplicação percorre cada item, **Then** tenta obter Nome, Telefone, endereço do site e avaliação de cada um.
2. **Given** um comércio tem telefone, site e avaliação públicos, **When** a coleta desse item conclui, **Then** esses campos aparecem preenchidos no resultado.
3. **Given** um comércio não publica telefone ou site, **When** a coleta desse item conclui, **Then** o item permanece na lista com os campos ausentes marcados como indisponíveis (não descarta o comércio).
4. **Given** a coleta está em andamento, **When** o usuário acompanha o progresso, **Then** consegue ver quantos itens já foram processados em relação ao total.

---



### User Story 3 - Exportar resultados (Priority: P2)

Como usuário, quero exportar a lista enriquecida para um arquivo tabular (ex.: CSV), para usar os dados em planilhas, CRM ou campanhas.

**Why this priority**: Amplia o valor após a coleta, mas a visualização dos resultados já entrega utilidade. Pode vir após o MVP.

**Independent Test**: Após uma coleta com pelo menos um comércio, exportar e abrir o arquivo confirmando colunas Nome, Telefone, Site e Avaliação.

**Acceptance Scenarios**:

1. **Given** há resultados coletados, **When** o usuário solicita exportação, **Then** recebe um arquivo com uma linha por comércio e colunas para Nome, Telefone, Site e Avaliação.
2. **Given** alguns campos estão indisponíveis, **When** o arquivo é gerado, **Then** essas células ficam vazias (ou com marcador explícito de ausência), sem quebrar o formato do arquivo.

---



### Edge Cases

- Região inválida, ambígua ou inexistente: informar o problema e não inventar resultados.
- Termo de busca sem resultados na região: mensagem clara de lista vazia.
- Comércio sem telefone, sem site ou sem avaliação: manter o registro com um icone de X nos campos ausentes.
- Interrupção no meio da coleta (rede, cancelamento): preservar o que já foi coletado e indicar quais itens faltaram.
- Nomes ou endereços de site com caracteres especiais: preservar o texto sem corromper a exportação.
- Lista muito grande: permitir limite máximo configurável de comércios por execução para manter a operação previsível.
- Duplicatas aparentes na lista de resultados: preferir uma entrada por comércio distinto quando for possível identificá-lo de forma estável.



## Requirements *(mandatory)*



### Quality & Testing Constraints *(from constitution)*

- Cada user story MUST ter cenários de aceitação mapeáveis a testes
automatizados (unitários e/ou integração).
- A solução MUST permanecer enxuta: evitar requisitos que forcem
complexidade sem valor claro ao usuário.
- Comportamentos observáveis MUST ser descritos de forma testável
(Given/When/Then), sem ambiguidade.



### Functional Requirements

- **FR-001**: Usuário MUST poder informar uma região geográfica (nome de cidade, bairro ou localidade) como escopo da busca.
- **FR-002**: Usuário MUST poder informar um termo ou categoria de comércio (ex.: "restaurantes", "farmácias") para filtrar a busca.
- **FR-003**: Sistema MUST buscar comércios no Google Maps correspondentes à região e ao termo informados.
- **FR-004**: Sistema MUST percorrer cada comércio retornado na lista de resultados da busca.
- **FR-005**: Para cada comércio, o sistema MUST tentar coletar: Nome, Telefone, endereço do site (URL) e Avaliação.
- **FR-006**: Sistema MUST registrar campos ausentes de forma explícita quando o comércio não publica telefone, site ou avaliação.
- **FR-007**: Sistema MUST apresentar progresso da coleta (itens processados / total).
- **FR-008**: Usuário MUST poder exportar os resultados coletados em formato tabular com as colunas Nome, Telefone, Site e Avaliação.
- **FR-009**: Usuário MUST poder definir um limite máximo de comércios a processar por execução.
- **FR-010**: Sistema MUST informar falhas de busca ou de coleta de um item sem descartar silenciosamente o restante da lista.
- **FR-011**: Sistema MUST permitir cancelar uma coleta em andamento e conservar os resultados já obtidos até o cancelamento.



### Key Entities

- **Busca**: Região, termo/categoria, limite máximo opcional, estado (em andamento, concluída, cancelada, falhou).
- **Comércio**: Nome, Telefone (opcional), Site/URL (opcional), Avaliação (opcional), identificador estável quando disponível, status da coleta do item.
- **Resultado da Coleta**: Conjunto de comércios enriquecidos vinculados a uma Busca, com contagem de processados, sucesso parcial e falhas.



## Success Criteria *(mandatory)*



### Measurable Outcomes

- **SC-001**: Em uma região e termo com resultados públicos conhecidos, o usuário obtém uma lista de comércios em até 2 minutos para o início da listagem (primeiros itens visíveis).
- **SC-002**: Para pelo menos 90% dos comércios processados em uma execução de teste controlada, o Nome é preenchido corretamente.
- **SC-003**: Quando telefone, site ou avaliação estão publicamente disponíveis no anúncio do comércio, esses campos são capturados em pelo menos 80% dos casos na mesma execução de teste controlada.
- **SC-004**: Usuário consegue exportar e abrir o arquivo de resultados em uma planilha sem perda de colunas obrigatórias (Nome, Telefone, Site, Avaliação).
- **SC-005**: Em caso de item sem telefone/site/avaliação, 100% desses itens permanecem no resultado com ausência explícita do campo (nenhum comércio válido é omitido só por campo faltante).
- **SC-006**: Usuário consegue acompanhar o progresso e cancelar a coleta, mantendo os itens já coletados disponíveis após o cancelamento.



## Assumptions

- A fonte dos dados é o Google Maps (anúncios/listagens públicas de comércios).
- "Região" significa uma localidade textual (cidade, bairro ou ponto de referência), não um desenho livre no mapa na v1.
- O usuário informa um termo/categoria; a v1 não tenta enumerar "todos os comércios do mundo" sem filtro — isso seria inviável e pouco útil.
- Avaliação refere-se à nota pública do comércio no Google Maps (ex.: escala 1–5), quando existir.
- "Endereço do site" significa a URL do website do comércio, não o endereço físico (rua/número). Endereço físico fica fora do escopo da v1, salvo se surgir como dado auxiliar sem esforço extra.
- Uso responsável: o usuário é responsável por respeitar termos de uso, leis locais e limites razoáveis de volume/frequência.
- Interface da v1 é orientada a uso local por um único operador (não multi-tenant / não portal web público).
- Limite máximo padrão de comércios por execução existe para manter execuções previsíveis; o usuário pode ajustá-lo.
- Autenticação de usuário final não faz parte do escopo da v1.

