using System.Collections.Generic;

public static class RouteTileMetadata
{
    public static readonly Dictionary<TileType, string> DefaultEventNames = new Dictionary<TileType, string>
    {
        { TileType.Star, "star" },
        { TileType.Monster, "battle" },
        { TileType.Event, "randomevent" },
        { TileType.Boss, "boss" },
        { TileType.Trap, "trap" },
        { TileType.Heal, "heal" },
        { TileType.Teleport, "warp" },
        { TileType.Minigame, "randomminigame" },
        { TileType.SpecialBoss, "specialboss" },
        { TileType.Draw, "draw" },
        { TileType.Shop, "shop" },
        { TileType.Start, "start" },
        { TileType.Treasure, "treasurebox" },
        { TileType.Lava, "lava" },
        { TileType.iceeffect, "iceeffect" }
    };

    public static string GetDefaultEventName(TileType type)
    {
        return DefaultEventNames.TryGetValue(type, out string eventName) ? eventName : string.Empty;
    }

    public static bool ShouldAutoAssignEventName(TileType type, string currentEventName)
    {
        if (string.IsNullOrWhiteSpace(currentEventName))
        {
            return true;
        }

        string trimmedEventName = currentEventName.Trim();
        string defaultForType = GetDefaultEventName(type);
        if (string.Equals(trimmedEventName, defaultForType, System.StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        foreach (string defaultEvent in DefaultEventNames.Values)
        {
            if (string.Equals(trimmedEventName, defaultEvent, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
