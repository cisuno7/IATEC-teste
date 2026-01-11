# CASOS DE TESTE - AGENDA MANAGER
## Baseado em @src/tarefa.md e @architeture.md

---

## 1. AUTENTICAÇÃO E ACESSO

### TC-001: Login de Usuário
**Objetivo:** Verificar se o usuário consegue fazer login no sistema

**Pré-condições:**
- Usuário cadastrado no sistema
- Email e senha válidos

**Passos:**
1. Acessar a página de login
2. Preencher email e senha
3. Clicar em "Entrar"

**Resultado Esperado:**
- Login realizado com sucesso
- Redirecionamento para Dashboard
- Token JWT armazenado
- Usuário autenticado

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

### TC-002: Login com Credenciais Inválidas
**Objetivo:** Verificar tratamento de erro em login inválido

**Passos:**
1. Acessar página de login
2. Preencher email ou senha incorretos
3. Clicar em "Entrar"

**Resultado Esperado:**
- Mensagem de erro exibida
- Usuário permanece na tela de login
- Não há redirecionamento

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

### TC-003: Registro de Novo Usuário
**Objetivo:** Verificar criação de nova conta

**Passos:**
1. Acessar página de login
2. Clicar em "Criar conta"
3. Preencher nome, email, senha e confirmação de senha
4. Clicar em "Registrar"

**Resultado Esperado:**
- Conta criada com sucesso
- Redirecionamento para Dashboard
- Usuário autenticado automaticamente

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

### TC-004: Logout
**Objetivo:** Verificar encerramento de sessão

**Passos:**
1. Estar logado no sistema
2. Clicar no botão "Sair"

**Resultado Esperado:**
- Sessão encerrada
- Token removido
- Redirecionamento para tela de login

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

## 2. DASHBOARD E NAVEGAÇÃO

### TC-005: Redirecionamento Após Login
**Objetivo:** Verificar redirecionamento automático para Dashboard

**Pré-condições:**
- Login realizado com sucesso

**Resultado Esperado:**
- Após login, usuário é redirecionado para Dashboard
- URL contém `/dashboard`
- Header mostra nome do usuário

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

### TC-006: Agenda Individual por Usuário
**Objetivo:** Verificar que cada usuário vê apenas seus eventos

**Pré-condições:**
- Dois usuários cadastrados
- Cada um com eventos próprios

**Passos:**
1. Fazer login com Usuário A
2. Verificar eventos exibidos
3. Fazer logout
4. Fazer login com Usuário B
5. Verificar eventos exibidos

**Resultado Esperado:**
- Usuário A vê apenas eventos criados por ele e eventos compartilhados com ele
- Usuário B vê apenas eventos criados por ele e eventos compartilhados com ele
- Não há eventos cruzados entre usuários

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

## 3. CRIAÇÃO DE EVENTOS

### TC-007: Criar Evento Exclusivo
**Objetivo:** Verificar criação de evento exclusivo

**Passos:**
1. Clicar em "Novo Evento"
2. Preencher:
   - Nome: "Reunião de Equipe"
   - Descrição: "Discussão de projetos"
   - Data: Data futura
   - Hora: Hora válida
   - Local: "Sala de Reuniões"
   - Tipo: Exclusivo
3. Clicar em "Criar Evento"

**Resultado Esperado:**
- Evento criado com sucesso
- Evento aparece apenas na agenda do criador
- Evento não aparece para outros usuários
- Modal fecha automaticamente

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

### TC-008: Criar Evento Compartilhado
**Objetivo:** Verificar criação de evento compartilhado com participantes

**Pré-condições:**
- Pelo menos 2 usuários cadastrados

**Passos:**
1. Clicar em "Novo Evento"
2. Preencher dados do evento
3. Selecionar tipo: "Compartilhado"
4. Adicionar participantes (buscar e selecionar usuários)
5. Clicar em "Criar Evento"

**Resultado Esperado:**
- Evento criado com sucesso
- Evento aparece na agenda do criador
- Evento aparece na agenda dos participantes selecionados
- Participantes aparecem na lista do evento

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

### TC-009: Validação de Campos Obrigatórios
**Objetivo:** Verificar validação de formulário

**Passos:**
1. Clicar em "Novo Evento"
2. Tentar criar evento sem preencher campos obrigatórios
3. Clicar em "Criar Evento"

