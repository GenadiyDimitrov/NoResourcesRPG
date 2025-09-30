using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NoResourcesRPG.Shared.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NoResourcesRPG.Server.Database.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class CharacterEntity
    {
        public string Id { get; set; }

        public string Name { get; set; } = null!;
        public string Color { get; set; } = "#fff";
        public int Level { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Health { get; set; } = 100;
        public int MaxHealth { get; set; } = 100;
        public int ViewRange { get; set; }

        //navigation
        public ICollection<InventoryItemEntity> Inventory { get; set; } = new List<InventoryItemEntity>();


        // Foreign key
        public string UserId { get; set; }
        public UserEntity User { get; set; }
    }
    public class InventoryItemEntity
    {
        public string Id { get; set; }
        public string? TemplateId { get; set; }
        public ItemTemplateEntity? Template { get; set; }
        public int Quantity { get; set; }

        // Foreign key
        public string CharacterId { get; set; }
        public CharacterEntity Character { get; set; }
    }
    public class ItemTemplateEntity
    {
        public string Id { get; set; }
        public string Name { get; set; } = null!;
        public string ItemType { get; set; } = null!;
        public int BasePower { get; set; }

        //navigation
        public ICollection<InventoryItemEntity> Instances { get; set; } = new List<InventoryItemEntity>();
    }
    [Index(nameof(DisplayName), IsUnique = true)]
    [Index(nameof(UserName))]
    public class UserEntity : IdentityUser<string>
    {
        [Required]
        public string? DisplayName { get; set; }
        public ICollection<CharacterEntity> Characters { get; set; } = new List<CharacterEntity>();
    }
    public class UserRoleEntity : IdentityRole<string>
    {
    }
}
