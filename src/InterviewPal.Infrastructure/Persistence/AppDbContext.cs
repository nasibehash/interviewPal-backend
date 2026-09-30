using InterviewPal.Domain;
using Microsoft.EntityFrameworkCore;

namespace InterviewPal.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Technology> Technologies => Set<Technology>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Choice> Choices => Set<Choice>();
    public DbSet<QuestionReport> QuestionReports => Set<QuestionReport>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Technology>(e =>
        {
            e.HasKey(t => t.Slug);
            e.Property(t => t.Slug).HasMaxLength(50);
            e.Property(t => t.Name).HasMaxLength(100);
            e.Property(t => t.CurrentVersion).HasMaxLength(20);
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
    }
}