**Resultado Esperado:**
- Botão "Criar Evento" desabilitado
- Mensagens de erro aparecem nos campos vazios
- Evento não é criado

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

### TC-010: Validação de Tamanho de Campos
**Objetivo:** Verificar validação de tamanho mínimo/máximo

**Passos:**
1. Criar evento com:
   - Nome com menos de 3 caracteres
   - Descrição com menos de 10 caracteres
   - Local com menos de 3 caracteres

**Resultado Esperado:**
- Mensagens de erro específicas
- Evento não é criado

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

## 4. EDIÇÃO DE EVENTOS

### TC-011: Editar Evento (Criador)
**Objetivo:** Verificar edição de evento pelo criador

**Pré-condições:**
- Evento criado pelo usuário logado

**Passos:**
1. Clicar no botão de editar (✏️) em um evento
2. Modificar nome, descrição, data, hora, local
3. Clicar em "Salvar"

**Resultado Esperado:**
- Modal abre com dados preenchidos
- Alterações salvas com sucesso
- Evento atualizado na lista
- Botão "Salvar" habilitado após carregar dados

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

### TC-012: Editar Participantes de Evento Compartilhado
**Objetivo:** Verificar edição de participantes

**Pré-condições:**
- Evento compartilhado criado

**Passos:**
1. Editar evento compartilhado
2. Adicionar novo participante
3. Remover participante existente
4. Salvar alterações

**Resultado Esperado:**
- Participantes atualizados
- Novo participante vê o evento em sua agenda
- Participante removido não vê mais o evento

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

### TC-013: Alterar Tipo de Evento (Exclusivo ↔ Compartilhado)
**Objetivo:** Verificar mudança de tipo de evento

**Passos:**
1. Editar evento exclusivo
2. Alterar para compartilhado
3. Adicionar participantes
4. Salvar

**Resultado Esperado:**
- Tipo alterado com sucesso
- Evento aparece para participantes
- Evento pode ser alterado de volta para exclusivo

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

### TC-014: Editar Evento (Participante - Modo Leitura)
**Objetivo:** Verificar que participante não pode editar

**Pré-condições:**
- Evento compartilhado criado
- Usuário logado é participante (não criador)

**Resultado Esperado:**
- Botão de editar (✏️) NÃO aparece
- Botão de deletar (🗑️) NÃO aparece
- Botão de desativar NÃO aparece
- Apenas visualização (modo leitura)

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

## 5. REMOÇÃO DE EVENTOS

### TC-015: Deletar Evento
**Objetivo:** Verificar remoção física de evento

**Pré-condições:**
- Evento criado pelo usuário logado

**Passos:**
1. Clicar no botão de deletar (🗑️)
2. Confirmar exclusão (se houver confirmação)

**Resultado Esperado:**
- Evento removido permanentemente
- Evento desaparece da agenda do criador
- Evento desaparece da agenda dos participantes
- Relacionamentos na tabela `event_participants` removidos (cascade)

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

### TC-016: Deletar Evento (Participante)
**Objetivo:** Verificar que participante não pode deletar

**Pré-condições:**
- Evento compartilhado
- Usuário logado é participante

**Resultado Esperado:**
- Botão de deletar NÃO aparece
- Participante não consegue deletar evento

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

## 6. STATUS DE EVENTOS (ATIVO/INATIVO)

### TC-017: Desativar Evento
**Objetivo:** Verificar desativação de evento

**Pré-condições:**
- Evento ativo criado

**Passos:**
1. Clicar no botão "Desativar"

**Resultado Esperado:**
- Evento marcado como inativo
- Status muda para "Inativo"
- Botão muda para "Ativar"

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

### TC-018: Ativar Evento
**Objetivo:** Verificar ativação de evento inativo

**Pré-condições:**
- Evento inativo

**Passos:**
1. Clicar no botão "Ativar"

**Resultado Esperado:**
- Evento marcado como ativo
- Status muda para "Ativo"
- Botão muda para "Desativar"

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

### TC-019: Desativar Evento (Participante)
**Objetivo:** Verificar que participante não pode desativar

**Pré-condições:**
- Evento compartilhado
- Usuário logado é participante

**Resultado Esperado:**
- Botão "Desativar/Ativar" NÃO aparece
- Participante não consegue alterar status

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

## 7. FILTROS DO DASHBOARD

### TC-020: Filtro por Data
**Objetivo:** Verificar filtro por intervalo de datas

