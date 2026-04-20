namespace Application.Models;

public record UpdateProfileRequest(
    string FirstName,
    string LastName,
    DateTime? BirthDate
);