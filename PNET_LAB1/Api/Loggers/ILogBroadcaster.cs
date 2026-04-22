namespace Api.Loggers;

public interface ILogBroadcaster
{
    Task SendAsync(LogMessage log);
}