using ChimeBackend.Application.Services;
using ChimeBackend.Domain.Entities;
using ChimeBackend.Domain.Enums;
using ChimeBackend.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ChimeBackend.Tests;

public class AuthAppServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IVerificationCodeService> _verificationCodeServiceMock;
    private readonly AuthAppService _sut;

    public AuthAppServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _tokenServiceMock = new Mock<ITokenService>();
        _verificationCodeServiceMock = new Mock<IVerificationCodeService>();
        _sut = new AuthAppService(
            _userRepoMock.Object,
            _tokenServiceMock.Object,
            _verificationCodeServiceMock.Object
        );
    }

    [Fact]
    public void ValidateCode_ShouldCallVerificationCodeService()
    {
        // Arrange
        var phoneNumber = "13800138000";
        var code = "123456";
        _verificationCodeServiceMock
            .Setup(s => s.ValidateCode(phoneNumber, code))
            .Returns(true);

        // Act
        var result = _sut.ValidateCode(phoneNumber, code);

        // Assert
        result.Should().BeTrue();
        _verificationCodeServiceMock.Verify(s => s.ValidateCode(phoneNumber, code), Times.Once);
    }

    [Fact]
    public void SendCode_ShouldCallVerificationCodeService()
    {
        // Arrange
        var phoneNumber = "13800138000";

        // Act
        _sut.SendCode(phoneNumber);

        // Assert
        _verificationCodeServiceMock.Verify(s => s.SendCode(phoneNumber), Times.Once);
    }

    [Fact]
    public async Task PhoneLoginAsync_WhenCodeInvalid_ShouldReturnNull()
    {
        // Arrange
        var phoneNumber = "13800138000";
        var code = "wrong_code";
        _verificationCodeServiceMock
            .Setup(s => s.ValidateCode(phoneNumber, code))
            .Returns(false);

        // Act
        var result = await _sut.PhoneLoginAsync(phoneNumber, code);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task PhoneLoginAsync_WhenNewUser_ShouldCreateUserAndReturnTokens()
    {
        // Arrange
        var phoneNumber = "13800138000";
        var code = "123456";
        var userId = 1;

        _verificationCodeServiceMock
            .Setup(s => s.ValidateCode(phoneNumber, code))
            .Returns(true);
        _userRepoMock
            .Setup(s => s.GetByPhoneNumberAsync(phoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _userRepoMock
            .Setup(s => s.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _userRepoMock
            .Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _tokenServiceMock
            .Setup(s => s.GenerateTokens(It.IsAny<User>()))
            .Returns(("access_token", "refresh_token", DateTime.UtcNow.AddDays(1)));

        // Act
        var result = await _sut.PhoneLoginAsync(phoneNumber, code);

        // Assert
        result.Should().NotBeNull();
        result!.AccessToken.Should().Be("access_token");
        result.RefreshToken.Should().Be("refresh_token");
        result.User.VersionMode.Should().Be((int)VersionMode.SelfDiscipline);
        _userRepoMock.Verify(s => s.AddAsync(It.Is<User>(u => u.PhoneNumber == phoneNumber), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PhoneLoginAsync_WhenExistingUser_ShouldReturnTokensWithoutCreating()
    {
        // Arrange
        var phoneNumber = "13800138000";
        var code = "123456";
        var existingUser = new User
        {
            Id = 1,
            PhoneNumber = phoneNumber,
            Nickname = "TestUser",
            VersionMode = VersionMode.SelfDiscipline
        };

        _verificationCodeServiceMock
            .Setup(s => s.ValidateCode(phoneNumber, code))
            .Returns(true);
        _userRepoMock
            .Setup(s => s.GetByPhoneNumberAsync(phoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);
        _tokenServiceMock
            .Setup(s => s.GenerateTokens(existingUser))
            .Returns(("access_token", "refresh_token", DateTime.UtcNow.AddDays(1)));

        // Act
        var result = await _sut.PhoneLoginAsync(phoneNumber, code);

        // Assert
        result.Should().NotBeNull();
        result!.User.Id.Should().Be(1);
        _userRepoMock.Verify(s => s.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MiniProgramLoginAsync_WhenNewUser_ShouldCreateUserWithOpenId()
    {
        // Arrange
        var code = "test_code";
        var expectedOpenId = $"dev_openid_{code}";

        _userRepoMock
            .Setup(s => s.GetByOpenIdAsync(expectedOpenId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _userRepoMock
            .Setup(s => s.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _userRepoMock
            .Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _tokenServiceMock
            .Setup(s => s.GenerateTokens(It.IsAny<User>()))
            .Returns(("access_token", "refresh_token", DateTime.UtcNow.AddDays(1)));

        // Act
        var result = await _sut.MiniProgramLoginAsync(code);

        // Assert
        result.Should().NotBeNull();
        _userRepoMock.Verify(s => s.AddAsync(It.Is<User>(u => u.OpenId == expectedOpenId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MiniProgramLoginAsync_WhenExistingUser_ShouldReturnTokens()
    {
        // Arrange
        var code = "test_code";
        var existingUser = new User
        {
            Id = 1,
            OpenId = $"dev_openid_{code}",
            VersionMode = VersionMode.SelfDiscipline
        };

        _userRepoMock
            .Setup(s => s.GetByOpenIdAsync($"dev_openid_{code}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);
        _tokenServiceMock
            .Setup(s => s.GenerateTokens(existingUser))
            .Returns(("access_token", "refresh_token", DateTime.UtcNow.AddDays(1)));

        // Act
        var result = await _sut.MiniProgramLoginAsync(code);

        // Assert
        result.Should().NotBeNull();
        result!.User.Id.Should().Be(1);
    }

    [Fact]
    public async Task AppleLoginAsync_WhenNewUser_ShouldCreateUserWithUnionId()
    {
        // Arrange
        var identityToken = "test_identity_token";
        var authorizationCode = "test_auth_code";
        var expectedUnionId = $"dev_apple_{identityToken}";

        _userRepoMock
            .Setup(s => s.GetByUnionIdAsync(expectedUnionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _userRepoMock
            .Setup(s => s.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _userRepoMock
            .Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _tokenServiceMock
            .Setup(s => s.GenerateTokens(It.IsAny<User>()))
            .Returns(("access_token", "refresh_token", DateTime.UtcNow.AddDays(1)));

        // Act
        var result = await _sut.AppleLoginAsync(identityToken, authorizationCode, 0);

        // Assert
        result.Should().NotBeNull();
        _userRepoMock.Verify(s => s.AddAsync(It.Is<User>(u => u.UnionId == expectedUnionId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BindPhoneAsync_WhenCodeInvalid_ShouldReturnNull()
    {
        // Arrange
        var userId = 1;
        var phoneNumber = "13800138000";
        var code = "wrong_code";

        _verificationCodeServiceMock
            .Setup(s => s.ValidateCode(phoneNumber, code))
            .Returns(false);

        // Act
        var result = await _sut.BindPhoneAsync(userId, phoneNumber, code);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task BindPhoneAsync_WhenUserNotFound_ShouldReturnNull()
    {
        // Arrange
        var userId = 999;
        var phoneNumber = "13800138000";
        var code = "123456";

        _verificationCodeServiceMock
            .Setup(s => s.ValidateCode(phoneNumber, code))
            .Returns(true);
        _userRepoMock
            .Setup(s => s.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.BindPhoneAsync(userId, phoneNumber, code);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task BindPhoneAsync_WhenSuccess_ShouldUpdateUserAndReturnTokens()
    {
        // Arrange
        var userId = 1;
        var phoneNumber = "13800138000";
        var code = "123456";
        var existingUser = new User
        {
            Id = userId,
            Nickname = "TestUser",
            VersionMode = VersionMode.SelfDiscipline
        };

        _verificationCodeServiceMock
            .Setup(s => s.ValidateCode(phoneNumber, code))
            .Returns(true);
        _userRepoMock
            .Setup(s => s.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);
        _userRepoMock
            .Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _tokenServiceMock
            .Setup(s => s.GenerateTokens(existingUser))
            .Returns(("access_token", "refresh_token", DateTime.UtcNow.AddDays(1)));

        // Act
        var result = await _sut.BindPhoneAsync(userId, phoneNumber, code);

        // Assert
        result.Should().NotBeNull();
        existingUser.PhoneNumber.Should().Be(phoneNumber);
        _userRepoMock.Verify(s => s.Update(existingUser), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenTokenInvalid_ShouldReturnNull()
    {
        // Arrange
        var refreshToken = "invalid_token";
        _tokenServiceMock
            .Setup(s => s.GetUserIdFromToken(refreshToken))
            .Returns((int?)null);

        // Act
        var result = await _sut.RefreshTokenAsync(refreshToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenUserNotFound_ShouldReturnNull()
    {
        // Arrange
        var refreshToken = "valid_token";
        _tokenServiceMock
            .Setup(s => s.GetUserIdFromToken(refreshToken))
            .Returns(1);
        _userRepoMock
            .Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.RefreshTokenAsync(refreshToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenSuccess_ShouldReturnNewTokens()
    {
        // Arrange
        var refreshToken = "valid_token";
        var user = new User
        {
            Id = 1,
            Nickname = "TestUser",
            VersionMode = VersionMode.SelfDiscipline
        };

        _tokenServiceMock
            .Setup(s => s.GetUserIdFromToken(refreshToken))
            .Returns(1);
        _userRepoMock
            .Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _tokenServiceMock
            .Setup(s => s.GenerateTokens(user))
            .Returns(("new_access_token", "new_refresh_token", DateTime.UtcNow.AddDays(1)));

        // Act
        var result = await _sut.RefreshTokenAsync(refreshToken);

        // Assert
        result.Should().NotBeNull();
        result!.AccessToken.Should().Be("new_access_token");
        result.RefreshToken.Should().Be("new_refresh_token");
    }
}
