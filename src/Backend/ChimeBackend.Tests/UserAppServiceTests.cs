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
    private readonly Mock<ILogService> _logServiceMock;
    private readonly UserAppService _sut;

    public UserAppServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _logServiceMock = new Mock<ILogService>();
        _sut = new UserAppService(_userRepoMock.Object, _logServiceMock.Object);
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
        // DailyCalorie会被自动计算为BMR：10*55 + 6.25*165 - 5*25 - 161 = 1295.25
        user.DailyCalorie.Should().Be(1295.25m);
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
}
