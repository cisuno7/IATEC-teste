A partir deste ponto, todo o código deve seguir rigorosamente:

- Clean Architecture
- Domain-Driven Design (DDD)
- SOLID
- Clean Code
- CQRS
- ZERO comentários no código
- Métodos pequenos, nomes autoexplicativos
- Nenhuma lógica em Controllers
- Nenhuma dependência do domínio com infraestrutura ou framework
- Controllers apenas orquestram comandos e queries
- DTOs apenas para transporte
- Repositórios apenas no Infrastructure
- Application não conhece EF Core
- Domain não conhece nada externo

========================
REGRAS DE CÓDIGO
========================

1. Não escreva comentários.
2. Não use regiões.
3. Não use métodos longos.
4. Não use ifs complexos.
5. Nomes claros substituem comentários.
6. Use Value Objects quando fizer sentido.
7. Use Enums apenas no Domain.
8. Use Imutabilidade sempre que possível.
9. Não use static para lógica de negócio.
10. Controllers não validam regra de negócio.

========================
ARQUITETURA
========================

Camadas:
- Domain: entidades, regras, enums, value objects
- Application: commands, queries, handlers, interfaces
- Infrastructure: EF Core, repositórios, mapeamentos
- Api: controllers, autenticação, versionamento

========================
REQUISITOS FUNCIONAIS
========================

- Login de usuário
- Agenda individual por usuário
- Eventos com:
  - Nome
  - Descrição
  - Data
  - Local
  - Participantes
- Evento exclusivo ou compartilhado
- Evento compartilhado aparece na agenda dos participantes
- Evento pode ser editado
- Evento pode ser removido (remoção total)
- Evento pode ser ativo ou inativo
- Dashboard com filtros:
  - Data
  - Texto livre
  - Dia / Semana / Mês

========================
BACK-END
========================

- ASP.NET Web API
- API versionada (/api/v1)
- CQRS com separação clara
- Entity Framework Core com PostgreSQL
- AutoMapper
- Repositórios por interface
- Injeção de dependência
- Sem lógica no controller
- Sem DTOs vazando domínio

========================
FRONT-END
========================

- Angular com modules
- Services isolados
- Guards para autenticação
- Environments configurados
- Componentes pequenos e coesos
- Sem lógica de negócio nos componentes

========================
OBJETIVO
========================

Continue a implementação existente ou refatore o que já foi criado para atender EXATAMENTE essas regras.

O foco é qualidade de código, clareza arquitetural e aderência total a Clean Architecture e DDD.

Não explique o código.
Não adicione comentários.
Apenas implemente.
