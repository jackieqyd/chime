using System.Net.Http.Json;
using System.Text.Json;
using ChimeBackend.Application.Services;
using Microsoft.Extensions.Logging;

namespace ChimeBackend.Infrastructure.Services;

public class WxService : IWxService
{
    private readonly HttpClient _httpClient;
    private readonly string _appId;
    private readonly string _appSecret;
    private readonly ILogger<WxService> _logger;

    public WxService(
        HttpClient httpClient,
        ILogger<WxService> logger)
    {
        _httpClient = httpClient;
        _appId = Environment.GetEnvironmentVariable("WECHAT_APP_ID")
            ?? throw new InvalidOperationException("WECHAT_APP_ID environment variable is not set");
        _appSecret = Environment.GetEnvironmentVariable("WECHAT_APP_SECRET")
            ?? throw new InvalidOperationException("WECHAT_APP_SECRET environment variable is not set");
        _logger = logger;
    }

    public async Task<string> GetOpenIdAsync(string code, CancellationToken cancellationToken = default)
    {
        var url = $"https://api.weixin.qq.com/sns/jscode2session?appid={_appId}&secret={_appSecret}&js_code={code}&grant_type=authorization_code";

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
                _logger.LogError("WeChat API error: {errcode} - {errmsg}", errcode, errmsg);
                throw new InvalidOperationException($"WeChat API error: {errcode} - {errmsg}");
            }

            throw new InvalidOperationException("WeChat API response does not contain openid");
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            _logger.LogError(ex, "Failed to get openid from WeChat API");
            throw new InvalidOperationException("Failed to get openid from WeChat API", ex);
        }
    }
}
