using NoResourcesRPG.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NoResourcesRPG.Shared.DTOs;
public class MapUpdateDto
{
    public Character Player { get; set; }
    public Dictionary<string, Character> Players { get; set; } = [];
    public List<Resource> Resources { get; set; } = [];
}
