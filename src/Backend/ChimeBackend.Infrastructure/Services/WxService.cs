using System.Net.Http.Json;
using System.Text.Json;
using ChimeBackend.Application.Services;

namespace ChimeBackend.Infrastructure.Services;

public class WxService : IWxService
{
    private readonly HttpClient _httpClient;
    private readonly ILogService _logger;
    private readonly WeChatSettings _weChatSettings;

    public WxService(
        HttpClient httpClient,
        ILogService logger,
        WeChatSettings weChatSettings)
    {
        _httpClient = httpClient;
        _logger = logger;
        _weChatSettings = weChatSettings ?? throw new ArgumentNullException(nameof(weChatSettings));
    }

    public async Task<string> GetOpenIdAsync(string code, CancellationToken cancellationToken = default)
    {
        var url = $"https://api.weixin.qq.com/sns/jscode2session?appid={_weChatSettings.AppId}&secret={_weChatSettings.AppSecret}&js_code={code}&grant_type=authorization_code";

        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);

            if (json.TryGetProperty("openid", out var openidProp))
            {
                return openidProp.GetString() ?? throw new InvalidOperationException("openid is null");
            }

            if (json.TryGetProperty("errcode", out var errcodeProp))
            {
                var errcode = errcodeProp.GetInt32();
                var errmsg = json.TryGetProperty("errmsg", out var errmsgProp) ? errmsgProp.GetString() : "unknown error";
                _logger.Error("WeChat API error: {errcode} - {errmsg}", null, errcode, errmsg);
                throw new InvalidOperationException($"WeChat API error: {errcode} - {errmsg}");
            }

            throw new InvalidOperationException("WeChat API response does not contain openid");
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            _logger.Error("Failed to get openid from WeChat API", ex);
            throw new InvalidOperationException("Failed to get openid from WeChat API", ex);
        }
    }
}
