using ChimeBackend.Domain.Enums;

namespace ChimeBackend.Domain.Entities;

public class FoodRecord
{
    public long Id { get; set; }
    public int UserId { get; set; }

    // 记录信息
    public DateTime RecordDate { get; set; }
    public MealType MealType { get; set; }

    // 食物信息（冗余存储）
    public long? FoodId { get; set; }              // 关联食物库ID
    public string FoodName { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }

    // 营养成分（快照）
    public decimal Weight { get; set; }            // 重量(g)
    public decimal Calories { get; set; }          // 热量(kcal)
    public decimal Protein { get; set; }           // 蛋白质(g)
    public decimal Fat { get; set; }              // 脂肪(g)
    public decimal Carbohydrate { get; set; }       // 碳水(g)
    public decimal? Iron { get; set; }             // 铁(mg)
    public decimal? Sodium { get; set; }           // 钠(mg)

    // 每100g原始营养数据（用于前端克重重算）
    public decimal? EnergyPer100g { get; set; }       // 每100g热量(kcal)
    public decimal? ProteinPer100g { get; set; }       // 每100g蛋白质(g)
    public decimal? FatPer100g { get; set; }          // 每100g脂肪(g)
    public decimal? CarbohydratePer100g { get; set; }  // 每100g碳水(g)
    public decimal? IronPer100g { get; set; }          // 每100g铁(mg)
    public decimal? SodiumPer100g { get; set; }        // 每100g钠(mg)

    // 价格
    public decimal? Price { get; set; }

    // 照片
    public string? PhotoUrl { get; set; }
    public string? PhotoLocalPath { get; set; }

    // 备注
    public string? Remark { get; set; }

    // 时间戳
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }

    // 导航属性
    public User? User { get; set; }
}
