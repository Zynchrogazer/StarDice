using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum BoardAIPersonality
{
    Balanced,
    Aggressive,
    Greedy,
    Defensive,
    Hunter
}

public class AIController : MonoBehaviour
{
    [Header("Board AI Personality")]
    [Tooltip("บุคลิกตั้งต้นของ AI บนบอร์ด (AI มีหน้าที่ป่วนผู้เล่น ไม่ใช่แข่งเก็บแต้ม)")]
    [SerializeField] private BoardAIPersonality personality = BoardAIPersonality.Balanced;
    [SerializeField] private bool logDecisionScores = true;
    [SerializeField] private float randomScoreNoise = 0f;

    [Header("Nuisance AI Tuning")]
    [Tooltip("เปิดให้ AI สลับบุคลิกตามสถานการณ์แบบซอมบี้ เช่น อยู่ใกล้ผู้เล่นจะไล่ป่วน, roaming จะเลือกทางกดดัน")]
    [SerializeField] private bool autoSwitchPersonality = true;
    [SerializeField, Min(1)] private int ambushDistance = 3;
    [SerializeField, Min(1)] private int personalitySwitchMinDecisions = 2;
    [SerializeField, Min(1)] private int personalitySwitchMaxDecisions = 4;

    private PlayerState myState;
    private PlayerPathWalker myWalker;
    private RouteManager routeManager;
    private BoardAIPersonality activePersonality;
    private int decisionsUntilPersonalitySwitch;

    private void Awake()
    {
        myState = GetComponent<PlayerState>();
        myWalker = GetComponent<PlayerPathWalker>();
        activePersonality = personality;
        ResetPersonalitySwitchCountdown();
    }

    private void Start()
    {
        ResolveRouteManager();
    }

    // --- 1. สั่งให้ AI เริ่มเทิร์น (เรียกจาก GameManager) ---
    public void StartAITurn()
    {
        if (myState == null || !myState.isAI) return;

        Debug.Log($"🤖 AI {name} is thinking...");
        StartCoroutine(ThinkAndAct());
    }

    private IEnumerator ThinkAndAct()
    {
        // แกล้งคิดแป๊บนึง (ให้คนดูทัน)
        yield return new WaitForSeconds(1.5f);

        // 1. เช็คว่าต้องเลือก Norma ไหม? (ถ้าเพิ่งเริ่มเกม หรือเวลอัป)
        // (ปกติระบบ Norma จะเด้ง UI แต่เราจะเขียนดักไว้ใน NormaSystem ว่าถ้าเป็น AI ให้ข้าม UI)

        // 2. สั่งทอยลูกเต๋า
        Debug.Log("🤖 AI is rolling dice!");
        if (DiceRollerFromPNG.TryGet(out var diceRoller))
            diceRoller.RollDice();
    }

    // --- 2. ฟังก์ชันตัดสินใจเลือกทางแยก (ถูกเรียกจาก PlayerPathWalker) ---
    public Transform ChoosePath(List<Transform> choices)
    {
        if (choices == null || choices.Count == 0)
            return null;

        if (choices.Count == 1)
            return choices[0];

        if (ResolveRouteManager() == null)
        {
            Transform fallbackChoice = choices[Random.Range(0, choices.Count)];
            Debug.LogWarning($"🤖 {name} cannot find RouteManager. Fallback random path: {fallbackChoice.name}");
            return fallbackChoice;
        }

        BoardAIPersonality decisionPersonality = GetDecisionPersonality(choices);
        Transform bestChoice = choices[0];
        float bestScore = float.MinValue;

        foreach (Transform choice in choices)
        {
            float score = EvaluatePathChoice(choice, decisionPersonality);

            if (logDecisionScores)
            {
                NodeConnection nodeData = GetNodeData(choice);
                string tileType = nodeData != null ? nodeData.type.ToString() : "Unknown";
                Debug.Log($"🤖 {name} [{decisionPersonality}] nuisance-evaluates {choice.name} ({tileType}) = {score:0.##}");
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestChoice = choice;
            }
        }

        Debug.Log($"🤖 {name} [{decisionPersonality}] chose nuisance path {bestChoice.name} with score {bestScore:0.##}");
        return bestChoice;
    }

    private float EvaluatePathChoice(Transform choice, BoardAIPersonality decisionPersonality)
    {
        NodeConnection nodeData = GetNodeData(choice);
        if (nodeData == null)
            return -999f;

        float score = GetNuisanceTileScore(nodeData.type);
        score += GetPlayerDisruptionScore(nodeData.tileID, decisionPersonality);
        score += GetHazardPressureScore(nodeData, decisionPersonality);
        score += GetPersonalityModifier(nodeData.type, decisionPersonality);

        if (randomScoreNoise > 0f)
            score += Random.Range(-randomScoreNoise, randomScoreNoise);

        return score;
    }

