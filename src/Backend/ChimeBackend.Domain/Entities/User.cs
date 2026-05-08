using ChimeBackend.Domain.Enums;

namespace ChimeBackend.Domain.Entities;

public class User
{
    public int Id { get; set; }

    // 认证相关
    public string? OpenId { get; set; }        // 微信OpenId
    public string? UnionId { get; set; }        // 微信UnionId
    public string? PhoneNumber { get; set; }   // 手机号

    // 基础信息
    public string? Nickname { get; set; }
    public string? Avatar { get; set; }
    public Gender? Gender { get; set; }
    public decimal? Height { get; set; }        // 身高(cm)
    public decimal? Weight { get; set; }        // 体重(kg)
    public int? Age { get; set; }

    // 偏好设置
    public VersionMode? VersionMode { get; set; } = null;
    public ActivityLevel ActivityLevel { get; set; } = ActivityLevel.Light;
    public int Goal { get; set; }              // 0-维持，1-减脂，2-增肌
    public decimal? DailyCalorie { get; set; }     // 自定义每日热量目标

    // 状态
    public int Status { get; set; } = 1;      // 0-禁用，1-正常
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }

    // 导航属性
    public ICollection<FoodRecord> FoodRecords { get; set; } = new List<FoodRecord>();
    public ICollection<DailySummary> DailySummaries { get; set; } = new List<DailySummary>();
    public ICollection<CustomFood> CustomFoods { get; set; } = new List<CustomFood>();
}
