using Application.Contacts;
using Application.Models;
using Domain.Enums;
using Domain.Model;

namespace Application.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepo;

    public UserService(IUserRepository userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task<UserProfileResponse> GetProfileAsync(Guid userId)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        return MapToProfile(user);
    }

    public async Task UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (string.IsNullOrWhiteSpace(request.FirstName))
            throw new ArgumentException("First name cannot be empty.");

        if (string.IsNullOrWhiteSpace(request.LastName))
            throw new ArgumentException("Last name cannot be empty.");

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.BirthDate = request.BirthDate;

        await _userRepo.UpdateAsync(user);
    }

    public async Task<IEnumerable<UserResponse>> GetAllUsersAsync()
    {
        var users = await _userRepo.GetAllAsync();
        return users.Select(MapToUserResponse);
    }

    public async Task ChangeUserRoleAsync(Guid userId, UserRole newRole)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (!Enum.IsDefined(typeof(UserRole), newRole))
            throw new ArgumentException("Invalid role.");

        user.Role = newRole;
        await _userRepo.UpdateAsync(user);
    }

    public async Task DeleteUserAsync(Guid userId)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        await _userRepo.DeleteAsync(userId);
    }

    private static UserProfileResponse MapToProfile(User u) =>
        new(u.Id, u.FirstName, u.LastName, u.Email,
            u.BirthDate, u.CreateDate, u.Role);

    private static UserResponse MapToUserResponse(User u) =>
        new(u.Id, u.FirstName, u.LastName, u.Email, u.Role, u.CreateDate);
}