    private BoardAIPersonality GetDecisionPersonality(List<Transform> choices)
    {
        if (!autoSwitchPersonality)
            return personality;

        int nearestHumanDistance = GetNearestHumanDistanceFromCurrentNode();
        if (nearestHumanDistance >= 0 && nearestHumanDistance <= ambushDistance)
            return SwitchPersonality(BoardAIPersonality.Hunter, "human nearby");

        decisionsUntilPersonalitySwitch--;
        if (decisionsUntilPersonalitySwitch > 0)
            return activePersonality;

        BoardAIPersonality nextPersonality = PickRoamingPersonality(choices);
        return SwitchPersonality(nextPersonality, "roaming nuisance");
    }

    private BoardAIPersonality PickRoamingPersonality(List<Transform> choices)
    {
        int disruptiveOptions = 0;
        int denialOptions = 0;

        foreach (Transform choice in choices)
        {
            NodeConnection nodeData = GetNodeData(choice);
            if (nodeData == null)
                continue;

            if (IsDisruptiveTile(nodeData.type))
                disruptiveOptions++;
            else if (IsCompetitiveRewardTile(nodeData.type))
                denialOptions++;
        }

        if (disruptiveOptions > 0)
            return BoardAIPersonality.Aggressive;

        if (denialOptions > 0)
            return BoardAIPersonality.Greedy;

        return Random.value < 0.55f ? BoardAIPersonality.Balanced : BoardAIPersonality.Hunter;
    }

    private BoardAIPersonality SwitchPersonality(BoardAIPersonality nextPersonality, string reason)
    {
        if (activePersonality != nextPersonality && logDecisionScores)
            Debug.Log($"🤖 {name} switches board nuisance personality {activePersonality} -> {nextPersonality} ({reason})");

        activePersonality = nextPersonality;
        ResetPersonalitySwitchCountdown();
        return activePersonality;
    }

    private void ResetPersonalitySwitchCountdown()
    {
        int min = Mathf.Max(1, personalitySwitchMinDecisions);
        int max = Mathf.Max(min, personalitySwitchMaxDecisions);
        decisionsUntilPersonalitySwitch = Random.Range(min, max + 1);
    }

    private NodeConnection GetNodeData(Transform choice)
    {
        if (choice == null || ResolveRouteManager() == null)
            return null;

        int tileID = routeManager.ExtractNumberFromName(choice.name);
        return routeManager.GetNodeData(tileID);
    }

    private RouteManager ResolveRouteManager()
    {
        if (routeManager == null)
            RouteManager.TryGet(out routeManager);

        return routeManager;
    }

    private float GetNuisanceTileScore(TileType tileType)
    {
        switch (tileType)
        {
            case TileType.Teleport:
                return 18f;
            case TileType.Event:
            case TileType.Minigame:
                return 10f;
            case TileType.Normal:
                return 4f;
            case TileType.Start:
                return 2f;
            case TileType.Heal:
                return -12f;
            case TileType.Trap:
            case TileType.iceeffect:
                return -6f;
            case TileType.Lava:
                return -10f;
            case TileType.Draw:
            case TileType.Shop:
                return -10f;
            case TileType.Star:
            case TileType.Treasure:
                return -14f;
            case TileType.Monster:
            case TileType.Boss:
            case TileType.SpecialBoss:
                return -18f;
            default:
                return 0f;
        }
    }

    private float GetPlayerDisruptionScore(int choiceTileID, BoardAIPersonality decisionPersonality)
    {
        int distance = GetNearestHumanDistance(choiceTileID);
        if (distance < 0)
            return 0f;

        if (distance == 0)
            return 90f;

        float maxScore = decisionPersonality == BoardAIPersonality.Hunter ? 60f : 42f;
        float falloff = decisionPersonality == BoardAIPersonality.Hunter ? 10f : 8f;
        return Mathf.Clamp(maxScore - distance * falloff, 0f, maxScore);
    }

    private float GetHazardPressureScore(NodeConnection nodeData, BoardAIPersonality decisionPersonality)
    {
        if (nodeData == null || !IsHazardTile(nodeData.type))
            return 0f;

        int distance = GetNearestHumanDistance(nodeData.tileID);
        if (distance < 0 || distance > ambushDistance + 1)
            return 0f;

        // Hazard tiles are not good by themselves. They become useful only when
        // the AI can use that route to pressure/block a nearby human player.
        float pressure = Mathf.Max(0f, ambushDistance + 1 - distance) * 6f;

        if (decisionPersonality == BoardAIPersonality.Aggressive)
            pressure += 8f;
        else if (decisionPersonality == BoardAIPersonality.Hunter)
            pressure += 4f;

        return pressure;
    }

