using Cost;
using InfosecAcademyBudgetManagement.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InfosecAcademyBudgetManagement.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<CostItem> CostItems { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<BudgetPlan> BudgetPlans { get; set; }
        public DbSet<BudgetLine> BudgetLines { get; set; }
        public DbSet<EmailAccountSetting> EmailAccountSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.FirstName).HasMaxLength(50).IsRequired();
                entity.Property(u => u.LastName).HasMaxLength(50).IsRequired();
                entity.Property(u => u.Department).HasMaxLength(100);
            });

            modelBuilder.Entity<EmailAccountSetting>(entity =>
            {
                entity.Property(e => e.SmtpHost).HasMaxLength(200).IsRequired();
                entity.Property(e => e.SenderEmail).HasMaxLength(256).IsRequired();
                entity.Property(e => e.SenderName).HasMaxLength(100);
                entity.Property(e => e.Username).HasMaxLength(256).IsRequired();
                entity.Property(e => e.EncryptedPassword).IsRequired();
            });

            modelBuilder.Entity<CostItem>()
                .Property(c => c.CategoryId)
                .HasDefaultValue(5);

            modelBuilder.Entity<BudgetLine>()
                .HasIndex(b => new { b.BudgetPlanId, b.CategoryId, b.Month })
                .IsUnique();

            var seedTime = new DateTime(2026, 02, 06, 0, 0, 0, DateTimeKind.Utc);
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Fixed", CreatedAt = seedTime, UpdatedAt = seedTime, IsDeleted = false },
                new Category { Id = 2, Name = "Utilities", CreatedAt = seedTime, UpdatedAt = seedTime, IsDeleted = false },
                new Category { Id = 3, Name = "Office", CreatedAt = seedTime, UpdatedAt = seedTime, IsDeleted = false },
                new Category { Id = 4, Name = "Travel", CreatedAt = seedTime, UpdatedAt = seedTime, IsDeleted = false },
                new Category { Id = 5, Name = "Other", CreatedAt = seedTime, UpdatedAt = seedTime, IsDeleted = false }
            );
        }
    }
}
