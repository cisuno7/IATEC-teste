# Design de Banco de Dados - Agenda Manager
## Supabase (PostgreSQL) - Alinhado com DDD, Clean Architecture e CQRS

---

## 1. VISÃO GERAL DO MODELO DE DADOS

### Entidades Principais

**Users**
- Representa usuários do sistema
- Autenticação via email/password hash
- Estado ativo/inativo
- Relacionamento 1:N com eventos criados
- Relacionamento N:M com eventos participados

**Events**
- Representa eventos da agenda
- Pertence a um criador (Users)
- Tipo: Exclusive (0) ou Shared (1)
- Estado: ativo/inativo
- Campos: nome, descrição, data, local
- Relacionamento N:M com participantes (Users)

**EventParticipants**
- Tabela de junção para relacionamento N:M
- Conecta Events e Users
- Permite múltiplos participantes por evento compartilhado

### Princípios Aplicados

**DDD (Domain-Driven Design)**
- Estrutura reflete o domínio real (agenda, eventos, participantes)
- Value Objects (Email, EventName, etc.) são armazenados como colunas simples
- Enums (EventType) mapeados como inteiros
- Agregados claros: User e Event são agregados raiz

**Clean Architecture**
- Banco é pura infraestrutura
- Nenhuma regra de negócio no banco (apenas constraints de integridade)
- Estrutura suporta mapeamento via EF Core sem acoplamento

**CQRS (Command Query Responsibility Segregation)**
- Índices otimizados para queries de leitura (dashboard)
- Estrutura permite leituras eficientes sem bloquear escritas
- Separação clara entre dados de escrita e leitura

---

## 2. DIAGRAMA LÓGICO (Descrição Textual)

```
┌─────────────────┐
│     Users       │
├─────────────────┤
│ id (PK, UUID)   │
│ name            │
│ email (UNIQUE)   │
│ password_hash   │
│ created_at      │
│ is_active       │
└────────┬────────┘
         │
         │ 1:N (creator_id)
         │
         ▼
┌─────────────────┐
│     Events       │
├─────────────────┤
│ id (PK, UUID)   │
│ name            │
│ description     │
│ date            │
│ location        │
│ type            │
│ is_active       │
│ created_at      │
│ updated_at      │
│ creator_id (FK, UUID) │
└────────┬────────┘
         │
         │ N:M
         │
         ▼
┌──────────────────────┐
│ EventParticipants    │
├──────────────────────┤
│ event_id (FK, PK, UUID) │
│ user_id (FK, PK, UUID)  │
│ created_at           │
└──────────────────────┘
         │
         │ N:1
         │
         ▼
┌─────────────────┐
│     Users       │
└─────────────────┘
```

**Relacionamentos:**
- Users 1:N Events (um usuário cria muitos eventos)
- Events N:M Users (eventos compartilhados têm muitos participantes)
- EventParticipants é tabela de junção

**Regras de Negócio no Domínio (não no banco):**
- Eventos exclusivos: apenas creator_id pode ver
- Eventos compartilhados: creator_id + participantes podem ver
- Apenas creator_id pode editar/remover
- Remoção de evento remove participantes (CASCADE)

---

## 3. SQL COMPLETO DO SCHEMA

