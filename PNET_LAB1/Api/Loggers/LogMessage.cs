namespace Api.Loggers;

public class LogMessage
{
    public string Level { get; set; } = default!;
    public string Category { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string? Exception { get; set; }
    public DateTime Timestamp { get; set; }
}