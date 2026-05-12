using ChimeBackend.Application.DTOs;
using ChimeBackend.Domain.Entities;
using ChimeBackend.Domain.Enums;
using ChimeBackend.Domain.Repositories;

namespace ChimeBackend.Application.Services;

public class AuthAppService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IVerificationCodeService _verificationCodeService;
    private readonly IWxService _wxService;

    public AuthAppService(
        IUserRepository userRepository,
        ITokenService tokenService,
        IVerificationCodeService verificationCodeService,
        IWxService wxService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _verificationCodeService = verificationCodeService;
        _wxService = wxService;
    }

    public bool ValidateCode(string phoneNumber, string code)
    {
        return _verificationCodeService.ValidateCode(phoneNumber, code);
    }

    public void SendCode(string phoneNumber)
    {
        _verificationCodeService.SendCode(phoneNumber);
    }

    public async Task<AuthResult?> PhoneLoginAsync(string phoneNumber, string verificationCode, CancellationToken cancellationToken = default)
    {
        if (!_verificationCodeService.ValidateCode(phoneNumber, verificationCode))
            return null;

        var user = await _userRepository.GetByPhoneNumberAsync(phoneNumber, cancellationToken);
        if (user == null)
        {
            user = new User
            {
                PhoneNumber = phoneNumber,
                VersionMode = VersionMode.SelfDiscipline,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _userRepository.AddAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);
        }

        var (accessToken, refreshToken, expiresAt) = _tokenService.GenerateTokens(user);
        return new AuthResult(accessToken, refreshToken, 86400, new UserInfoResult(user.Id, user.Nickname, user.Avatar, user.VersionMode.HasValue ? (int)user.VersionMode.Value : null), false);
    }

    public async Task<AuthResult?> MiniProgramLoginAsync(string code, string? nickname, string? avatar, CancellationToken cancellationToken = default)
    {
        // 通过微信服务换取 openid（AppSecret 只在后端，不暴露给前端）
        var openId = await _wxService.GetOpenIdAsync(code, cancellationToken);

        var user = await _userRepository.GetByOpenIdAsync(openId, cancellationToken);
        if (user == null)
        {
            // 新用户：创建用户，昵称和头像来自微信授权，VersionMode不设置（为null），让前端引导选择版本
            user = new User
            {
                OpenId = openId,
                Nickname = nickname,
                Avatar = avatar,
                VersionMode = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _userRepository.AddAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            // 返回新用户信息，VersionMode为null
            var (accessToken, refreshToken, expiresAt) = _tokenService.GenerateTokens(user);
            return new AuthResult(accessToken, refreshToken, 86400, new UserInfoResult(user.Id, user.Nickname, user.Avatar, null), true);
        }

        // 已有用户：返回完整信息，标记为非新用户
        var (token, refreshToken2, expiresAt2) = _tokenService.GenerateTokens(user);
        return new AuthResult(token, refreshToken2, 86400, new UserInfoResult(user.Id, user.Nickname, user.Avatar, user.VersionMode.HasValue ? (int)user.VersionMode.Value : null), false);
    }

    public async Task<AuthResult?> AppleLoginAsync(string identityToken, string authorizationCode, int realUserStatus, CancellationToken cancellationToken = default)
    {
        var appleUserId = $"dev_apple_{identityToken}";

        var user = await _userRepository.GetByUnionIdAsync(appleUserId, cancellationToken);
        if (user == null)
        {
            user = new User
            {
                UnionId = appleUserId,
                VersionMode = VersionMode.SelfDiscipline,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _userRepository.AddAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);
        }

        var (accessToken, refreshToken, expiresAt) = _tokenService.GenerateTokens(user);
        return new AuthResult(accessToken, refreshToken, 86400, new UserInfoResult(user.Id, user.Nickname, user.Avatar, user.VersionMode.HasValue ? (int)user.VersionMode.Value : null), false);
    }

    public async Task<AuthResult?> BindPhoneAsync(int userId, string phoneNumber, string verificationCode, CancellationToken cancellationToken = default)
    {
        if (!_verificationCodeService.ValidateCode(phoneNumber, verificationCode))
            return null;

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null) return null;

        user.PhoneNumber = phoneNumber;
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        var (accessToken, refreshToken, expiresAt) = _tokenService.GenerateTokens(user);
        return new AuthResult(accessToken, refreshToken, 86400, new UserInfoResult(user.Id, user.Nickname, user.Avatar, user.VersionMode.HasValue ? (int)user.VersionMode.Value : null), false);
    }

    public async Task<AuthResult?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var userId = _tokenService.GetUserIdFromToken(refreshToken);
        if (userId == null) return null;

        var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
        if (user == null) return null;

        var (accessToken, newRefreshToken, expiresAt) = _tokenService.GenerateTokens(user);
        return new AuthResult(accessToken, newRefreshToken, 86400, new UserInfoResult(user.Id, user.Nickname, user.Avatar, user.VersionMode.HasValue ? (int)user.VersionMode.Value : null), false);
    }
}
