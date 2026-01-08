# Agenda Manager - Sistema de Gerenciamento de Eventos

Uma aplicação completa de gerenciamento de agenda desenvolvida seguindo os melhores padrões de arquitetura de software, incluindo **SOLID**, **Clean Architecture** e **CQRS**.

## 🏗️ Arquitetura

O projeto segue uma arquitetura limpa e organizada, separada em camadas bem definidas:

### Backend (.NET 8)

```
src/
├── Api/                    # Camada de Apresentação (API REST)
│   ├── Controllers/v1/     # Controllers versionados
│   ├── Program.cs         # Configuração da aplicação
│   └── appsettings.json   # Configurações
│
├── Application/           # Camada de Aplicação (Use Cases)
│   ├── Commands/          # Comandos CQRS (escrita)
│   ├── Queries/           # Queries CQRS (leitura)
│   ├── DTOs/              # Objetos de Transferência de Dados
│   └── Interfaces/        # Contratos da camada
│
├── Domain/               # Camada de Domínio (Regras de Negócio)
│   ├── Entities/         # Entidades de domínio
│   ├── Enums/            # Enumeradores
│   └── Interfaces/       # Interfaces do domínio
│
└── Infrastructure/       # Camada de Infraestrutura
    ├── Data/             # Contexto do banco e Unit of Work
    ├── Repositories/     # Implementações de repositórios
    └── Mappings/         # Mapeamentos AutoMapper
```

### Frontend (Angular)

```
src/app/
├── core/                 # Serviços compartilhados
│   ├── auth/            # Autenticação e autorização
│   └── services/        # Serviços da aplicação
│
├── modules/             # Módulos funcionais
│   ├── auth/           # Módulo de autenticação
│   └── dashboard/      # Módulo do dashboard
│
├── shared/             # Componentes compartilhados
└── environments/       # Configurações de ambiente
```

## 🛠️ Tecnologias Utilizadas

### Backend
- **.NET 8** - Framework principal
- **ASP.NET Core Web API** - API REST
- **Entity Framework Core** - ORM
- **PostgreSQL** - Banco de dados
- **AutoMapper** - Mapeamento objeto-objeto
- **MediatR** - Implementação CQRS
- **JWT** - Autenticação baseada em tokens
- **Npgsql** - Provider PostgreSQL

### Frontend
- **Angular 17+** - Framework SPA
- **TypeScript** - Tipagem estática
- **RxJS** - Programação reativa
- **Angular Reactive Forms** - Formulários reativos

## 📋 Funcionalidades

### ✅ Implementadas
- ✅ **Autenticação JWT** - Login e proteção de rotas
- ✅ **CRUD de Eventos** - Criar, ler, atualizar e deletar eventos
- ✅ **Tipos de Evento** - Exclusivo (privado) e Compartilhado
- ✅ **Participantes** - Gerenciamento de participantes em eventos compartilhados
- ✅ **Dashboard com Filtros** - Filtragem por data, texto e períodos
- ✅ **API REST Versionada** - Endpoints versionados (v1)
- ✅ **CQRS** - Separação clara entre comandos e queries
- ✅ **Clean Architecture** - Separação em camadas bem definidas
- ✅ **SOLID Principles** - Princípios aplicados em todas as camadas

### 🚧 Pendências (para implementação futura)
- 🔄 **Validação de Senhas** - Implementar hash bcrypt/argon2
- 🔄 **Migrations** - Configurar Entity Framework migrations
- 🔄 **Testes Unitários** - Cobertura de testes
- 🔄 **Modais de CRUD** - Interface completa para criação/edição
- 🔄 **Notificações** - Sistema de notificações/toasts
- 🔄 **Paginação** - Paginação de resultados

## 🚀 Como Executar

### Pré-requisitos
- .NET 10 SDK (LTS - Long Term Support)
- Node.js 18+
- PostgreSQL 13+ (ou Docker)
- Angular CLI

### Configuração Inicial

1. **Configurar variáveis de ambiente (opcional):**
```bash
# Copiar arquivo de exemplo
cp env.example .env

# Editar configurações conforme necessário
# O projeto já vem com configurações padrão funcionais
```

2. **Executar script de configuração (Windows):**
```bash
# Script PowerShell (recomendado)
./start-dev.ps1

# Ou script Batch
start-dev.bat
```

Este script irá:
- ✅ Verificar se todas as dependências estão instaladas
- ✅ Instalar pacotes .NET (dotnet restore)
- ✅ Instalar pacotes Node.js (npm install)

### Backend (.NET 8)

1. **Configurar banco de dados:**

   **Opção A - PostgreSQL local:**
   ```bash
   # Criar banco PostgreSQL
   createdb AgendaManagerDev
   ```

   **Opção B - Docker (Recomendado):**
   ```bash
   # Iniciar PostgreSQL com Docker
   docker-compose up -d postgres

   # (Opcional) Iniciar PgAdmin para gerenciar o banco
   docker-compose up -d pgadmin
   # PgAdmin disponível em: http://localhost:8080
   # Email: admin@agendmanager.com | Senha: admin
   ```

   **Connection string:** Alterar em `src/Api/appsettings.Development.json` se necessário.

2. **Executar a API:**
```bash
# Instalar dependências
dotnet restore

# Executar aplicação
dotnet run --project AgendaManager.Api.csproj
```

