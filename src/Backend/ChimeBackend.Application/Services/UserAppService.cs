using ChimeBackend.Application.DTOs;
using ChimeBackend.Application.Extensions;
using ChimeBackend.Domain.Entities;
using ChimeBackend.Domain.Enums;
using ChimeBackend.Domain.Repositories;

namespace ChimeBackend.Application.Services;

public class UserAppService
{
    private readonly IUserRepository _userRepository;

    public UserAppService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserProfileResult?> GetProfileAsync(int userId, CancellationToken cancellationToken = default)
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
            (int)user.VersionMode,
            (int)user.ActivityLevel,
            user.Goal,
            user.DailyCalorie
        );
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
            (int)user.VersionMode,
            (int)user.ActivityLevel,
            user.Goal,
            user.DailyCalorie
        );
    }

    public async Task<DailyCalorieResult?> GetDailyCalorieAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null) return null;

        var (bmr, tdee, recommended) = CalculateCalories(user);
        var goalDesc = user.Goal switch
        {
            1 => "减脂",
            2 => "增肌",
            _ => "维持"
        };

        return new DailyCalorieResult(bmr, tdee, recommended, user.Goal, goalDesc);
    }

    private static (decimal bmr, decimal tdee, decimal recommended) CalculateCalories(User user)
    {
        decimal bmr;
        if (user.Gender == Gender.Male)
        {
            bmr = 66.47m + (13.75m * (user.Weight ?? 70)) + (5.003m * (user.Height ?? 170)) - (6.755m * (user.Age ?? 30));
        }
        else
        {
            bmr = 655.1m + (9.563m * (user.Weight ?? 60)) + (1.85m * (user.Height ?? 160)) - (4.676m * (user.Age ?? 30));
        }

        var activityMultiplier = user.ActivityLevel switch
        {
            ActivityLevel.Sedentary => 1.2m,
            ActivityLevel.Light => 1.375m,
            ActivityLevel.Moderate => 1.55m,
            ActivityLevel.High => 1.725m,
            ActivityLevel.Extreme => 1.9m,
            _ => 1.375m
        };

        var tdee = bmr * activityMultiplier;

        decimal recommended;
        if (user.DailyCalorie.HasValue && user.DailyCalorie > 0)
        {
            recommended = user.DailyCalorie.Value;
        }
        else
        {
            recommended = user.Goal switch
            {
                1 => (tdee * 0.8m).Round(),
                2 => (tdee * 1.1m).Round(),
                _ => tdee.Round()
            };
        }

        return (bmr.Round(2), tdee.Round(2), recommended);
    }
}
