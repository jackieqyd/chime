using System.Transactions;
using ChimeBackend.Application.DTOs;
using ChimeBackend.Application.Extensions;
using ChimeBackend.Domain.Entities;
using ChimeBackend.Domain.Enums;
using ChimeBackend.Domain.Repositories;

namespace ChimeBackend.Application.Services;

public class FoodRecordAppService
{
    private readonly IFoodRecordRepository _foodRecordRepository;
    private readonly IDailySummaryRepository _dailySummaryRepository;
    private readonly IUserRepository _userRepository;

    public FoodRecordAppService(
        IFoodRecordRepository foodRecordRepository,
        IDailySummaryRepository dailySummaryRepository,
        IUserRepository userRepository)
    {
        _foodRecordRepository = foodRecordRepository;
        _dailySummaryRepository = dailySummaryRepository;
        _userRepository = userRepository;
    }

    public async Task<FoodRecordResult> AddRecordAsync(
        int userId,
        DateTime recordDate,
        int mealType,
        List<FoodItemInput> foods,
        string? photoUrl,
        string? photoLocalPath,
        string? remark,
        CancellationToken cancellationToken = default)
    {
        var mealTypeEnum = (MealType)mealType;
        var records = foods.Select(f => BuildFoodRecord(userId, recordDate, mealTypeEnum, f, photoUrl, photoLocalPath, remark)).ToList();

        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await _foodRecordRepository.AddRangeAsync(records, cancellationToken);
        await UpdateDailySummaryAsync(userId, recordDate, cancellationToken);
        transaction.Complete();

        return new FoodRecordResult(
            records.First().Id,
            recordDate,
            mealType,
            records.Select(r => ToFoodItemResult(r)).ToList(),
            photoUrl
        );
    }

    public async Task<FoodRecordResult> ReplaceRecordsAsync(
        int userId,
        DateTime recordDate,
        int mealType,
        List<FoodItemInput> foods,
        string? photoUrl,
        string? photoLocalPath,
        string? remark,
        CancellationToken cancellationToken = default)
    {
        var mealTypeEnum = (MealType)mealType;

        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await _foodRecordRepository.RemoveRangeAsync(userId, recordDate, mealTypeEnum, cancellationToken);

        var records = foods.Select(f => BuildFoodRecord(userId, recordDate, mealTypeEnum, f, photoUrl, photoLocalPath, remark)).ToList();
        await _foodRecordRepository.AddRangeAsync(records, cancellationToken);
        await UpdateDailySummaryAsync(userId, recordDate, cancellationToken);
        transaction.Complete();

        return new FoodRecordResult(
            records.First().Id,
            recordDate,
            mealType,
            records.Select(r => ToFoodItemResult(r)).ToList(),
            photoUrl
        );
    }

    public async Task<FoodRecordListResult> QueryRecordsAsync(
        int userId,
        DateTime? startDate,
        DateTime? endDate,
        int? mealType,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 50) pageSize = 20;

        var mealTypeEnum = mealType.HasValue ? (MealType)mealType.Value : (MealType?)null;

        var totalCount = await _foodRecordRepository.CountAsync(
            userId, startDate, endDate, mealTypeEnum, cancellationToken);
        var records = await _foodRecordRepository.QueryAsync(
            userId, startDate, endDate, mealTypeEnum, page, pageSize, cancellationToken);

        var dtos = records
            .GroupBy(r => new { r.RecordDate, r.MealType })
            .Select(g => new FoodRecordResult(
                g.First().Id,
                g.Key.RecordDate,
                (int)g.Key.MealType,
                g.Select(f => ToFoodItemResult(f)).ToList(),
                g.First().PhotoUrl
            ))
            .ToList();

