using AthensSecret.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AthensSecret.Api.Data;

public class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
    {
    }

    public DbSet<Player> Players { get; set; }
    public DbSet<GameSession> GameSessions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Player
        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Username).HasMaxLength(100).IsRequired();
            entity.HasIndex(p => p.Username).IsUnique();
        });

        // Configure GameSession
        modelBuilder.Entity<GameSession>(entity =>
        {
            entity.HasKey(gs => gs.Id);
            entity.Property(gs => gs.CurrentTrial).HasMaxLength(50);
            
            // Foreign key relationship
            entity.HasOne(gs => gs.Player)
                  .WithMany(p => p.GameSessions)
                  .HasForeignKey(gs => gs.PlayerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
