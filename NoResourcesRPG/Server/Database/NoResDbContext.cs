using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NoResourcesRPG.Server.Database.Models;
using NoResourcesRPG.Shared.Models;
using System.Collections.Generic;

namespace NoResourcesRPG.Server.Database
{
    public class NoResDbContext : IdentityDbContext<
        IdentityUser<string>,
        IdentityRole<string>,
        string,
        IdentityUserClaim<string>,
        IdentityUserRole<string>,
        IdentityUserLogin<string>,
        IdentityRoleClaim<string>,
        IdentityUserToken<string>>
    {
        public const int BatchSize = 1000;
        public static readonly string DefaultSchema = "public";
        public static readonly string DefaultMigrationsTable = "__EFMigrationsHistory";


        public DbSet<CharacterEntity> Characters => Set<CharacterEntity>();
        public DbSet<InventoryItemEntity> InventoryItems => Set<InventoryItemEntity>();
        public DbSet<ItemTemplateEntity> ItemTemplates => Set<ItemTemplateEntity>();

        public NoResDbContext(DbContextOptions<NoResDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new CharacterEntityConfig());
            modelBuilder.ApplyConfiguration(new InventoryItemEntityConfig());
            modelBuilder.ApplyConfiguration(new ItemTemplateEntityConfig());

            base.OnModelCreating(modelBuilder);
        }
    }
}
