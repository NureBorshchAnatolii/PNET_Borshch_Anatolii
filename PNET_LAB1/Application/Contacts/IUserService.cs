using Application.Models;
using Domain.Enums;

namespace Application.Contacts;

public interface IUserService
{
    Task<UserProfileResponse> GetProfileAsync(Guid userId);
    Task UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
    Task<IEnumerable<UserResponse>> GetAllUsersAsync();
    Task ChangeUserRoleAsync(Guid userId, UserRole newRole);
    Task DeleteUserAsync(Guid userId);
}