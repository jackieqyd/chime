namespace ChimeBackend.Domain.Entities;

public class CustomFood
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FoodName { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? Unit { get; set; }
    public decimal Calories { get; set; }
    public decimal Protein { get; set; }
    public decimal Fat { get; set; }
    public decimal Carbohydrate { get; set; }
    public decimal? Iron { get; set; }
    public decimal? Sodium { get; set; }
    public decimal? DefaultWeight { get; set; }
    public byte Status { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // 导航属性
    public User? User { get; set; }
}
