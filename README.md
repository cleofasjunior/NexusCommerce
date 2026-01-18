# 🩰 Nexus Commerce

> **Uma plataforma de e-commerce distribuída, escalável e resiliente, especializada no nicho de Balé, orquestrada via Arquitetura de Microsserviços.**

[![NexusCommerce CI](https://github.com/cleofasjunior/NexusCommerce/actions/workflows/ci.yml/badge.svg)](https://github.com/cleofasjunior/NexusCommerce/actions/workflows/ci.yml)
![.NET](https://img.shields.io/badge/.NET-9.0-purple)
![Architecture](https://img.shields.io/badge/Architecture-Microservices-blue)
![License](https://img.shields.io/badge/License-MIT-green)

**Nexus Commerce** é um ecossistema de software robusto que simula as operações reais de uma loja especializada em artigos de Balé. O projeto foi concebido para demonstrar a implementação prática de padrões avançados de engenharia de software, como Consistência Eventual, CQRS simplificado e desacoplamento via mensageria, garantindo alta disponibilidade e integridade de dados.

---

## 🏗️ Arquitetura e Design

O projeto segue uma **Arquitetura de Microsserviços** orientada a eventos, utilizando **Clean Architecture** internamente em cada serviço para separar regras de negócio de detalhes de infraestrutura.

### 🧩 Componentes do Sistema

| Serviço | Porta | Tecnologia | Responsabilidade |
| :--- | :--- | :--- | :--- |
| **Nexus.Gateway** | `5000` | **Ocelot** | Ponto único de entrada, Roteamento, Agregação de Swagger. |
| **Nexus.Identity** | `5001` | **ASP.NET Identity** | Autenticação centralizada, Emissão de Tokens JWT. |
| **Nexus.Stock** | `6001` | **EF Core + SQL** | Catálogo de Produtos, Gestão de Variantes (Cor/Tam), Baixa Assíncrona. |
| **Nexus.Sales** | `5002` | **MassTransit** | Gestão de Pedidos, Orquestração de Eventos de Venda. |



### 🔄 Fluxo de Mensageria (Eventual Consistency)
O sistema utiliza **RabbitMQ** para comunicação assíncrona, assegurando que o processo de vendas não seja bloqueado por operações de banco de dados pesadas.

1.  O cliente finaliza um pedido em **Sales**.
2.  O pedido é salvo como "Pending" e um evento `OrderCreatedEvent` é publicado na fila.
3.  O serviço **Stock** consome este evento e abate o estoque da variante específica (ex: Sapatilha 36/Rosa).
4.  O sistema garante a integridade do estoque sem acoplamento temporal entre os serviços.

---

## 🛠️ Tecnologias Utilizadas

* **Linguagem:** C# (.NET 9)
* **Banco de Dados:** SQL Server (Entity Framework Core com Migrations Automáticas)
* **Mensageria:** RabbitMQ (via MassTransit)
* **API Gateway:** Ocelot
* **Documentação:** Swagger UI (Agregado no Gateway)
* **Testes:** xUnit (Unitários) & VS Code REST Client (Integração)
* **CI/CD:** GitHub Actions

---

## 🚀 Como Executar o Projeto

### Pré-requisitos
* [.NET SDK 9.0](https://dotnet.microsoft.com/download)
* [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Rodando localmente)
* [RabbitMQ](https://www.rabbitmq.com/) (Rodando localmente ou Docker)

### ⚡ Inicialização Rápida (Script)
Para facilitar a execução de múltiplos microsserviços sem a necessidade de Docker Compose (focando em economia de memória), utilize o script orquestrador incluído:

1.  Abra o PowerShell na pasta `tests/Integration`.
2.  Execute o comando:
    ```powershell
    ./start-all.ps1
    ```
3.  O script abrirá 4 terminais independentes, inicializando toda a arquitetura.

### 🔗 Acessando a Aplicação
Após iniciar, acesse o **Portal do Desenvolvedor** (API Gateway):
👉 **http://localhost:5000/swagger/index.html**

---

## 🧪 Testes e Qualidade

O projeto possui duas camadas de testes para garantir a integridade do sistema.

### 1. Testes Unitários (Automatizados)
Focam nas regras de negócio (Domínio), como validação de estoque negativo e cálculo de total do pedido.
```powershell
dotnet test

```

### 2. Testes de Integração (Manuais/Scriptados)

Localizados em `tests/Integration`, utilize a extensão **REST Client** do VS Code para executar fluxos reais.

* `test-flow.http`: Realiza login, cria produto com variante, executa venda e valida baixa de estoque.
* `test-secure.http`: Valida se o Gateway bloqueia requisições sem Token JWT.

---

## 🔐 Credenciais Padrão

Para realizar operações de escrita (POST/PUT), utilize o endpoint de Login no serviço **Identity** para obter um Bearer Token.

* **Email:** `admin@nexus.com`
* **Senha:** `SenhaForte@123`

---

## 📂 Estrutura de Pastas

```text
NexusCommerce/
├── src/
│   ├── BuildingBlocks/  # Códigos compartilhados (Eventos, DTOs)
│   └── Services/        # Os 4 microsserviços (Identity, Sales, Stock, Gateway)
├── tests/
│   ├── Unit/            # Projetos xUnit (Sales e Stock)
│   └── Integration/     # Scripts .http e .ps1
├── docs/                # Documentação detalhada (Guias e Aprendizados)
└── .github/             # Pipelines de CI/CD

```


## 👨‍💻 Autor

Desenvolvido por **Cleófas Júnior** - Doutor em Educação.
