using Microsoft.AspNetCore.SignalR;
using NoResourcesRPG.Server.Hubs;
using NoResourcesRPG.Shared;
using NoResourcesRPG.Shared.DTOs;
using NoResourcesRPG.Shared.Enums;
using NoResourcesRPG.Shared.Models;
using System;
using static System.Collections.Specialized.BitVector32;

namespace NoResourcesRPG.Server.Services;
public class GameWorldService
{
    private readonly int _globalMapSeed;
    private readonly Dictionary<string, Character> _players = [];
    private readonly Dictionary<string, Resource> _resources = [];
    private readonly PriorityQueue<TimedEvent, DateTime> eventQueue = new();
    public GameWorldService()
    {
        _globalMapSeed = 12345;
    }

    public PriorityQueue<TimedEvent, DateTime> Queue => eventQueue;

    private int GetTileSeed(int x, int y)
    {
        unchecked
        {
            int seed = 17;
            seed = seed * 31 + _globalMapSeed; // incorporate global map seed
            seed = seed * 31 + x;
            seed = seed * 31 + y;
            return seed;
        }
    }
    public void RemovePlayer(string playerId)
    {
        _players.Remove(playerId);
    }
    public Character? GetPlayer(string playerId)
    {
        if (!_players.TryGetValue(playerId, out var player) || player is null)
            return null;

        return player;
    }
    public Character? AddPlayer(Character player)
    {
        //todo update from data ? position , inventory, stats, ...
        _players[player.Id] = player;

        return player;
    }

    public Character? UpdatePlayerPosition(PlayerActionDto action)
    {
        // Must have a valid player
        if (GetPlayer(action.PlayerId) is not Character player)
            return null;

        player.Health--;
        if (player.Health < 0)
        {
            player.Health = player.MaxHealth;
            player.Level++;
        }
        int newX = player.X + action.X;
        int newY = player.Y + action.Y;
        if (newX < 0) newX = 0;
        if (newY < 0) newY = 0;
        player.X = newX;
        player.Y = newY;

        return player;
    }

    public Character? CollectResource(PlayerActionDto action)
    {

        string key = $"{_globalMapSeed}_{action.X}_{action.Y}";

        // Must have a valid player
        if (GetPlayer(action.PlayerId) is not Character player)
            return null;


        // If resource was already touched → use it
        if (_resources.TryGetValue(key, out Resource? resource))
        {
            // If resource is empty but not yet ready to respawn
            if (resource.Amount <= 0 && resource.RespawnAt.HasValue && resource.RespawnAt > DateTime.UtcNow)
                return null;
        }
        else
        {
            var seed = GetTileSeed(action.X, action.Y);
            // Procedurally generate the resource (uncollected state)
            if (!Resource.Create(seed, action.X, action.Y, out resource))
                return null;

            // Add to touched resources dictionary
            _resources[key] = resource;
        }

        // Mine the resource
        int keyType = (int)resource.Type;
        if (!player.Inventory.TryGetValue(keyType, out var typeRescourses))
        {
            typeRescourses = [];
            player.Inventory[keyType] = typeRescourses;
        }
        if (!typeRescourses.TryGetValue(resource.SubTypeKey, out var resourceInInventory))
        {
            resourceInInventory = new ResourceInventoryData
            {
                Amount = 0,
                Color = resource.Color,
                Name = resource.Name
            }; // copy without amount
            typeRescourses[resource.SubTypeKey] = resourceInInventory;
        }
        resource.Amount--;
        resourceInInventory.Amount++;

        if (resource.Amount <= 0)
        {
            resource.Amount = 0;
            resource.RespawnAt = DateTime.UtcNow.Add(resource.SubType.RespawnTime); // schedule respawn
        }

        return player;
    }


    public MapUpdateDto GetNearby(string playerId, bool skipRes = false, bool skipPlayers = false)
    {
        // Must have a valid player
        if (GetPlayer(playerId) is not Character player)
            return new MapUpdateDto();

        int viewRange = player.ViewRange;

        var nearbyPlayers = skipPlayers ? [] : _players.Values
            .Where(p => p != player && Math.Abs(p.X - player.X) <= viewRange && Math.Abs(p.Y - player.Y) <= viewRange)
            .ToDictionary(p => p.Id, p => p);

        var nearbyResources = new List<Resource>();
        if (!skipRes)
        {
            // generate resources on the fly for each tile in view
            for (int dx = -viewRange; dx <= viewRange; dx++)
            {
                for (int dy = -viewRange; dy <= viewRange; dy++)
                {
                    int tx = player.X + dx;
                    int ty = player.Y + dy;


                    string key = $"{_globalMapSeed}_{tx}_{ty}";
                    // Check if resource already collected / cached
                    if (_resources.TryGetValue(key, out var existing))
                    {

                        // respawn if timer expired
                        if (existing.Amount == 0)
                        {
                            if (existing.RespawnAt.HasValue && existing.RespawnAt <= DateTime.UtcNow)
                                _resources.Remove(key);
                            else continue;
                        }
                        else
                        {
                            nearbyResources.Add(existing);
                            continue;
                        }
                    }
                    var seed = GetTileSeed(tx, ty);

                    // Procedurally generate a resource for unseen tiles
                    var rndGen = new Random(seed);
                    if (rndGen.NextDouble() < 0.2) // 20% chance
                    {
                        if (Resource.Create(seed, tx, ty, out var res))
                        {
                            // Only store it if collected or waiting for respawn
                            nearbyResources.Add(res);
                        }
                    }
                }
            }
        }
        return new MapUpdateDto
        {
            Player = player,
            Players = nearbyPlayers,
            Resources = nearbyResources,
            TimeOfUpdate = DateTime.UtcNow
        };
    }
    public void AddEvent(TimedEvent e)
    {
        eventQueue.Enqueue(e, e.ExecuteAt);
    }
}
