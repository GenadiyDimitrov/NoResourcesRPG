using NoResourcesRPG.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoResourcesRPG.Shared.Models;
public class Character
{

    public string Id { get; set; }
    public string Color { get; set; }
    public string Name { get; set; }
    public int Level { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Health { get; set; } = 100;
    public int MaxHealth { get; set; } = 100;
    public int ViewRange { get; set; }
    public Dictionary<int, Dictionary<string, ResourceInventoryData>> Inventory { get; set; } = [];
}

