# Integração Supabase + .NET 10 + Supabase Auth
## Guia Completo para Agenda Manager

---

## VISÃO GERAL

Este documento explica como integrar o Supabase (PostgreSQL + Auth) com o projeto .NET 10, mantendo Clean Architecture e DDD.

**Duas abordagens possíveis:**

1. **Supabase Auth + RLS** (mais simples, menos controle)
2. **JWT próprio + EF Core direto** (mais controle, Clean Architecture pura)

---

## OPÇÃO 1: SUPABASE AUTH + RLS (Recomendado para MVP)

### Vantagens
- Autenticação pronta (email/senha, Google, GitHub, etc.)
- RLS gerencia segurança automaticamente
- Menos código no backend
- Supabase Dashboard para gerenciar usuários

### Desvantagens
- Acoplamento com Supabase
- Menos controle sobre regras de negócio
- Domain não é 100% puro (depende de `auth.uid()`)

---

## PASSO 1: INSTALAR PACOTES NUGET

```bash
# Cliente Supabase para .NET
dotnet add package supabase-csharp

# Autenticação JWT (para validar tokens do Supabase)
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer

# Entity Framework Core (para acesso direto ao banco)
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

---

## PASSO 2: CONFIGURAR APPSETTINGS.JSON

```json
{
  "Supabase": {
    "Url": "https://seu-projeto.supabase.co",
    "Key": "sua-anon-key",
    "JwtSecret": "seu-jwt-secret"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=db.seu-projeto.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=sua-senha;SSL Mode=Require;Trust Server Certificate=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

**Onde encontrar:**
- **Url**: Supabase Dashboard → Settings → API → Project URL
- **Key**: Supabase Dashboard → Settings → API → `anon` `public` key
- **JwtSecret**: Supabase Dashboard → Settings → API → JWT Secret
- **Password**: Supabase Dashboard → Settings → Database → Connection string

---

## PASSO 3: CONFIGURAR PROGRAM.CS

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Supabase;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuração Supabase
var supabaseUrl = builder.Configuration["Supabase:Url"];
var supabaseKey = builder.Configuration["Supabase:Key"];
var jwtSecret = builder.Configuration["Supabase:JwtSecret"];

// Cliente Supabase (Singleton)
builder.Services.AddSingleton(provider =>
{
    var options = new SupabaseOptions
    {
        AutoConnectRealtime = false
    };
    var client = new Supabase.Client(supabaseUrl, supabaseKey, options);
    client.InitializeAsync().Wait();
    return client;
});

// Entity Framework Core (acesso direto ao PostgreSQL)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Autenticação JWT (validar tokens do Supabase)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = supabaseUrl,
            ValidAudience = "authenticated",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

// MediatR, Repositories, etc.
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateEventCommandHandler).Assembly);
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

---

## PASSO 4: CRIAR AUTHCONTROLLER COM SUPABASE AUTH

```csharp
using Microsoft.AspNetCore.Mvc;
using Supabase;

namespace AgendaManager.Api.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly Supabase.Client _supabase;

    public AuthController(Supabase.Client supabase)
    {
        _supabase = supabase;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        try
        {
            var response = await _supabase.Auth.SignUp(dto.Email, dto.Password);
            
            if (response?.User == null)
                return BadRequest("Registration failed");

            return Ok(new
            {
                user = response.User,
                session = response.Session
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            var response = await _supabase.Auth.SignIn(dto.Email, dto.Password);
            
            if (response?.Session == null)
                return Unauthorized("Invalid credentials");

            return Ok(new
            {
                token = response.Session.AccessToken,
                refreshToken = response.Session.RefreshToken,
                expiresAt = response.Session.ExpiresAt,
                user = response.User
            });
        }
        catch (Exception ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _supabase.Auth.SignOut();
        return Ok();
    }

    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        var user = _supabase.Auth.CurrentUser;
        if (user == null)
            return Unauthorized();

        return Ok(user);
    }
}

public record RegisterDto(string Email, string Password, string Name);
public record LoginDto(string Email, string Password);
```

