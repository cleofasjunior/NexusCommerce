# 🏛️ Guia de Estrutura e Arquitetura - Nexus Commerce

Este documento detalha as decisões de design, a organização de pastas e os padrões arquiteturais adotados no projeto **Nexus Commerce**. O objetivo é oferecer uma visão clara de como os microsserviços são construídos e como se comunicam.

---

## 1. Visão Geral da Arquitetura

O sistema utiliza uma **Arquitetura de Microsserviços** baseada em eventos (Event-Driven Architecture), onde cada serviço possui responsabilidade única e seu próprio banco de dados (Database-per-Service), garantindo desacoplamento e escalabilidade.

### Diagrama Conceitual
```text
[Cliente/Front-end]
       ⬇
[ API Gateway (Ocelot) :5000 ]
       ⬇
   ---------------------------------------------------
   ⬇                    ⬇                          ⬇
[Identity API]      [Sales API]  ➡ (Event) ➡  [Stock API]
(Autenticação)      (Pedidos)     RabbitMQ     (Catálogo)
       ⬇                ⬇                          ⬇
   [SQL Auth]       [SQL Sales]                [SQL Stock]

```

---

## 2. Organização do Código (Clean Architecture Simplificada)

Internamente, cada microsserviço segue uma estrutura baseada nos princípios da **Clean Architecture** e **Domain-Driven Design (DDD)**, adaptada para a agilidade necessária em microsserviços.

### Camadas do Projeto

Para cada serviço (ex: `Nexus.Sales`), a estrutura interna é dividida em:

1. **API (Apresentação):**
* Contém os `Controllers` e configurações de entrada (`Program.cs`).
* Responsável apenas por receber requisições HTTP e devolver respostas.
* Configuração do Swagger e Injeção de Dependência.


2. **Application (Aplicação):**
* Contém os `Services`, `Consumers` (MassTransit) e `DTOs`.
* Orquestra o fluxo de dados entre a API e o Domínio.
* Exemplo: `StockUpdateConsumer.cs` (Recebe a mensagem e chama a entidade).


3. **Domain (Domínio - O Coração):**
* Contém as **Entidades** (`Order`, `ProductVariant`) e Enums.
* Aqui residem as **Regras de Negócio**.
* *Exemplo:* A lógica `DecreaseQuantity` que impede estoque negativo fica na Entidade, não no Controller.


4. **Infra (Infraestrutura):**
* Contém o `DbContext` (Entity Framework Core) e mapeamentos.
* Responsável pelo acesso a dados e conexões externas.



---

## 3. O Núcleo Compartilhado (`BuildingBlocks`)

Para evitar duplicação de código, criamos o projeto **`Nexus.Shared`**.

* **Localização:** `src/BuildingBlocks/Nexus.Shared`
* **Responsabilidade:** Contém contratos de mensagens (Eventos) e configurações comuns.
* **Componentes Chave:**
* `Messages/OrderCreatedEvent.cs`: O contrato que garante que *Sales* e *Stock* falem a mesma língua no RabbitMQ.
* `OpenApiInfo`: Configurações padronizadas de documentação.



---

## 4. Detalhamento dos Serviços

### 🔐 Nexus.Identity (Porta 5001)

* **Foco:** Segurança.
* **Tecnologia:** ASP.NET Core Identity.
* **Endpoints:** `/register`, `/login`.
* **Saída:** Gera Tokens JWT (Bearer) assinados com chave simétrica.

### 📦 Nexus.Stock (Porta 6001)

* **Foco:** Gestão de Produtos e Controle de Estoque.
* **Diferencial de Negócio:** Suporte a **Variantes** (Grade de Tamanho e Cor), essencial para o nicho de vestuário/balé.
* **Comunicação:**
* *Consumidor:* Escuta filas do RabbitMQ para dar baixa automática em itens vendidos.



### 🛒 Nexus.Sales (Porta 5002)

* **Foco:** Processamento de Vendas.
* **Fluxo:**
1. Recebe o pedido.
2. Verifica validade básica (Preço, Cliente).
3. Salva no banco como "Pendente".
4. Publica evento `OrderCreatedEvent`.


* **Integração Síncrona:** Possui um `StockIntegrationService` (HttpClient) para consultas rápidas de existência de produto antes da venda.

### 🌐 Nexus.Gateway (Porta 5000)

* **Foco:** Roteamento Inteligente.
* **Tecnologia:** Ocelot.
* **Funcionalidade Extra:** **Swagger Aggregation**. O Gateway lê os JSONs de documentação dos microsserviços e monta uma interface única para o desenvolvedor.

---

## 5. Estrutura de Diretórios e Arquivos

Abaixo, a árvore de diretórios oficial do projeto:

```text
NexusCommerce/
│
├── src/
│   ├── BuildingBlocks/
│   │   └── Nexus.Shared/         # Biblioteca de Eventos e DTOs
│   │
│   └── Services/
│       ├── Nexus.Gateway/        # Ocelot + Swagger UI Central
│       ├── Nexus.Identity/       # Auth Service
│       ├── Nexus.Sales/          # Sales Service + Producer
│       └── Nexus.Stock/          # Stock Service + Consumer
│
├── tests/
│   ├── Unit/                     # Testes xUnit (Regras de Domínio)
│   │   ├── Nexus.Sales.UnitTests/
│   │   └── Nexus.Stock.UnitTests/
│   │
│   └── Integration/              # Scripts de Teste (.http e .ps1)
│
├── docs/                         # Documentação do Projeto
├── .github/workflows/            # Pipelines de CI/CD (GitHub Actions)
├── .gitignore                    # Regras de exclusão do Git
├── NexusCommerce.sln             # Arquivo de Solução Unificada
└── README.md                     # Ponto de partida

```

---

## 6. Padrões de Comunicação

### Síncrona (Request/Response)

Utilizada apenas quando a resposta imediata é crítica ou para leitura de dados pelo Gateway.

* *Cliente -> Gateway -> API*

### Assíncrona (Fire-and-Forget / Event-Driven)

Utilizada para operações transacionais que envolvem múltiplos serviços (Consistência Eventual).

* *Sales -> (Publica Evento) -> RabbitMQ -> (Consome Evento) -> Stock*
* **Ferramenta:** MassTransit (Abstração robusta sobre o RabbitMQ).

---

## 7. Decisões de Banco de Dados

* **Database-per-Service:** Cada serviço tem sua própria Connection String e Schema. Isso impede que uma mudança no banco de Vendas quebre o serviço de Estoque.
* **Migrations Automáticas:** Configuramos o `Program.cs` para aplicar migrações pendentes (`db.Database.Migrate()`) na inicialização, facilitando o deploy em novos ambientes sem intervenção manual no SQL.

---

**Autor:** Cleófas Júnior - Doutor em Educação.
