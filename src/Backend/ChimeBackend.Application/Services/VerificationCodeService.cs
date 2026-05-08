namespace ChimeBackend.Application.Services;

public interface IVerificationCodeService
{
    void SendCode(string phoneNumber);
    bool ValidateCode(string phoneNumber, string code);
    string GetCurrentCode(string phoneNumber); // 仅用于开发环境
}

public class InMemoryVerificationCodeService : IVerificationCodeService
{
    private readonly Dictionary<string, (string code, DateTime expiresAt)> _codes = new();

    public void SendCode(string phoneNumber)
    {
        // 开发环境：生成6位数字验证码
        var code = Random.Shared.Next(100000, 999999).ToString();
        var expiresAt = DateTime.UtcNow.AddMinutes(5);
        _codes[phoneNumber] = (code, expiresAt);

        // TODO: 生产环境应调用短信网关（如阿里云）发送真实验证码
        Console.WriteLine($"[验证码] {phoneNumber}: {code}");
    }

    public bool ValidateCode(string phoneNumber, string code)
    {
        if (!_codes.TryGetValue(phoneNumber, out var stored))
            return false;

        if (DateTime.UtcNow > stored.expiresAt)
        {
            _codes.Remove(phoneNumber);
            return false;
        }

        var isValid = stored.code == code;
        if (isValid)
            _codes.Remove(phoneNumber); // 验证码使用后删除

        return isValid;
    }

    public string GetCurrentCode(string phoneNumber)
    {
        if (_codes.TryGetValue(phoneNumber, out var stored) && DateTime.UtcNow <= stored.expiresAt)
            return stored.code;
        return string.Empty;
    }
}