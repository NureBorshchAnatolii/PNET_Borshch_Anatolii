using Domain.Model;

namespace Application.Contacts;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}