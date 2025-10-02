using NoResourcesRPG.Shared.Enums;
using NoResourcesRPG.Shared.Static;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NoResourcesRPG.Shared.Models;
public record class Resource
{
    public static bool Create(int seed, int x, int y, out Resource resource)
    {
        resource = null!;

        if (x < 0 || y < 0)
            return false;

        if (ResourceData.TypesAndSubTypes.Count == 0)
            return false;

        var seedRandom = new Random(seed);

        int index = seedRandom.Next(ResourceData.TypesAndSubTypes.Count);
        var data = ResourceData.TypesAndSubTypes.ElementAt(index);

        var type = data.Key;
        var subTypes = data.Value;

        if (subTypes is null || subTypes.Count == 0)
            return false;

        int indexSub = seedRandom.Next(subTypes.Count);
        var dataSub = subTypes.ElementAt(indexSub);
        var subType = subTypes.ElementAt(indexSub).Value;

        DateTime dateTime = DateTime.UtcNow;
        double minutesSinceEpoch = (DateTime.UtcNow - DateTime.UnixEpoch).TotalMinutes;
        long bucket = (long)(minutesSinceEpoch / subType.RespawnTime.TotalMinutes);

        var rnd = new Random(seed + (int)bucket);
        int amount = rnd.Next(subType.MinAmount, subType.MaxAmount);


        resource = new Resource
        {
            Id = Guid.NewGuid().ToString(),
            Type = type,
            X = x,
            Y = y,
            Amount = amount,
            Name = subType.Name,
            SubTypeKey = dataSub.Key,
            SubType = subType,
            Color = subType.Color
        };

        return true;
    }
    [JsonIgnore] public string Id { get; set; }
    [JsonIgnore] public ResourceSubType SubType { get; set; }
    public string Color { get; set; }
    public string Name { get; set; }
    public string SubTypeKey { get; set; }
    public ResourceType Type { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Amount { get; set; } = 1;
    public DateTime? RespawnAt { get; set; } // null if not collected

}
