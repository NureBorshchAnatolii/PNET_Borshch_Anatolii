namespace Api.Loggers;

public class SignalRLoggerProvider : ILoggerProvider
{
    private readonly ILogBroadcaster _broadcaster;

    public SignalRLoggerProvider(ILogBroadcaster broadcaster)
    {
        _broadcaster = broadcaster;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new SignalRLogger(categoryName, _broadcaster);
    }

    public void Dispose()
    {
    }
}