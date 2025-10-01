using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using NoResourcesRPG.Shared.DTOs;
using NoResourcesRPG.Shared.Models;
using NoResourcesRPG.Shared.Static;

namespace NoResourcesRPG.Client.Services;
public class GameService
{
    private readonly NavigationManager _nav;
    private readonly ICharacterService _characterService;
    private readonly ITokenService _tokenService;
    public HubConnection Connection { get; private set; }

    public event Action<MapUpdateDto> OnMapUpdate;
    public event Action<Character> OnPlayerUpdate;
    public event Action<string> OnPlayerDisconnected;
    public event Action<Character> OnSelfUpdate;

    public GameService(NavigationManager nav, ICharacterService characterService, ITokenService tokenService)
    {
        _tokenService = tokenService;
        _characterService = characterService;
        _nav = nav;
    }

    public async Task Init()
    {
        Connection = new HubConnectionBuilder()
            .WithUrl(_nav.ToAbsoluteUri("/gamehub"), options =>
            {
                options.AccessTokenProvider = async () => await _tokenService.GetTokenAsync();
            })
            .WithAutomaticReconnect()
            .Build();

        Connection.On<string>(SignalRMethods.NearbyPlayerDisconnected, update => OnPlayerDisconnected?.Invoke(update));
        Connection.On<Character>(SignalRMethods.UpdateNearbyPlayer, update => OnPlayerUpdate?.Invoke(update));
        Connection.On<MapUpdateDto>(SignalRMethods.UpdateNearby, update => OnMapUpdate?.Invoke(update));
        Connection.On<Character>(SignalRMethods.UpdatePlayer, update => OnSelfUpdate?.Invoke(update));

        await Connection.StartAsync();
        var charId = await _characterService.GetCurrentCharacterAsync();
        await Connection.InvokeAsync(SignalRMethods.JoinGame, charId);
    }

    public Task Move(PlayerActionDto action) => Connection.InvokeAsync(SignalRMethods.MovePlayer, action);
    public Task Collect(PlayerActionDto action) => Connection.InvokeAsync(SignalRMethods.CollectResource, action);
    public Task Action(PlayerActionDto action) => Connection.InvokeAsync(SignalRMethods.Action, action);
}