namespace ChimeBackend.Domain.Entities;

public class FoodCategory
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int CateId { get; set; }
    public byte IsSubcategory { get; set; }
    public int? ParentCategoryId { get; set; }

    // 导航属性
    public ICollection<Food> Foods { get; set; } = new List<Food>();
}
