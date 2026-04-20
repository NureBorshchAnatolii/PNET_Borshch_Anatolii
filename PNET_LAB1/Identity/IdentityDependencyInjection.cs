using Application.Contacts;
using Identity.Auth;
using Identity.Jwt;
using Microsoft.Extensions.DependencyInjection;

namespace Identity;

public static class IdentityDependencyInjection
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services)
    {
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IAuthService, AuthService>();
        
        return services;
    }
}