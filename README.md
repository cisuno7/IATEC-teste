# Agenda Manager

Sistema de gerenciamento de agendas e eventos desenvolvido com **Clean Architecture**, **DDD**, **SOLID** e **CQRS**.

## 📋 Versões Instaladas

- **.NET SDK**: 10.0
- **Angular**: 17.0.0
- **Node.js**: 18+ (recomendado)
- **PostgreSQL**: Gerenciado via Supabase

## 🏗️ Arquitetura do Projeto

Este projeto segue rigorosamente os princípios de:

- **Clean Architecture** - Separação de responsabilidades em camadas
- **Domain-Driven Design (DDD)** - Modelagem orientada ao domínio
- **SOLID** - Princípios de design orientado a objetos
- **Clean Code** - Código limpo e legível
- **CQRS** - Separação de comandos e consultas

### Princípios Arquiteturais

- ✅ **ZERO comentários no código** - Nomes autoexplicativos
- ✅ **Métodos pequenos** - Responsabilidade única
- ✅ **Nenhuma lógica em Controllers** - Apenas orquestração
- ✅ **Nenhuma dependência do domínio com infraestrutura**
- ✅ **Controllers apenas orquestram comandos e queries**
- ✅ **DTOs apenas para transporte**
- ✅ **Repositórios apenas no Infrastructure**
- ✅ **Application não conhece EF Core**
- ✅ **Domain não conhece nada externo**

### Regras de Código

1. Não escreva comentários
2. Não use regiões
3. Não use métodos longos
4. Não use ifs complexos
5. Nomes claros substituem comentários
6. Use Value Objects quando fizer sentido
7. Use Enums apenas no Domain
8. Use Imutabilidade sempre que possível
9. Não use static para lógica de negócio
10. Controllers não validam regra de negócio

### Camadas da Arquitetura

```
┌─────────────────────────────────────┐
│           Api Layer                 │
│  (Controllers, Authentication)      │
└─────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────┐
│        Application Layer            │
│  (Commands, Queries, Handlers)     │
└─────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────┐
│          Domain Layer               │
│  (Entities, Value Objects, Rules)  │
└─────────────────────────────────────┘
                  ↑
┌─────────────────────────────────────┐
│      Infrastructure Layer           │
│  (EF Core, Repositories, Services) │
└─────────────────────────────────────┘
```

#### **Domain Layer** (Camada de Domínio)
- Entidades (`User`, `Event`)
- Value Objects (`Email`, `EventName`, `EventDescription`, `Location`)
- Enums (`EventType`)
- Interfaces de repositório
- Regras de negócio puras
- **Não conhece nada externo**

#### **Application Layer** (Camada de Aplicação)
- Commands (CreateEvent, UpdateEvent, DeleteEvent, etc.)
- Queries (GetDashboardEvents, GetUserById, etc.)
- Handlers (Command Handlers, Query Handlers)
- DTOs (Data Transfer Objects)
- Interfaces de serviços
- **Não conhece EF Core**

#### **Infrastructure Layer** (Camada de Infraestrutura)
- Entity Framework Core
- Repositórios concretos
- DbContext e Migrations
- Mapeamentos (AutoMapper Profiles)
- Serviços de infraestrutura (JWT, DateTime, etc.)
- **Implementa interfaces do Domain e Application**

#### **Api Layer** (Camada de Apresentação)
- Controllers versionados (`/api/v1`)
- Autenticação JWT
- Swagger/OpenAPI
- **Apenas orquestra comandos e queries via MediatR**

## 📁 Estrutura do Projeto

