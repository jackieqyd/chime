using ChimeBackend.Application.DTOs;
using ChimeBackend.Application.Extensions;
using ChimeBackend.Domain.Entities;
using ChimeBackend.Domain.Enums;
using ChimeBackend.Domain.Repositories;

namespace ChimeBackend.Application.Services;

public class UserAppService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogService _logService;

    public UserAppService(IUserRepository userRepository, ILogService logService)
    {
        _userRepository = userRepository;
        _logService = logService;
    }

    public async Task<UserProfileResult?> GetProfileAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null) return null;

            return new UserProfileResult(
                user.Id,
                user.Nickname,
                user.Avatar,
                (int?)user.Gender,
                user.Height,
                user.Weight,
                user.Age,
                (int?)user.VersionMode,
                (int?)user.ActivityLevel,
                (int?)user.Goal,
                user.DailyCalorie
            );
        }
        catch (Exception ex)
        {
            _logService.Error("GetProfileAsync failed: UserId={UserId}", ex, userId);
            return null;
        }
    }

    public async Task<UserProfileResult?> UpdateProfileAsync(
        int userId,
        string? nickname,
        string? avatar,
        int? gender,
        decimal? height,
        decimal? weight,
        int? age,
        int? versionMode,
        int? activityLevel,
        int? goal,
        decimal? dailyCalorie,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null) return null;

            if (nickname != null) user.Nickname = nickname;
            if (avatar != null) user.Avatar = avatar;
            if (gender.HasValue) user.Gender = (Gender)gender.Value;
            if (height.HasValue) user.Height = height.Value;
            if (weight.HasValue) user.Weight = weight.Value;
            if (age.HasValue) user.Age = age.Value;
            if (versionMode.HasValue) user.VersionMode = (VersionMode)versionMode.Value;
            if (activityLevel.HasValue) user.ActivityLevel = (ActivityLevel)activityLevel.Value;
            if (goal.HasValue) user.Goal = goal.Value;
            if (dailyCalorie.HasValue) user.DailyCalorie = dailyCalorie.Value;

            // 根据性别、体重、身高、年龄计算BMR并更新DailyCalorie
            if (user.Gender.HasValue && user.Weight.HasValue && user.Height.HasValue && user.Age.HasValue)
            {
                user.DailyCalorie = CalculateBmr(user.Gender.Value, user.Weight.Value, user.Height.Value, user.Age.Value);
            }

            user.UpdatedAt = DateTime.UtcNow;
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync(cancellationToken);

            return new UserProfileResult(
                user.Id,
                user.Nickname,
                user.Avatar,
                (int?)user.Gender,
                user.Height,
                user.Weight,
                user.Age,
                (int?)user.VersionMode,
                (int?)user.ActivityLevel,
                (int?)user.Goal,
                user.DailyCalorie
            );
        }
        catch (Exception ex)
        {
            _logService.Error("UpdateProfileAsync failed: UserId={UserId}", ex, userId);
            return null;
        }
    }

    private static decimal CalculateBmr(Gender gender, decimal weight, decimal height, int age)
    {
        // 男性：10*体重 + 6.25*身高 - 5*年龄 + 5
        // 女性：10*体重 + 6.25*身高 - 5*年龄 - 161
        return gender == Gender.Male
            ? 10m * weight + 6.25m * height - 5m * age + 5m
            : 10m * weight + 6.25m * height - 5m * age - 161m;
    }
}
