using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NoResourcesRPG.Server.Database;
using NoResourcesRPG.Server.Mappers;
using NoResourcesRPG.Server.Services;
using NoResourcesRPG.Shared;
using NoResourcesRPG.Shared.DTOs;
using NoResourcesRPG.Shared.Models;
using System;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace NoResourcesRPG.Server.Hubs;
[Authorize] // requires valid JWT
public class GameHub : Hub
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly GameWorldService _world;
    private readonly Random _r = new();
    public static readonly Dictionary<string, string> _connectionIdToPlayerId = [];
    public static readonly Dictionary<string, string> _playerIdToConnectionId = [];
    public static readonly List<string> _dirtyPlayers = [];
    public GameHub(GameWorldService world, IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _world = world;
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (!_connectionIdToPlayerId.TryGetValue(Context.ConnectionId, out var playerId))
            return base.OnDisconnectedAsync(exception);
        _connectionIdToPlayerId.Remove(Context.ConnectionId);
        _playerIdToConnectionId.Remove(playerId);

        var nearby = _world.GetNearby(playerId, true);

        _world.RemovePlayer(playerId);

        if (nearby == null)
            return base.OnDisconnectedAsync(exception);

        if (nearby.Players != null && nearby.Players.Count > 0)
        {
            IEnumerable<string> otherIds = nearby.Players.Keys;
            SendDisconnect(otherIds, playerId);
        }


        return base.OnDisconnectedAsync(exception);
    }
    public async Task JoinGame(string characterId)
    {
        var userId = Context.UserIdentifier; // comes from JWT "sub" claim
        var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NoResDbContext>();

        var entity = await dbContext.Characters.FindAsync(characterId);
        if (entity == null || entity.UserId != userId)
        {
            // invalid character or not owned by user
            return;
        }
        var player = _world.AddPlayer(entity.Adapt<Character>());
        if (player != null)
        {
            string connectionId = Context.ConnectionId;
            _connectionIdToPlayerId[connectionId] = player.Id;
            _playerIdToConnectionId[player.Id] = connectionId;
            SendUpdates(player);
        }
    }

    public void MovePlayer(PlayerActionDto action)
    {
        var nearby = _world.GetNearby(action.PlayerId, true);
        var player = _world.UpdatePlayerPosition(action);
        SendUpdates(player, nearby.Players.Keys);
    }

    public void CollectResource(PlayerActionDto action)
    {
        var player = _world.CollectResource(action);
        SendUpdates(player);
    }
    private void SendUpdates(Character? player, IEnumerable<string>? oldNearby = null)
    {
        if (player == null) return;

        var nearby = _world.GetNearby(player.Id);

        if (nearby == null) return;

        //_ = Clients.Caller.SendAsync(SignalRMethods.UpdatePlayer, player);
        //_ = Clients.Caller.SendAsync(SignalRMethods.UpdateNearby, nearby);


        /*IEnumerable<string>*/
        oldNearby ??= [];
        IEnumerable<string> currentNearby = nearby.Players != null && nearby.Players.Count > 0 ? nearby.Players.Keys : [];

        _dirtyPlayers.Add(player.Id);
        _dirtyPlayers.AddRange(oldNearby);
        _dirtyPlayers.AddRange(currentNearby);

        //var removedNearbyP = oldNearby.Except(currentNearby);
        //if (removedNearbyP.Any())
        //    SendDisconnect(removedNearbyP, player.Id); //notify removed players
        //if (currentNearby.Any())
        //    _ = Clients.Clients(GetConnectionIds(currentNearby)).SendAsync(SignalRMethods.UpdateNearbyPlayer, player); //notify current players
    }
    private void SendDisconnect(IEnumerable<string>? playerIds, string dcId)
    {
        _dirtyPlayers.AddRange(playerIds ?? []);
        //_ = Clients.Clients(GetConnectionIds(playerIds)).SendAsync(SignalRMethods.NearbyPlayerDisconnected, dcId);
    }

    private static IEnumerable<string> GetConnectionIds(IEnumerable<string>? playerIds)
    {
        if (playerIds is null || !playerIds.Any()) return [];
        HashSet<string> ids = [];
        foreach (var p in playerIds)
        {
            if (!_playerIdToConnectionId.TryGetValue(p, out var id)) continue;
            ids.Add(id);
        }
        return ids;
    }
}

