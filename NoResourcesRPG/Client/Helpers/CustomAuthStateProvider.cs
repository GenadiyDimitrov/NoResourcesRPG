
using NoResourcesRPG.Client.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;


namespace NoResourcesRPG.Client.Helpers;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ITokenService _tokenService;
    private ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public CustomAuthStateProvider(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _tokenService.GetTokenAsync();
        if (string.IsNullOrEmpty(token)) return new AuthenticationState(_anonymous);

        var identity = ParseClaimsFromJwt(token);
        var user = new ClaimsPrincipal(new ClaimsIdentity(identity, "jwt"));
        return new AuthenticationState(user);
    }

    public void NotifyAuthenticationStateChanged() => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwt);
        return token.Claims;
    }
    public async Task Logout()
    {
        await _tokenService.RemoveTokenAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }
}
