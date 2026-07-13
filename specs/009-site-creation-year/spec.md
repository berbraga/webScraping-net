# Feature Specification: Ano de Criação via Copyright do Site

**Feature Branch**: `009-site-creation-year`

**Created**: 2026-07-13

**Status**: Draft

**Input**: User description: "Abandonar WHOIS; extrair o ano de criação a partir de padrões de copyright no rodapé do site do comércio (© / Copyright / intervalos); retornar o ano mais antigo como inteiro; em falhas HTTP ou ausência de padrão retornar nulo sem travar o scraper; integrar no processo principal no campo site_creation_year."

## Clarifications

### Session 2026-07-13

- Q: Quando a leitura do site para extrair o ano deve ocorrer no fluxo da coleta? → A: Após coletar os campos Places de todos os itens; ler os sites em paralelo; só então marcar a busca como concluída
- Q: Um intervalo ou ano solto no rodapé sem marcador de copyright deve contar? → A: Sim — qualquer ano/intervalo 19xx–20xx no rodapé/final da página conta, mesmo sem ©/Copyright
- Q: Qual o tempo máximo por leitura de site antes de marcar o ano como ausente? → A: Até ~10 segundos por site
- Q: Se vários comércios na mesma busca tiverem a mesma URL de site, reutilizar o ano? → A: Reutilizar na mesma busca (sucesso ou falha); não baixar a mesma URL de novo
- Q: Qual o limite máximo de leituras de site em paralelo ao mesmo tempo? → A: No máximo ~10 leituras simultâneas (fila para o restante)

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Obter o ano mais antigo de copyright do site (Priority: P1)

Como usuário, quero que, após a coleta dos dados Places (nome, telefone, site, avaliação) de todos os comércios da busca, o sistema leia as páginas dos sites em paralelo, obtenha o ano indicado nos avisos de copyright (prioritariamente no rodapé) e grave `site_creation_year` (menor ano em intervalos), e só então marque a busca como concluída — para estimar antiguidade online sem depender de registro de domínio.

**Why this priority**: É o valor central da feature e substitui a abordagem baseada em registro de domínio.

**Independent Test**: A partir de páginas com copyright conhecido (intervalo, ano único, ou sem padrão), verificar que o resultado do comércio contém o ano esperado, um único ano válido, ou ausência explícita — sem falhar a coleta; a busca só fica concluída depois dessa fase de leitura em paralelo.

**Acceptance Scenarios**:

1. **Given** os campos Places de todos os itens já foram coletados e um comércio tem URL cujo site exibe copyright com intervalo (ex.: “© 2016-2026” ou “2018 - 2024”), **When** a fase de leitura de sites em paralelo termina, **Then** `site_creation_year` é o menor ano do intervalo (ex.: 2016 ou 2018) e a busca pode ser marcada concluída.
2. **Given** a página exibe um único ano de copyright (ex.: “Copyright 2015” ou “© 2020”), **When** a fase de leitura de sites termina, **Then** `site_creation_year` é esse ano.
3. **Given** a página não contém padrão reconhecível de copyright com ano, **When** a fase de leitura termina, **Then** `site_creation_year` fica nulo/ausente e os demais campos do comércio permanecem intactos.
4. **Given** um comércio sem URL de site, **When** a fase de leitura de sites roda, **Then** não há tentativa de leitura de página para esse item e `site_creation_year` fica nulo/ausente.
5. **Given** uma busca com vários sites, **When** a fase de anos está em andamento, **Then** as leituras ocorrem em paralelo com no máximo ~10 simultâneas (excedentes aguardam na fila) e a busca permanece em processamento até essa fase acabar.
6. **Given** dois ou mais comércios na mesma busca com a mesma URL de site, **When** a fase de leitura processa esses itens, **Then** a página é obtida no máximo uma vez e todos recebem o mesmo `site_creation_year` (valor ou ausência).

---

### User Story 2 - Não interromper a coleta quando o site falha (Priority: P1)

Como usuário, quero que erros ao acessar o site (timeout, página não encontrada, certificado inválido, site fora do ar, resposta ilegível) resultem apenas em `site_creation_year` ausente, para que a busca continue e o restante dos dados do comércio seja preservado.

**Why this priority**: Sites de clientes são instáveis; sem resiliência a feature piora a coleta existente.

**Independent Test**: Forçar falhas de acesso/parse em um ou mais itens e confirmar busca concluída com demais campos ok e `site_creation_year` ausente nesses itens.

**Acceptance Scenarios**:

