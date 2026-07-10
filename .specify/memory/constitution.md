<!--
Sync Impact Report
- Version change: (template placeholders) → 1.0.0
- Modified principles:
  - [PRINCIPLE_1_NAME] → I. Clareza e Legibilidade
  - [PRINCIPLE_2_NAME] → II. Simplicidade Enxuta
  - [PRINCIPLE_3_NAME] → III. Testes Automatizados (NÃO NEGOCIÁVEL)
  - [PRINCIPLE_4_NAME] → IV. Responsabilidade Única
  - [PRINCIPLE_5_NAME] → V. Design Testável e Extensível
- Added sections: Restrições de Código, Fluxo de Qualidade
- Removed sections: (nenhuma — placeholders substituídos)
- Templates requiring updates:
  - ✅ .specify/templates/plan-template.md (Constitution Check)
  - ✅ .specify/templates/tasks-template.md (testes obrigatórios)
  - ✅ .specify/templates/spec-template.md (requisitos de qualidade/testes)
  - ⚠ .specify/templates/commands/*.md (diretório inexistente)
  - ⚠ README.md / docs/quickstart.md (ainda não existem)
- Follow-up TODOs: nenhum placeholder deferido
-->

# webScraping-net Constitution

## Core Principles

### I. Clareza e Legibilidade

Código MUST ser legível por humanos antes de ser "inteligente".
Nomes MUST descrever intenção (funções, tipos, variáveis).
Funções MUST ser pequenas e fazer uma coisa.
Condições de borda MUST ficar encapsuladas em um único local.
Comentários MUST explicar intenção ou consequências — nunca ruído
ou código morto. Código comentado MUST ser removido.
**Rationale**: Código fácil de ler reduz bugs, acelera revisão e
mantém o projeto compreensível ao crescer.

### II. Simplicidade Enxuta

O projeto MUST permanecer enxuto e fácil de entender.
YAGNI: não implementar o que não é necessário agora.
Complexidade desnecessária (abstrações prematuras, camadas extras,
configuração excessiva) MUST ser rejeitada em revisão.
Duplicação MUST ser eliminada quando a abstração resultante for
mais clara que a repetição.
Toda complexidade adicional MUST ser justificada no Complexity
Tracking do plano de implementação.
**Rationale**: Um codebase pequeno e direto é mais barato de
manter, testar e evoluir.

### III. Testes Automatizados (NÃO NEGOCIÁVEL)

Toda funcionalidade nova ou alterada MUST ter testes automatizados
que validem o comportamento esperado.
Testes MUST ser legíveis, rápidos, independentes e repetíveis.
Cenários de aceitação da spec MUST mapear para testes executáveis.
Nenhuma mudança MUST ser considerada concluída sem testes passando
localmente (e no CI, quando existir).
Preferir testes unitários para regras de negócio; usar integração
quando o valor estiver na colaboração entre componentes.
**Rationale**: Testes automatizados são a rede de segurança que
permite refatorar com confiança e manter qualidade contínua.

### IV. Responsabilidade Única

Cada módulo, classe ou função MUST ter um motivo claro para mudar.
Separar I/O (HTTP, filesystem, scraping) da lógica de domínio.
Preferir polimorfismo e composição a cadeias longas de if/else
ou switch/case para variar comportamento.
Seguir a Lei de Demeter: um tipo conhece apenas suas dependências
diretas.
**Rationale**: Responsabilidades bem delimitadas tornam o código
testável, reutilizável e menos frágil a mudanças.

### V. Design Testável e Extensível

Dependências externas MUST ser injetadas (não acopladas via
new/static oculto) nos pontos que precisam de teste ou troca.
Preferir métodos de instância a estáticos quando houver estado
ou colaboração.
Dados configuráveis MUST viver em níveis altos (config/opções),
não espalhados como números mágicos no código.
Efeitos colaterais MUST ser explícitos e isolados das funções
puras de transformação.
**Rationale**: Design testável reduz atrito para escrever testes
e permite evoluir o sistema sem reescrever o núcleo.

## Restrições de Código

- Preferir estruturas simples e bem definidas; evitar híbridos
  objeto/dados confusos.
- Encapsular estrutura interna; expor comportamento, não detalhes.
- Constantes nomeadas no lugar de números mágicos.
- Evitar condicionais negativas quando a forma positiva for clara.
- Não introduzir frameworks, pacotes ou camadas sem necessidade
  demonstrável na feature atual.
- Manter o grafo de dependências acíclico e explícito.

## Fluxo de Qualidade

1. Spec define cenários de aceitação testáveis.
2. Plano passa no Constitution Check antes da pesquisa/design.
3. Tasks incluem testes automatizados por user story (obrigatório).
4. Implementação: testes falham → código → testes passam →
   refatorar mantendo verde.
5. Revisão verifica: clareza, simplicidade, cobertura de testes e
   justificativa de complexidade.

## Governance

Esta constituição prevalece sobre hábitos locais e preferências
ad hoc. Em conflito, prevalece o princípio mais restritivo de
clareza, simplicidade e testes.

**Emendas**:
- Alterações MUST atualizar este arquivo, incrementar a versão
  (semver) e registrar impacto no Sync Impact Report.
- MAJOR: remoção/redefinição incompatível de princípios.
- MINOR: novo princípio/seção ou expansão material de orientação.
- PATCH: clarificações, redação, correções sem mudança semântica.
- `LAST_AMENDED_DATE` MUST ser a data da emenda (ISO YYYY-MM-DD).

**Compliance**:
- PRs e revisões MUST verificar aderência aos princípios.
- Violações exigem entrada em Complexity Tracking no `plan.md`
  ou correção antes do merge.
- Features Speckit (`/speckit-plan`, `/speckit-tasks`,
  `/speckit-implement`) MUST respeitar os gates desta constituição.

**Version**: 1.0.0 | **Ratified**: 2026-07-10 | **Last Amended**: 2026-07-10
