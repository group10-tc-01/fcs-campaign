# fcg-campaign

Serviço de **Campanhas e Transparência** da plataforma **Conexão Solidária**. É responsável pela administração de campanhas por **GestorONG**, exposição pública de campanhas ativas e atualização idempotente do valor arrecadado após o processamento de doações.

> Microsserviço que compõe o MVP da Conexão Solidária junto a `fcg-identity`, `fcg-donations`, `fcg-donation-worker`, `fcg-audit-logs`, `fcg-solidarity-web` e `fcg-solidarity-infra`.

---

## Responsabilidades

- Criação, edição, conclusão e cancelamento de **Campanhas** por usuários com role `GestorONG`.
- Listagem administrativa de campanhas.
- Listagem pública de campanhas ativas para o **Painel de Transparência**.
- Validação interna de elegibilidade de campanha para receber intenção de doação.
- Atualização interna e idempotente do `TotalAmountRaised` quando uma doação é processada.
- Persistência própria no `CampaignsDb`, sem foreign key para databases de outros serviços.
- Auditoria explícita de eventos relevantes via tópico Kafka `audit-log-requested`.

A aplicação **não** recebe intenções de doação diretamente. Esse fluxo pertence à `fcg-donations`, que valida a campanha via API interna e publica eventos para processamento assíncrono pelo `fcg-donation-worker`.

