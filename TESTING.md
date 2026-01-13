# Guia de Testes Automatizados

Este documento explica como configurar e executar os testes automatizados do projeto.

## 📋 Índice

- [Backend - xUnit (.NET)](#backend---xunit-net)
- [Frontend - Jasmine/Karma (Angular)](#frontend---jasminekarma-angular)

---

## 🔷 Backend - xUnit (.NET)

### Configuração

O projeto de testes está localizado em `src/Domain.Tests/` e utiliza:
- **xUnit** - Framework de testes
- **Moq** - Framework de mocking
- **FluentAssertions** - Assertions mais legíveis

### Estrutura

```
src/
├── Domain.Tests/
│   ├── AgendaManager.Domain.Tests.csproj
│   └── EmailTests.cs (exemplo)
├── Domain/
│   └── Domain.csproj
└── Application/
    └── Application.csproj
```

### Executar Testes

#### Via CLI (PowerShell)

```powershell
# Executar todos os testes
dotnet test src/Domain.Tests/AgendaManager.Domain.Tests.csproj

# Executar com detalhes
dotnet test src/Domain.Tests/AgendaManager.Domain.Tests.csproj --verbosity normal

# Executar com cobertura de código
dotnet test src/Domain.Tests/AgendaManager.Domain.Tests.csproj --collect:"XPlat Code Coverage"
```

#### Via Visual Studio

1. Abra o **Test Explorer** (Test → Test Explorer)
2. Clique em **Run All** ou execute testes individuais

### Exemplo de Teste

```csharp
using AgendaManager.Domain.ValueObjects;

namespace AgendaManager.Domain.Tests;

public class EmailTests
{
    [Fact]
    public void Create_ValidEmail_ReturnsEmail()
    {
        var emailString = "test@example.com";
        var email = Email.Create(emailString);

        Assert.Equal(emailString.ToLower(), email.Value);
    }
}
```

### Pacotes Instalados

- `xunit` (2.9.2)
- `xunit.runner.visualstudio` (2.8.2)
- `Microsoft.NET.Test.Sdk` (17.11.1)
- `Moq` (4.20.72)
- `FluentAssertions` (7.0.0)
- `coverlet.collector` (6.0.2) - Para cobertura de código

---

## 🔷 Frontend - Jasmine/Karma (Angular)

### Configuração

O Angular já está configurado com:
- **Jasmine** - Framework de testes
- **Karma** - Test runner
- **HttpClientTestingModule** - Para testar serviços HTTP

### Arquivos de Configuração

- `karma.conf.js` - Configuração do Karma
- `tsconfig.spec.json` - Configuração TypeScript para testes
- `src/test.ts` - Setup do ambiente de testes

### Executar Testes

#### Via CLI

```powershell
# Executar todos os testes (modo watch)
npm test

# Executar uma vez e sair
npm test -- --watch=false

# Executar com cobertura
npm test -- --code-coverage
```

#### Via Angular CLI

```powershell
ng test

# Executar uma vez
ng test --watch=false

# Com cobertura
ng test --code-coverage
```

### Estrutura de Testes

Os arquivos de teste devem seguir o padrão `*.spec.ts` e estar no mesmo diretório do arquivo testado:

```
src/app/
├── core/
│   └── services/
│       ├── event.service.ts
│       └── event.service.spec.ts
└── modules/
    └── auth/
        └── login/
            ├── login.component.ts
            └── login.component.spec.ts
```

### Exemplo de Teste de Serviço

```typescript
import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { EventService } from './event.service';

describe('EventService', () => {
  let service: EventService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [EventService]
    });
    service = TestBed.inject(EventService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
```

### Exemplo de Teste de Componente

```typescript
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { LoginComponent } from './login.component';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [LoginComponent],
      imports: [ReactiveFormsModule]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
```

### Pacotes Instalados (já no package.json)

- `jasmine-core` (~5.1.0)
- `karma` (~6.4.0)
- `karma-jasmine` (~5.1.0)
- `karma-chrome-launcher` (~3.2.0)
- `karma-coverage` (~2.2.0)
- `karma-jasmine-html-reporter` (~2.1.0)
- `@types/jasmine` (~5.1.0)

---

## 📝 Boas Práticas

### Backend (xUnit)

1. **Nomenclatura**: Use `[Fact]` para testes simples e `[Theory]` para testes parametrizados
2. **Organização**: Agrupe testes relacionados em classes
3. **Mocking**: Use `Moq` para isolar dependências
4. **Assertions**: Use `FluentAssertions` para assertions mais legíveis

### Frontend (Jasmine)

1. **Nomenclatura**: Use `describe` para agrupar e `it` para casos de teste
2. **Setup/Teardown**: Use `beforeEach` e `afterEach` para preparar e limpar
3. **Isolamento**: Use `HttpTestingController` para testar serviços HTTP
4. **Spies**: Use `jasmine.createSpyObj` para criar mocks de serviços

---

## 🚀 Próximos Passos

1. Adicionar mais testes unitários para handlers, services e componentes
2. Configurar testes de integração
3. Configurar CI/CD para executar testes automaticamente
4. Aumentar a cobertura de código

---

## 📚 Recursos

- [Documentação xUnit](https://xunit.net/)
- [Documentação Jasmine](https://jasmine.github.io/)
- [Documentação Karma](https://karma-runner.github.io/)
- [Angular Testing Guide](https://angular.io/guide/testing)
