using ChimeBackend.Application.DTOs;
using ChimeBackend.Application.Extensions;
using ChimeBackend.Domain.Repositories;

namespace ChimeBackend.Application.Services;

public class FoodAppService
{
    private readonly IFoodRepository _foodRepository;
    private readonly IFoodCategoryRepository _categoryRepository;

    public FoodAppService(IFoodRepository foodRepository, IFoodCategoryRepository categoryRepository)
    {
        _foodRepository = foodRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<CategoryListResult> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.GetAllAsync(cancellationToken);

        return new CategoryListResult(
            categories.Select(c => new CategoryResult(
                c.Id,
                c.Title,
                c.CateId,
                c.IsSubcategory == 1,
                c.ParentCategoryId
            )).ToList()
        );
    }

    public async Task<FoodSearchResult> SearchFoodsAsync(
        string? keyword,
        int? categoryId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 50) pageSize = 20;

        var totalCount = await _foodRepository.CountAsync(keyword, categoryId, cancellationToken);
        var foods = await _foodRepository.SearchAsync(keyword, categoryId, page, pageSize, cancellationToken);

        return new FoodSearchResult(
            foods.Select(f => new FoodResult(
                f.Id,
                f.FoodName,
                f.AliasName,
                f.CateId,
                ((f.Energy ?? 0) / 4.184m).Round(),
                f.Protein ?? 0,
                f.Fat ?? 0,
                f.Carbohydrate ?? 0,
                f.DietaryFiber ?? 0,
                f.Sodium ?? 0,
                f.Calcium ?? 0,
                f.Iron ?? 0,
                f.VitaminC ?? 0
            )).ToList(),
            totalCount,
            page,
            pageSize
        );
    }

    public async Task<FoodDetailResult?> GetFoodDetailAsync(long id, CancellationToken cancellationToken = default)
    {
        var food = await _foodRepository.GetByIdAsync(id, cancellationToken);
        if (food == null) return null;

        return new FoodDetailResult(
            food.Id,
            food.FoodName,
            food.AliasName,
            food.EnglishName,
            food.EdiblePart,
            food.CateId,
            food.Category?.Title ?? "",
            new NutritionResult(
                food.Water,
                ((food.Energy ?? 0) / 4.184m).Round(),
                food.Protein,
                food.Fat,
                food.Carbohydrate,
                food.DietaryFiber,
                food.Cholesterol,
                food.Carotene,
                food.VitaminA,
                food.VitaminE,
                food.Thiamin,
                food.Riboflavin,
                food.Niacin,
                food.VitaminC,
                food.Calcium,
                food.Phosphorus,
                food.Potassium,
                food.Sodium,
                food.Magnesium,
                food.Iron,
                food.Zinc,
                food.Selenium,
                food.Copper,
                food.Manganese,
                food.Iodine,
                food.Sfa,
                food.Mufa,
                food.Pufa,
                food.FattyAcidsTotal
            )
        );
    }
}
