namespace ChimeBackend.Domain.Entities;

public class DailySummary
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime RecordDate { get; set; }

    // 汇总数据
    public decimal TotalCalories { get; set; }
    public decimal TotalProtein { get; set; }
    public decimal TotalFat { get; set; }
    public decimal TotalCarbohydrate { get; set; }
    public decimal? TotalIron { get; set; }
    public decimal? TotalSodium { get; set; }
    public decimal? TotalPrice { get; set; }
    public int RecordCount { get; set; }

    // 推荐值
    public decimal? RecommendedCalories { get; set; }

    // 时间戳
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }

    // 导航属性
    public User? User { get; set; }
}
