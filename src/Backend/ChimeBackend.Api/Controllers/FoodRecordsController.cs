using System.Security.Claims;
using ChimeBackend.Api.DTOs;
using ChimeBackend.Application.DTOs;
using ChimeBackend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChimeBackend.Api.Controllers;

[ApiController]
[Route("api/food-records")]
[Authorize]
public class FoodRecordsController : ControllerBase
{
    private readonly FoodRecordAppService _foodRecordAppService;

    public FoodRecordsController(FoodRecordAppService foodRecordAppService)
    {
        _foodRecordAppService = foodRecordAppService;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return int.Parse(userIdClaim!.Value);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<FoodRecordDto>>> AddRecord(
        [FromBody] AddFoodRecordRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var foods = request.Foods.Select(MapToFoodItemInput).ToList();

        var result = await _foodRecordAppService.AddRecordAsync(
            userId,
            request.RecordDate,
            request.MealType,
            foods,
            request.PhotoUrl,
            request.PhotoLocalPath,
            request.Remark,
            cancellationToken);

        return Ok(ApiResponse<FoodRecordDto>.Success(MapToFoodRecordDto(result), "记录添加成功"));
    }

    [HttpPut("replace")]
    public async Task<ActionResult<ApiResponse<FoodRecordDto>>> ReplaceRecords(
        [FromBody] AddFoodRecordRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var foods = request.Foods.Select(MapToFoodItemInput).ToList();

        var result = await _foodRecordAppService.ReplaceRecordsAsync(
            userId,
            request.RecordDate,
            request.MealType,
            foods,
            request.PhotoUrl,
            request.PhotoLocalPath,
            request.Remark,
            cancellationToken);

        return Ok(ApiResponse<FoodRecordDto>.Success(MapToFoodRecordDto(result), "记录替换成功"));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<FoodRecordListDto>>> GetRecords(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int? mealType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();

        var result = await _foodRecordAppService.QueryRecordsAsync(
            userId,
            startDate,
            endDate,
            mealType,
            page,
            pageSize,
            cancellationToken);

        return Ok(ApiResponse<FoodRecordListDto>.Success(new FoodRecordListDto(
            result.Items.Select(MapToFoodRecordDto).ToList(),
            result.TotalCount,
            result.Page,
            result.PageSize
        )));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteRecord(
        long id,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        var success = await _foodRecordAppService.DeleteRecordAsync(id, userId, cancellationToken);

        if (!success)
            return NotFound(ApiResponse.Fail(404, "记录不存在"));

        return Ok(ApiResponse.Success("删除成功"));
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<DailySummaryDto>>> GetDailySummary(
        [FromQuery] DateTime? date,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var targetDate = date ?? DateTime.UtcNow.Date;

        var result = await _foodRecordAppService.GetDailySummaryAsync(
            userId, targetDate, cancellationToken);

        return Ok(ApiResponse<DailySummaryDto>.Success(new DailySummaryDto(
            result.RecordDate,
            result.TotalCalories,
            result.TotalProtein,
            result.TotalFat,
            result.TotalCarbohydrate,
            result.TotalIron,
            result.TotalSodium,
            result.TotalPrice,
            result.RecordCount,
            result.RecommendedCalories,
            result.RemainingCalories,
            result.MealBreakdown.ToDictionary(
                kvp => kvp.Key,
                kvp => new MealBreakdownDto(kvp.Value.Energy, kvp.Value.Count)
            )
        )));
    }

    private static FoodItemInput MapToFoodItemInput(AddFoodItemRequest f) =>
        new(
            f.FoodId,
            f.FoodName,
            f.CategoryId,
            f.CategoryName,
            f.Weight,
            f.Energy,
            f.Protein,
            f.Fat,
            f.Carbohydrate,
            f.Iron,
            f.Sodium,
            f.Price,
            f.EnergyPer100g,
            f.ProteinPer100g,
            f.FatPer100g,
            f.CarbohydratePer100g,
            f.IronPer100g,
            f.SodiumPer100g
        );

    private static FoodRecordDto MapToFoodRecordDto(FoodRecordResult r) =>
        new(
            r.Id,
            r.RecordDate,
            r.MealType,
            r.Foods.Select(MapToFoodItemDto).ToList(),
            r.PhotoUrl
        );

    private static FoodItemDto MapToFoodItemDto(FoodItemResult f) =>
        new(
            f.Id,
            f.FoodId,
            f.FoodName,
            f.CategoryId,
            f.CategoryName,
            f.Weight,
            f.Calories,
            f.Protein,
            f.Fat,
            f.Carbohydrate,
            f.Iron,
            f.Sodium,
            f.Price,
            f.EnergyPer100g,
            f.ProteinPer100g,
            f.FatPer100g,
            f.CarbohydratePer100g,
            f.IronPer100g,
            f.SodiumPer100g
        );
}
