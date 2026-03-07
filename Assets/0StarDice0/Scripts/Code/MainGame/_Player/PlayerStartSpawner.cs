using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PlayerStartSpawner : MonoBehaviour
{
    public RouteManager routeManager;
    public List<PlayerPathWalker> allPlayers;

    // สมุดจดตำแหน่ง (Static)
    public static Dictionary<string, int> LastKnownPositions = new Dictionary<string, int>();

    public static void SaveAllPlayersPositions(List<PlayerPathWalker> players)
    {
        LastKnownPositions.Clear();
        foreach (var p in players)
        {
            if (p != null)
            {
                if (LastKnownPositions.ContainsKey(p.name))
                    LastKnownPositions[p.name] = p.currentNodeID;
                else
                    LastKnownPositions.Add(p.name, p.currentNodeID);

                Debug.Log($"💾 Save Position: {p.name} at Node {p.currentNodeID}");
            }
        }
    }

    IEnumerator Start()
    {
        // ⏳ รอให้ RouteManager พร้อมจริงๆ
        while (routeManager == null || routeManager.nodeConnections == null || routeManager.nodeConnections.Count == 0)
        {
            Debug.Log("⏳ Waiting for RouteManager to initialize...");
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        // อัปเดตรายชื่อคนใน GameTurnManager
        if (GameTurnManager.Instance != null)
        {
            GameTurnManager.Instance.allPlayers.Clear();
            foreach (var p in allPlayers)
            {
                if (p != null)
                {
                    // 🛠️ แก้ไข: ต้องดึง GetComponent<PlayerState>() เพราะ GameTurnManager เก็บ PlayerState ไม่ใช่ Walker
                    var state = p.GetComponent<PlayerState>();
                    if (state != null)
                    {
                        GameTurnManager.Instance.allPlayers.Add(state);
                    }
                }
            }
        }

        SpawnAllPlayers();
    }

    public void SpawnAllPlayers()
    {
        // หาจุด Start ทั้งหมดเตรียมไว้สำหรับ Player
        List<NodeConnection> startNodes = routeManager.nodeConnections
            .Where(n => n.type == TileType.Start).ToList();

        foreach (var playerWalker in allPlayers)
        {
            if (playerWalker == null) continue;

            playerWalker.gameObject.SetActive(true);
            NodeConnection targetData = null;

            // 1. เช็คว่ามีตำแหน่งเดิมที่ Save ไว้ไหม? (สำหรับตอนกลับมาจากฉากต่อสู้)
            if (LastKnownPositions.ContainsKey(playerWalker.name))
            {
                int savedID = LastKnownPositions[playerWalker.name];
                targetData = routeManager.nodeConnections.Find(x => x.tileID == savedID);
            }

            // 2. ถ้าไม่มี Save (เพิ่งเริ่มเกม) -> ใช้ Logic สุ่มเกิดตามที่คุณขอ
            if (targetData == null)
            {
                PlayerState state = playerWalker.GetComponent<PlayerState>();

                if (state != null && state.isAI)
                {
                    // 🤖 AI: สุ่มเกิดที่ "ช่องไหนก็ได้"
                    if (routeManager.nodeConnections.Count > 0)
                    {
                        int randomIndex = Random.Range(0, routeManager.nodeConnections.Count);
                        targetData = routeManager.nodeConnections[randomIndex];
                        Debug.Log($"🤖 AI {playerWalker.name} สุ่มเกิดที่ช่อง ID: {targetData.tileID}");
                    }
                }
                else
                {
                    // 👤 Player: สุ่มเกิดที่ "ช่อง Start" เท่านั้น
                    if (startNodes.Count > 0)
                    {
                        int randomIndex = Random.Range(0, startNodes.Count);
                        targetData = startNodes[randomIndex];
                        Debug.Log($"👤 Player {playerWalker.name} สุ่มเกิดที่ Start ID: {targetData.tileID}");
                    }
                    else
                    {
                        // Fallback: ถ้าแมพไม่มีจุด Start เลย ให้เอาช่องแรกสุด (กัน Error)
                        if (routeManager.nodeConnections.Count > 0)
                            targetData = routeManager.nodeConnections[0];
                    }
                }
            }

            // 3. สั่งย้ายตำแหน่ง
            if (targetData != null && targetData.node != null)
            {
                playerWalker.TeleportToNode(targetData.node);
                playerWalker.currentNodeID = targetData.tileID; // ยัด ID กันเหนียว
            }
            else
            {
                Debug.LogError($"⛔ ไม่พบตำแหน่งเกิดสำหรับ {playerWalker.name}");
            }
        }
    }
}