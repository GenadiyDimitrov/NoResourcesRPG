namespace NoResourcesRPG.Shared.Models;

public record class ResourceInventoryData
{
    public string Color { get; set; }
    public string Name { get; set; }
    public int Amount { get; set; } = 1;
}
