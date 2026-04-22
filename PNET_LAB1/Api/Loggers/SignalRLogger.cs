namespace Api.Loggers;

public class SignalRLogger : ILogger
{
    private readonly ILogBroadcaster _broadcaster;
    private readonly string _categoryName;

    public SignalRLogger(string categoryName, ILogBroadcaster broadcaster)
    {
        _categoryName = categoryName;
        _broadcaster = broadcaster;
    }

    public IDisposable? BeginScope<TState>(TState state)
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);

        var log = new LogMessage
        {
            Level = logLevel.ToString(),
            Category = _categoryName,
            Message = message,
            Exception = exception?.ToString(),
            Timestamp = DateTime.UtcNow
        };

        _ = _broadcaster.SendAsync(log);
    }
}