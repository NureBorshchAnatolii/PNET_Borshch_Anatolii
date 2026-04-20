using Application.Contacts;
using Application.Models;
using Domain.Enums;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Persistance.DbContext;

namespace Identity.Auth;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _db;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly JwtSettings _jwtSettings;
 
    public AuthService(
        ApplicationDbContext db,
        IJwtTokenGenerator jwtTokenGenerator,
        IOptions<JwtSettings> jwtSettings)
    {
        _db = db;
        _jwtTokenGenerator = jwtTokenGenerator;
        _jwtSettings = jwtSettings.Value;
    }
 
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var emailTaken = await _db.Users
            .AnyAsync(u => u.Email == request.Email.ToLower(), ct);
 
        if (emailTaken)
            throw new InvalidOperationException("Email is already in use.");
 
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.ToLower().Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            BirthDate = request.BirthDate,
            CreateDate = DateTime.UtcNow,
            Role = UserRole.User
        };
 
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
 
        return BuildAuthResponse(user);
    }
 
    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower(), ct);
 
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");
 
        return BuildAuthResponse(user);
    }
 
    private AuthResponse BuildAuthResponse(User user)
    {
        var token = _jwtTokenGenerator.GenerateToken(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);
 
        return new AuthResponse(
            Token: token,
            Email: user.Email,
            FirstName: user.FirstName,
            LastName: user.LastName,
            Role: user.Role.ToString(),
            ExpiresAt: expiresAt
        );
    }
}