1. **Given** a URL do site não responde em até ~10 segundos ou retorna erro de acesso, **When** o item é processado na fase de leitura, **Then** `site_creation_year` fica nulo e as demais leituras em paralelo seguem.
2. **Given** vários comércios na mesma busca, **When** apenas alguns sites falham, **Then** os demais podem ter ano preenchido e os que falharam mostram ausência, sem abortar a busca.
3. **Given** o acesso ao site falha, **When** o status do item é observado, **Then** a falha de leitura de copyright **não** é tratada como falha da coleta dos dados já obtidos via Places (nome, telefone, site, avaliação).

---

### User Story 3 - Ver e exportar o ano na lista de resultados (Priority: P1)

Como usuário, quero ver `site_creation_year` na tabela de resultados (coluna dedicada, imediatamente à direita da coluna do site) e na exportação tabular, para usar o ano junto com os demais dados do comércio.

**Why this priority**: Sem superfície na lista/exportação, o campo só existiria nos bastidores; o produto já centra valor na tabela.

**Independent Test**: Busca com itens com e sem ano → coluna à direita de Site com ano ou indicador de indisponível; CSV com a mesma coluna.

**Acceptance Scenarios**:

1. **Given** a busca concluída, **When** o usuário vê a tabela, **Then** existe coluna dedicada ao ano de criação do site imediatamente à direita da coluna do site.
2. **Given** um item com `site_creation_year` preenchido, **When** a linha é exibida, **Then** o ano aparece como número inteiro de quatro dígitos.
3. **Given** um item sem ano, **When** a linha é exibida, **Then** o mesmo padrão visual de indisponibilidade dos demais campos ausentes é usado.
4. **Given** o usuário exporta, **When** o arquivo é gerado, **Then** inclui coluna equivalente ao ano de criação do site, com valor ou célula vazia.

---

### Edge Cases

- URL inválida ou inacessível: não quebrar a coleta; ano ausente.
- Timeout curto de acesso à página (~10s): ano ausente e seguir.
- Página sem `<footer>`: ainda tentar achar copyright nas últimas porções de texto da página; se nada válido, ano ausente.
- Intervalos com espaços ou traços variados (“2016-2026”, “2016 - 2026”): usar o menor ano.
- Múltiplos avisos ou anos diferentes na área considerada: usar o menor ano válido encontrado no rodapé / final da página (marcador de copyright opcional).
- Anos fora de padrões 19xx/20xx: ignorar; não inventar valor.
- Anos soltos ou intervalos sem ©/Copyright no rodapé/final: ainda assim elegíveis se forem 19xx/20xx.
- Conteúdo não HTML ou corpo vazio: ano ausente.
- Certificado SSL inválido ou erro de rede: ano ausente.
- Comércios sem site: coluna visível, valor indisponível.
- Buscas com muitas URLs distintas: paralelismo limitado a ~10 conexões ativas; não liberar todas de uma vez.

## Requirements *(mandatory)*

### Quality & Testing Constraints *(from constitution)*

- Cada user story MUST ter cenários de aceitação mapeáveis a testes
  automatizados (unitários e/ou integração).
- A solução MUST permanecer enxuta: evitar requisitos que forcem
  complexidade sem valor claro ao usuário.
- Comportamentos observáveis MUST ser descritos de forma testável
  (Given/When/Then), sem ambiguidade.

### Functional Requirements

