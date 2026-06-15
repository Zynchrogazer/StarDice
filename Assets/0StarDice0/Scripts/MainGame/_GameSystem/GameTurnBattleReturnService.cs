using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles board cleanup after battle scenes without mixing collision/relocation rules into the turn FSM.
/// </summary>
public static class GameTurnBattleReturnService
{
    public static void RelocateBattleMonster(PlayerState endedPlayer, IReadOnlyList<PlayerState> allPlayers)
    {
        if (endedPlayer == null || !RouteManager.TryGet(out var routeManager))
        {
            return;
        }

        PlayerPathWalker endedWalker = endedPlayer.GetComponent<PlayerPathWalker>();
        if (endedWalker == null)
        {
            return;
        }

        PlayerState monsterToRelocate = FindMonsterToRelocate(endedPlayer, allPlayers, endedWalker.currentNodeID);
        if (monsterToRelocate != null)
        {
            RelocateMovingMonster(monsterToRelocate, routeManager, allPlayers);
        }
    }

    private static PlayerState FindMonsterToRelocate(PlayerState endedPlayer, IReadOnlyList<PlayerState> allPlayers, int battleTileId)
    {
        if (endedPlayer.isAI)
        {
            return endedPlayer;
        }

        for (int i = 0; i < allPlayers.Count; i++)
        {
            PlayerState player = allPlayers[i];
            if (player == null || player == endedPlayer || !player.isAI)
            {
                continue;
            }

            PlayerPathWalker walker = player.GetComponent<PlayerPathWalker>();
            if (walker != null && walker.currentNodeID == battleTileId)
            {
                return player;
            }
        }

        return null;
    }

    private static void RelocateMovingMonster(PlayerState monsterAI, RouteManager routeManager, IReadOnlyList<PlayerState> allPlayers)
    {
        HashSet<int> occupiedIds = CollectOccupiedNodeIds(allPlayers);
        List<Transform> candidateNodes = CollectRelocationCandidates(routeManager, occupiedIds);
        if (candidateNodes.Count == 0)
        {
            Debug.LogWarning("⚠️ ไม่มีช่องว่างเหลือให้มอนสเตอร์หนีเลย!");
            return;
        }

        Transform randomNode = candidateNodes[Random.Range(0, candidateNodes.Count)];
        PlayerPathWalker aiWalker = monsterAI.GetComponent<PlayerPathWalker>();
        if (aiWalker == null)
        {
            return;
        }

        aiWalker.TeleportToNode(randomNode);
        Debug.Log($"<color=orange>💨 [Manager] จับมอนสเตอร์ AI ({monsterAI.name}) วาร์ปหนีไปซ่อนที่ {randomNode.name} แล้ว!</color>");
    }

    private static HashSet<int> CollectOccupiedNodeIds(IReadOnlyList<PlayerState> allPlayers)
    {
        HashSet<int> occupiedIds = new HashSet<int>();
        for (int i = 0; i < allPlayers.Count; i++)
        {
            PlayerPathWalker walker = allPlayers[i]?.GetComponent<PlayerPathWalker>();
            if (walker != null)
            {
                occupiedIds.Add(walker.currentNodeID);
            }
        }

        return occupiedIds;
    }

    private static List<Transform> CollectRelocationCandidates(RouteManager routeManager, HashSet<int> occupiedIds)
    {
        List<Transform> candidateNodes = new List<Transform>();
        foreach (var nodeConnection in routeManager.nodeConnections)
        {
            if (nodeConnection == null || nodeConnection.node == null)
            {
                continue;
            }

            bool isBlockedTile = nodeConnection.type == TileType.Start || nodeConnection.type == TileType.Shop;
            if (!isBlockedTile && !occupiedIds.Contains(nodeConnection.tileID))
            {
                candidateNodes.Add(nodeConnection.node);
            }
        }

        return candidateNodes;
    }
}
