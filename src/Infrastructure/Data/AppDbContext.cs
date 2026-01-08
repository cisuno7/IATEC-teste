using AgendaManager.Domain.Entities;
using AgendaManager.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace AgendaManager.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Event> Events { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(u => u.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
            
            entity.OwnsOne(u => u.Email, email =>
            {
                email.Property(e => e.Value)
                    .HasColumnName("email")
                    .IsRequired()
                    .HasMaxLength(150);

                email.HasIndex(e => e.Value)
                    .IsUnique()
                    .HasDatabaseName("idx_users_email");
            });
            
            entity.Property(u => u.PasswordHash).HasColumnName("password_hash").IsRequired();
            entity.Property(u => u.CreatedAt).HasColumnName("created_at").IsRequired().HasDefaultValueSql("now()");
            entity.Property(u => u.IsActive).HasColumnName("is_active").IsRequired().HasDefaultValue(true);
            
            entity.HasIndex(u => u.IsActive).HasDatabaseName("idx_users_is_active");
            
            entity.HasMany(u => u.CreatedEvents)
                  .WithOne(e => e.Creator)
                  .HasForeignKey(e => e.CreatorId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasMany(u => u.ParticipatedEvents)
                  .WithMany(e => e.Participants)
                  .UsingEntity<Dictionary<string, object>>(
                      "event_participants",
                      j => j.HasOne<Event>().WithMany().HasForeignKey("event_id").OnDelete(DeleteBehavior.Cascade),
                      j => j.HasOne<User>().WithMany().HasForeignKey("user_id").OnDelete(DeleteBehavior.Cascade),
                      j =>
                      {
                          j.HasKey("event_id", "user_id");
                          j.ToTable("event_participants");
                          j.Property<DateTime>("created_at").HasColumnName("created_at").HasDefaultValueSql("now()");
                      });
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("events");
            
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            
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
            
            entity.Property(e => e.Date).HasColumnName("date").IsRequired();
            entity.Property(e => e.Type).HasColumnName("type").IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("is_active").IsRequired().HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired().HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.CreatorId).HasColumnName("creator_id").IsRequired();
            
            entity.HasOne(e => e.Creator)
                  .WithMany(u => u.CreatedEvents)
                  .HasForeignKey(e => e.CreatorId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasIndex(e => e.CreatorId).HasDatabaseName("idx_events_creator_id");
            entity.HasIndex(e => e.Date).HasDatabaseName("idx_events_date");
            entity.HasIndex(e => e.Type).HasDatabaseName("idx_events_type");
            entity.HasIndex(e => e.IsActive).HasDatabaseName("idx_events_is_active");
            entity.HasIndex(e => new { e.CreatorId, e.IsActive }).HasDatabaseName("idx_events_creator_active");
            entity.HasIndex(e => new { e.Date, e.IsActive }).HasDatabaseName("idx_events_date_active");
            
            entity.HasIndex(e => new { e.CreatorId, e.IsActive, e.Date })
                  .HasDatabaseName("idx_events_dashboard_filter")
                  .HasFilter("\"is_active\" = true");
        });
    }
}
