namespace ChimeBackend.Api.DTOs;

public record SendCodeRequest(string PhoneNumber);
public record LoginRequest(string PhoneNumber, string VerificationCode);
public record MiniProgramLoginRequest(string Code);
public record AppleLoginRequest(string IdentityToken, string AuthorizationCode, int RealUserStatus);
public record BindPhoneRequest(string PhoneNumber, string VerificationCode);
public record RefreshTokenRequest(string RefreshToken);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    UserDto? User
);

public record UserDto(
    int Id,
    string? Nickname,
    string? Avatar,
    int? VersionMode
);

public record ApiResponse<T>(int Code, string Message, T? Data)
{
    public static ApiResponse<T> Success(T data, string message = "success") =>
        new(200, message, data);

    public static ApiResponse<T> Fail(int code, string message) =>
        new(code, message, default);
}

public record ApiResponse(int Code, string Message)
{
    public static ApiResponse Success(string message = "success") =>
        new(200, message);

    public static ApiResponse Fail(int code, string message) =>
        new(code, message);
}