Documentação completa da arquitetura: [group10-tc-01/fcg-fase05-docs](https://github.com/group10-tc-01/fcg-fase05-docs).

Referências diretas:

- [Visão geral da arquitetura](https://github.com/group10-tc-01/fcg-fase05-docs/blob/main/architecture/overview.md)
- [Modelo da fcg-campaigns](https://github.com/group10-tc-01/fcg-fase05-docs/blob/main/architecture/fcg-campaigns-model.md)
- [Fluxos de endpoints](https://github.com/group10-tc-01/fcg-fase05-docs/blob/main/architecture/endpoint-flows.md)
- [Modelo de banco de dados](https://github.com/group10-tc-01/fcg-fase05-docs/blob/main/architecture/database-model.md)

ADRs relevantes:

- [ADR 0002 - Separação de serviços de campanhas e doações](https://github.com/group10-tc-01/fcg-fase05-docs/blob/main/adr/0002-service-boundaries-for-campaigns-and-donations.md)
- [ADR 0005 - Campanhas gerenciam campanhas e transparência](https://github.com/group10-tc-01/fcg-fase05-docs/blob/main/adr/0005-campaigns-own-campaign-management-and-transparency.md)
- [ADR 0007 - Validação de elegibilidade via HTTP](https://github.com/group10-tc-01/fcg-fase05-docs/blob/main/adr/0007-validate-campaign-eligibility-over-http.md)
- [ADR 0010 - Worker atualiza campanhas via API interna](https://github.com/group10-tc-01/fcg-fase05-docs/blob/main/adr/0010-worker-updates-campaigns-through-internal-api.md)
- [ADR 0011 - SQL Server para databases de serviço](https://github.com/group10-tc-01/fcg-fase05-docs/blob/main/adr/0011-use-sql-server-for-service-databases.md)
- [ADR 0012 - Entity Framework Core](https://github.com/group10-tc-01/fcg-fase05-docs/blob/main/adr/0012-use-entity-framework-core.md)

---

## Perfis e Roles

Roles canônicas do MVP:

| Role        | Uso no serviço |
|-------------|----------------|
| `GestorONG` | Pode criar, editar, concluir, cancelar e consultar campanhas administrativas |
| `Doador`    | Não administra campanhas; participa por meio da `fcg-donations` |

> Termos a evitar: `Admin`, `User`, `Manager`, `SuperAdmin`. Os perfis canônicos do domínio são **Doador** e **GestorONG**.

---

## Estrutura do projeto

```text
src/
  Fcg.Campaign.Domain/                  # Campaign, CampaignDonationEntry, regras de domínio
  Fcg.Campaign.Application/             # Casos de uso, CQRS, validação, auditoria
  Fcg.Campaign.Messages/                # Contratos de mensagens quando necessário
  Fcg.Campaign.Infrastructure.Auth/     # Validação de JWT emitido pelo Keycloak
  Fcg.Campaign.Infrastructure.SqlServer/# EF Core + CampaignsDb
  Fcg.Campaign.Infrastructure.Kafka/    # Publicação de eventos de auditoria
  Fcg.Campaign.WebApi/                  # Controllers v1, controllers internos, middlewares, DI, /health
tests/
  Fcg.Campaign.UnitTests/
  Fcg.Campaign.IntegratedTests/
  Fcg.Campaign.FunctionalTests/
  Fcg.Campaign.CommomTestsUtilities/
```

Estrutura interna alinhada ao padrão da fase 04 ([ADR 0023](https://github.com/group10-tc-01/fcg-fase05-docs/blob/main/adr/0023-use-phase-04-dotnet-service-structure.md)).

---

## Endpoints

Base path público versionado: `/api/v1`.

| Método | Rota                                      | Acesso      | Descrição |
|--------|-------------------------------------------|-------------|-----------|
| POST   | `/api/v1/campaigns`                       | `GestorONG` | Cria uma campanha ativa |
| PUT    | `/api/v1/campaigns/{id}`                  | `GestorONG` | Edita uma campanha ativa |
| PATCH  | `/api/v1/campaigns/{id}/complete`         | `GestorONG` | Conclui uma campanha ativa |
| PATCH  | `/api/v1/campaigns/{id}/cancel`           | `GestorONG` | Cancela uma campanha ativa |
| GET    | `/api/v1/campaigns?page=1&pageSize=10`    | `GestorONG` | Lista campanhas para gestão |
| GET    | `/api/v1/transparency/campaigns`          | Público     | Lista campanhas ativas para transparência |
| GET    | `/health`                                 | Operacional | Healthcheck |

Endpoints internos entre serviços:

| Método | Rota                                                   | Acesso interno | Descrição |
|--------|--------------------------------------------------------|----------------|-----------|
| GET    | `/internal/campaigns/{id}/donation-eligibility`        | Cluster/rede   | Informa se a campanha pode receber doação |
| POST   | `/internal/campaigns/{id}/donation-processed`          | Cluster/rede   | Reflete uma doação processada de forma idempotente |

Padrão de resposta `ApiResponse<T>` documentado em [endpoints.md](https://github.com/group10-tc-01/fcg-fase05-docs/blob/main/architecture/endpoints.md).

### Exemplo: criar campanha

```http
POST /api/v1/campaigns
Authorization: Bearer <jwt-gestorong>
Content-Type: application/json

{
  "title": "Campanha de inverno",
  "description": "Arrecadação para compra de cobertores.",
  "startDate": "2026-06-01T00:00:00Z",
  "endDate": "2026-07-01T00:00:00Z",
  "financialGoal": 10000.00
}
```

### Exemplo: consultar transparência

```http
GET /api/v1/transparency/campaigns?page=1&pageSize=10
```

### Exemplo: validar elegibilidade de doação

```http
GET /internal/campaigns/{campaignId}/donation-eligibility
```

### Exemplo: refletir doação processada

```http
POST /internal/campaigns/{campaignId}/donation-processed
Content-Type: application/json

{
  "donationId": "00000000-0000-0000-0000-000000000001",
  "amount": 100.00,
  "processedAt": "2026-06-01T12:00:00Z"
}
```

O endpoint é idempotente por `CampaignId + DonationId`. Uma segunda chamada com a mesma doação não soma o valor novamente.

---

## Regras de negócio

- Apenas `GestorONG` administra campanhas.
- Toda campanha nova nasce com status `Active`.
- Apenas campanhas `Active` podem ser editadas.
- Transições permitidas:
  - `Active -> Completed`
  - `Active -> Canceled`
- Campanhas `Completed` ou `Canceled` não voltam para `Active`.
- `EndDate` não pode estar no passado.
- `FinancialGoal` deve ser maior que zero.
- Apenas campanhas `Active` aparecem no painel público de transparência.
- Apenas campanhas `Active` podem receber doação.
- Doações processadas são refletidas no total arrecadado com idempotência por `DonationId`.

---

## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://docs.docker.com/get-docker/) e Docker Compose
- Keycloak/realm da plataforma disponível via `fcg-identity` ou ambiente integrado
- Portas livres no host: `5433` (SQL Server), `9092` (Kafka), `2181` (Zookeeper), `5341` (Seq), `8080` (API em container), `5000` (API local)

---

## Subindo o ambiente local

O `docker-compose.yml` deste repositório sobe **apenas** as dependências locais deste serviço (SQL Server, Kafka, Zookeeper, Seq) e, opcionalmente, a própria API. Para o ambiente completo integrado da Conexão Solidária utilize o repositório `fcg-solidarity-infra`.

### 1. Subir dependências

```bash
docker compose up -d sqlserver zookeeper kafka seq
```

URLs úteis:

- Seq: http://localhost:5341
- SQL Server: `localhost,5433` (`sa` / `Your_password123`)
- Kafka: `localhost:9092`

### 2. Aplicar migrations

```bash
dotnet ef database update \
  --project src/Fcg.Campaign.Infrastructure.SqlServer \
  --startup-project src/Fcg.Campaign.WebApi
```

### 3. Rodar a API localmente

```bash
dotnet restore
dotnet build
dotnet run --project src/Fcg.Campaign.WebApi
```

Por padrão a API sobe em `http://localhost:5000` (perfil `Development`). Acesse o Swagger em `http://localhost:5000/swagger`.

### 3b. Rodar a API também em container

```bash
docker compose up -d --build api
```

A API ficará exposta em `http://localhost:8080`.

---

## Configuração

Configuração principal em `src/Fcg.Campaign.WebApi/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=localhost,5433;Database=CampaignsDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True;"
  },
  "KafkaSettings": {
    "BootstrapServers": "localhost:9092",
    "TopicName": "audit-log-requested"
  },
  "Keycloak": {
    "Authority": "http://localhost:8081/realms/conexao-solidaria",
    "Audience": "solidarity-api"
  }
}
```

O JWT é emitido pelo Keycloak via `fcg-identity`. Este serviço apenas valida o token e a role `GestorONG`.

---

## Testes

```bash
# Todos os testes
dotnet test

# Por projeto
dotnet test tests/Fcg.Campaign.UnitTests
dotnet test tests/Fcg.Campaign.IntegratedTests
dotnet test tests/Fcg.Campaign.FunctionalTests
```

Cobertura mínima exigida pela esteira: **80%** ([ADR 0025](https://github.com/group10-tc-01/fcg-fase05-docs/blob/main/adr/0025-test-strategy-for-apis-and-worker.md)).

---

## Observabilidade

- Logs estruturados com **Serilog**.
- **OpenTelemetry** para tracing e métricas.
- **Seq** disponível no ambiente local.
- Endpoint operacional:
  - `GET /health`

Endpoints internos e operacionais não devem ser publicados na borda pública da plataforma ([ADR 0027](https://github.com/group10-tc-01/fcg-fase05-docs/blob/main/adr/0027-keep-internal-apis-cluster-private.md)).

---

## CI/CD

A esteira está em `.github/workflows/` reutilizando os workflows reutilizáveis do repositório `fcg-pipelines` ([ADR 0022](https://github.com/group10-tc-01/fcg-fase05-docs/blob/main/adr/0022-reuse-fcg-pipelines-for-ci-cd.md)):

- `branch-name-check.yml` - política de nomes de branch
- `dotnet-service-ci.yml` - build .NET, testes, SonarCloud, Trivy, build da imagem Docker
- `dotnet-service-delivery.yml` - push da imagem para Azure Container Registry e deploy em AKS

Gates principais: secret scan (Gitleaks), dependency scan, restore/build, testes com cobertura mínima de 80%, SonarCloud, Docker build, Trivy, deploy condicional e healthcheck pós-rollout.

---

## Kubernetes

Manifests Kubernetes deste serviço (Deployment, Service, ConfigMap, Secret) ficam em `k8s/` ou diretório equivalente neste repositório. Para o ambiente integrado (Kind local ou AKS), com Keycloak, Kafka, Prometheus e Grafana compartilhados, consulte o repositório `fcg-solidarity-infra` ([ADR 0026](https://github.com/group10-tc-01/fcg-fase05-docs/blob/main/adr/0026-use-separated-kubernetes-namespaces.md)).

Namespace alvo: `fcg-campaigns`.

---

## Banco de dados

- Engine: **SQL Server** ([ADR 0011](https://github.com/group10-tc-01/fcg-fase05-docs/blob/main/adr/0011-use-sql-server-for-service-databases.md))
- ORM: **Entity Framework Core** ([ADR 0012](https://github.com/group10-tc-01/fcg-fase05-docs/blob/main/adr/0012-use-entity-framework-core.md))
- Database: `CampaignsDb`
- Tabelas principais:
  - `Campaigns`
  - `CampaignDonationEntries`

Para aplicar as migrations:

```bash
dotnet ef database update \
  --project src/Fcg.Campaign.Infrastructure.SqlServer \
  --startup-project src/Fcg.Campaign.WebApi
```

---

## Auditoria

Eventos publicados no tópico Kafka `audit-log-requested`:

- `CampaignCreated`
- `CampaignUpdated`
- `CampaignCompleted`
- `CampaignCanceled`
- `DonationReflected`
- `DuplicateDonationIgnored`

Auditoria é explícita nos casos de uso relevantes e não usa outbox neste serviço, conforme modelo consolidado em [database-model.md](https://github.com/group10-tc-01/fcg-fase05-docs/blob/main/architecture/database-model.md).

---

## Como este serviço atende ao hackathon

| Requisito do hackathon | Onde é atendido |
|------------------------|-----------------|
| Gestão de campanhas por `GestorONG` | Endpoints `/api/v1/campaigns` protegidos por role |
| Campanhas com status `Active`, `Completed`, `Canceled` | Entidade `Campaign` e `CampaignStatus` |
| Painel público de transparência | `GET /api/v1/transparency/campaigns` |
| Bloquear doação para campanha concluída/cancelada | `GET /internal/campaigns/{id}/donation-eligibility` |
| Atualizar valor arrecadado após processamento assíncrono | `POST /internal/campaigns/{id}/donation-processed` |
| Idempotência no processamento da doação | Tabela `CampaignDonationEntries` com índice único `CampaignId + DonationId` |
| Autenticação JWT e RBAC | Validação de token Keycloak e role `GestorONG` |
| Microsserviço distinto | `fcg-campaign` separado de `fcg-identity`, `fcg-donations` e `fcg-donation-worker` |
| Banco próprio por serviço | `CampaignsDb` |
| Docker e pipeline | `Dockerfile`, `docker-compose.yml` e workflows em `.github/workflows` |

Os fluxos de cadastro/login pertencem à `fcg-identity`; as intenções de doação pertencem à `fcg-donations`; o consumo do evento de doação pertence ao `fcg-donation-worker`.
