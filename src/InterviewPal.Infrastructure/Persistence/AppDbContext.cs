using InterviewPal.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace InterviewPal.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Technology> Technologies => Set<Technology>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Choice> Choices => Set<Choice>();
    public DbSet<QuestionReport> QuestionReports => Set<QuestionReport>();

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PracticeHistoryEntry> PracticeHistory => Set<PracticeHistoryEntry>();
    public DbSet<QuestionStat> QuestionStats => Set<QuestionStat>();
    public DbSet<LessonExerciseProgress> LessonProgress => Set<LessonExerciseProgress>();

    /// <summary>Every DateTime is UTC. SQLite hands back "unspecified" kinds and PostgreSQL refuses to write them.</summary>
    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        builder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Technology>(e =>
        {
            e.HasKey(t => t.Slug);
            e.Property(t => t.Slug).HasMaxLength(50);
            e.Property(t => t.Name).HasMaxLength(100);
            e.Property(t => t.CurrentVersion).HasMaxLength(20);
            e.Property(t => t.SupportedFrom).HasMaxLength(20);
        });

        b.Entity<Question>(e =>
        {
            e.HasKey(q => q.Id);
            e.Property(q => q.Id).HasMaxLength(120);
            e.Property(q => q.TechnologySlug).HasMaxLength(50);
            e.Property(q => q.MinVersion).HasMaxLength(20);
            e.Property(q => q.MaxVersion).HasMaxLength(20);
            e.Property(q => q.ContentHash).HasMaxLength(64);
            e.HasOne(q => q.Technology).WithMany(t => t.Questions)
                .HasForeignKey(q => q.TechnologySlug).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(q => q.Choices).WithOne(c => c.Question)
                .HasForeignKey(c => c.QuestionId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(q => new { q.TechnologySlug, q.Level });
        });

        b.Entity<QuestionReport>(e =>
        {
            e.HasOne(r => r.Question).WithMany()
                .HasForeignKey(r => r.QuestionId).OnDelete(DeleteBehavior.Cascade);
            e.Property(r => r.Message).HasMaxLength(1000);
        });

        b.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Email).HasMaxLength(254);
            e.Property(u => u.DisplayName).HasMaxLength(40);
            e.Property(u => u.PasswordHash).HasMaxLength(256);
            e.HasIndex(u => u.Email).IsUnique();
            e.HasMany(u => u.RefreshTokens).WithOne(t => t.User)
                .HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<RefreshToken>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.TokenHash).HasMaxLength(64);
            e.HasIndex(t => t.TokenHash).IsUnique();
        });

        // Progress belongs to the user: deleting the account deletes it too.
        b.Entity<PracticeHistoryEntry>(e =>
        {
            e.HasKey(h => h.Id);
            e.HasOne<User>().WithMany().HasForeignKey(h => h.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(h => new { h.UserId, h.At });
        });

        b.Entity<QuestionStat>(e =>
        {
            e.HasKey(s => new { s.UserId, s.QuestionId });
            e.Property(s => s.QuestionId).HasMaxLength(120);
            e.HasOne<User>().WithMany().HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<LessonExerciseProgress>(e =>
        {
            e.HasKey(p => new { p.UserId, p.LessonId, p.ExerciseId });
            e.Property(p => p.LessonId).HasMaxLength(100);
            e.Property(p => p.ExerciseId).HasMaxLength(50);
            e.HasOne<User>().WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}

public class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
    v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