```
teste-IATEC/
├── src/
│   ├── Api/                           # Camada de Apresentação
│   │   ├── Controllers/
│   │   │   └── v1/                    # API versionada
│   │   │       ├── AuthController.cs
│   │   │       ├── EventsController.cs
│   │   │       └── UsersController.cs
│   │   ├── Program.cs                 # Configuração da API
│   │   └── appsettings.json          # Configurações
│   │
│   ├── Application/                   # Camada de Aplicação
│   │   ├── Commands/                  # Comandos (Write)
│   │   │   ├── Auth/
│   │   │   │   ├── LoginCommand.cs
│   │   │   │   └── RegisterCommand.cs
│   │   │   └── Events/
│   │   │       ├── CreateEventCommand.cs
│   │   │       ├── UpdateEventCommand.cs
│   │   │       ├── DeleteEventCommand.cs
│   │   │       ├── ActivateEventCommand.cs
│   │   │       └── DeactivateEventCommand.cs
│   │   │
│   │   ├── Queries/                   # Queries (Read)
│   │   │   ├── Auth/
│   │   │   │   └── GetCurrentUserQuery.cs
│   │   │   ├── Events/
│   │   │   │   ├── GetDashboardEventsQuery.cs
│   │   │   │   └── GetEventByIdQuery.cs
│   │   │   └── Users/
│   │   │       └── GetActiveUsersQuery.cs
│   │   │
│   │   ├── Handlers/                  # Handlers (CQRS)
│   │   │   ├── Auth/
│   │   │   │   ├── LoginCommandHandler.cs
│   │   │   │   └── RegisterCommandHandler.cs
│   │   │   └── Events/
│   │   │       ├── CreateEventCommandHandler.cs
│   │   │       ├── UpdateEventCommandHandler.cs
│   │   │       ├── DeleteEventCommandHandler.cs
│   │   │       ├── ActivateEventCommandHandler.cs
│   │   │       ├── DeactivateEventCommandHandler.cs
│   │   │       └── GetDashboardEventsQueryHandler.cs
│   │   │
│   │   ├── DTOs/                      # Data Transfer Objects
│   │   │   ├── AuthResponseDto.cs
│   │   │   ├── LoginDto.cs
│   │   │   ├── RegisterDto.cs
│   │   │   ├── EventDto.cs
│   │   │   ├── CreateEventDto.cs
│   │   │   ├── UpdateEventDto.cs
│   │   │   ├── ParticipantDto.cs
│   │   │   └── UserDto.cs
│   │   │
│   │   ├── Interfaces/                # Contratos
│   │   │   ├── IAuthService.cs
│   │   │   ├── ITokenExtractor.cs
│   │   │   ├── IDateTimeProvider.cs
│   │   │   ├── ICommandHandler.cs
│   │   │   └── IQueryHandler.cs
│   │   │
│   │   └── Exceptions/
│   │       └── InvalidDateRangeException.cs
│   │
│   ├── Domain/                        # Camada de Domínio
│   │   ├── Entities/                  # Entidades
│   │   │   ├── User.cs
│   │   │   └── Event.cs
│   │   │
│   │   ├── ValueObjects/              # Value Objects
│   │   │   ├── Email.cs
│   │   │   ├── EventName.cs
│   │   │   ├── EventDescription.cs
│   │   │   └── Location.cs
│   │   │
│   │   ├── Enums/
│   │   │   └── EventType.cs
│   │   │
│   │   ├── Interfaces/                # Contratos do domínio
│   │   │   ├── IRepository.cs
│   │   │   ├── IUserRepository.cs
│   │   │   ├── IEventRepository.cs
│   │   │   └── IUnitOfWork.cs
│   │   │
│   │   └── Services/
│   │       └── EventParticipantService.cs
│   │
│   ├── Infrastructure/                # Camada de Infraestrutura
│   │   ├── Data/
│   │   │   ├── AppDbContext.cs        # DbContext EF Core
│   │   │   └── UnitOfWork.cs          # Unit of Work pattern
│   │   │
│   │   ├── Repositories/              # Implementações de repositórios
│   │   │   ├── UserRepository.cs
│   │   │   └── EventRepository.cs
│   │   │
│   │   ├── Mappings/
│   │   │   └── EventProfile.cs        # AutoMapper profiles
│   │   │
│   │   └── Services/
│   │       ├── AuthService.cs         # JWT Service
│   │       ├── TokenExtractor.cs
│   │       └── DateTimeProvider.cs
│   │
│   ├── app/                           # Frontend Angular
│   │   ├── app.component.ts
│   │   ├── app.module.ts
│   │   │
│   │   ├── core/                      # Módulo core
│   │   │   ├── auth/
│   │   │   │   ├── auth.service.ts
│   │   │   │   ├── auth.guard.ts
│   │   │   │   └── auth.interceptor.ts
│   │   │   └── services/
│   │   │
│   │   ├── modules/
│   │   │   ├── auth/                  # Módulo de autenticação
│   │   │   │   ├── login/
│   │   │   │   └── login.component.ts
│   │   │   │
│   │   │   └── dashboard/             # Módulo dashboard
│   │   │       ├── pages/
│   │   │       │   └── dashboard.page.ts
│   │   │       └── components/
│   │   │           └── event-modal/
│   │   │               └── event-modal.component.ts
│   │   │
│   │   └── shared/                    # Componentes compartilhados
│   │
│   ├── Domain.Tests/                  # Testes do domínio
│   │   ├── EmailTests.cs
│   │   ├── EventTests.cs
│   │   └── EventTypeTests.cs
│   │
│   └── Application.Tests/             # Testes da aplicação
│       ├── Handlers/
│       │   ├── CreateEventCommandHandlerTests.cs
│       │   ├── UpdateEventCommandHandlerTests.cs
│       │   ├── DeleteEventCommandHandlerTests.cs
│       │   ├── ActivateEventCommandHandlerTests.cs
│       │   ├── DeactivateEventCommandHandlerTests.cs
│       │   └── GetDashboardEventsQueryHandlerTests.cs
│       └── Exceptions/
│           └── InvalidDateRangeExceptionTests.cs
│
├── AgendaManager.Api.csproj           # Projeto principal .NET
├── package.json                       # Dependências Node.js/Angular
├── angular.json                       # Configuração Angular
├── tsconfig.json                      # Configuração TypeScript
├── start-dev.ps1                      # Script PowerShell de inicialização
├── start-dev.bat                      # Script CMD de inicialização
└── README.md                          # Este arquivo
```

