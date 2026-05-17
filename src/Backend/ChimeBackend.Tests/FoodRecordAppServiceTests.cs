using ChimeBackend.Application.DTOs;
using ChimeBackend.Application.Services;
using ChimeBackend.Domain.Entities;
using ChimeBackend.Domain.Enums;
using ChimeBackend.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ChimeBackend.Tests;

public class FoodRecordAppServiceTests
{
    private readonly Mock<IFoodRecordRepository> _foodRecordRepoMock;
    private readonly Mock<IDailySummaryRepository> _dailySummaryRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<ILogService> _logServiceMock;
    private readonly FoodRecordAppService _sut;

    public FoodRecordAppServiceTests()
    {
        _foodRecordRepoMock = new Mock<IFoodRecordRepository>();
        _dailySummaryRepoMock = new Mock<IDailySummaryRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _logServiceMock = new Mock<ILogService>();
        _sut = new FoodRecordAppService(
            _foodRecordRepoMock.Object,
            _dailySummaryRepoMock.Object,
            _userRepoMock.Object,
            _logServiceMock.Object
        );
    }

    [Fact]
    public async Task AddRecordAsync_ShouldCalculateNutritionCorrectly()
    {
        // Arrange
        var userId = 1;
        var recordDate = DateTime.UtcNow.Date;
        var mealType = (int)MealType.Breakfast;
        var foods = new List<FoodItemInput>
        {
            new(null, "白米饭", null, null, 150m, 116m, 2.6m, 0.3m, 25.9m, 0.3m, 2.0m, null, 116m, 2.6m, 0.3m, 25.9m, 0.3m, 2.0m)
        };

        _foodRecordRepoMock
            .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<FoodRecord>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _foodRecordRepoMock
            .Setup(r => r.GetByUserIdAndDateAsync(userId, recordDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FoodRecord>());
        _foodRecordRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FoodRecord?)null);
        _userRepoMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _dailySummaryRepoMock
            .Setup(r => r.GetByUserIdAndDateAsync(userId, recordDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DailySummary?)null);
        _dailySummaryRepoMock
            .Setup(r => r.AddAsync(It.IsAny<DailySummary>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.AddRecordAsync(userId, recordDate, mealType, foods, null, null, null);

        // Assert
        result.Should().NotBeNull();
        result.Foods.Should().HaveCount(1);

        // 116 * 150 / 100 = 174
        var foodItem = result.Foods.First();
        foodItem.Calories.Should().Be(174m);
        foodItem.Protein.Should().Be(3.9m); // 2.6 * 150 / 100
        foodItem.Fat.Should().Be(0.4m);    // 0.3 * 150 / 100
        foodItem.Carbohydrate.Should().Be(38.8m); // 25.9 * 150 / 100
    }

    [Fact]
    public async Task QueryRecordsAsync_ShouldReturnGroupedRecords()
    {
        // Arrange
        var userId = 1;
        var recordDate = DateTime.UtcNow.Date;
        var records = new List<FoodRecord>
        {
            new() { Id = 1, UserId = userId, RecordDate = recordDate, MealType = MealType.Breakfast, FoodName = "白米饭", Weight = 150, Calories = 174, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, UserId = userId, RecordDate = recordDate, MealType = MealType.Breakfast, FoodName = "鸡蛋", Weight = 50, Calories = 72, CreatedAt = DateTime.UtcNow }
        };

        _foodRecordRepoMock
            .Setup(r => r.CountAsync(userId, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);
        _foodRecordRepoMock
            .Setup(r => r.QueryAsync(userId, null, null, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);

        // Act
        var result = await _sut.QueryRecordsAsync(userId, null, null, null, 1, 20);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(1); // Grouped by date + mealType
        result.Items.First().Foods.Should().HaveCount(2);
    }

    [Fact]
    public async Task DeleteRecordAsync_WhenRecordNotFound_ShouldReturnFalse()
    {
        // Arrange
        _foodRecordRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FoodRecord?)null);

        // Act
        var result = await _sut.DeleteRecordAsync(999, 1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteRecordAsync_WhenRecordExists_ShouldReturnTrue()
    {
        // Arrange
        var record = new FoodRecord { Id = 1, UserId = 1, RecordDate = DateTime.UtcNow.Date };
        _foodRecordRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);
        _foodRecordRepoMock
            .Setup(r => r.GetByUserIdAndDateAsync(1, record.RecordDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FoodRecord>());
        _userRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _dailySummaryRepoMock
            .Setup(r => r.GetByUserIdAndDateAsync(1, record.RecordDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DailySummary?)null);

        // Act
        var result = await _sut.DeleteRecordAsync(1, 1);

        // Assert
        result.Should().BeTrue();
        _foodRecordRepoMock.Verify(r => r.Remove(record), Times.Once);
    }

    [Fact]
    public async Task GetDailySummaryAsync_ShouldCalculateRecommendedCalories_ForMaleUser()
    {
        // Arrange
        var userId = 1;
        var user = new User
        {
            Id = userId,
            Gender = Gender.Male,
            Weight = 70,
            Height = 170,
            Age = 30,
            ActivityLevel = ActivityLevel.Moderate,
            Goal = 0
        };

        _userRepoMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _foodRecordRepoMock
            .Setup(r => r.GetByUserIdAndDateAsync(userId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FoodRecord>());

        // Act
        var result = await _sut.GetDailySummaryAsync(userId, DateTime.UtcNow.Date);

        // Assert
        // BMR = 66.47 + (13.75 * 70) + (5.003 * 170) - (6.755 * 30) = 66.47 + 962.5 + 850.51 - 202.65 = 1676.83
        // TDEE = 1676.83 * 1.55 = 2599.09
        result.RecommendedCalories.Should().BeApproximately(2599m, 1m);
    }

    [Fact]
    public async Task GetDailySummaryAsync_WhenUserHasCustomCalorie_ShouldReturnCustomValue()
    {
        // Arrange
        var userId = 1;
        var user = new User
        {
            Id = userId,
            DailyCalorie = 1500m
        };

        _userRepoMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _foodRecordRepoMock
            .Setup(r => r.GetByUserIdAndDateAsync(userId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FoodRecord>());

        // Act
        var result = await _sut.GetDailySummaryAsync(userId, DateTime.UtcNow.Date);

        // Assert
        result.RecommendedCalories.Should().Be(1500m);
    }

    [Fact]
    public async Task GetDailySummaryAsync_WhenGoalIsLoss_ShouldReduceTdeeBy20Percent()
    {
        // Arrange
        var userId = 1;
        var user = new User
        {
            Id = userId,
            Gender = Gender.Male,
            Weight = 70,
            Height = 170,
            Age = 30,
            ActivityLevel = ActivityLevel.Moderate,
            Goal = 1 // 减脂
        };

        _userRepoMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _foodRecordRepoMock
            .Setup(r => r.GetByUserIdAndDateAsync(userId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FoodRecord>());

        // Act
        var result = await _sut.GetDailySummaryAsync(userId, DateTime.UtcNow.Date);

        // Assert
        // TDEE = 2599.09, Target = 2599.09 * 0.8 = 2079
        result.RecommendedCalories.Should().BeApproximately(2079m, 1m);
    }

    [Fact]
    public async Task GetDailySummaryAsync_WhenGoalIsMuscleGain_ShouldIncreaseTdeeBy10Percent()
    {
        // Arrange
        var userId = 1;
        var user = new User
        {
            Id = userId,
            Gender = Gender.Male,
            Weight = 70,
            Height = 170,
            Age = 30,
            ActivityLevel = ActivityLevel.Moderate,
            Goal = 2 // 增肌
        };

        _userRepoMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _foodRecordRepoMock
            .Setup(r => r.GetByUserIdAndDateAsync(userId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FoodRecord>());

        // Act
        var result = await _sut.GetDailySummaryAsync(userId, DateTime.UtcNow.Date);

        // Assert
        // TDEE = 2599.09, Target = 2599.09 * 1.1 = 2859
        result.RecommendedCalories.Should().BeApproximately(2859m, 1m);
    }
}
