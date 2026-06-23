using LLeague.Api.Application.Abstractions;
using LLeague.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace LLeague.Api.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    // One DbSet per table.
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<Division> Divisions => Set<Division>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<MatchParticipant> MatchParticipants => Set<MatchParticipant>();
    public DbSet<MissionValue> MissionValues => Set<MissionValue>();
    public DbSet<RobotGameTable> Tables => Set<RobotGameTable>();
    public DbSet<Scoresheet> Scoresheets => Set<Scoresheet>();
    public DbSet<Season> Seasons => Set<Season>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamDivision> TeamDivisions => Set<TeamDivision>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // --- Store enums as strings (readable in the DB, not 0/1/2) ---
        _ = modelBuilder.Entity<Match>()
            .Property(m => m.Stage)
            .HasConversion<string>()
            .HasMaxLength(16);
        _ = modelBuilder.Entity<Match>()
            .Property(m => m.Status)
            .HasConversion<string>()
            .HasMaxLength(16);
        _ = modelBuilder.Entity<Scoresheet>()
            .Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(16);

        // --- Season -> Events ---
        _ = modelBuilder.Entity<Event>()
            .HasOne(e => e.Season).WithMany(s => s.Events)
            .HasForeignKey(e => e.SeasonId)
            .OnDelete(DeleteBehavior.Cascade);

        // --- Event -> Divisions ---
        _ = modelBuilder.Entity<Division>()
            .HasOne(d => d.Event).WithMany(e => e.Divisions)
            .HasForeignKey(d => d.EventId).OnDelete(DeleteBehavior.Cascade);

        // --- Team: unique on (Number, Region); Region is 2 chars ---
        _ = modelBuilder.Entity<Team>().HasIndex(t => new { t.Number, t.Region }).IsUnique();
        _ = modelBuilder.Entity<Team>().Property(t => t.Region).HasMaxLength(2);

        // --- TeamDivision: a team is enrolled once per division ---
        _ = modelBuilder.Entity<TeamDivision>().HasIndex(td => new { td.TeamId, td.DivisionId }).IsUnique();
        _ = modelBuilder.Entity<TeamDivision>()
            .HasOne(td => td.Division).WithMany(d => d.TeamDivisions)
            .HasForeignKey(td => td.DivisionId).OnDelete(DeleteBehavior.Cascade);
        _ = modelBuilder.Entity<TeamDivision>()
            .HasOne(td => td.Team).WithMany(t => t.TeamDivisions)
            .HasForeignKey(td => td.TeamId).OnDelete(DeleteBehavior.Cascade);

        // --- Division -> Tables / Matches ---
        _ = modelBuilder.Entity<RobotGameTable>()
            .HasOne(t => t.Division).WithMany(d => d.Tables)
            .HasForeignKey(t => t.DivisionId).OnDelete(DeleteBehavior.Cascade);
        _ = modelBuilder.Entity<Match>()
            .HasOne(m => m.Division).WithMany(d => d.Matches)
            .HasForeignKey(m => m.DivisionId).OnDelete(DeleteBehavior.Cascade);

        // --- Match -> Participants; participant -> Table/Team (restrict so we don't delete history) ---
        _ = modelBuilder.Entity<MatchParticipant>()
            .HasOne(p => p.Match).WithMany(m => m.Participants)
            .HasForeignKey(p => p.MatchId).OnDelete(DeleteBehavior.Cascade);
        _ = modelBuilder.Entity<MatchParticipant>()
            .HasOne(p => p.Table).WithMany()
            .HasForeignKey(p => p.TableId).OnDelete(DeleteBehavior.Restrict);
        _ = modelBuilder.Entity<MatchParticipant>()
            .HasOne(p => p.Team).WithMany()
            .HasForeignKey(p => p.TeamId).OnDelete(DeleteBehavior.Restrict);

        // --- MatchParticipant -> Scoresheet (one-to-one) ---
        _ = modelBuilder.Entity<Scoresheet>()
            .HasOne(s => s.MatchParticipant).WithOne(p => p.Scoresheet)
            .HasForeignKey<Scoresheet>(s => s.MatchParticipantId).OnDelete(DeleteBehavior.Cascade);
        _ = modelBuilder.Entity<Scoresheet>().HasIndex(s => s.MatchParticipantId).IsUnique();

        // --- Scoresheet -> MissionValues ---
        _ = modelBuilder.Entity<MissionValue>()
            .HasOne(mv => mv.Scoresheet).WithMany(s => s.Missions)
            .HasForeignKey(mv => mv.ScoresheetId)
            .OnDelete(DeleteBehavior.Cascade);

        _ = modelBuilder.Entity<AdminUser>().HasIndex(a => a.Username).IsUnique();
    }
}