**Passos:**
1. Preencher data inicial
2. Preencher data final
3. Aplicar filtro

**Resultado Esperado:**
- Apenas eventos no intervalo de datas são exibidos
- Eventos fora do intervalo são ocultados

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

### TC-021: Filtro por Hora
**Objetivo:** Verificar filtro por intervalo de horas

**Passos:**
1. Preencher hora inicial
2. Preencher hora final
3. Aplicar filtro

**Resultado Esperado:**
- Apenas eventos no intervalo de horas são exibidos
- Filtro combina data + hora corretamente

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

### TC-022: Filtro por Texto Livre
**Objetivo:** Verificar busca por texto em qualquer campo

**Passos:**
1. Digitar texto no campo de busca
2. Aplicar filtro

**Resultado Esperado:**
- Eventos que contêm o texto em nome, descrição ou local são exibidos
- Busca é case-insensitive
- Busca funciona em tempo real

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

### TC-023: Filtro "Eventos do Dia"
**Objetivo:** Verificar filtro de eventos do dia atual

**Passos:**
1. Clicar no botão "Hoje" ou "Dia"

**Resultado Esperado:**
- Apenas eventos do dia atual são exibidos
- Eventos de outros dias são ocultados

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

### TC-024: Filtro "Eventos da Semana"
**Objetivo:** Verificar filtro de eventos da semana atual

**Passos:**
1. Clicar no botão "Semana"

**Resultado Esperado:**
- Apenas eventos da semana atual são exibidos
- Eventos de outras semanas são ocultados

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

### TC-025: Filtro "Eventos do Mês"
**Objetivo:** Verificar filtro de eventos do mês atual

**Passos:**
1. Clicar no botão "Mês"

**Resultado Esperado:**
- Apenas eventos do mês atual são exibidos
- Eventos de outros meses são ocultados

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

### TC-026: Limpar Filtros
**Objetivo:** Verificar limpeza de filtros aplicados

**Passos:**
1. Aplicar vários filtros
2. Clicar em "Limpar filtros" ou similar

**Resultado Esperado:**
- Todos os filtros são removidos
- Todos os eventos são exibidos novamente

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

## 8. COMPARTILHAMENTO DE EVENTOS

### TC-027: Evento Compartilhado Aparece para Participantes
**Objetivo:** Verificar que evento compartilhado aparece na agenda dos participantes

**Pré-condições:**
- Usuário A cria evento compartilhado
- Usuário B é adicionado como participante

**Passos:**
1. Usuário B faz login
2. Acessa Dashboard

**Resultado Esperado:**
- Evento compartilhado aparece na agenda do Usuário B
- Evento mostra nome do criador
- Evento está em modo leitura para Usuário B

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

### TC-028: Evento Exclusivo Não Aparece para Outros
**Objetivo:** Verificar que evento exclusivo é privado

**Pré-condições:**
- Usuário A cria evento exclusivo

**Passos:**
1. Usuário B faz login
2. Acessa Dashboard

**Resultado Esperado:**
- Evento exclusivo NÃO aparece na agenda do Usuário B
- Apenas Usuário A vê o evento

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

## 9. REQUISITOS TÉCNICOS - BACKEND

### TC-029: API Versionada
**Objetivo:** Verificar que API está versionada

**Passos:**
1. Verificar URLs das requisições

**Resultado Esperado:**
- Todas as URLs começam com `/api/v1/`
- Versionamento implementado

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

### TC-030: CQRS Implementado
**Objetivo:** Verificar separação de Commands e Queries

**Verificações:**
- Commands para operações de escrita (Create, Update, Delete)
- Queries para operações de leitura (Get, List)
- Handlers separados para cada Command/Query

**Resultado Esperado:**
- Estrutura CQRS implementada
- Separação clara entre Commands e Queries

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

### TC-031: Entity Framework Core
**Objetivo:** Verificar uso de EF Core para gerenciamento do banco

**Verificações:**
- Migrations criadas
- DbContext configurado
- Repositórios usam EF Core

**Resultado Esperado:**
- EF Core configurado corretamente
- Migrations aplicadas

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

### TC-032: Clean Architecture
**Objetivo:** Verificar aderência a Clean Architecture

**Verificações:**
- Domain não conhece Infrastructure
- Application não conhece EF Core
- Controllers apenas orquestram
- Sem lógica de negócio em Controllers

