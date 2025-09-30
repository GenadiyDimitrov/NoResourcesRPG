using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NoResourcesRPG.Server.Database;
using NoResourcesRPG.Server.Database.Models;
using NoResourcesRPG.Server.Helpers;
using NoResourcesRPG.Server.Hubs;
using NoResourcesRPG.Server.Mappers;
using NoResourcesRPG.Shared;
using NoResourcesRPG.Shared.Models;
using System;
using static NoResourcesRPG.Server.Helpers.GameLoopHelpers;

namespace NoResourcesRPG.Server.Services
{
    public class GameLoopService : BackgroundService
    {
        private IServiceScopeFactory _scopeFactory;
        private readonly GameWorldService _world;
        private readonly IHubContext<GameHub> _hub;
        public readonly Dictionary<string, Character?> _dirtyPlayers = [];
        private readonly TimeSpan _physicsInterval = TimeSpan.FromMilliseconds(17); // ~60hz 60/s
        private readonly TimeSpan _networkInterval = TimeSpan.FromMilliseconds(100); // 10hz 10/s
        private readonly TimeSpan _backupInterval = TimeSpan.FromSeconds(60); // 1hz 1/m

        private DateTime _lastPhysics = DateTime.UtcNow;
        private DateTime _lastNetwork = DateTime.UtcNow;
        private DateTime _lastBackup = DateTime.UtcNow;

        public GameLoopService(IHubContext<GameHub> hub, GameWorldService world, IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _world = world;
            _hub = hub;
            SetHubContext(hub);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                if (now - _lastPhysics >= _physicsInterval)
                {
                    _lastPhysics = now;
                    DoPhysics(now);
                }

                if (now - _lastNetwork >= _networkInterval)
                {
                    _lastNetwork = now;
                    await FlushNetworkAsync();
                }

                if (now - _lastBackup >= _backupInterval)
                {
                    _lastBackup = now;
                    await BackupAsync();
                }

                await Task.Delay(5, stoppingToken);
            }
        }

        private void DoPhysics(DateTime now)
        {
        }

        private async Task BackupAsync()
        {
            if (_dirtyPlayers.Count == 0) return;
            List<Character?> playersToUpdate;
            lock (_dirtyPlayers) // ensure thread safety if multiple threads add/clear
            {
                if (_dirtyPlayers.Count == 0) return;
                playersToUpdate = [.. _dirtyPlayers.Values];
                _dirtyPlayers.Clear();
            }
            var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetService<NoResDbContext>();
            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            { // Collect ids once, do a single DB round-trip
                var ids = playersToUpdate
                    .Where(c => c != null)
                    .Select(c => c!.Id)
                    .ToList();
                if (ids.Count == 0)
                    return;

                // Load all entities in one query instead of per-player
                var entities = await dbContext.Characters
                    .Where(x => ids.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id);
                foreach (var @char in playersToUpdate)
                {
                    if (@char is null) continue;
                    if (!entities.TryGetValue(@char.Id, out var entity)) continue;

                    // Mapster: dto → entity (in-place update)
                    @char.Adapt(entity);
                }

                // EF is already tracking the entities, no need for UpdateRange
                await dbContext.SaveChangesAsync();

                // Commit transaction
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                // Rollback on error
                await transaction.RollbackAsync();
            }
        }

        private async Task FlushNetworkAsync()
        {
            if (GameHub._dirtyPlayers.Count == 0) return;
            HashSet<string> playersToUpdate = [];
            playersToUpdate.UnionWith(GameHub._dirtyPlayers);
            GameHub._dirtyPlayers.Clear();
            foreach (var playerId in playersToUpdate)
            {
                if (!GameHub._playerIdToConnectionId.TryGetValue(playerId, out var connectionId)) continue;
                var player = _world.GetPlayer(playerId);
                if (player == null) continue;
                var nearby = _world.GetNearby(playerId);
                _hub.Clients.Client(connectionId).SendAsync(SignalRMethods.UpdateNearby, nearby);
                _dirtyPlayers[playerId] = player;
            }
        }
    }

}