## 🚀 Pré-requisitos

Antes de começar, certifique-se de ter instalado:

1. **.NET SDK 10.0**
   ```powershell
   # Verificar instalação
   dotnet --version
   
   # Se não tiver, instale via:
   winget install Microsoft.DotNet.SDK.10
   ```

2. **Node.js 18+**
   ```powershell
   # Verificar instalação
   node --version
   
   # Baixar em: https://nodejs.org/
   ```

3. **Angular CLI**
   ```powershell
   # Instalar globalmente
   npm install -g @angular/cli@17
   
   # Verificar instalação
   ng version
   ```

4. **PostgreSQL** (ou acesso ao Supabase)
   - O projeto está configurado para usar Supabase
   - A string de conexão está em `src/Api/appsettings.json`

## 📦 Instalação

### Opção 1: Script Automatizado (Windows)

Execute o script de inicialização:

```powershell
# PowerShell
.\start-dev.ps1

# Ou CMD
.\start-dev.bat
```

O script irá:
- ✅ Verificar todas as dependências
- ✅ Instalar pacotes .NET (`dotnet restore`)
- ✅ Instalar pacotes Node.js (`npm install`)

### Opção 2: Instalação Manual

```powershell
# 1. Restaurar dependências .NET
dotnet restore

# 2. Instalar dependências Node.js
npm install
```

## ▶️ Como Executar o Projeto

O projeto precisa de **dois terminais** rodando simultaneamente: um para o backend e outro para o frontend.

### Terminal 1 - Backend (.NET API)

```powershell
# Navegar até a raiz do projeto
cd C:\Users\Cisun\teste-IATEC

# Executar com Hot Reload (recomendado para desenvolvimento)
dotnet watch run --project AgendaManager.Api.csproj
```

**O backend estará disponível em:**
- 🌐 API:`http://localhost:5000`
- 📚 Swagger: `https://localhost:5000/swagger`

### Terminal 2 - Frontend (Angular)

```powershell
# Navegar até a raiz do projeto (mesmo diretório)
cd C:\Users\Cisun\teste-IATEC

# Executar o servidor de desenvolvimento
npm start

# Ou usando Angular CLI diretamente
ng serve
```

**O frontend estará disponível em:**
- 🌐 Aplicação: `http://localhost:4200`

> ⚠️ **Importante**: Mantenha ambos os terminais abertos durante o desenvolvimento. O backend precisa estar rodando para o frontend funcionar corretamente.

## 🧪 Executando os Testes

### Testes Backend (.NET / xUnit)

```powershell
# Todos os testes
dotnet test

# Testes do domínio
dotnet test src/Domain.Tests/AgendaManager.Domain.Tests.csproj

# Testes da aplicação
dotnet test src/Application.Tests/AgendaManager.Application.Tests.csproj

# Com verbosidade
dotnet test --verbosity normal
```

### Testes Frontend (Angular / Jasmine/Karma)

```powershell
# Executar testes
npm test

# Ou
ng test
```

## 🛠️ Tecnologias Utilizadas

