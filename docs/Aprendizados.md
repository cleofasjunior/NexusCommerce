# 💡 Jornada de Aprendizagem e Decisões Técnicas - Nexus Commerce

> **Autor:** Cleófas Júnior - Doutor em Educação

> **Contexto:** Transição de Carreira e Especialização em Arquitetura de Software (.NET)

---

## 1. Introdução: A Mudança de Paradigma

O desenvolvimento do **Nexus Commerce** não foi apenas um exercício de codificação, mas uma jornada de mudança mental do paradigma monolítico para a **Arquitetura de Microsserviços**.

O principal desafio inicial foi compreender que, num sistema distribuído, a consistência imediata é um luxo caro. Aceitar e implementar a **Consistência Eventual** foi o ponto de viragem deste projecto. Percebi que não precisamos de "travar" a venda enquanto o stock é atualizado; podemos garantir que isso aconteça em segundo plano, proporcionando uma experiência muito mais rápida ao utilizador.

---

## 2. Desafios Técnicos e Soluções

Durante a construção, deparei-me com problemas clássicos de sistemas distribuídos e apliquei soluções de mercado ("Enterprise Patterns").

### 🐇 A Comunicação Assíncrona (RabbitMQ & MassTransit)
No início, a tentação de fazer chamadas HTTP diretas (Síncronas) entre Vendas e Stock era grande. Porém, isso criaria um acoplamento forte: se o Stock caísse, as Vendas paravam.
* **Aprendizado:** A implementação do **RabbitMQ** através da biblioteca **MassTransit** foi fundamental. Aprendi a configurar *Producers* (Produtores) e *Consumers* (Consumidores), garantindo que a mensagem de venda seja entregue mesmo que o serviço de destino esteja temporariamente indisponível.

### 🌐 O Guardião da Entrada (Ocelot API Gateway)
Expor várias portas (5001, 5002, 6001) ao cliente front-end seria um erro de segurança e usabilidade.
* **Solução:** A configuração do **Ocelot** ensinou-me sobre encaminhamento de pedidos (routing) e como centralizar a porta de entrada.
* **Destaque:** O maior desafio foi configurar o **Swagger Aggregation**. Consegui fazer com que o Gateway não só encaminhasse as chamadas de API, mas também unificasse a documentação visual num único portal, facilitando imenso a vida de quem consome a API.

### 🔐 Segurança Distribuída (JWT)
Gerir autenticação em múltiplos serviços é complexo.
* **Aprendizado:** Compreendi a importância de um serviço dedicado de Identidade (`Nexus.Identity`). Aprendi a gerar Tokens JWT assinados com chaves simétricas e a configurar os outros serviços apenas para *validar* essa assinatura, sem precisarem de aceder à base de dados de utilizadores.

---

## 3. Engenharia de Software e Qualidade

### 🏗️ Clean Architecture e DDD
Não queria apenas criar controladores e modelos misturados.
* **Aplicação:** Forcei a separação em camadas (`API`, `Application`, `Domain`, `Infra`).
* **Resultado:** Isso ficou evidente nos **Testes Unitários**. Como a lógica de "diminuir stock" estava isolada na Entidade de Domínio (`ProductVariant`), foi trivial testá-la sem depender da base de dados ou do Entity Framework.

### 🧩 O Problema da Duplicação de Código
Percebi que tanto o serviço de Vendas quanto o de Stock precisavam de conhecer o evento `OrderCreatedEvent`.
* **Solução:** A criação do projecto **`Nexus.Shared`** (Building Blocks) foi essencial. Aprendi que partilhar contratos (DTOs e Interfaces) através de uma biblioteca comum evita erros de tipagem e duplicidade de código.

### 🧪 Testes Automatizados (A Rede de Segurança)
A implementação de testes com **xUnit** trouxe uma confiança extra.
* **Reflexão:** Ver o teste falhar quando tentei vender um produto com stock zero, e depois passar após a correção, reforçou a importância do TDD (Test Driven Development) para garantir regras de negócio robustas.

---

## 4. DevOps e Cultura de Entrega

Um código que só funciona na "minha máquina" não tem valor.
* **GitHub Actions:** Configurei um pipeline de CI (Integração Contínua) que compila e testa o projecto a cada *push*. Isso garante que a "main" esteja sempre saudável.
* **Scripting:** A criação do `start-all.ps1` e dos ficheiros `.http` na pasta `tests/Integration` foi uma lição de *Developer Experience (DX)*. Facilitar a vida de quem vai testar o projecto é tão importante quanto o código em si.

---

## 5. Conclusão

O projeto **Nexus Commerce** foi uma síntese prática de conceitos avançados. Saio deste projeto com o domínio sobre:
1.  **Orquestração de Microsserviços** (Gateway, Auth, Business Services).
2.  **Mensageria e Desacoplamento** (RabbitMQ).
3.  **Qualidade de Código** (Testes, Clean Arch).
4.  **Automação** (CI/CD).

Como educador e agora engenheiro de software, vejo que a tecnologia, tal como a educação, exige uma base estrutural sólida para sustentar o crescimento. Este projeto é essa base.

---
**Cleófas Júnior**
*Campina Grande, Paraíba - 2026*