using AthensSecret.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AthensSecret.Api.Data;

public class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
    {
    }

    public DbSet<Player> Players { get; set; }
    public DbSet<Response> Responses { get; set; }
    public DbSet<GameConfiguration> GameConfigurations { get; set; }
    public DbSet<GameSession> GameSessions { get; set; }
    public DbSet<MirrorsTrial> MirrorsTrials { get; set; }
    public DbSet<OilTreeTrial> OilTreeTrials { get; set; }
    public DbSet<PathTrial> PathTrials { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Player
        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(p => p.LastName).HasMaxLength(100).IsRequired();
            entity.Property(p => p.Email).HasMaxLength(255).IsRequired();
            entity.HasIndex(p => p.Email).IsUnique();
        });

        // Configure Response (One-to-One with Player)
        modelBuilder.Entity<Response>(entity =>
        {
            entity.HasKey(r => r.PlayerId);
            entity.HasOne(r => r.Player)
                  .WithOne(p => p.Response)
                  .HasForeignKey<Response>(r => r.PlayerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure GameConfiguration (Standalone - no relationships)
        modelBuilder.Entity<GameConfiguration>(entity =>
        {
            entity.HasKey(gc => gc.Id);
            entity.Property(gc => gc.OilTreeWisdomInvestmentFunction).HasMaxLength(20).IsRequired();
        });

        // Configure GameSession
        modelBuilder.Entity<GameSession>(entity =>
        {
            entity.HasKey(gs => gs.Id);
            
            // Foreign key relationship - only to Player
            entity.HasOne(gs => gs.Player)
                  .WithMany(p => p.GameSessions)
                  .HasForeignKey(gs => gs.PlayerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure MirrorsTrial
        modelBuilder.Entity<MirrorsTrial>(entity =>
        {
            entity.HasKey(mt => mt.GameSessionId);
            entity.HasOne(mt => mt.GameSession)
                  .WithOne(gs => gs.MirrorsTrial)
                  .HasForeignKey<MirrorsTrial>(mt => mt.GameSessionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure OilTreeTrial
        modelBuilder.Entity<OilTreeTrial>(entity =>
        {
            entity.HasKey(ott => ott.GameSessionId);
            entity.HasOne(ott => ott.GameSession)
                  .WithOne(gs => gs.OilTreeTrial)
                  .HasForeignKey<OilTreeTrial>(ott => ott.GameSessionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure PathTrial
        modelBuilder.Entity<PathTrial>(entity =>
        {
            entity.HasKey(pt => pt.GameSessionId);
            entity.HasOne(pt => pt.GameSession)
                  .WithOne(gs => gs.PathTrial)
                  .HasForeignKey<PathTrial>(pt => pt.GameSessionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