        return new FoodRecordListResult(dtos, totalCount, page, pageSize);
    }

    public async Task<bool> DeleteRecordAsync(
        long id,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var record = await _foodRecordRepository.GetByIdAsync(id, cancellationToken);
        if (record == null || record.UserId != userId)
            return false;

        var recordDate = record.RecordDate;
        _foodRecordRepository.Remove(record);
        await UpdateDailySummaryAsync(userId, recordDate, cancellationToken);

        return true;
    }

    public async Task<DailySummaryResult> GetDailySummaryAsync(
        int userId,
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        var recommendedCalories = CalculateRecommendedCalories(user);

        var records = await _foodRecordRepository.GetByUserIdAndDateAsync(
            userId, date, cancellationToken);

        var totalCalories = records.Sum(r => r.Calories);
        var totalProtein = records.Sum(r => r.Protein);
        var totalFat = records.Sum(r => r.Fat);
        var totalCarbohydrate = records.Sum(r => r.Carbohydrate);
        var totalIron = records.Sum(r => r.Iron ?? 0);
        var totalSodium = records.Sum(r => r.Sodium ?? 0);
        var totalPrice = records.Sum(r => r.Price ?? 0);

        var mealBreakdown = records
            .GroupBy(r => r.MealType)
            .ToDictionary(
                g => g.Key.ToString().ToLower(),
                g => new MealBreakdownResult(g.Sum(r => r.Calories), g.Count())
            );

        return new DailySummaryResult(
            date,
            totalCalories.Round(),
            totalProtein.Round(),
            totalFat.Round(),
            totalCarbohydrate.Round(),
            totalIron.Round(),
            totalSodium.Round(),
            totalPrice,
            records.Count,
            recommendedCalories,
            (recommendedCalories - totalCalories).Round(),
            mealBreakdown
        );
    }

    private async Task UpdateDailySummaryAsync(
        int userId,
        DateTime recordDate,
        CancellationToken cancellationToken)
    {
        var records = await _foodRecordRepository.GetByUserIdAndDateAsync(
            userId, recordDate, cancellationToken);

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        var recommendedCalories = CalculateRecommendedCalories(user);

        var summary = await _dailySummaryRepository.GetByUserIdAndDateAsync(
            userId, recordDate, cancellationToken);

        if (summary == null)
        {
            summary = new DailySummary
            {
                UserId = userId,
                RecordDate = recordDate,
                TotalCalories = records.Sum(r => r.Calories).Round(),
                TotalProtein = records.Sum(r => r.Protein).Round(),
                TotalFat = records.Sum(r => r.Fat).Round(),
                TotalCarbohydrate = records.Sum(r => r.Carbohydrate).Round(),
                TotalIron = records.Sum(r => r.Iron ?? 0).Round(),
                TotalSodium = records.Sum(r => r.Sodium ?? 0).Round(),
                TotalPrice = records.Sum(r => r.Price ?? 0),
                RecordCount = records.Count,
                RecommendedCalories = recommendedCalories,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _dailySummaryRepository.AddAsync(summary, cancellationToken);
        }
        else
        {
            summary.TotalCalories = records.Sum(r => r.Calories).Round();
            summary.TotalProtein = records.Sum(r => r.Protein).Round();
            summary.TotalFat = records.Sum(r => r.Fat).Round();
            summary.TotalCarbohydrate = records.Sum(r => r.Carbohydrate).Round();
            summary.TotalIron = records.Sum(r => r.Iron ?? 0).Round();
            summary.TotalSodium = records.Sum(r => r.Sodium ?? 0).Round();
            summary.TotalPrice = records.Sum(r => r.Price ?? 0);
            summary.RecordCount = records.Count;
            summary.RecommendedCalories = recommendedCalories;
            summary.UpdatedAt = DateTime.UtcNow;

            _dailySummaryRepository.Update(summary);
        }
    }

    private FoodRecord BuildFoodRecord(
        int userId,
        DateTime recordDate,
        MealType mealType,
        FoodItemInput food,
        string? photoUrl,
        string? photoLocalPath,
        string? remark)
    {
        // 优先使用每100g原始数据计算，避免前端传入已计算的值导致重复计算
        var energyPer100g = food.EnergyPer100g ?? food.Energy;
        var proteinPer100g = food.ProteinPer100g ?? food.Protein;
        var fatPer100g = food.FatPer100g ?? food.Fat;
        var carbPer100g = food.CarbohydratePer100g ?? food.Carbohydrate;
        var ironPer100g = food.IronPer100g ?? food.Iron;
        var sodiumPer100g = food.SodiumPer100g ?? food.Sodium;

        return new FoodRecord
        {
            UserId = userId,
            RecordDate = recordDate,
            MealType = mealType,
            FoodId = food.FoodId,
            FoodName = food.FoodName,
            CategoryId = food.CategoryId,
            CategoryName = food.CategoryName,
            Weight = food.Weight,
            Calories = (energyPer100g * food.Weight / 100).Round(),
            Protein = (proteinPer100g * food.Weight / 100).Round(),
            Fat = (fatPer100g * food.Weight / 100).Round(),
            Carbohydrate = (carbPer100g * food.Weight / 100).Round(),
            Iron = (ironPer100g * food.Weight / 100).Round(),
            Sodium = (sodiumPer100g * food.Weight / 100).Round(),
            Price = food.Price,
            PhotoUrl = photoUrl,
            PhotoLocalPath = photoLocalPath,
            Remark = remark,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            EnergyPer100g = food.EnergyPer100g,
            ProteinPer100g = food.ProteinPer100g,
            FatPer100g = food.FatPer100g,
            CarbohydratePer100g = food.CarbohydratePer100g,
            IronPer100g = food.IronPer100g,
            SodiumPer100g = food.SodiumPer100g
        };
    }

    private static FoodItemResult ToFoodItemResult(FoodRecord r)
    {
        return new FoodItemResult(
            r.Id, r.FoodId, r.FoodName, r.CategoryId, r.CategoryName,
            r.Weight, r.Calories.Round(), r.Protein.Round(), r.Fat.Round(), r.Carbohydrate.Round(),
            r.Iron.HasValue ? r.Iron.Value.Round() : null,
            r.Sodium.HasValue ? r.Sodium.Value.Round() : null,
            r.Price,
            r.EnergyPer100g, r.ProteinPer100g, r.FatPer100g, r.CarbohydratePer100g,
            r.IronPer100g, r.SodiumPer100g
        );
    }

    private decimal CalculateRecommendedCalories(User? user)
    {
        if (user == null) return 2000m;

        if (user.DailyCalorie.HasValue && user.DailyCalorie > 0)
            return user.DailyCalorie.Value;

        decimal bmr;
        if (user.Gender == Gender.Male)
        {
            bmr = 66.47m + (13.75m * (user.Weight ?? 70)) + (5.003m * (user.Height ?? 170)) - (6.755m * (user.Age ?? 30));
        }
        else
        {
            bmr = 655.1m + (9.563m * (user.Weight ?? 60)) + (1.85m * (user.Height ?? 160)) - (4.676m * (user.Age ?? 30));
        }

        var activityMultiplier = user.ActivityLevel switch
        {
            ActivityLevel.Sedentary => 1.2m,
            ActivityLevel.Light => 1.375m,
            ActivityLevel.Moderate => 1.55m,
            ActivityLevel.High => 1.725m,
            ActivityLevel.Extreme => 1.9m,
            _ => 1.375m
        };

        var tdee = bmr * activityMultiplier;

        return user.Goal switch
        {
            1 => (tdee * 0.8m).Round(),
            2 => (tdee * 1.1m).Round(),
            _ => tdee.Round()
        };
    }
}