```sql
-- ============================================
-- EXTENSÕES NECESSÁRIAS
-- ============================================

CREATE EXTENSION IF NOT EXISTS "pgcrypto";


-- ============================================
-- TABELA: users
-- ============================================

CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL,
    email VARCHAR(150) NOT NULL UNIQUE,
    password_hash TEXT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    is_active BOOLEAN NOT NULL DEFAULT true
);

-- ============================================
-- TABELA: events
-- ============================================

CREATE TABLE events (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(200) NOT NULL,
    description VARCHAR(1000),
    date TIMESTAMPTZ NOT NULL,
    location VARCHAR(300),
    type INTEGER NOT NULL DEFAULT 0,
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ,
    creator_id UUID NOT NULL,
    
    CONSTRAINT events_creator_fk FOREIGN KEY (creator_id) 
        REFERENCES users(id) 
        ON DELETE RESTRICT,
    CONSTRAINT events_type_valid CHECK (type IN (0, 1))
);

-- ============================================
-- TABELA: event_participants (Junction Table)
-- ============================================

CREATE TABLE event_participants (
    event_id UUID NOT NULL,
    user_id UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT event_participants_pk PRIMARY KEY (event_id, user_id),
    CONSTRAINT event_participants_event_fk FOREIGN KEY (event_id) 
        REFERENCES events(id) 
        ON DELETE CASCADE,
    CONSTRAINT event_participants_user_fk FOREIGN KEY (user_id) 
        REFERENCES users(id) 
        ON DELETE CASCADE
);

-- ============================================
-- ÍNDICES PARA PERFORMANCE
-- ============================================

-- Users
CREATE INDEX idx_users_email ON users(email) WHERE is_active = true;
CREATE INDEX idx_users_is_active ON users(is_active);

-- Events (Otimização para Dashboard)
CREATE INDEX idx_events_creator_id ON events(creator_id);
CREATE INDEX idx_events_date ON events(date);
CREATE INDEX idx_events_type ON events(type);
CREATE INDEX idx_events_is_active ON events(is_active);
CREATE INDEX idx_events_creator_active ON events(creator_id, is_active);
CREATE INDEX idx_events_date_active ON events(date, is_active);

-- Índice composto para filtros de dashboard (mais comum)
CREATE INDEX idx_events_dashboard_filter ON events(creator_id, is_active, date) 
    WHERE is_active = true;

-- Índice GIN para busca full-text (nome, descrição, local)
CREATE INDEX idx_events_search_text ON events USING GIN (
    to_tsvector('portuguese', COALESCE(name, '') || ' ' || COALESCE(description, '') || ' ' || COALESCE(location, ''))
);

-- Event Participants
CREATE INDEX idx_event_participants_event_id ON event_participants(event_id);
CREATE INDEX idx_event_participants_user_id ON event_participants(user_id);
CREATE INDEX idx_event_participants_user_event ON event_participants(user_id, event_id);

-- ============================================
-- VIEWS PARA CQRS (Otimização de Leitura)
-- ============================================

-- View: Dashboard Events (Query otimizada)
CREATE OR REPLACE VIEW dashboard_events_view AS
SELECT 
    e.id,
    e.name,
    e.description,
    e.date,
    e.location,
    e.type,
    e.is_active,
    e.created_at,
    e.updated_at,
    e.creator_id,
    u.name AS creator_name,
    u.email AS creator_email,
    COALESCE(
        json_agg(
            json_build_object(
                'id', p.id,
                'name', p.name,
                'email', p.email
            )
        ) FILTER (WHERE p.id IS NOT NULL),
        '[]'::json
    ) AS participants
FROM events e
INNER JOIN users u ON e.creator_id = u.id
LEFT JOIN event_participants ep ON e.id = ep.event_id
LEFT JOIN users p ON ep.user_id = p.id
WHERE e.is_active = true
GROUP BY e.id, u.id, u.name, u.email;

```

---

## 4. POLÍTICAS RLS (ROW LEVEL SECURITY)

**IMPORTANTE: RLS é opcional e só deve ser usado se Supabase Auth for adotado.**
**Em produção com JWT próprio (.NET), RLS deve ser desativado e a segurança gerenciada pela camada Application (handlers).**

