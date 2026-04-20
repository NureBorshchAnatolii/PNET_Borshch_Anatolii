using Domain.Enums;

namespace Application.Models;

public record UserResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    UserRole Role,
    DateTime CreatedAt
);