Resultado do Desafio IATEC

O sistema foi desenvolvido atendendo 100% dos requisitos funcionais, técnicos e desejáveis descritos no desafio, seguindo rigorosamente os princípios de Clean Architecture, DDD, SOLID e CQRS.

O backend foi implementado em .NET 10 com Entity Framework Core e PostgreSQL, utilizando migrations versionadas e abordagem baseline para integração com Supabase sem impacto no schema existente.

O frontend foi desenvolvido em Angular 17, com estrutura modular, separação de responsabilidades e comunicação REST versionada.

Todos os fluxos críticos — autenticação, agenda individual, eventos exclusivos e compartilhados, filtros avançados, edição e remoção com cascade — foram implementados e validados.

O projeto mantém isolamento total entre camadas, sem vazamento de infraestrutura para domínio ou aplicação, e controllers atuam apenas como orquestradores via MediatR.