---

## PASSO 5: ATUALIZAR DOMAIN ENTITIES PARA UUID

**User.cs:**
```csharp
public class User
{
    public Guid Id { get; private set; }  // int → Guid
    public string Name { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; }
    public ICollection<Event> CreatedEvents { get; private set; } = null!;
    public ICollection<Event> ParticipatedEvents { get; private set; } = null!;

    private User() { }

    public static User Create(Guid id, string name, Email email)
    {
        return new User
        {
            Id = id,
            Name = name.Trim(),
            Email = email,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedEvents = new List<Event>(),
            ParticipatedEvents = new List<Event>()
        };
    }
}
```

**Event.cs:**
```csharp
public class Event
{
    public Guid Id { get; private set; }  // int → Guid
    public EventName Name { get; private set; } = null!;
    public EventDescription Description { get; private set; } = null!;
    public DateTime Date { get; private set; }
    public Location Location { get; private set; } = null!;
    public EventType Type { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid CreatorId { get; private set; }  // int → Guid
    public User Creator { get; private set; } = null!;
    public ICollection<User> Participants { get; private set; } = null!;

    private Event() { }

    public static Event Create(string name, string description, DateTime date, string location, EventType type, Guid creatorId)
    {
        return new Event
        {
            Id = Guid.NewGuid(),
            Name = EventName.Create(name),
            Description = EventDescription.Create(description),
            Date = date,
            Location = Location.Create(location),
            Type = type,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatorId = creatorId,
            Participants = new List<User>()
        };
    }
}
```

---

## PASSO 6: ATUALIZAR APPDBCONTEXT

```csharp
using AgendaManager.Domain.Entities;
using AgendaManager.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace AgendaManager.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Event> Events { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(u => u.Name).IsRequired().HasMaxLength(100);
            
            entity.OwnsOne(u => u.Email, email =>
            {
                email.Property(e => e.Value).HasColumnName("Email").IsRequired().HasMaxLength(150);
            });
            
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.CreatedAt).IsRequired();
            entity.Property(u => u.IsActive).IsRequired();
            
            entity.HasMany(u => u.CreatedEvents)
                  .WithOne(e => e.Creator)
                  .HasForeignKey(e => e.CreatorId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasMany(u => u.ParticipatedEvents)
                  .WithMany(e => e.Participants)
                  .UsingEntity<Dictionary<string, object>>(
                      "event_participants",
                      j => j.HasOne<User>().WithMany().HasForeignKey("user_id"),
                      j => j.HasOne<Event>().WithMany().HasForeignKey("event_id"));
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            
            entity.OwnsOne(e => e.Name, name =>
            {
                name.Property(n => n.Value).HasColumnName("name").IsRequired().HasMaxLength(200);
            });
            
            entity.OwnsOne(e => e.Description, description =>
            {
                description.Property(d => d.Value).HasColumnName("description").HasMaxLength(1000);
            });
            
            entity.OwnsOne(e => e.Location, location =>
            {
                location.Property(l => l.Value).HasColumnName("location").HasMaxLength(300);
            });
            
            entity.Property(e => e.Date).IsRequired().HasColumnName("date");
            entity.Property(e => e.Type).IsRequired().HasColumnName("type");
            entity.Property(e => e.IsActive).IsRequired().HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).IsRequired().HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.CreatorId).IsRequired().HasColumnName("creator_id");
        });
    }
}
```

---

## PASSO 7: ATUALIZAR HANDLERS PARA USAR GUID

**CreateEventCommandHandler.cs:**
```csharp
public async Task<EventDto> Handle(CreateEventCommand request, CancellationToken cancellationToken)
{
    var userId = GetCurrentUserId(); // Agora retorna Guid
    
    var eventEntity = Event.Create(
        request.Dto.Name,
        request.Dto.Description,
        request.Dto.Date,
        request.Dto.Location,
        request.Dto.Type,
        userId  // Guid
    );

    await _unitOfWork.Events.AddAsync(eventEntity);
    await _unitOfWork.SaveChangesAsync();

    return MapToDto(eventEntity);
}

private Guid GetCurrentUserId()
{
    var userIdClaim = _httpContextAccessor.HttpContext?.User
        .FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        throw new UnauthorizedAccessException("Invalid user token");

    return userId;
}
```

