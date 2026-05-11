namespace ChimeBackend.Application.DTOs;

public record AuthResult(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    UserInfoResult User,
    bool IsNewUser
);

public record UserInfoResult(
    int Id,
    string? Nickname,
    string? Avatar,
    int? VersionMode
);
