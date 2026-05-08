using System.Security.Claims;
using ChimeBackend.Api.DTOs;
using ChimeBackend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChimeBackend.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthAppService _authAppService;

    public AuthController(AuthAppService authAppService)
    {
        _authAppService = authAppService;
    }

    [HttpPost("code")]
    [AllowAnonymous]
    public ActionResult<ApiResponse> SendCode([FromBody] SendCodeRequest request)
    {
        if (string.IsNullOrEmpty(request.PhoneNumber) || request.PhoneNumber.Length != 11)
            return BadRequest(ApiResponse.Fail(400, "手机号格式不正确"));

        _authAppService.SendCode(request.PhoneNumber);
        return Ok(ApiResponse.Success("验证码已发送"));
    }

    [HttpPost("phone-login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> PhoneLogin(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authAppService.PhoneLoginAsync(
            request.PhoneNumber,
            request.VerificationCode,
            cancellationToken);

        if (result == null)
            return BadRequest(ApiResponse<AuthResponse>.Fail(400, "验证码错误或已过期"));

        return Ok(ApiResponse<AuthResponse>.Success(new AuthResponse(
            result.AccessToken,
            result.RefreshToken,
            result.ExpiresIn,
            new UserDto(result.User.Id, result.User.Nickname, result.User.Avatar, result.User.VersionMode)
        )));
    }

    [HttpPost("miniprogram-login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> MiniProgramLogin(
        [FromBody] MiniProgramLoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authAppService.MiniProgramLoginAsync(request.Code, cancellationToken);

        return Ok(ApiResponse<AuthResponse>.Success(new AuthResponse(
            result!.AccessToken,
            result.RefreshToken,
            result.ExpiresIn,
            new UserDto(result.User.Id, result.User.Nickname, result.User.Avatar, result.User.VersionMode)
        )));
    }

    [HttpPost("apple-login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> AppleLogin(
        [FromBody] AppleLoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authAppService.AppleLoginAsync(
            request.IdentityToken,
            request.AuthorizationCode,
            request.RealUserStatus,
            cancellationToken);

        return Ok(ApiResponse<AuthResponse>.Success(new AuthResponse(
            result!.AccessToken,
            result.RefreshToken,
            result.ExpiresIn,
            new UserDto(result.User.Id, result.User.Nickname, result.User.Avatar, result.User.VersionMode)
        )));
    }

    [HttpPost("bind-phone")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> BindPhone(
        [FromBody] BindPhoneRequest request,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized(ApiResponse<AuthResponse>.Fail(401, "未授权"));

        var result = await _authAppService.BindPhoneAsync(
            userId,
            request.PhoneNumber,
            request.VerificationCode,
            cancellationToken);

        if (result == null)
            return BadRequest(ApiResponse<AuthResponse>.Fail(400, "验证码错误或已过期"));

        return Ok(ApiResponse<AuthResponse>.Success(new AuthResponse(
            result.AccessToken,
            result.RefreshToken,
            result.ExpiresIn,
            new UserDto(result.User.Id, result.User.Nickname, result.User.Avatar, result.User.VersionMode)
        )));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authAppService.RefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (result == null)
            return BadRequest(ApiResponse<AuthResponse>.Fail(400, "无效的刷新令牌"));

        return Ok(ApiResponse<AuthResponse>.Success(new AuthResponse(
            result.AccessToken,
            result.RefreshToken,
            result.ExpiresIn,
            new UserDto(result.User.Id, result.User.Nickname, result.User.Avatar, result.User.VersionMode)
        )));
    }
}
