using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Gui.Auth;

public sealed class AppAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _httpClient;

    private const string TokenKey = "auth_token";

    public AppAuthStateProvider(ILocalStorageService localStorage, HttpClient httpClient)
    {
        _localStorage = localStorage;
        _httpClient = httpClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorage.GetItemAsync(TokenKey);

        if (string.IsNullOrWhiteSpace(token) || IsTokenExpired(token))
        {
            ClearHttpClientToken();
            return Unauthenticated();
        }

        SetHttpClientToken(token);

        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task MarkUserAsAuthenticatedAsync(string token)
    {
        await _localStorage.SetItemAsync(TokenKey, token);
        SetHttpClientToken(token);

        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public async Task MarkUserAsLoggedOutAsync()
    {
        await _localStorage.RemoveItemAsync(TokenKey);
        ClearHttpClientToken();
        NotifyAuthenticationStateChanged(Task.FromResult(Unauthenticated()));
    }
    
    private static AuthenticationState Unauthenticated() =>
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    private void SetHttpClientToken(string token) =>
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

    private void ClearHttpClientToken() =>
        _httpClient.DefaultRequestHeaders.Authorization = null;

    private static bool IsTokenExpired(string token)
    {
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        return jwt.ValidTo < DateTime.UtcNow;
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var claims = new List<Claim>();

        foreach (var claim in jwt.Claims)
        {
            if (claim.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                claims.Add(new Claim(ClaimTypes.Role, claim.Value));

            else if (claim.Type == JwtRegisteredClaimNames.Sub)
            {
                claims.Add(claim);
                claims.Add(new Claim(ClaimTypes.NameIdentifier, claim.Value));
            }

            else if (claim.Type == JwtRegisteredClaimNames.Email)
            {
                claims.Add(claim);
                claims.Add(new Claim(ClaimTypes.Email, claim.Value));
            }

            else
            {
                claims.Add(claim);
            }
        }

        return claims;
    }
}