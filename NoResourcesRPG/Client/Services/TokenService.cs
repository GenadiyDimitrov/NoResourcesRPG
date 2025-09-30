
using Microsoft.JSInterop;

namespace NoResourcesRPG.Client.Services;

public class TokenService : ITokenService
{
    private readonly IJSRuntime _js;
    private const string KEY = "authToken";

    public TokenService(IJSRuntime js) => _js = js;

    public async Task SetTokenAsync(string token) => await _js.InvokeVoidAsync("localStorage.setItem", KEY, token);
    public async Task<string?> GetTokenAsync() => await _js.InvokeAsync<string?>("localStorage.getItem", KEY);
    public async Task RemoveTokenAsync() => await _js.InvokeVoidAsync("localStorage.removeItem", KEY);
}


public class CharacterService : ICharacterService
{
    private readonly IJSRuntime _js;
    private const string KEY = "charSelection";

    public CharacterService(IJSRuntime js) => _js = js;

    public async Task SetCurrentCharacterAsync(string id) => await _js.InvokeVoidAsync("localStorage.setItem", KEY, id);
    public async Task<string?> GetCurrentCharacterAsync() => await _js.InvokeAsync<string?>("localStorage.getItem", KEY);
    public async Task RemoveCurrentCharacterAsync() => await _js.InvokeVoidAsync("localStorage.removeItem", KEY);
}