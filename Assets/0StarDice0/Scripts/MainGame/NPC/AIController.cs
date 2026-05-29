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
    [SerializeField] private BoardAIPersonality personality = BoardAIPersonality.Balanced;
    [SerializeField] private bool logDecisionScores = true;
    [SerializeField] private float randomScoreNoise = 0f;

    private PlayerState myState;
    private PlayerPathWalker myWalker;
    private RouteManager routeManager;

    private void Awake()
    {
        myState = GetComponent<PlayerState>();
        myWalker = GetComponent<PlayerPathWalker>();
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

        Transform bestChoice = choices[0];
        float bestScore = float.MinValue;

        foreach (Transform choice in choices)
        {
            float score = EvaluatePathChoice(choice);

            if (logDecisionScores)
            {
                NodeConnection nodeData = GetNodeData(choice);
                string tileType = nodeData != null ? nodeData.type.ToString() : "Unknown";
                Debug.Log($"🤖 {name} [{personality}] evaluates {choice.name} ({tileType}) = {score:0.##}");
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestChoice = choice;
            }
        }

        Debug.Log($"🤖 {name} [{personality}] chose {bestChoice.name} with score {bestScore:0.##}");
        return bestChoice;
    }

    private float EvaluatePathChoice(Transform choice)
    {
        NodeConnection nodeData = GetNodeData(choice);
        if (nodeData == null)
            return -999f;

        float score = GetBaseTileScore(nodeData.type);
        score += GetHealthSituationScore(nodeData.type);
        score += GetPersonalityModifier(nodeData.type);

        if (personality == BoardAIPersonality.Hunter)
            score += GetHunterScore(nodeData.tileID);

        if (randomScoreNoise > 0f)
            score += Random.Range(-randomScoreNoise, randomScoreNoise);

        return score;
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

    private float GetBaseTileScore(TileType tileType)
    {
        switch (tileType)
        {
            case TileType.Heal:
                return 25f;
            case TileType.Star:
                return 20f;
            case TileType.Treasure:
                return 18f;
            case TileType.SpecialBoss:
                return 20f;
            case TileType.Boss:
                return 15f;
            case TileType.Monster:
                return 10f;
            case TileType.Draw:
                return 10f;
            case TileType.Shop:
                return 8f;
            case TileType.Event:
            case TileType.Teleport:
            case TileType.Minigame:
                return 5f;
            case TileType.Trap:
                return -25f;
            case TileType.Lava:
            case TileType.iceeffect:
                return -35f;
            default:
                return 0f;
        }
    }

    private float GetHealthSituationScore(TileType tileType)
    {
        if (myState == null || myState.MaxHealth <= 0)
            return 0f;

        float hpRatio = (float)myState.PlayerHealth / myState.MaxHealth;
        if (hpRatio >= 0.35f)
            return 0f;

        switch (tileType)
        {
            case TileType.Heal:
            case TileType.Start:
                return 60f;
            case TileType.Trap:
            case TileType.Lava:
            case TileType.iceeffect:
            case TileType.Monster:
            case TileType.Boss:
            case TileType.SpecialBoss:
                return -50f;
            default:
                return 0f;
        }
    }

    private float GetPersonalityModifier(TileType tileType)
    {
        switch (personality)
        {
            case BoardAIPersonality.Aggressive:
                switch (tileType)
                {
                    case TileType.Monster:
                    case TileType.Boss:
                    case TileType.SpecialBoss:
                        return 35f;
                    case TileType.Heal:
                        return -5f;
                }
                break;

            case BoardAIPersonality.Greedy:
                switch (tileType)
                {
                    case TileType.Star:
                    case TileType.Treasure:
                    case TileType.Shop:
                    case TileType.Draw:
                        return 35f;
                }
                break;

            case BoardAIPersonality.Defensive:
                switch (tileType)
                {
                    case TileType.Heal:
                    case TileType.Start:
                        return 35f;
                    case TileType.Trap:
                    case TileType.Lava:
                    case TileType.iceeffect:
                    case TileType.Monster:
                    case TileType.Boss:
                    case TileType.SpecialBoss:
                        return -35f;
                }
                break;

            case BoardAIPersonality.Hunter:
                switch (tileType)
                {
                    case TileType.Monster:
                    case TileType.Boss:
                    case TileType.SpecialBoss:
                        return 15f;
                    case TileType.Trap:
                    case TileType.Lava:
                    case TileType.iceeffect:
                        return -10f;
                }
                break;
        }

        return 0f;
    }

    private float GetHunterScore(int choiceTileID)
    {
        PlayerPathWalker targetWalker = FindNearestHumanWalker(choiceTileID);
        if (targetWalker == null)
            return 0f;

        int distance = GetGraphDistance(choiceTileID, targetWalker.currentNodeID);
        if (distance < 0)
            return 0f;

        return Mathf.Clamp(40f - distance * 6f, 0f, 40f);
    }

    private PlayerPathWalker FindNearestHumanWalker(int fromTileID)
    {
        PlayerState[] players = FindObjectsOfType<PlayerState>();
        PlayerPathWalker nearestWalker = null;
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
            {
                nearestDistance = distance;
                nearestWalker = walker;
            }
        }

        return nearestWalker;
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
        // Logic: ถ้าพลังโจมตีเยอะ เลือก Wins / ถ้าเลือดเยอะ เลือก Stars
        if (myState != null && myState.CurrentAttack > 12) return NormaType.Wins;
        return NormaType.Stars;
    }
}