```sql
-- ============================================
-- HABILITAR RLS
-- ============================================

ALTER TABLE users ENABLE ROW LEVEL SECURITY;
ALTER TABLE events ENABLE ROW LEVEL SECURITY;
ALTER TABLE event_participants ENABLE ROW LEVEL SECURITY;

-- ============================================
-- POLÍTICAS: users
-- ============================================

-- Usuários podem ver apenas seus próprios dados
CREATE POLICY "Users can view own data"
    ON users FOR SELECT
    USING (auth.uid()::text = id::text);

-- Usuários podem atualizar apenas seus próprios dados
CREATE POLICY "Users can update own data"
    ON users FOR UPDATE
    USING (auth.uid()::text = id::text);

-- ============================================
-- POLÍTICAS: events
-- ============================================

-- Usuários podem ver eventos que criaram
CREATE POLICY "Users can view own created events"
    ON events FOR SELECT
    USING (creator_id::text = auth.uid()::text);

-- Usuários podem ver eventos compartilhados em que participam
CREATE POLICY "Users can view shared events they participate"
    ON events FOR SELECT
    USING (
        type = 1 AND
        EXISTS (
            SELECT 1 FROM event_participants
            WHERE event_id = events.id
            AND user_id::text = auth.uid()::text
        )
    );

-- Apenas criador pode inserir eventos
CREATE POLICY "Only creator can insert events"
    ON events FOR INSERT
    WITH CHECK (creator_id::text = auth.uid()::text);

-- Apenas criador pode atualizar eventos
CREATE POLICY "Only creator can update events"
    ON events FOR UPDATE
    USING (creator_id::text = auth.uid()::text);

-- Apenas criador pode deletar eventos
CREATE POLICY "Only creator can delete events"
    ON events FOR DELETE
    USING (creator_id::text = auth.uid()::text);

-- ============================================
-- POLÍTICAS: event_participants
-- ============================================

-- Usuários podem ver participantes de eventos que criaram ou participam
CREATE POLICY "Users can view participants of accessible events"
    ON event_participants FOR SELECT
    USING (
        EXISTS (
            SELECT 1 FROM events
            WHERE id = event_participants.event_id
            AND (
                creator_id::text = auth.uid()::text
                OR (
                    type = 1 AND
                    EXISTS (
                        SELECT 1 FROM event_participants ep
                        WHERE ep.event_id = events.id
                        AND ep.user_id::text = auth.uid()::text
                    )
                )
            )
        )
    );

-- Apenas criador do evento pode adicionar participantes
CREATE POLICY "Only event creator can add participants"
    ON event_participants FOR INSERT
    WITH CHECK (
        EXISTS (
            SELECT 1 FROM events
            WHERE id = event_participants.event_id
            AND creator_id::text = auth.uid()::text
            AND type = 1
        )
    );

-- Apenas criador do evento pode remover participantes
CREATE POLICY "Only event creator can remove participants"
    ON event_participants FOR DELETE
    USING (
        EXISTS (
            SELECT 1 FROM events
            WHERE id = event_participants.event_id
            AND creator_id::text = auth.uid()::text
        )
    );
```

**Quando usar RLS:**
- Apenas se Supabase Auth for adotado como sistema de autenticação
- `auth.uid()` retorna o UUID do usuário autenticado no Supabase
- RLS funciona automaticamente em todas as queries

**Quando NÃO usar RLS:**
- Se o backend .NET usar JWT próprio (recomendado para Clean Architecture)
- Desabilitar RLS: `ALTER TABLE users DISABLE ROW LEVEL SECURITY;` (e demais tabelas)
- Segurança gerenciada 100% pela camada Application (handlers verificam permissões)
- Domain entities já possuem métodos `CanUserView()` e `CanUserEdit()`

---

## 5. OBSERVAÇÕES DE INTEGRAÇÃO COM .NET 10 + EF CORE

### Compatibilidade

**IDs como UUID**
- Schema usa `UUID` (padrão Supabase, melhor para RLS)
- **Código .NET precisa usar `Guid` ao invés de `int` nas entidades Domain**
- EF Core mapeia automaticamente `Guid` ↔ `UUID`
- Benefícios: segurança (não expõe sequência), distribuição, RLS nativo
- **Mudança necessária:** Atualizar `User.Id`, `Event.Id`, `Event.CreatorId` de `int` para `Guid`

**Value Objects**
- `Email`, `EventName`, `EventDescription`, `Location` são Value Objects no Domain
- No banco, são armazenados como colunas simples (VARCHAR)
- EF Core mapeia via `OwnsOne()` no `AppDbContext`

**Enums**
- `EventType` enum no Domain (Exclusive=0, Shared=1)
- No banco: `INTEGER` com constraint CHECK
- EF Core mapeia automaticamente

**Relacionamentos**
- `Users 1:N Events`: FK `creator_id` em `events`
- `Events N:M Users`: Tabela `event_participants`
- EF Core mapeia via `HasMany().WithMany()`

### Migrations

**Criar Migration Inicial:**
```bash
dotnet ef migrations add InitialCreate --project AgendaManager.Api.csproj
```

**Aplicar no Supabase:**
- Opção 1: Executar SQL diretamente no Supabase SQL Editor
- Opção 2: Usar `dotnet ef database update` (requer connection string configurada)

### Performance e CQRS

**Queries de Leitura (Dashboard)**
- View `dashboard_events_view` otimizada para leitura
- Índices compostos para filtros comuns
- Índice GIN para busca full-text