- **FR-001**: Após a coleta dos campos Places de todos os comércios da busca, o sistema MUST, para cada item com URL de site, obter o conteúdo da página e tentar extrair um ano de copyright, gravando `site_creation_year`; a busca MUST ser marcada como concluída somente depois dessa fase.
- **FR-012**: A fase de leitura de sites MUST executar as requisições em paralelo, com no máximo aproximadamente **10** leituras simultâneas; URLs distintas excedentes MUST aguardar na fila até haver vaga.
- **FR-002**: A extração MUST priorizar a região de rodapé da página; se insuficiente, MUST considerar o final do conteúdo textual da página.
- **FR-003**: O sistema MUST reconhecer anos únicos ou intervalos (com ou sem símbolos/palavras de copyright próximos) na área analisada (rodapé / final da página) e MUST definir `site_creation_year` como o **menor** ano válido encontrado (ex.: intervalo 2016–2026 → 2016; “© 2020” → 2020; “2018 - 2024” sem a palavra Copyright → 2018).
- **FR-013**: Na área de busca (rodapé ou final da página), a presença de marcador ©/Copyright MUST NOT ser obrigatória para aceitar um ano ou intervalo 19xx/20xx.
- **FR-004**: Somente anos no intervalo de séculos 19xx e 20xx MUST ser considerados válidos; demais números MUST ser ignorados.
- **FR-005**: `site_creation_year` MUST ser um número inteiro de quatro dígitos quando obtido; caso contrário MUST ser nulo/ausente (nunca string parcial ou valor inventado).
- **FR-006**: Falhas de acesso, tempo esgotado, certificado inválido, página inexistente, conteúdo ilegível ou ausência de padrão MUST resultar em `site_creation_year` nulo sem interromper a coleta nem invalidar os demais campos do comércio.
- **FR-007**: O acesso à página do site MUST respeitar um tempo máximo de aproximadamente **10 segundos** por tentativa; ao estourar, tratar como indisponível e seguir sem bloquear as demais leituras paralelas.
- **FR-008**: A obtenção do ano MUST ocorrer em fase própria após Places, com leituras em paralelo, de modo que falhas/timeouts de um site não impeçam a conclusão das demais leituras nem o fechamento da busca.
- **FR-009**: A tabela de resultados MUST exibir o ano em coluna própria com cabeçalho **Criação do site**, imediatamente à direita da coluna do endereço do site, reutilizando o padrão visual de indisponibilidade quando nulo.
- **FR-010**: A exportação tabular MUST incluir a coluna **Criação do site** imediatamente após a coluna do site, alinhada a `site_creation_year`.
- **FR-011**: Esta feature MUST NOT depender de consulta a registro de domínio (WHOIS/equivalente) para obter o ano; a fonte é exclusivamente o conteúdo do site do comércio.
- **FR-014**: Dentro de uma mesma execução de busca, o sistema MUST reutilizar o resultado da leitura de uma URL de site já vista (ano obtido ou nulo por falha/ausência), sem nova obtenção da página para os demais comércios com a mesma URL.

### Key Entities

- **Comércio (resultado)**: Registro existente com nome, telefone, site, avaliação etc.; passa a incluir opcionalmente `site_creation_year`.
- **Ano de criação do site (`site_creation_year`)**: Inteiro de quatro dígitos derivado do menor ano 19xx/20xx encontrado no rodapé/final da página, ou ausente.
- **Ano no rodapé**: Trecho no rodapé ou final da página contendo ano(s) 19xx/20xx (com ou sem ©/Copyright).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Em páginas de teste com copyright claro (ano único ou intervalo), pelo menos 95% das coletas retornam o menor ano esperado em `site_creation_year`.
- **SC-002**: Falhas de acesso ao site nunca impedem a conclusão da busca nem a retenção dos demais campos do comércio (0 interrupções causadas por essa leitura).
- **SC-003**: Nenhuma tentativa de leitura de página por site bloqueia por mais de ~10 segundos antes de degradar para ano ausente.
- **SC-004**: Em 100% das visualizações da tabela de resultados da feature, a coluna **Criação do site** aparece imediatamente à direita da coluna do site.
- **SC-005**: Arquivos exportados incluem a coluna **Criação do site** coerente com o que a tabela mostra para a mesma busca.
- **SC-006**: Usuário distingue ano presente vs. ausente em no máximo uma passada visual pela linha do comércio.

## Assumptions

- A URL do site já coletada (Google Places) é a fonte de partida; não se descobre site por outros meios nesta feature.
- A leitura de copyright ocorre numa **segunda fase** (após enrich Places de todos os itens), em paralelo com teto de ~10 simultâneas; “concluída” só após essa fase.
- Reuso de URL vale apenas dentro da mesma execução de busca; buscas posteriores podem ler de novo.
- “Ano de criação do site” nesta feature significa o **menor ano 19xx/20xx** encontrado no rodapé/final da página (proxy de antiguidade anunciada), **não** a data de registro do domínio.
- Consulta a registro de domínio (WHOIS/RDAP/equivalente) está **fora de escopo**; esta feature a substitui como abordagem de produto para o ano/criação do site.
- Timeout de acesso à página: ~10 segundos por site (configurável na mesma ordem de grandeza), alinhado a SC-003.
- Cabeçalho canônico na UI e na exportação: **Criação do site** (campo de dados: `site_creation_year`).
- Coluna posicionada imediatamente à direita do endereço do site (mesmo padrão desejado na feature anterior de data de site).
- Buscas antigas sem o campo permanecem com valor ausente até nova coleta.
- Não é necessário baquear o navegador completo (scripts pesados); basta o conteúdo HTML acessível por requisição HTTP da URL do site.
- Se vários padrões válidos existirem na área analisada, prevalece o menor ano entre eles.
- Marcador ©/Copyright é desejável mas **não obrigatório** para aceitar o ano na área analisada.
