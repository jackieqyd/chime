using ChimeBackend.Api.DTOs;
using ChimeBackend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChimeBackend.Api.Controllers;

[ApiController]
[Route("api/foods")]
public class FoodsController : ControllerBase
{
    private readonly FoodAppService _foodAppService;

    public FoodsController(FoodAppService foodAppService)
    {
        _foodAppService = foodAppService;
    }

    [HttpGet("categories")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<FoodCategoryDto>>>> GetCategories(CancellationToken cancellationToken)
    {
        var result = await _foodAppService.GetCategoriesAsync(cancellationToken);

        var dtos = result.Categories.Select(c => new FoodCategoryDto(
            c.Id,
            c.Title,
            c.CateId,
            c.IsSubcategory,
            c.ParentCategoryId
        )).ToList();

        return Ok(ApiResponse<List<FoodCategoryDto>>.Success(dtos));
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<FoodSearchResultDto>>> SearchFoods(
        [FromQuery] string? keyword,
        [FromQuery] int? categoryId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _foodAppService.SearchFoodsAsync(keyword, categoryId, page, pageSize, cancellationToken);

        return Ok(ApiResponse<FoodSearchResultDto>.Success(new FoodSearchResultDto(
            result.Items.Select(f => new FoodDto(
                f.Id,
                f.FoodName,
                f.AliasName,
                f.CateId,
                f.Energy,
                f.Protein,
                f.Fat,
                f.Carbohydrate,
                f.DietaryFiber,
                f.Sodium,
                f.Calcium,
                f.Iron,
                f.VitaminC
            )).ToList(),
            result.TotalCount,
            result.Page,
            result.PageSize
        )));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<FoodDetailDto>>> GetFoodDetail(
        long id,
        CancellationToken cancellationToken)
    {
        var result = await _foodAppService.GetFoodDetailAsync(id, cancellationToken);

        if (result == null)
            return NotFound(ApiResponse<FoodDetailDto>.Fail(404, "食物不存在"));

        return Ok(ApiResponse<FoodDetailDto>.Success(new FoodDetailDto(
            result.Id,
            result.FoodName,
            result.AliasName,
            result.EnglishName,
            result.EdiblePart,
            result.CateId,
            result.CategoryName,
            new NutritionDto(
                result.Nutrition.Water,
                result.Nutrition.Energy,
                result.Nutrition.Protein,
                result.Nutrition.Fat,
                result.Nutrition.Carbohydrate,
                result.Nutrition.DietaryFiber,
                result.Nutrition.Cholesterol,
                result.Nutrition.Carotene,
                result.Nutrition.VitaminA,
                result.Nutrition.VitaminE,
                result.Nutrition.Thiamin,
                result.Nutrition.Riboflavin,
                result.Nutrition.Niacin,
                result.Nutrition.VitaminC,
                result.Nutrition.Calcium,
                result.Nutrition.Phosphorus,
                result.Nutrition.Potassium,
                result.Nutrition.Sodium,
                result.Nutrition.Magnesium,
                result.Nutrition.Iron,
                result.Nutrition.Zinc,
                result.Nutrition.Selenium,
                result.Nutrition.Copper,
                result.Nutrition.Manganese,
                result.Nutrition.Iodine,
                result.Nutrition.Sfa,
                result.Nutrition.Mufa,
                result.Nutrition.Pufa,
                result.Nutrition.FattyAcidsTotal
            )
        )));
    }
}