---

## PASSO 8: FRONTEND (ANGULAR) - CONSUMIR SUPABASE AUTH

**auth.service.ts:**
```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export interface AuthResponse {
  token: string;
  refreshToken: string;
  expiresAt: number;
  user: {
    id: string;
    email: string;
    name: string;
  };
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject = new BehaviorSubject<any>(null);
  public currentUser = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    const token = localStorage.getItem('token');
    if (token) {
      this.loadCurrentUser();
    }
  }

  register(email: string, password: string, name: string): Observable<any> {
    return this.http.post(`${environment.apiUrl}/api/v1/auth/register`, {
      email,
      password,
      name
    });
  }

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${environment.apiUrl}/api/v1/auth/login`, {
      email,
      password
    }).pipe(
      tap(response => {
        localStorage.setItem('token', response.token);
        localStorage.setItem('refreshToken', response.refreshToken);
        this.currentUserSubject.next(response.user);
      })
    );
  }

  logout(): Observable<any> {
    return this.http.post(`${environment.apiUrl}/api/v1/auth/logout`, {}).pipe(
      tap(() => {
        localStorage.removeItem('token');
        localStorage.removeItem('refreshToken');
        this.currentUserSubject.next(null);
      })
    );
  }

  private loadCurrentUser(): void {
    this.http.get(`${environment.apiUrl}/api/v1/auth/me`).subscribe({
      next: (user) => this.currentUserSubject.next(user),
      error: () => {
        localStorage.removeItem('token');
        localStorage.removeItem('refreshToken');
      }
    });
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }
}
```

---

## OPÇÃO 2: JWT PRÓPRIO (SEM SUPABASE AUTH)

Se preferir **não usar Supabase Auth** e manter JWT próprio:

1. **Não instale `supabase-csharp`**
2. **Use apenas EF Core** para acesso ao banco
3. **Mantenha o `AuthController` atual** (com hash de senha próprio)
4. **Desabilite RLS no Supabase:**
   ```sql
   ALTER TABLE users DISABLE ROW LEVEL SECURITY;
   ALTER TABLE events DISABLE ROW LEVEL SECURITY;
   ALTER TABLE event_participants DISABLE ROW LEVEL SECURITY;
   ```
5. **Segurança 100% no Application layer** (handlers verificam permissões)

---

## COMPARAÇÃO: SUPABASE AUTH vs JWT PRÓPRIO

| Critério | Supabase Auth | JWT Próprio |
|----------|---------------|-------------|
| Complexidade | Baixa | Média |
| Controle | Médio | Alto |
| Clean Architecture | Parcial | Total |
| RLS | Sim | Não |
| Provedores externos | Sim (Google, GitHub) | Não (precisa implementar) |
| Gerenciamento de usuários | Dashboard Supabase | Código próprio |
| Recomendado para | MVP, prototipagem | Produção, entrevista técnica |

---

## CHECKLIST DE IMPLEMENTAÇÃO

- [ ] Instalar pacotes NuGet (`supabase-csharp` ou apenas `Npgsql.EntityFrameworkCore.PostgreSQL`)
- [ ] Configurar `appsettings.json` com credenciais do Supabase
- [ ] Atualizar `Program.cs` com autenticação JWT
- [ ] Atualizar Domain entities: `int` → `Guid`
- [ ] Atualizar `AppDbContext` com mapeamento de UUIDs
- [ ] Atualizar handlers para usar `Guid`
- [ ] Atualizar repositories para usar `Guid`
- [ ] Criar/atualizar `AuthController`
- [ ] Testar registro e login
- [ ] Configurar RLS (se usar Supabase Auth) ou desabilitar (se usar JWT próprio)
- [ ] Atualizar frontend Angular para consumir nova API

---

**Documento criado para integração Supabase + .NET 10 + Clean Architecture.**

