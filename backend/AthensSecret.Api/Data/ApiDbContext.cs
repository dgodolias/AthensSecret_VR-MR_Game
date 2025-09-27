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
    public DbSet<OliveTreeTrial> OliveTreeTrials { get; set; }
    public DbSet<PathTrial> PathTrials { get; set; }
    public DbSet<ResponsesStatistics> ResponsesStatistics { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Player - map to PLAYER table
        modelBuilder.Entity<Player>(entity =>
        {
            entity.ToTable("player");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).HasColumnName("id");
            entity.Property(p => p.FirstName).HasColumnName("first_name").HasMaxLength(100).IsRequired();
            entity.Property(p => p.LastName).HasColumnName("last_name").HasMaxLength(100).IsRequired();
            entity.Property(p => p.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            entity.Property(p => p.Age).HasColumnName("age").IsRequired();
            entity.HasIndex(p => p.Email).IsUnique();
        });

        // Configure Response (One-to-One with Player) - map to responses table
        modelBuilder.Entity<Response>(entity =>
        {
            entity.ToTable("responses");
            entity.HasKey(r => r.PlayerId);
            entity.Property(r => r.PlayerId).HasColumnName("player_id");
            entity.Property(r => r.Q1).HasColumnName("q1");  // Patience
            entity.Property(r => r.Q2).HasColumnName("q2");  // Risk tolerance
            entity.HasOne(r => r.Player)
                  .WithOne(p => p.Response)
                  .HasForeignKey<Response>(r => r.PlayerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure GameConfiguration (Standalone - no relationships) - map to game_configuration table
        modelBuilder.Entity<GameConfiguration>(entity =>
        {
            entity.ToTable("game_configuration");
            entity.HasKey(gc => gc.Id);
            entity.Property(gc => gc.Id).HasColumnName("id");
            entity.Property(gc => gc.StartingWisdom).HasColumnName("startingwisdom");
            entity.Property(gc => gc.MirrorWisdomIfWaits).HasColumnName("mirrorwisdomifwaits");
            entity.Property(gc => gc.MirrorWisdomIfRisksCorrectly).HasColumnName("mirrorwisdomifriskscorrectly");
            entity.Property(gc => gc.MirrorWisdomIfRisksFalsely).HasColumnName("mirrorwisdomifrisksfalsely");
            entity.Property(gc => gc.OliveTreeWisdomNotInvestment).HasColumnName("olivetreewisdomnotinvestment");
            entity.Property(gc => gc.OliveTreeWisdomInvestmentFunction).HasColumnName("olivetreewisdominvestmentfunction").HasMaxLength(20).IsRequired();
            entity.Property(gc => gc.SafePathWisdom).HasColumnName("safepathwisdom");
            entity.Property(gc => gc.UncertainPathWisdom).HasColumnName("uncertainpathwisdom");
            entity.Property(gc => gc.UncertainPathWisdomSmallPlank).HasColumnName("uncertainpathwisdomsmallplank");
            entity.Property(gc => gc.UncertainPathWisdomMediumPlank).HasColumnName("uncertainpathwisdommediumplank");
            entity.Property(gc => gc.UncertainPathWisdomBigPlank).HasColumnName("uncertainpathwisdombigplank");
        });

        // Configure GameSession - map to game_sessions table
        modelBuilder.Entity<GameSession>(entity =>
        {
            entity.ToTable("game_sessions");
            entity.HasKey(gs => gs.Id);
            entity.Property(gs => gs.Id).HasColumnName("id");
            entity.Property(gs => gs.PlayerId).HasColumnName("player_id");
            entity.Property(gs => gs.StartedAt).HasColumnName("started_at");
            entity.Property(gs => gs.EndedAt).HasColumnName("ended_at");
            
            // Foreign key relationship - only to Player
            entity.HasOne(gs => gs.Player)
                  .WithMany(p => p.GameSessions)
                  .HasForeignKey(gs => gs.PlayerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure MirrorsTrial - map to mirrors_trial table
        modelBuilder.Entity<MirrorsTrial>(entity =>
        {
            entity.ToTable("mirrors_trial");
            entity.HasKey(mt => mt.GameSessionId);
            entity.Property(mt => mt.GameSessionId).HasColumnName("game_session_id");
            entity.Property(mt => mt.StartTime).HasColumnName("start_time");
            entity.Property(mt => mt.EndTime).HasColumnName("end_time");
            entity.Property(mt => mt.TotalGainedWisdom).HasColumnName("total_gained_wisdom");
            entity.HasOne(mt => mt.GameSession)
                  .WithOne(gs => gs.MirrorsTrial)
                  .HasForeignKey<MirrorsTrial>(mt => mt.GameSessionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure OliveTreeTrial - map to olive_tree_trial table
        modelBuilder.Entity<OliveTreeTrial>(entity =>
        {
            entity.ToTable("olive_tree_trial");
            entity.HasKey(ott => ott.GameSessionId);
            entity.Property(ott => ott.GameSessionId).HasColumnName("game_session_id");
            entity.Property(ott => ott.StartTime).HasColumnName("start_time");
            entity.Property(ott => ott.EndTime).HasColumnName("end_time");
            entity.Property(ott => ott.TotalGainedWisdom).HasColumnName("total_gained_wisdom");
            entity.Property(ott => ott.InvestmentStartTime).HasColumnName("investment_start_time");
            entity.HasOne(ott => ott.GameSession)
                  .WithOne(gs => gs.OliveTreeTrial)
                  .HasForeignKey<OliveTreeTrial>(ott => ott.GameSessionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure PathTrial - map to path_trial table
        modelBuilder.Entity<PathTrial>(entity =>
        {
            entity.ToTable("path_trial");
            entity.HasKey(pt => pt.GameSessionId);
            entity.Property(pt => pt.GameSessionId).HasColumnName("game_session_id");
            entity.Property(pt => pt.StartTime).HasColumnName("start_time");
            entity.Property(pt => pt.EndTime).HasColumnName("end_time");
            entity.Property(pt => pt.TotalGainedWisdom).HasColumnName("total_gained_wisdom");
            entity.Property(pt => pt.SafePath).HasColumnName("safe_path");
            entity.HasOne(pt => pt.GameSession)
                  .WithOne(gs => gs.PathTrial)
                  .HasForeignKey<PathTrial>(pt => pt.GameSessionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure ResponsesStatistics - map to responses_statistics table
        modelBuilder.Entity<ResponsesStatistics>(entity =>
        {
            entity.ToTable("responses_statistics");
            entity.HasKey(rs => rs.Age);
            entity.Property(rs => rs.Age).HasColumnName("age");
            entity.Property(rs => rs.Patience).HasColumnName("patience").IsRequired();
            entity.Property(rs => rs.Risk).HasColumnName("risk").IsRequired();
        });
    }
}
