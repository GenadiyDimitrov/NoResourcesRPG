namespace NoResourcesRPG.Shared.Static;

public static class SignalRMethods
{
    //server to client
    public const string UpdatePlayer = "UpdatePlayer";
    public const string UpdateNearby = "UpdateNearby";

    public const string UpdateNearbyPlayer = "UpdateNearbyPlayer";
    public const string NearbyPlayerDisconnected = "NearbyPlayerDisconnected";



    //client to server
    public const string JoinGame = "JoinGame";
    public const string MovePlayer = "MovePlayer";
    public const string CollectResource = "CollectResource";
    public const string Action = "Action";
}

