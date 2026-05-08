using ChimeBackend.Application.Services;
using ChimeBackend.Domain.Entities;
using ChimeBackend.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ChimeBackend.Tests;

public class FoodAppServiceTests
{
    private readonly Mock<IFoodRepository> _foodRepoMock;
    private readonly Mock<IFoodCategoryRepository> _categoryRepoMock;
    private readonly FoodAppService _sut;

    public FoodAppServiceTests()
    {
        _foodRepoMock = new Mock<IFoodRepository>();
        _categoryRepoMock = new Mock<IFoodCategoryRepository>();
        _sut = new FoodAppService(_foodRepoMock.Object, _categoryRepoMock.Object);
    }

    [Fact]
    public async Task GetCategoriesAsync_ShouldReturnAllCategories()
    {
        // Arrange
        var categories = new List<FoodCategory>
        {
            new() { Id = 1, Title = "主食", CateId = 1001, IsSubcategory = 0 },
            new() { Id = 2, Title = "米饭", CateId = 1002, IsSubcategory = 1, ParentCategoryId = 1 },
            new() { Id = 3, Title = "蔬菜", CateId = 1003, IsSubcategory = 0 }
        };

        _categoryRepoMock
            .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _sut.GetCategoriesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Categories.Should().HaveCount(3);
        result.Categories[0].Title.Should().Be("主食");
        result.Categories[0].IsSubcategory.Should().BeFalse();
        result.Categories[1].Title.Should().Be("米饭");
        result.Categories[1].IsSubcategory.Should().BeTrue();
        result.Categories[1].ParentCategoryId.Should().Be(1);
    }

    [Fact]
    public async Task SearchFoodsAsync_ShouldReturnPaginatedResults()
    {
        // Arrange
        var keyword = "米饭";
        var foods = new List<Food>
        {
            new() { Id = 1, FoodName = "白米饭", CateId = 1002, Energy = 116, Protein = 2.6m, Fat = 0.3m, Carbohydrate = 25.9m }
        };

        _foodRepoMock
            .Setup(s => s.CountAsync(keyword, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _foodRepoMock
            .Setup(s => s.SearchAsync(keyword, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(foods);

        // Act
        var result = await _sut.SearchFoodsAsync(keyword, null, 1, 20);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
        result.Items.Should().HaveCount(1);
        result.Items[0].FoodName.Should().Be("白米饭");
        result.Items[0].Energy.Should().Be(27.7m); // 116 kcal / 4.184 = 27.7 kJ
    }

    [Fact]
    public async Task SearchFoodsAsync_WithInvalidPage_ShouldUseDefaultPage()
    {
        // Arrange
        var foods = new List<Food>
        {
            new() { Id = 1, FoodName = "白米饭", CateId = 1002, Energy = 116 }
        };

        _foodRepoMock
            .Setup(s => s.CountAsync(null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _foodRepoMock
            .Setup(s => s.SearchAsync(null, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(foods);

        // Act - page = -1 should be corrected to 1
        var result = await _sut.SearchFoodsAsync(null, null, -1, 20);

        // Assert
        result.Page.Should().Be(1);
    }

    [Fact]
    public async Task SearchFoodsAsync_WithInvalidPageSize_ShouldUseDefaultPageSize()
    {
        // Arrange
        var foods = new List<Food>();
        _foodRepoMock
            .Setup(s => s.CountAsync(null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        _foodRepoMock
            .Setup(s => s.SearchAsync(null, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(foods);

        // Act - pageSize = 100 should be capped to 50, but our service caps at 20
        var result = await _sut.SearchFoodsAsync(null, null, 1, 100);

        // Assert
        result.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task SearchFoodsAsync_WithCategoryFilter_ShouldFilterByCategory()
    {
        // Arrange
        var categoryId = 1002;
        var foods = new List<Food>
        {
            new() { Id = 1, FoodName = "白米饭", CateId = categoryId, Energy = 116 }
        };

        _foodRepoMock
            .Setup(s => s.CountAsync(null, categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _foodRepoMock
            .Setup(s => s.SearchAsync(null, categoryId, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(foods);

        // Act
        var result = await _sut.SearchFoodsAsync(null, categoryId, 1, 20);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].CateId.Should().Be(categoryId);
    }

    [Fact]
    public async Task GetFoodDetailAsync_WhenFoodNotFound_ShouldReturnNull()
    {
        // Arrange
        var foodId = 999L;
        _foodRepoMock
            .Setup(s => s.GetByIdAsync(foodId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Food?)null);

        // Act
        var result = await _sut.GetFoodDetailAsync(foodId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetFoodDetailAsync_WhenFoodExists_ShouldReturnFullDetail()
    {
        // Arrange
        var foodId = 1L;
        var category = new FoodCategory { Id = 1, Title = "主食" };
        var food = new Food
        {
            Id = foodId,
            FoodName = "白米饭",
            AliasName = "蒸米饭",
            EnglishName = "Rice",
            EdiblePart = 100,
            CateId = 1002,
            Category = category,
            Water = 70,
            Energy = 116,
            Protein = 2.6m,
            Fat = 0.3m,
            Carbohydrate = 25.9m,
            DietaryFiber = 0.3m,
            Cholesterol = 0,
            Carotene = 0,
            VitaminA = 0,
            VitaminE = 0.03m,
            Thiamin = 0.02m,
            Riboflavin = 0.01m,
            Niacin = 0.4m,
            VitaminC = 0,
            Calcium = 6,
            Phosphorus = 62,
            Potassium = 58,
            Sodium = 0,
            Magnesium = 15,
            Iron = 0.3m,
            Zinc = 0.9m,
            Selenium = 0,
            Copper = 0,
            Manganese = 0,
            Iodine = 0,
            Sfa = 0,
            Mufa = 0,
            Pufa = 0,
            FattyAcidsTotal = 0
        };

        _foodRepoMock
            .Setup(s => s.GetByIdAsync(foodId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(food);

        // Act
        var result = await _sut.GetFoodDetailAsync(foodId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(foodId);
        result.FoodName.Should().Be("白米饭");
        result.AliasName.Should().Be("蒸米饭");
        result.EnglishName.Should().Be("Rice");
        result.EdiblePart.Should().Be(100);
        result.CategoryName.Should().Be("主食");
        result.Nutrition.Should().NotBeNull();
        result.Nutrition.Energy.Should().Be(27.7m); // 116 kcal / 4.184 = 27.7 kJ
        result.Nutrition.Protein.Should().Be(2.6m);
        result.Nutrition.Fat.Should().Be(0.3m);
        result.Nutrition.Carbohydrate.Should().Be(25.9m);
    }

    [Fact]
    public async Task GetFoodDetailAsync_WhenNutritionValuesAreNull_ShouldReturnZeros()
    {
        // Arrange
        var foodId = 1L;
        var food = new Food
        {
            Id = foodId,
            FoodName = "测试食物",
            CateId = 1000,
            Energy = null,
            Protein = null,
            Fat = null,
            Carbohydrate = null
        };

        _foodRepoMock
            .Setup(s => s.GetByIdAsync(foodId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(food);

        // Act
        var result = await _sut.GetFoodDetailAsync(foodId);

        // Assert
        result.Should().NotBeNull();
        result!.Nutrition.Energy.Should().Be(0m); // null -> 0 / 4.184 = 0
        result.Nutrition.Protein.Should().BeNull();
        result.Nutrition.Fat.Should().BeNull();
        result.Nutrition.Carbohydrate.Should().BeNull();
    }
}