A API estará disponível em `https://localhost:5001`

### Frontend (Angular)

1. **Instalar dependências:**
```bash
npm install
```

2. **Executar aplicação:**
```bash
npm start
# ou
ng serve
```

A aplicação estará disponível em `http://localhost:4200`

## 📚 Padrões e Princípios Aplicados

### SOLID Principles
- **S** - Single Responsibility: Cada classe tem uma única responsabilidade
- **O** - Open/Closed: Classes abertas para extensão, fechadas para modificação
- **L** - Liskov Substitution: Subtipos podem substituir seus tipos base
- **I** - Interface Segregation: Interfaces específicas ao invés de genéricas
- **D** - Dependency Inversion: Dependências de abstrações, não concretizações

### CQRS (Command Query Responsibility Segregation)
- **Commands**: Operações de escrita (Create, Update, Delete)
- **Queries**: Operações de leitura (Get)
- **Separation of Concerns**: Responsabilidades claramente separadas

### Domain-Driven Design (DDD)
- **Entities**: Objetos com identidade e ciclo de vida
- **Value Objects**: Objetos imutáveis que representam conceitos
- **Aggregates**: Grupos de entidades tratadas como uma unidade
- **Repositories**: Abstração de acesso a dados
- **Domain Services**: Lógica de negócio que não pertence a uma entidade

### Clean Architecture
- **Independência de Frameworks**: O núcleo não depende de frameworks externos
- **Testabilidade**: Código facilmente testável
- **Independência de UI**: Interfaces podem mudar sem afetar regras de negócio
- **Independência de Banco**: Regras de negócio não dependem do banco de dados

## 🔧 Estrutura dos Projetos

### Domain Layer
```csharp
// Entidades ricas com regras de negócio
public class Event
{
    public static Event Create(/*...*/) { /* Validações */ }
    public void Update(/*...*/) { /* Regras de negócio */ }
    public bool CanUserEdit(int userId) { /* Autorização */ }
}
```

### Application Layer
```csharp
// Handlers CQRS
public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, EventDto>
{
    public async Task<EventDto> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        // Orquestração da lógica de negócio
    }
}
```

### Infrastructure Layer
```csharp
// Repositórios implementando interfaces do domínio
public class EventRepository : Repository<Event>, IEventRepository
{
    public async Task<IEnumerable<Event>> GetFilteredEventsAsync(/*...*/)
    {
        // Queries complexas do EF Core
    }
}
```

### API Layer
```csharp
// Controllers enxutos, apenas orquestração HTTP
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class EventsController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpPost]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto dto)
    {
        var command = new CreateEventCommand(dto, GetCurrentUserId());
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(CreateEvent), result);
    }
}
```

## 🔒 Segurança

- **JWT Authentication**: Tokens seguros com expiração
- **Authorization**: Controle de acesso baseado em claims
- **Input Validation**: Validação de entrada em múltiplas camadas
- **SQL Injection Protection**: Uso de EF Core com parâmetros
- **CORS**: Configuração adequada para origens permitidas

## 📊 API Endpoints

### Autenticação
```
POST /api/v1/auth/login
```

### Eventos
```
GET  /api/v1/events/dashboard  # Lista eventos com filtros
POST /api/v1/events            # Cria novo evento
PUT  /api/v1/events/{id}       # Atualiza evento
DELETE /api/v1/events/{id}     # Remove evento (soft delete)
```

## 🎯 Decisões Técnicas

### Por que CQRS?
- **Performance**: Queries otimizadas separadamente dos comandos
- **Escalabilidade**: Possibilidade de escalar leitura e escrita independentemente
- **Manutenibilidade**: Código mais organizado e fácil de entender

### Por que PostgreSQL?
- **JSON Support**: Armazenamento de dados complexos
- **Performance**: Excelente para queries complexas
- **Confiabilidade**: ACID compliance e transações robustas

### Por que JWT?
- **Stateless**: Não requer armazenamento de sessão no servidor
- **Escalável**: Fácil distribuição em múltiplos servidores
- **Flexível**: Claims customizáveis para autorização granular

### Por que Angular?
- **Type Safety**: TypeScript previne erros em tempo de desenvolvimento
- **Modular**: Lazy loading e tree-shaking para otimização
- **Reactive**: RxJS para programação reativa e assíncrona

## 🔄 Próximos Passos

1. **Completar Implementação**
   - Modais de criação/edição de eventos
   - Sistema de notificações
   - Validação de senhas com hash
   - Migrations do EF Core

2. **Testes**
   - Testes unitários (xUnit)
   - Testes de integração
   - Testes E2E (Cypress)

3. **DevOps**
   - CI/CD com GitHub Actions
   - Docker containers
   - Deploy no Azure/Kubernetes

4. **Monitoramento**
   - Application Insights
   - Health checks
   - Logging estruturado

## 🤝 Contribuição

Este projeto foi desenvolvido como teste prático seguindo rigorosamente as melhores práticas da indústria. Para dúvidas ou sugestões, entre em contato.

---

**Desenvolvido por:** Candidato a Desenvolvedor Fullstack Sênior
**Tecnologias:** .NET 8 + Angular + PostgreSQL
**Padrões:** SOLID, Clean Architecture, CQRS, DDD
"# IATEC-teste" 
