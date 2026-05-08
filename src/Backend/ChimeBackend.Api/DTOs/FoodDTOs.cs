namespace ChimeBackend.Api.DTOs;

public record FoodCategoryDto(
    int Id,
    string Title,
    int CateId,
    bool IsSubcategory,
    int? ParentCategoryId
);

public record FoodDto(
    long Id,
    string FoodName,
    string? AliasName,
    int CateId,
    decimal Energy,
    decimal Protein,
    decimal Fat,
    decimal Carbohydrate,
    decimal DietaryFiber,
    decimal Sodium,
    decimal Calcium,
    decimal Iron,
    decimal VitaminC
);

public record FoodSearchResultDto(
    List<FoodDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);

public record FoodDetailDto(
    long Id,
    string FoodName,
    string? AliasName,
    string? EnglishName,
    decimal? EdiblePart,
    int CateId,
    string CategoryName,
    NutritionDto Nutrition
);

public record NutritionDto(
    decimal? Water,
    decimal? Energy,
    decimal? Protein,
    decimal? Fat,
    decimal? Carbohydrate,
    decimal? DietaryFiber,
    decimal? Cholesterol,
    decimal? Carotene,
    decimal? VitaminA,
    decimal? VitaminE,
    decimal? Thiamin,
    decimal? Riboflavin,
    decimal? Niacin,
    decimal? VitaminC,
    decimal? Calcium,
    decimal? Phosphorus,
    decimal? Potassium,
    decimal? Sodium,
    decimal? Magnesium,
    decimal? Iron,
    decimal? Zinc,
    decimal? Selenium,
    decimal? Copper,
    decimal? Manganese,
    decimal? Iodine,
    decimal? Sfa,
    decimal? Mufa,
    decimal? Pufa,
    decimal? FattyAcidsTotal
);
