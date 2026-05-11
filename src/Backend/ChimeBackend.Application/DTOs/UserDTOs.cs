namespace ChimeBackend.Application.DTOs;

public record UserProfileResult(
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
