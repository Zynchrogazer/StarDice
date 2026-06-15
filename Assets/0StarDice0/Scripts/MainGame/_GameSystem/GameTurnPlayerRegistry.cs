using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Keeps player discovery/reconnect logic out of GameTurnManager so the manager can focus on FSM transitions.
/// </summary>
public static class GameTurnPlayerRegistry
{
    public static Scene RefreshBoardPlayers(List<PlayerState> players)
    {
        players.Clear();

        RouteManager currentMap = Object.FindFirstObjectByType<RouteManager>();
        Scene boardScene = currentMap != null ? currentMap.gameObject.scene : SceneManager.GetActiveScene();
        if (currentMap == null)
        {
            Debug.LogError("😱 [Manager] ไม่เจอ RouteManager ในฉากนี้!");
        }

        PlayerState[] discoveredPlayers = Object.FindObjectsByType<PlayerState>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < discoveredPlayers.Length; i++)
        {
            PlayerState player = discoveredPlayers[i];
            if (!IsValidBoardPlayer(player, boardScene))
            {
                continue;
            }

            players.Add(player);
            ReconnectWalker(player, currentMap);
        }

        players.Sort(CompareTurnOrder);
        Debug.Log($"<color=green>[Manager] ♻️ Refresh Players & Map: {players.Count} players from board scene '{boardScene.name}'</color>");
        return boardScene;
    }

    private static bool IsValidBoardPlayer(PlayerState player, Scene boardScene)
    {
        return player != null && player.gameObject != null && player.gameObject.scene == boardScene;
    }

    private static void ReconnectWalker(PlayerState player, RouteManager currentMap)
    {
        if (currentMap == null)
        {
            return;
        }

        PlayerPathWalker walker = player.GetComponent<PlayerPathWalker>();
        walker?.ReconnectReferences(currentMap);
    }

    private static int CompareTurnOrder(PlayerState a, PlayerState b)
    {
        int typeComparison = a.isAI.CompareTo(b.isAI);
        return typeComparison != 0 ? typeComparison : string.Compare(a.name, b.name);
    }
}
