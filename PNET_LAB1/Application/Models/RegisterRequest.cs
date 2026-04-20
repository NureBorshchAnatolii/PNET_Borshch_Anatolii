namespace Application.Models;

public record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    DateTime? BirthDate
);