**Commands (Escrita)**
- Tabelas base sem overhead de views
- Índices não bloqueiam escritas
- CASCADE garante integridade sem lógica extra

### Segurança

**Se usar Supabase Auth:**
- RLS policies ativas (seção 4)
- `auth.uid()` identifica usuário automaticamente
- Segurança no banco + Application layer

**Se usar JWT próprio (.NET) - RECOMENDADO:**
- **Desabilitar RLS:** `ALTER TABLE users DISABLE ROW LEVEL SECURITY;` (e demais tabelas)
- Segurança 100% gerenciada por Application layer (handlers)
- Domain entities já possuem `CanUserView()` e `CanUserEdit()`
- Handlers verificam permissões antes de queries/commands
- Mantém Clean Architecture: banco = infraestrutura pura

### Connection String

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=db.xxxxx.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=your_password;SSL Mode=Require;Trust Server Certificate=true"
  }
}
```

---

## 6. JUSTIFICATIVAS ARQUITETURAIS

### Por que essa modelagem atende DDD?

1. **Agregados Claros**
   - `User` e `Event` são agregados raiz
   - `EventParticipants` é parte do agregado `Event`
   - Limites de agregado respeitados

2. **Value Objects Preservados**
   - Email, EventName, etc. são conceitos do domínio
   - Armazenados como colunas simples (persistência)
   - **Validação 100% no Domain** (sem CHECKs no banco)
   - Banco apenas armazena, Domain valida

3. **Enums do Domain**
   - EventType é enum no Domain
   - Banco armazena como INTEGER (representação)
   - Sem acoplamento

### Como suporta CQRS?

1. **Separação de Leitura/Escrita**
   - Tabelas base para escrita (commands)
   - View otimizada para leitura (queries)
   - Índices específicos para cada operação

2. **Performance de Queries**
   - Índices compostos para filtros de dashboard
   - Índice GIN para busca full-text
   - View pré-agregada reduz JOINs em runtime

3. **Escritas Não Bloqueadas**
   - Índices não interferem em INSERT/UPDATE
   - CASCADE automático (sem lógica extra)
   - Transações simples

### Como o dashboard será eficiente?

1. **Índices Estratégicos**
   - `idx_events_dashboard_filter`: filtro mais comum (creator + active + date)
   - `idx_events_date_active`: ordenação por data
   - `idx_events_search_text`: busca full-text

2. **View Otimizada**
   - `dashboard_events_view`: pré-agrega participantes
   - Reduz múltiplos JOINs em cada query
   - JSON aggregation eficiente

3. **Filtros Suportados**
   - Data: índice em `date`
   - Texto: índice GIN full-text
   - Dia/Semana/Mês: range queries em `date`
   - Ativo: filtro em `is_active`

### Como evita acoplamento?

1. **Banco é Infraestrutura Pura**
   - Nenhuma regra de negócio no banco
   - Apenas constraints de integridade referencial (FKs, UNIQUE)
   - **Sem CHECKs de validação** (validação 100% no Domain: `Email.Create()`, `EventName.Create()`, etc.)
   - **Sem funções de autorização** (lógica no Application layer: handlers verificam `CanUserView()`, `CanUserEdit()`)
   - RLS opcional (apenas se Supabase Auth for usado)

2. **Domain Independente**
   - Domain não conhece estrutura do banco
   - Value Objects são conceitos puros
   - Enums são tipos do Domain

3. **EF Core como Mapeador**
   - `AppDbContext` mapeia Domain → Database
   - Infrastructure conhece ambos
   - Application conhece apenas Domain

---

## 7. CHECKLIST DE IMPLEMENTAÇÃO

- [ ] Executar SQL no Supabase SQL Editor
- [ ] Configurar connection string no `appsettings.json`
- [ ] Atualizar `AppDbContext` com mapeamento de Value Objects
- [ ] Criar migration inicial (ou usar SQL direto)
- [ ] Testar queries de dashboard
- [ ] Configurar RLS (se usar Supabase Auth) ou desabilitar (se usar JWT próprio - RECOMENDADO)
- [ ] Validar performance com dados de teste
- [ ] Documentar connection string para equipe

---

**Documento criado seguindo princípios de Clean Architecture, DDD e CQRS.**
**Pronto para implementação em Supabase (PostgreSQL).**