    private float GetPersonalityModifier(TileType tileType, BoardAIPersonality decisionPersonality)
    {
        switch (decisionPersonality)
        {
            case BoardAIPersonality.Aggressive:
                switch (tileType)
                {
                    case TileType.Trap:
                    case TileType.Lava:
                    case TileType.iceeffect:
                        return 8f;
                    case TileType.Monster:
                    case TileType.Boss:
                    case TileType.SpecialBoss:
                        return -5f;
                    case TileType.Heal:
                        return -12f;
                }
                break;

            case BoardAIPersonality.Greedy:
                switch (tileType)
                {
                    case TileType.Star:
                    case TileType.Treasure:
                    case TileType.Shop:
                    case TileType.Draw:
                        return 24f; // เดินไปยึด/บังช่องรางวัลมากกว่าเก็บแต้มแข่ง
                    case TileType.Teleport:
                        return 8f;
                }
                break;

            case BoardAIPersonality.Defensive:
                switch (tileType)
                {
                    case TileType.Teleport:
                        return 14f;
                    case TileType.Normal:
                    case TileType.Start:
                        return 8f;
                    case TileType.Heal:
                        return -10f;
                    case TileType.Monster:
                    case TileType.Boss:
                    case TileType.SpecialBoss:
                        return -24f;
                }
                break;

            case BoardAIPersonality.Hunter:
                switch (tileType)
                {
                    case TileType.Normal:
                    case TileType.Teleport:
                        return 10f;
                    case TileType.Trap:
                    case TileType.Lava:
                    case TileType.iceeffect:
                        return 0f;
                    case TileType.Star:
                    case TileType.Treasure:
                    case TileType.Shop:
                        return -8f;
                }
                break;
        }

        return 0f;
    }

    private bool IsDisruptiveTile(TileType tileType)
    {
        switch (tileType)
        {
            case TileType.Trap:
            case TileType.Lava:
            case TileType.iceeffect:
            case TileType.Teleport:
            case TileType.Event:
            case TileType.Minigame:
                return true;
            default:
                return false;
        }
    }

    private bool IsHazardTile(TileType tileType)
    {
        switch (tileType)
        {
            case TileType.Trap:
            case TileType.Lava:
            case TileType.iceeffect:
                return true;
            default:
                return false;
        }
    }

    private bool IsCompetitiveRewardTile(TileType tileType)
    {
        switch (tileType)
        {
            case TileType.Star:
            case TileType.Treasure:
            case TileType.Shop:
            case TileType.Draw:
                return true;
            default:
                return false;
        }
    }

    private int GetNearestHumanDistanceFromCurrentNode()
    {
        if (myWalker == null)
            return -1;

        return GetNearestHumanDistance(myWalker.currentNodeID);
    }

    private int GetNearestHumanDistance(int fromTileID)
    {
        PlayerState[] players = FindObjectsOfType<PlayerState>();
        int nearestDistance = int.MaxValue;

        foreach (PlayerState player in players)
        {
            if (player == null || player.isAI)
                continue;

            PlayerPathWalker walker = player.GetComponent<PlayerPathWalker>();
            if (walker == null)
                continue;

            int distance = GetGraphDistance(fromTileID, walker.currentNodeID);
            if (distance >= 0 && distance < nearestDistance)
                nearestDistance = distance;
        }

        return nearestDistance == int.MaxValue ? -1 : nearestDistance;
    }

    private int GetGraphDistance(int startTileID, int targetTileID)
    {
        if (startTileID == targetTileID)
            return 0;

        if (ResolveRouteManager() == null)
            return -1;

        NodeConnection startNode = routeManager.GetNodeData(startTileID);
        NodeConnection targetNode = routeManager.GetNodeData(targetTileID);
        if (startNode == null || targetNode == null || startNode.node == null || targetNode.node == null)
            return -1;

        Queue<Transform> frontier = new Queue<Transform>();
        Dictionary<Transform, int> distances = new Dictionary<Transform, int>();

        frontier.Enqueue(startNode.node);
        distances[startNode.node] = 0;

        while (frontier.Count > 0)
        {
            Transform current = frontier.Dequeue();
            int currentDistance = distances[current];

            foreach (Transform next in routeManager.GetAllConnectedNodes(current))
            {
                if (next == null || distances.ContainsKey(next))
                    continue;

                int nextDistance = currentDistance + 1;
                if (next == targetNode.node)
                    return nextDistance;

                distances[next] = nextDistance;
                frontier.Enqueue(next);
            }
        }

        return -1;
    }

    // --- 3. ฟังก์ชันตัดสินใจเลือก Norma (ถูกเรียกจาก NormaSystem) ---
    public NormaType ChooseNorma(int rank)
    {
        // Board AI เป็นตัวป่วน ไม่ใช่คู่แข่งหลัก จึงเลือก Norma แบบปลอดภัย ไม่ optimize แข่งผู้เล่น
        return NormaType.Stars;
    }
}
