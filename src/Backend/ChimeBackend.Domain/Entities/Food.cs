namespace ChimeBackend.Domain.Entities;

public class Food
{
    public long Id { get; set; }
    public int CateId { get; set; }
    public string FoodName { get; set; } = string.Empty;
    public string? AliasName { get; set; }
    public string? EnglishName { get; set; }
    public decimal? EdiblePart { get; set; }        // 可食部(%)

    // 基本营养素
    public decimal? Water { get; set; }              // 水分(g)
    public decimal? Energy { get; set; }             // 能量(kcal/100g)
    public decimal? Protein { get; set; }             // 蛋白质(g)
    public decimal? Fat { get; set; }               // 脂肪(g)
    public decimal? Carbohydrate { get; set; }       // 碳水化合物(g)
    public decimal? DietaryFiber { get; set; }       // 膳食纤维(g)
    public decimal? Cholesterol { get; set; }         // 胆固醇(mg)
    public decimal? Ash { get; set; }                // 灰分(g)

    // 维生素
    public decimal? Carotene { get; set; }            // 胡萝卜素(μg)
    public decimal? VitaminA { get; set; }           // 维生素A(μg)
    public decimal? VitaminE { get; set; }           // 维生素E(mg)
    public decimal? Thiamin { get; set; }            // 硫胺素/维生素B1(mg)
    public decimal? Riboflavin { get; set; }          // 核黄素/维生素B2(mg)
    public decimal? Niacin { get; set; }             // 烟酸/维生素B3(mg)
    public decimal? VitaminC { get; set; }           // 维生素C(mg)

    // 矿物质
    public decimal? Calcium { get; set; }             // 钙(mg)
    public decimal? Phosphorus { get; set; }           // 磷(mg)
    public decimal? Potassium { get; set; }           // 钾(mg)
    public decimal? Sodium { get; set; }              // 钠(mg)
    public decimal? Magnesium { get; set; }           // 镁(mg)
    public decimal? Iron { get; set; }                // 铁(mg)
    public decimal? Zinc { get; set; }               // 锌(mg)
    public decimal? Selenium { get; set; }            // 硒(μg)
    public decimal? Copper { get; set; }              // 铜(mg)
    public decimal? Manganese { get; set; }           // 锰(mg)
    public decimal? Iodine { get; set; }             // 碘(μg)

    // 脂肪酸
    public decimal? Sfa { get; set; }                // 饱和脂肪酸(g)
    public decimal? Mufa { get; set; }               // 单不饱和脂肪酸(g)
    public decimal? Pufa { get; set; }               // 多不饱和脂肪酸(g)
    public decimal? FattyAcidsTotal { get; set; }    // 脂肪酸总量(g)

    // 系统字段
    public string? CreatedBy { get; set; }
    public byte Status { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // 导航属性
    public FoodCategory? Category { get; set; }
}
