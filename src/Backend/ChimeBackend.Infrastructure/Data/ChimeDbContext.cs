using ChimeBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChimeBackend.Infrastructure.Data;

public class ChimeDbContext : DbContext
{
    public ChimeDbContext(DbContextOptions<ChimeDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<FoodRecord> FoodRecords { get; set; } = null!;
    public DbSet<DailySummary> DailySummaries { get; set; } = null!;
    public DbSet<CustomFood> CustomFoods { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.Gender).HasConversion<int>();
            entity.Property(e => e.VersionMode).HasConversion<int?>();
            entity.Property(e => e.ActivityLevel).HasConversion<int?>();
            entity.Property(e => e.Goal).HasConversion<int?>();
            entity.Property(e => e.Status).HasConversion<int>();
            entity.HasIndex(e => e.OpenId).IsUnique();
            entity.HasIndex(e => e.PhoneNumber).IsUnique();
            entity.HasIndex(e => e.UnionId);
        });

        // FoodRecord
        modelBuilder.Entity<FoodRecord>(entity =>
        {
            entity.ToTable("FoodRecords");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd().HasColumnType("bigint");
            entity.Property(e => e.UserId).HasColumnType("int");
            entity.Property(e => e.Calories).HasColumnName("Energy").HasPrecision(18,2);
            entity.Property(e => e.Protein).HasPrecision(18,2);
            entity.Property(e => e.Fat).HasPrecision(18,2);
            entity.Property(e => e.Carbohydrate).HasPrecision(18,2);
            entity.Property(e => e.Iron).HasPrecision(18,2);
            entity.Property(e => e.Sodium).HasPrecision(18,2);
            entity.Property(e => e.Weight).HasPrecision(18,2);
            entity.Property(e => e.Price).HasPrecision(18,2);
            entity.Property(e => e.MealType).HasConversion<int>();
            entity.Property(e => e.RecordDate).HasColumnType("date");
            entity.HasIndex(e => new { e.UserId, e.RecordDate });
            entity.HasOne(e => e.User)
                .WithMany(u => u.FoodRecords)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // DailySummary
        modelBuilder.Entity<DailySummary>(entity =>
        {
            entity.ToTable("DailySummaries");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd().HasColumnType("int");
            entity.Property(e => e.UserId).HasColumnType("int");
            entity.Property(e => e.TotalCalories).HasColumnName("TotalEnergy").HasPrecision(18,2);
            entity.Property(e => e.RecommendedCalories).HasColumnName("RecommendedEnergy").HasPrecision(18,2);
            entity.Property(e => e.TotalProtein).HasPrecision(18,2);
            entity.Property(e => e.TotalFat).HasPrecision(18,2);
            entity.Property(e => e.TotalCarbohydrate).HasPrecision(18,2);
            entity.Property(e => e.TotalIron).HasPrecision(18,2);
            entity.Property(e => e.TotalSodium).HasPrecision(18,2);
            entity.Property(e => e.TotalPrice).HasPrecision(18,2);
            entity.Property(e => e.RecordDate).HasColumnType("date");
            entity.HasIndex(e => new { e.UserId, e.RecordDate }).IsUnique();
            entity.HasOne(e => e.User)
                .WithMany(u => u.DailySummaries)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // CustomFood
        modelBuilder.Entity<CustomFood>(entity =>
        {
            entity.ToTable("CustomFoods");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => e.UserId);
            entity.HasOne(e => e.User)
                .WithMany(u => u.CustomFoods)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