### Backend
- **.NET 10.0** - Framework principal
- **Entity Framework Core 10.0** - ORM
- **PostgreSQL** (via Supabase) - Banco de dados
- **MediatR 12.1.1** - Implementação CQRS
- **AutoMapper 12.0.1** - Mapeamento de objetos
- **JWT Bearer 10.0.0** - Autenticação
- **BCrypt.Net 4.0.3** - Hash de senhas
- **Swagger 6.5.0** - Documentação da API
- **xUnit** - Framework de testes
- **Moq** - Mocking para testes
- **FluentAssertions** - Assertions expressivas

### Frontend
- **Angular 17.0.0** - Framework
- **RxJS 7.8.0** - Programação reativa
- **TypeScript 5.2.0** - Linguagem
- **Jasmine/Karma** - Testes unitários

## 📝 Requisitos Funcionais

- ✅ **Login de usuário** - Autenticação JWT
- ✅ **Agenda individual por usuário** - Cada usuário tem sua agenda
- ✅ **Eventos com:**
  - Nome
  - Descrição
  - Data
  - Local
  - Participantes
- ✅ **Evento exclusivo ou compartilhado** - Tipos de eventos
- ✅ **Evento compartilhado aparece na agenda dos participantes** - Visibilidade automática
- ✅ **Evento pode ser editado** - Atualização completa
- ✅ **Evento pode ser removido** - Remoção total (cascade)
- ✅ **Evento pode ser ativo ou inativo** - Ativação/desativação
- ✅ **Dashboard com filtros:**
  - Data (início e fim)
  - Texto livre (busca)
  - Período (Dia / Semana / Mês)

## 🔧 Configuração

### Banco de Dados

A string de conexão está configurada em `src/Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=...;Port=5432;Database=postgres;..."
  }
}
```

As migrations são aplicadas automaticamente em modo de desenvolvimento ao iniciar a API.

### JWT Secret

O JWT Secret está configurado em `src/Api/appsettings.json`. Em produção, use variáveis de ambiente.

### API Versionada

A API está versionada em `/api/v1`:
- `/api/v1/auth/login`
- `/api/v1/auth/register`
- `/api/v1/events`
- `/api/v1/users`

## 🏛️ Padrões Arquiteturais Implementados

### CQRS (Command Query Responsibility Segregation)

- **Commands** (Write): `CreateEventCommand`, `UpdateEventCommand`, `DeleteEventCommand`
- **Queries** (Read): `GetDashboardEventsQuery`, `GetEventByIdQuery`
- **Handlers**: Cada comando/query tem seu handler específico

### Repository Pattern

- Interfaces no **Domain**
- Implementações no **Infrastructure**
- **Unit of Work** para transações

### Value Objects

- `Email` - Validação e imutabilidade
- `EventName` - Nome do evento
- `EventDescription` - Descrição do evento
- `Location` - Localização do evento

### Dependency Injection

- Todos os serviços registrados no `Program.cs`
- Interfaces injetadas via construtor
- Testabilidade garantida

## 🐛 Troubleshooting

### Erro: "Port already in use"
```powershell
# Backend - usar outra porta
dotnet watch run --project AgendaManager.Api.csproj --urls "http://localhost:5001"

# Frontend - usar outra porta
ng serve --port 4201
```

### Erro: "Database migration failed"
- Verifique a string de conexão em `appsettings.json`
- Certifique-se de que o Supabase está acessível
- Verifique as credenciais do banco

### Erro: "Angular CLI not found"
```powershell
npm install -g @angular/cli@17
```

### Erro: "MediatR handler not found"
- Verifique se o handler está no assembly correto
- Confirme o registro no `Program.cs`: `cfg.RegisterServicesFromAssembly(...)`

## 📚 Documentação Adicional

- **Swagger**: Disponível em `https://localhost:5001/swagger` quando a API estiver rodando
- **Health Check**: Disponível em `https://localhost:5001/health`

## 🎯 Objetivo do Projeto

Este projeto foi desenvolvido como parte do desafio técnico IATEC, demonstrando:

- ✅ Aderência total a **Clean Architecture** e **DDD**
- ✅ Separação clara de responsabilidades
- ✅ Código limpo e testável
- ✅ Implementação completa de **CQRS**
- ✅ Testes unitários abrangentes
- ✅ Qualidade de código profissional

---

**Desenvolvido com Clean Architecture, DDD, SOLID e CQRS** 
