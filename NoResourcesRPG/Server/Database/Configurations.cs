using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NoResourcesRPG.Server.Database.Models;

namespace NoResourcesRPG.Server.Database
{
    // CharacterEntity Fluent configuration
    public class CharacterEntityConfig : IEntityTypeConfiguration<CharacterEntity>
    {
        public void Configure(EntityTypeBuilder<CharacterEntity> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(c => c.Level)
                   .IsRequired();

            // One user → many characters
            builder.HasOne(c => c.User)
                   .WithMany(u => u.Characters)
                   .HasForeignKey(c => c.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // One-to-many with inventory
            builder.HasMany(c => c.Inventory)
                   .WithOne(i => i.Character)
                   .HasForeignKey(i => i.CharacterId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

    // Inventory configuration
    public class InventoryItemEntityConfig : IEntityTypeConfiguration<InventoryItemEntity>
    {
        public void Configure(EntityTypeBuilder<InventoryItemEntity> builder)
        {
            builder.HasKey(i => i.Id);

            builder.Property(i => i.Quantity)
                   .IsRequired();

            // Optional relationship to template
            builder.HasOne(i => i.Template)
                   .WithMany(t => t.Instances)
                   .HasForeignKey(i => i.TemplateId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }

    // ItemTemplate configuration
    public class ItemTemplateEntityConfig : IEntityTypeConfiguration<ItemTemplateEntity>
    {
        public void Configure(EntityTypeBuilder<ItemTemplateEntity> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(t => t.ItemType)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(t => t.BasePower)
                   .IsRequired();
        }
    }
}
