using Microsoft.EntityFrameworkCore;
using TrafficLaw.API.Models;

namespace TrafficLaw.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TrafficViolation> TrafficViolations { get; set; }
        public DbSet<LegalCitation> LegalCitations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TrafficViolation>()
                .HasIndex(v => v.QaId)
                .IsUnique();

            modelBuilder.Entity<TrafficViolation>()
                .HasIndex(v => v.VehicleType)
                .HasDatabaseName("IX_TrafficViolations_VehicleType");

            modelBuilder.Entity<LegalCitation>()
                .HasIndex(c => c.ViolationId)
                .HasDatabaseName("IX_LegalCitations_ViolationId");

            modelBuilder.Entity<LegalCitation>()
                .HasOne(c => c.Violation)
                .WithMany(v => v.Citations)
                .HasForeignKey(c => c.ViolationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}