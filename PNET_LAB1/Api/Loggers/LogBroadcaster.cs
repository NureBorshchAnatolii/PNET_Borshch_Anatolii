using Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Api.Loggers;

public class LogBroadcaster : ILogBroadcaster
{
    private readonly IHubContext<LogHub> _hubContext;

    public LogBroadcaster(IHubContext<LogHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task SendAsync(LogMessage log)
    {
        return _hubContext.Clients.All.SendAsync("Log", log);
    }
}