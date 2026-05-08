using ChimeBackend.Application.Services;
using ChimeBackend.Domain.Entities;
using ChimeBackend.Domain.Enums;
using ChimeBackend.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ChimeBackend.Tests;

public class UserAppServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly UserAppService _sut;

    public UserAppServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _sut = new UserAppService(_userRepoMock.Object);
    }

    [Fact]
    public async Task GetProfileAsync_WhenUserNotFound_ShouldReturnNull()
    {
        // Arrange
        var userId = 999;
        _userRepoMock
            .Setup(s => s.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.GetProfileAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProfileAsync_WhenUserExists_ShouldReturnProfile()
    {
        // Arrange
        var userId = 1;
        var user = new User
        {
            Id = userId,
            Nickname = "TestUser",
            Avatar = "avatar_url",
            Gender = Gender.Male,
            Height = 175,
            Weight = 70,
            Age = 30,
            VersionMode = VersionMode.SelfDiscipline,
            ActivityLevel = ActivityLevel.Moderate,
            Goal = 1,
            DailyCalorie = 2000
        };

        _userRepoMock
            .Setup(s => s.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.GetProfileAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.Nickname.Should().Be("TestUser");
        result.Avatar.Should().Be("avatar_url");
        result.Gender.Should().Be((int)Gender.Male);
        result.Height.Should().Be(175);
        result.Weight.Should().Be(70);
        result.Age.Should().Be(30);
        result.VersionMode.Should().Be((int)VersionMode.SelfDiscipline);
        result.ActivityLevel.Should().Be((int)ActivityLevel.Moderate);
        result.Goal.Should().Be(1);
        result.DailyCalorie.Should().Be(2000);
    }

    [Fact]
    public async Task UpdateProfileAsync_WhenUserNotFound_ShouldReturnNull()
    {
        // Arrange
        var userId = 999;
        _userRepoMock
            .Setup(s => s.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.UpdateProfileAsync(userId, "NewName", null, null, null, null, null, null, null, null, null);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateProfileAsync_WhenSuccess_ShouldUpdateAndReturnProfile()
    {
        // Arrange
        var userId = 1;
        var user = new User
        {
            Id = userId,
            Nickname = "OldName",
            VersionMode = VersionMode.SelfDiscipline,
            ActivityLevel = ActivityLevel.Light,
            Goal = 0
        };

        _userRepoMock
            .Setup(s => s.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepoMock
            .Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateProfileAsync(
            userId,
            "NewName",
            null,
            (int)Gender.Female,
            165,
            55,
            25,
            (int)VersionMode.Indulgent,
            (int)ActivityLevel.High,
            2,
            1800
        );

        // Assert
        result.Should().NotBeNull();
        result!.Nickname.Should().Be("NewName");
        user.Nickname.Should().Be("NewName");
        user.Gender.Should().Be(Gender.Female);
        user.Height.Should().Be(165);
        user.Weight.Should().Be(55);
        user.Age.Should().Be(25);
        user.VersionMode.Should().Be(VersionMode.Indulgent);
        user.ActivityLevel.Should().Be(ActivityLevel.High);
        user.Goal.Should().Be(2);
        user.DailyCalorie.Should().Be(1800);
        _userRepoMock.Verify(s => s.Update(user), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_WithPartialUpdates_ShouldOnlyUpdateProvidedFields()
    {
        // Arrange
        var userId = 1;
        var user = new User
        {
            Id = userId,
            Nickname = "OldName",
            Height = 180,
            Weight = 80,
            Age = 40,
            VersionMode = VersionMode.SelfDiscipline,
            ActivityLevel = ActivityLevel.Moderate,
            Goal = 0
        };

        _userRepoMock
            .Setup(s => s.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepoMock
            .Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act - only update nickname and weight
        var result = await _sut.UpdateProfileAsync(userId, "NewName", null, null, null, 75, null, null, null, null, null);

        // Assert
        result.Should().NotBeNull();
        result!.Nickname.Should().Be("NewName");
        result.Weight.Should().Be(75);
        // Unchanged fields should remain
        result.Height.Should().Be(180);
        result.Age.Should().Be(40);
    }

    [Fact]
    public async Task GetDailyCalorieAsync_WhenUserNotFound_ShouldReturnNull()
    {
        // Arrange
        var userId = 999;
        _userRepoMock
            .Setup(s => s.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.GetDailyCalorieAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetDailyCalorieAsync_ForMaleWithModerateActivity_ShouldCalculateCorrectly()
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
            .Setup(s => s.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.GetDailyCalorieAsync(userId);

        // Assert
        result.Should().NotBeNull();
        // BMR = 66.47 + (13.75 * 70) + (5.003 * 170) - (6.755 * 30) = 1676.83
        result!.Bmr.Should().BeApproximately(1676.83m, 0.01m);
        // TDEE = 1676.83 * 1.55 = 2599.09
        result.Tdee.Should().BeApproximately(2599.09m, 0.01m);
        // Goal 0 = 维持 = TDEE
        result.Recommended.Should().BeApproximately(2599m, 1m);
        result.Goal.Should().Be(0);
        result.GoalDesc.Should().Be("维持");
    }

    [Fact]
    public async Task GetDailyCalorieAsync_ForFemaleWithLightActivity_ShouldCalculateCorrectly()
    {
        // Arrange
        var userId = 1;
        var user = new User
        {
            Id = userId,
            Gender = Gender.Female,
            Weight = 55,
            Height = 160,
            Age = 25,
            ActivityLevel = ActivityLevel.Light,
            Goal = 0
        };

        _userRepoMock
            .Setup(s => s.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.GetDailyCalorieAsync(userId);

        // Assert
        result.Should().NotBeNull();
        // BMR = 655.1 + (9.563 * 55) + (1.85 * 160) - (4.676 * 25) = 655.1 + 525.965 + 296 - 116.9 = 1360.165
        result!.Bmr.Should().BeApproximately(1360.17m, 0.01m);
        // TDEE = 1360.165 * 1.375 = 1870.23
        result.Tdee.Should().BeApproximately(1870.23m, 0.01m);
        result.Recommended.Should().BeApproximately(1870m, 1m);
    }

    [Fact]
    public async Task GetDailyCalorieAsync_WhenGoalIsLoss_ShouldReduceTdeeBy20Percent()
    {
        // Arrange
        var userId = 1;
        var user = new User
        {
            Id = userId,
            Gender = Gender.Male,
            Weight = 80,
            Height = 175,
            Age = 35,
            ActivityLevel = ActivityLevel.Sedentary,
            Goal = 1
        };

        _userRepoMock
            .Setup(s => s.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.GetDailyCalorieAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.GoalDesc.Should().Be("减脂");
        // TDEE * 0.8
        result.Recommended.Should().BeLessThan(result.Tdee);
    }

    [Fact]
    public async Task GetDailyCalorieAsync_WhenGoalIsMuscleGain_ShouldIncreaseTdeeBy10Percent()
    {
        // Arrange
        var userId = 1;
        var user = new User
        {
            Id = userId,
            Gender = Gender.Male,
            Weight = 65,
            Height = 172,
            Age = 28,
            ActivityLevel = ActivityLevel.Moderate,
            Goal = 2
        };

        _userRepoMock
            .Setup(s => s.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.GetDailyCalorieAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.GoalDesc.Should().Be("增肌");
        // TDEE * 1.1
        result.Recommended.Should().BeGreaterThan(result.Tdee);
    }

    [Fact]
    public async Task GetDailyCalorieAsync_WhenCustomCalorieSet_ShouldReturnCustomValue()
    {
        // Arrange
        var userId = 1;
        var customCalorie = 1500m;
        var user = new User
        {
            Id = userId,
            Gender = Gender.Male,
            Weight = 90,
            Height = 180,
            Age = 40,
            ActivityLevel = ActivityLevel.High,
            Goal = 0,
            DailyCalorie = customCalorie
        };

        _userRepoMock
            .Setup(s => s.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.GetDailyCalorieAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Recommended.Should().Be(customCalorie);
    }
}