**Resultado Esperado:**
- Camadas bem separadas
- Dependências corretas

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

## 10. REQUISITOS TÉCNICOS - FRONTEND

### TC-033: Angular com Modules
**Objetivo:** Verificar organização em módulos

**Verificações:**
- Módulos criados (AuthModule, DashboardModule)
- Components organizados
- Services isolados

**Resultado Esperado:**
- Estrutura modular implementada
- Organização clara

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

### TC-034: Guards de Autenticação
**Objetivo:** Verificar proteção de rotas

**Passos:**
1. Tentar acessar `/dashboard` sem estar logado

**Resultado Esperado:**
- Redirecionamento para login
- Rota protegida por guard

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

### TC-035: Environments Configurados
**Objetivo:** Verificar configuração de environments

**Verificações:**
- `environment.ts` e `environment.prod.ts` existem
- API URL configurada

**Resultado Esperado:**
- Environments configurados corretamente

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

## 11. CASOS ESPECIAIS E BORDAS

### TC-036: Evento sem Participantes (Compartilhado)
**Objetivo:** Verificar criação de evento compartilhado sem participantes

**Passos:**
1. Criar evento tipo "Compartilhado"
2. Não adicionar participantes
3. Salvar

**Resultado Esperado:**
- Evento criado como compartilhado
- Apenas criador vê o evento
- Pode adicionar participantes depois

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

### TC-037: Remover Todos os Participantes
**Objetivo:** Verificar remoção de todos os participantes

**Pré-condições:**
- Evento compartilhado com participantes

**Passos:**
1. Editar evento
2. Remover todos os participantes
3. Salvar

**Resultado Esperado:**
- Evento permanece compartilhado
- Apenas criador vê o evento
- Pode adicionar participantes novamente

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

### TC-038: Token Expirado
**Objetivo:** Verificar tratamento de token expirado

**Passos:**
1. Aguardar expiração do token (1 hora)
2. Tentar realizar operação

**Resultado Esperado:**
- Erro 401 (Unauthorized)
- Redirecionamento para login
- Mensagem apropriada

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

### TC-039: Múltiplos Filtros Simultâneos
**Objetivo:** Verificar combinação de filtros

**Passos:**
1. Aplicar filtro de data
2. Aplicar filtro de texto
3. Aplicar filtro de hora

**Resultado Esperado:**
- Filtros combinados funcionam corretamente
- Apenas eventos que atendem todos os filtros são exibidos

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

### TC-040: Responsividade
**Objetivo:** Verificar layout responsivo

**Passos:**
1. Testar em diferentes tamanhos de tela
2. Testar em mobile

**Resultado Esperado:**
- Layout adapta-se corretamente
- Componentes são utilizáveis em mobile
- Modais responsivos

**Status:** ⬜ Não Testado / ✅ Passou / ❌ Falhou

---

## CHECKLIST FINAL

### Funcionalidades Obrigatórias
- [ ] Login de usuário
- [ ] Registro de usuário
- [ ] Agenda individual por usuário
- [ ] Criar evento (exclusivo e compartilhado)
- [ ] Editar evento
- [ ] Deletar evento
- [ ] Ativar/Desativar evento
- [ ] Filtros: data, hora, texto
- [ ] Filtros: dia, semana, mês
- [ ] Compartilhamento de eventos
- [ ] Modo leitura para participantes

### Requisitos Técnicos Backend
- [ ] .NET 10
- [ ] API REST/RESTful
- [ ] API versionada (/api/v1)
- [ ] CQRS implementado
- [ ] Entity Framework Core
- [ ] Clean Architecture
- [ ] SOLID
- [ ] Repositórios
- [ ] Injeção de dependência
- [ ] DTOs
- [ ] AutoMapper (desejável)

### Requisitos Técnicos Frontend
- [ ] Angular
- [ ] Modules organizados
- [ ] Components organizados
- [ ] Services isolados
- [ ] Guards de autenticação
- [ ] Environments configurados
- [ ] Sem lógica de negócio em components

---

## NOTAS DE TESTE

**Data do Teste:** _______________

**Testador:** _______________

**Ambiente:** [ ] Desenvolvimento [ ] Produção

**Observações:**
_________________________________________________________________
_________________________________________________________________
_________________________________________________________________

---

**Total de Testes:** 40
**Testes Passados:** ___
**Testes Falhados:** ___
**Taxa de Sucesso:** ___%