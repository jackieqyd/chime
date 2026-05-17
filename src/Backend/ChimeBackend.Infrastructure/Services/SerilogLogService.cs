using ChimeBackend.Application.Services;
using Serilog;
using Serilog.Events;

namespace ChimeBackend.Infrastructure.Services;

public class SerilogLogService : ILogService
{
    private readonly ILogger _logger;

    public SerilogLogService()
    {
        _logger = Log.Logger;
    }

    public void Information(string message, params object[] args)
    {
        _logger.Information(message, args);
    }

    public void Warning(string message, params object[] args)
    {
        _logger.Warning(message, args);
    }

    public void Error(string message, Exception? exception = null, params object[] args)
    {
        if (exception != null)
        {
            _logger.Error(exception, message, args);
        }
        else
        {
            _logger.Error(message, args);
        }
    }

    public void Debug(string message, params object[] args)
    {
        _logger.Debug(message, args);
    }
}
