# 🧪 Guia de Testes e Garantia de Qualidade - Nexus Commerce

Este documento descreve a estratégia de testes adotada no projeto **Nexus Commerce**. Devido à complexidade de uma arquitetura de microsserviços, adotamos uma abordagem em camadas para garantir tanto a lógica de negócio interna quanto a comunicação entre os serviços.

---

## 1. Visão Geral da Estratégia

Utilizamos uma adaptação da **Pirâmide de Testes**, focando em duas camadas principais:

1.  **Testes Unitários (Automatizados):** Validam a lógica de domínio (Regras de Negócio) isoladamente, sem dependências externas (Banco de Dados ou Rede).
2.  **Testes de Integração (Fluxo/E2E):** Validam a orquestração completa, segurança (JWT), roteamento (Gateway) e mensageria (RabbitMQ).

---

## 2. Camada 1: Testes Unitários (xUnit)

Esta camada garante que as regras de negócio cruciais não sejam quebradas por refatorações futuras.

* **Ferramenta:** xUnit
* **Localização:** `tests/Unit/`

### O que testamos?

| Contexto | Classe/Entidade | Cenário de Teste |
| :--- | :--- | :--- |
| **Estoque** | `ProductVariant` | Garante que o estoque não fique negativo ao tentar baixar uma quantidade maior que a disponível. |
| **Estoque** | `ProductVariant` | Garante que não é possível adicionar/remover quantidades negativas ou zeradas. |
| **Vendas** | `Order` | Valida se o cálculo do `TotalAmount` do pedido é a soma correta de todos os itens (`UnitPrice * Quantity`). |
| **Vendas** | `OrderItem` | Garante que não se cria um item de pedido com preço negativo ou quantidade zero. |

### 🚀 Como Executar

Abra o terminal na raiz do projeto (`NexusCommerce`) e execute:

```powershell
dotnet test

```

**Saída Esperada:**
O comando irá compilar a solução, descobrir os testes nos projetos `Nexus.Stock.UnitTests` e `Nexus.Sales.UnitTests` e exibir o resultado (Passed/Failed).

---

## 3. Camada 2: Testes de Integração (Toolkit)

Como microsserviços dependem de rede e infraestrutura, criamos um **Toolkit de Testes** para simular o uso real do sistema.

* **Localização:** `tests/Integration/`
* **Ferramentas:** PowerShell (Orquestração) + VS Code REST Client (Requisições).

### 📋 Passo a Passo para Teste de Fluxo Completo

#### Passo 1: Inicializar o Ambiente

Antes de testar, o ecossistema precisa estar rodando. Utilize o script automatizado que abre os 4 serviços (Identity, Stock, Sales, Gateway) em terminais separados.

1. Abra o terminal em `tests/Integration`.
2. Execute:
```powershell
./start-all.ps1

```



#### Passo 2: Executar os Cenários (.http)

Utilize a extensão **REST Client** do VS Code para executar os arquivos abaixo clicando em `Send Request`.

**Arquivo: `test-flow.http` (O Caminho Feliz)**
Este script conta uma história completa:

1. **Login Admin:** Autentica no *Identity* e recupera o Token JWT.
2. **Cadastro de Produto:** Cria uma "Sapatilha Coppélia" no *Stock* com grade (Tam 36/Rosa, Qtd 50).
3. **Venda:** Envia um pedido para o *Sales* comprando 5 unidades.
4. **Validação:** Consulta o *Stock* novamente para confirmar se o RabbitMQ processou a mensagem e o estoque caiu para 45.

**Arquivo: `test-secure.http` (Segurança)**
Tenta burlar o sistema para garantir que as portas estão fechadas:

1. Tenta cadastrar produtos sem Token (Deve retornar `401 Unauthorized`).
2. Tenta criar pedidos com Token inválido.

**Arquivo: `test-gateway.http` (Roteamento)**
Verifica a saúde do Ocelot:

1. Testa se o Swagger Agregado (`/swagger/index.html`) está acessível.
2. Testa se os JSONs de documentação individuais estão sendo roteados.

---

## 4. Integração Contínua (CI - GitHub Actions)

Para garantir que nenhum código quebrado entre na branch `main`, configuramos um pipeline automatizado.

* **Arquivo:** `.github/workflows/ci.yml`
* **Gatilho:** Qualquer `push` para a branch `main`.

### O que o Robô faz?

1. **Setup:** Prepara uma máquina Linux (Ubuntu) com .NET 9.
2. **Restore:** Baixa todas as dependências NuGet.
3. **Build:** Compila a solução `NexusCommerce.sln`.
4. **Test:** Executa o comando `dotnet test`.

Se qualquer teste unitário falhar ou se o código não compilar, o GitHub marca o commit com um ❌ vermelho, impedindo a entrega de código defeituoso.

---

## 5. Solução de Problemas Comuns

**Erro: "Connection refused" ao rodar os testes de integração**

* *Causa:* Os microsserviços não estão rodando.
* *Solução:* Execute o `./start-all.ps1` e verifique se as janelas abriram.

**Erro: Estoque não atualiza após a venda**

* *Causa:* O RabbitMQ pode estar desligado ou o consumidor (`StockUpdateConsumer`) travou.
* *Solução:* Verifique o log do terminal do serviço **Nexus.Stock**. Ele deve mostrar: `[RabbitMQ] Iniciando processamento do Pedido...`.

**Erro: Testes Unitários falhando**

* *Causa:* Alguma regra de negócio foi alterada no código fonte mas o teste não foi atualizado.
* *Solução:* Leia a mensagem de erro no terminal (ex: `Expected: 10, Actual: 9`) e ajuste o código ou o teste.
