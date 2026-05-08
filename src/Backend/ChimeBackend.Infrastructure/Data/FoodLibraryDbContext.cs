using ChimeBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChimeBackend.Infrastructure.Data;

public class FoodLibraryDbContext : DbContext
{
    public FoodLibraryDbContext(DbContextOptions<FoodLibraryDbContext> options) : base(options)
    {
    }

    public DbSet<Food> Foods { get; set; } = null!;
    public DbSet<FoodCategory> FoodCategories { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // FoodCategory
        modelBuilder.Entity<FoodCategory>(entity =>
        {
            entity.ToTable("FoodCategories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.CateId).HasColumnName("cate_id");
            entity.Property(e => e.IsSubcategory).HasColumnName("is_subcategory");
            entity.Property(e => e.ParentCategoryId).HasColumnName("parent_category_id");
        });

        // Food
        modelBuilder.Entity<Food>(entity =>
        {
            entity.ToTable("Foods");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd().HasColumnName("id");
            entity.Property(e => e.CateId).HasColumnName("cate_id");
            entity.Property(e => e.FoodName).HasColumnName("food_name");
            entity.Property(e => e.AliasName).HasColumnName("alias_name");
            entity.Property(e => e.EnglishName).HasColumnName("english_name");
            entity.Property(e => e.EdiblePart).HasColumnName("edible_part");
            entity.Property(e => e.Water).HasColumnName("water");
            entity.Property(e => e.Energy).HasColumnName("energy");
            entity.Property(e => e.Protein).HasColumnName("protein");
            entity.Property(e => e.Fat).HasColumnName("fat");
            entity.Property(e => e.Carbohydrate).HasColumnName("carbohydrate");
            entity.Property(e => e.DietaryFiber).HasColumnName("dietary_fiber");
            entity.Property(e => e.Cholesterol).HasColumnName("cholesterol");
            entity.Property(e => e.Ash).HasColumnName("ash");
            entity.Property(e => e.Carotene).HasColumnName("carotene");
            entity.Property(e => e.VitaminA).HasColumnName("vitamin_a");
            entity.Property(e => e.VitaminE).HasColumnName("vitamin_e");
            entity.Property(e => e.Thiamin).HasColumnName("thiamin");
            entity.Property(e => e.Riboflavin).HasColumnName("riboflavin");
            entity.Property(e => e.Niacin).HasColumnName("niacin");
            entity.Property(e => e.VitaminC).HasColumnName("vitamin_c");
            entity.Property(e => e.Calcium).HasColumnName("calcium");
            entity.Property(e => e.Phosphorus).HasColumnName("phosphorus");
            entity.Property(e => e.Potassium).HasColumnName("potassium");
            entity.Property(e => e.Sodium).HasColumnName("sodium");
            entity.Property(e => e.Magnesium).HasColumnName("magnesium");
            entity.Property(e => e.Iron).HasColumnName("iron");
            entity.Property(e => e.Zinc).HasColumnName("zinc");
            entity.Property(e => e.Selenium).HasColumnName("selenium");
            entity.Property(e => e.Copper).HasColumnName("copper");
            entity.Property(e => e.Manganese).HasColumnName("manganese");
            entity.Property(e => e.Iodine).HasColumnName("iodine");
            entity.Property(e => e.Sfa).HasColumnName("sfa");
            entity.Property(e => e.Mufa).HasColumnName("mufa");
            entity.Property(e => e.Pufa).HasColumnName("pufa");
            entity.Property(e => e.FattyAcidsTotal).HasColumnName("fatty_acids_total");
            entity.Property(e => e.Status).HasColumnName("Status");
            entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt");
            entity.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt");
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Foods)
                .HasForeignKey(e => e.CateId)
                .HasPrincipalKey(c => c.CateId)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}
