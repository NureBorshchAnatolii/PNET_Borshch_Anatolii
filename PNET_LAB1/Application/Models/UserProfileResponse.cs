using Domain.Enums;

namespace Application.Models;

public record UserProfileResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    DateTime? BirthDate,
    DateTime CreatedAt,
    UserRole Role
);