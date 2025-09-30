namespace NoResourcesRPG.Client.Services;

public interface ITokenService
{
    Task SetTokenAsync(string token);
    Task<string?> GetTokenAsync();
    Task RemoveTokenAsync();
}

public interface ICharacterService
{
    public Task SetCurrentCharacterAsync(string id);
    public Task<string?> GetCurrentCharacterAsync();
    public Task RemoveCurrentCharacterAsync();
}
