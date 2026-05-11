using System.Security.Claims;
using ChimeBackend.Api.DTOs;
using ChimeBackend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChimeBackend.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly UserAppService _userAppService;

    public UsersController(UserAppService userAppService)
    {
        _userAppService = userAppService;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return int.Parse(userIdClaim!.Value);
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetProfile(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _userAppService.GetProfileAsync(userId, cancellationToken);

        if (result == null)
            return NotFound(ApiResponse<UserProfileDto>.Fail(404, "用户不存在"));

        return Ok(ApiResponse<UserProfileDto>.Success(new UserProfileDto(
            result.Id,
            result.Nickname,
            result.Avatar,
            result.Gender,
            result.Height,
            result.Weight,
            result.Age,
            result.VersionMode,
            result.ActivityLevel,
            result.Goal,
            result.DailyCalorie
        )));
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _userAppService.UpdateProfileAsync(
            userId,
            request.Nickname,
            request.Avatar,
            request.Gender,
            request.Height,
            request.Weight,
            request.Age,
            request.VersionMode,
            request.ActivityLevel,
            request.Goal,
            request.DailyCalorie,
            cancellationToken);

        if (result == null)
            return NotFound(ApiResponse<UserProfileDto>.Fail(404, "用户不存在"));

        return Ok(ApiResponse<UserProfileDto>.Success(new UserProfileDto(
            result.Id,
            result.Nickname,
            result.Avatar,
            result.Gender,
            result.Height,
            result.Weight,
            result.Age,
            result.VersionMode,
            result.ActivityLevel,
            result.Goal,
            result.DailyCalorie
        ), "更新成功"));
    }
}

public record UserProfileDto(
    int Id,
    string? Nickname,
    string? Avatar,
    int? Gender,
    decimal? Height,
    decimal? Weight,
    int? Age,
    int? VersionMode,
    int? ActivityLevel,
    int? Goal,
    decimal? DailyCalorie
);

public record UpdateProfileRequest(
    string? Nickname,
    string? Avatar,
    int? Gender,
    decimal? Height,
    decimal? Weight,
    int? Age,
    int? VersionMode,
    int? ActivityLevel,
    int? Goal,
    decimal? DailyCalorie
);
