
using Mapster;
using NoResourcesRPG.Server.Database.Models;
using NoResourcesRPG.Shared.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NoResourcesRPG.Server.Mappers;
public static class MappingProfiles
{
    public static TypeAdapterConfig CustomConfig { get; }
    public static TypeAdapterConfig Global => TypeAdapterConfig.GlobalSettings;

    static MappingProfiles()
    {
        CustomConfig = CreateCustomConfig();

        RegisterMappings();
    }

    public static void RegisterMappings()
    {
        var config = TypeAdapterConfig.GlobalSettings;
        //config.NewConfig<CharacterEntity, Character>()
        // .Map(dest => dest.DisplayName, src => $"{src.Name} (Lvl {src.Level})")
        // .Map(dest => dest.CurrentXp, src => src.Experience); // if property names differ
    }
    private static TypeAdapterConfig CreateCustomConfig()
    {
        var config = new TypeAdapterConfig();

        //config.NewConfig<CharacterEntity, Character>()
        //      .Map(dest => dest.DisplayName, src => $"{src.Name} (Lvl {src.Level})")
        //      .Map(dest => dest.CurrentXp, src => src.Experience);

        return config;
    }
}
