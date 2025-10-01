using Mapster;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NoResourcesRPG.Server.Database;
using NoResourcesRPG.Server.Hubs;
using NoResourcesRPG.Server.Services;
using NoResourcesRPG.Shared.Models;
using NoResourcesRPG.Shared.Static;
using System.Collections.Concurrent;
using System.Diagnostics;
namespace NoResourcesRPG.Server.Services;
public class GameLoopService : BackgroundService
{
    private readonly SemaphoreSlim _physicsLock = new(1, 1);
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly GameWorldService _world;
    private readonly IHubContext<GameHub> _hub;
    private readonly ILogger<GameLoopService> _logger;
    // Thread-safe dirty players
    private readonly ConcurrentDictionary<string, Character?> _dirtyPlayers = new();

    private int _pendingPhysicsTicks = 0;


    public GameLoopService(
        IHubContext<GameHub> hub,
        GameWorldService world,
        IServiceScopeFactory scopeFactory,
        ILogger<GameLoopService> logger)
    {
        _scopeFactory = scopeFactory;
        _world = world;
        _hub = hub;
        _logger = logger;

        var now = DateTime.UtcNow;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var physicsInterval = TimeSpan.FromTicks(166667); // ~16.6667 ms
        var networkInterval = TimeSpan.FromMilliseconds(100);
        var backupInterval = TimeSpan.FromSeconds(60);

        var stopwatch = Stopwatch.StartNew();

        var nextPhysics = stopwatch.Elapsed;
        var nextNetwork = stopwatch.Elapsed + networkInterval;
        var nextBackup = stopwatch.Elapsed + backupInterval;

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = stopwatch.Elapsed;

            // Run physics with accumulator (may run multiple times if lagging)
            // --- Increment pending physics ticks ---
            while (now >= nextPhysics)
            {
                _pendingPhysicsTicks++;
                if (_pendingPhysicsTicks > 5)
                    _pendingPhysicsTicks = 5; // cap to avoid spiraling
                nextPhysics += physicsInterval;
            }
            // --- Process one physics tick asynchronously ---
            if (_pendingPhysicsTicks > 0)
            {
                if (await _physicsLock.WaitAsync(0, stoppingToken)) // only run if free
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await DoPhysicsAsync(DateTime.UtcNow);
                        }
                        finally
                        {
                            Interlocked.Decrement(ref _pendingPhysicsTicks);
                            _physicsLock.Release();
                        }
                    }, stoppingToken);
                }
            }

            // Network flush (async, don’t block physics)
            if (now >= nextNetwork)
            {
                _ = Task.Run(FlushNetworkAsync, stoppingToken);
                nextNetwork += networkInterval;
            }

            // Backup (async, don’t block physics)
            if (now >= nextBackup)
            {
                _ = Task.Run(BackupAsync, stoppingToken);
                nextBackup += backupInterval;
            }

            // Sleep until next event
            var nextTick = new[] { nextPhysics, nextNetwork, nextBackup }.Min();
            var delay = nextTick - stopwatch.Elapsed;
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, stoppingToken);
        }
    }

    private async Task DoPhysicsAsync(DateTime now)
    {
        int maxEventsPerTick = 1000;
        int processed = 0;

        while (_world.Queue.Count > 0 && _world.Queue.Peek().ExecuteAt <= now && processed < maxEventsPerTick && processed < maxEventsPerTick)
        {
            var e = _world.Queue.Dequeue();
            await e.Action();

            if (e.Repeating)
            {
                e.ExecuteAt += e.Interval;
                _world.Queue.Enqueue(e, e.ExecuteAt);
            }
            processed++;
        }
    }

    private async Task BackupAsync()
    {
        if (_dirtyPlayers.IsEmpty) return;

        List<Character?> playersToUpdate = [.. _dirtyPlayers.Values];
        _dirtyPlayers.Clear();

        if (playersToUpdate.Count == 0) return;

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<NoResDbContext>();

            await using var transaction = await dbContext.Database.BeginTransactionAsync();

            var ids = playersToUpdate
                .Where(c => c != null)
                .Select(c => c!.Id)
                .ToList();

            if (ids.Count == 0)
                return;

            var entities = await dbContext.Characters
                .Where(x => ids.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id);

            foreach (var @char in playersToUpdate)
            {
                if (@char is null) continue;
                if (!entities.TryGetValue(@char.Id, out var entity)) continue;

                // Mapster: dto → entity
                @char.Adapt(entity);
            }

            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Backup failed");
        }
    }

    private async Task FlushNetworkAsync()
    {
        if (GameHub._dirtyPlayers.Count == 0) return;

        HashSet<string> playersToUpdate = [];
        playersToUpdate.UnionWith(GameHub._dirtyPlayers);
        GameHub._dirtyPlayers.Clear();

        var tasks = playersToUpdate.Select(async playerId =>
        {
            if (!GameHub._playerIdToConnectionId.TryGetValue(playerId, out var connectionId)) return;

            var player = _world.GetPlayer(playerId);
            if (player == null) return;

            var nearby = _world.GetNearby(playerId);
            await _hub.Clients.Client(connectionId).SendAsync(SignalRMethods.UpdateNearby, nearby);

            _dirtyPlayers[playerId] = player;
        });

        await Task.WhenAll(tasks);
    }
}
