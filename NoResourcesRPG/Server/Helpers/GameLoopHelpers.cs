using NoResourcesRPG.Server.Hubs;
using NoResourcesRPG.Shared;
using Microsoft.AspNetCore.SignalR;

namespace NoResourcesRPG.Server.Helpers
{
    public static class GameLoopHelpers
    {
        private static readonly double _radius = 5000;
        private static readonly double _radiusSq = _radius * _radius;
        private static IHubContext<GameHub>? _hub;
        public static void SetHubContext(IHubContext<GameHub> hub) => _hub = hub;
        //public static MapDto ProgramMap { get; private set; }

        static GameLoopHelpers()
        {
            //ProgramMap = MapDto.Create("TestMap", 10_000, 100, 100);
        }
    }

}
