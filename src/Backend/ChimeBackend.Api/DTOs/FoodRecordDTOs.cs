namespace ChimeBackend.Api.DTOs;

public record AddFoodRecordRequest(
    DateTime RecordDate,
    int MealType,
    List<AddFoodItemRequest> Foods,
    string? PhotoUrl,
    string? PhotoLocalPath,
    string? Remark
);

public record AddFoodItemRequest(
    long? FoodId,
    string FoodName,
    int? CategoryId,
    string? CategoryName,
    decimal Weight,
    decimal Energy,
    decimal Protein,
    decimal Fat,
    decimal Carbohydrate,
    decimal Iron,
    decimal Sodium,
    decimal? Price,
    decimal? EnergyPer100g,
    decimal? ProteinPer100g,
    decimal? FatPer100g,
    decimal? CarbohydratePer100g,
    decimal? IronPer100g,
    decimal? SodiumPer100g
);

public record UpdateFoodRecordRequest(
    string? FoodName,
    decimal? Weight,
    decimal? Calories,
    decimal? Protein,
    decimal? Fat,
    decimal? Carbohydrate,
    string? Remark
);

public record FoodRecordDto(
    long Id,
    DateTime RecordDate,
    int MealType,
    List<FoodItemDto> Foods,
    string? PhotoUrl
);

public record FoodItemDto(
    long Id,
    long? FoodId,
    string FoodName,
    int? CategoryId,
    string? CategoryName,
    decimal Weight,
    decimal Calories,
    decimal Protein,
    decimal Fat,
    decimal Carbohydrate,
    decimal? Iron,
    decimal? Sodium,
    decimal? Price,
    decimal? EnergyPer100g,
    decimal? ProteinPer100g,
    decimal? FatPer100g,
    decimal? CarbohydratePer100g,
    decimal? IronPer100g,
    decimal? SodiumPer100g
);

public record FoodRecordListDto(
    List<FoodRecordDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);

public record DailySummaryDto(
    DateTime RecordDate,
    decimal TotalCalories,
    decimal TotalProtein,
    decimal TotalFat,
    decimal TotalCarbohydrate,
    decimal TotalIron,
    decimal TotalSodium,
    decimal TotalPrice,
    int RecordCount,
    decimal RecommendedCalories,
    decimal RemainingCalories,
    Dictionary<string, MealBreakdownDto> MealBreakdown
);

public record MealBreakdownDto(
    decimal Energy,
    int Count
);
