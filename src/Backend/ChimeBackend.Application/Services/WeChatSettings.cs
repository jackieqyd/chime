namespace ChimeBackend.Application.Services;

public class WeChatSettings
{
    public const string SectionName = "WeChatSettings";
    public string AppId { get; set; } = string.Empty;
    public string AppSecret { get; set; } = string.Empty;
}
