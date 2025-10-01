using NoResourcesRPG.Shared.Enums;
using NoResourcesRPG.Shared.Models;

namespace NoResourcesRPG.Shared.Static;

public static class ResourceData
{
    public static readonly Dictionary<ResourceType, Dictionary<string, ResourceSubType>> TypesAndSubTypes = new()
    {
        { ResourceType.Wood, new()
            {
              {"Oak",   new ResourceSubType("Oak", "#8B5A2B",TimeSpan.FromSeconds(30),1,5)},
              {"Birch",   new ResourceSubType("Birch", "#DEB887",TimeSpan.FromSeconds(60),1,10)},
              {"DarkOak",  new ResourceSubType("Dark Oak", "#654321",TimeSpan.FromSeconds(30),1,5)},
              {"Jungle",   new ResourceSubType("Jungle", "#556B2F",TimeSpan.FromSeconds(10),1,2)},
              {"Cherry",   new ResourceSubType("Cherry", "#D2691E",TimeSpan.FromSeconds(40),1,6)},
              {"Pine",   new ResourceSubType("Pine", "#228B22",TimeSpan.FromSeconds(50),1,8) },
            }
        },
        { ResourceType.Stone, new()
            {
                {"Granite", new ResourceSubType("Granite", "#BEBEBE",TimeSpan.FromSeconds(30),1,10)},
                {"Marble", new ResourceSubType("Marble", "#F5F5F5",TimeSpan.FromSeconds(30),1,10)},
                {"Limestone", new ResourceSubType("Limestone", "#E0E0E0",TimeSpan.FromSeconds(30),1,10)},
                {"Sandstone", new ResourceSubType("Sandstone", "#C2B280",TimeSpan.FromSeconds(30),1,10)},
                {"Basalt", new ResourceSubType("Basalt", "#2F4F4F",TimeSpan.FromSeconds(30),1,10)},
                {"Slate", new ResourceSubType("Slate", "#708090",TimeSpan.FromSeconds(30),1,10) },
            }
        },
        { ResourceType.Herb, new()
            {
                {"Mint", new ResourceSubType("Mint", "#98FF98",TimeSpan.FromSeconds(30),1,10)},
                {"Thyme", new ResourceSubType("Thyme", "#8FBC8F",TimeSpan.FromSeconds(30),1,10)},
                {"Basil", new ResourceSubType("Basil", "#6B8E23",TimeSpan.FromSeconds(30),1,10)},
                {"Rosemary", new ResourceSubType("Rosemary", "#556B2F",TimeSpan.FromSeconds(30),1,10)},
                {"Sage", new ResourceSubType("Sage", "#B2AC88",TimeSpan.FromSeconds(30),1,10)},
                {"Lavender", new ResourceSubType("Lavender", "#E6E6FA",TimeSpan.FromSeconds(30),1,10) },
            }
        }
    };
}