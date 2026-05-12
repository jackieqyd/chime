namespace ChimeBackend.Application.Services;

public interface IWxService
{
    /// <summary>
    /// 通过微信 code 换取 openid
    /// </summary>
    Task<string> GetOpenIdAsync(string code, CancellationToken cancellationToken = default);
}
