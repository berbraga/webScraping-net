# Research: Busca de Comércios no Google Maps

**Feature**: `001-maps-business-lookup`  
**Date**: 2026-07-10

## 1. Divisão Frontend / Backend

**Decision**: Next.js (JavaScript) no frontend; ASP.NET Core Web API (.NET 8) no
backend apenas para REST + persistência/orquestração.

**Rationale**: Atende o pedido do usuário; UI rica no React/Next; backend focado
em API e banco, sem Razor/Blazor.

**Alternatives considered**:
- Blazor + API no mesmo host — rejeitado (usuário pediu Next.js)
- Next.js full-stack (Route Handlers + DB) — rejeitado (usuário pediu .NET + C#)
- Scraping no browser (frontend) — rejeitado (expõe chaves, frágil, foge do BE)

## 2. Banco NoSQL

**Decision**: MongoDB com `MongoDB.Driver` oficial.

**Rationale**: Modelo documental casa com Busca + lista de Comércios; suporte
maduro em .NET; fácil de subir local (Docker); queries por `searchId` e status
simples.

**Alternatives considered**:
- Cosmos DB (API Mongo) — overkill para v1 local
- RavenDB — ecossistema menor para este caso
- Firebase/Firestore só no frontend — conflita com “BE integra com o banco”

## 3. Fonte de dados do Google Maps

**Decision**: Google Places API (New) — Text Search (ou equivalente) para listar
por região+termo; Place Details para telefone, website e rating. Acessada só
pelo backend via `IBusinessLookupSource`.

**Rationale**: Backend “API + DB” integra por HTTP estável; campos da spec
(nome, telefone, site, avaliação) existem na Places; testável com fake;
mais alinhado a ToS do que HTML scraping.

**Alternatives considered**:
- Playwright/Puppeteer scrapando maps.google.com — frágil, lento, risco ToS;
  deixa o backend bem além de “API + DB”
- SerpAPI / provedores terceiros — custo e dependência extra sem necessidade na v1

## 4. Progresso e cancelamento

**Decision**: Job em background (`BackgroundService` + fila in-process
`Channel<T>` ou equivalente). Frontend faz polling em `GET /api/searches/{id}`.
Cancelamento via `POST /api/searches/{id}/cancel` (flag cooperativa).

**Rationale**: Atende FR-007/FR-011 com o mínimo de infra (sem broker). Polling
é mais simples que WebSockets/SignalR na v1.

**Alternatives considered**:
- SignalR — melhor UX real-time, complexidade injustificada agora
- Hangfire/RabbitMQ — infra extra desnecessária para operador único

## 5. Exportação CSV

**Decision**: Endpoint `GET /api/searches/{id}/export` retorna CSV
(UTF-8, colunas Nome, Telefone, Site, Avaliação). Frontend dispara download.

**Rationale**: Um único lugar formata o arquivo; caracteres especiais tratados
no servidor; UI só baixa o blob.

**Alternatives considered**:
- Gerar CSV só no browser — possível, mas duplica regra e dificulta teste E2E da FR-008 no BE

## 6. Testes

**Decision**:
- Domínio/Application: unitários com fakes de `IBusinessLookupSource` e repositórios
- API: testes de integração com WebApplicationFactory + Mongo de teste
- Frontend: Vitest para formulário, lista, estados de progresso e cliente API mockado

**Rationale**: Constituição III — testes obrigatórios e mapeáveis às user stories.

**Alternatives considered**:
- Só E2E Playwright contra Google real — lento, flaky, caro em quota

## 7. Autenticação

**Decision**: Sem auth na v1 (conforme Assumptions da spec).

**Rationale**: Operador local único; YAGNI.

**Alternatives considered**: API key estática entre FE/BE — opcional depois se expor a rede
