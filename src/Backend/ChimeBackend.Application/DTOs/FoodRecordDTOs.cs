namespace ChimeBackend.Application.DTOs;

public record FoodItemInput(
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

public record FoodRecordResult(
    long Id,
    DateTime RecordDate,
    int MealType,
    List<FoodItemResult> Foods,
    string? PhotoUrl
);

public record FoodItemResult(
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

public record FoodRecordListResult(
    List<FoodRecordResult> Items,
    int TotalCount,
    int Page,
    int PageSize
);

public record DailySummaryResult(
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
    Dictionary<string, MealBreakdownResult> MealBreakdown
);

public record MealBreakdownResult(
    decimal Energy,
    int Count
);
