namespace Application.Models;

public record TestReportResponse(
    Guid TestId,
    string Title,
    string Category,
    decimal PassingScore,
    decimal AvgScore,
    int AttemptCount,
    string Status
);