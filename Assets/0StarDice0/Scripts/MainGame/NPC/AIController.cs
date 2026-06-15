using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum BoardAIPersonality
{
    Balanced,
    Hunter
}

public class AIController : MonoBehaviour
{
    [Header("Board AI Personality")]
    [Tooltip("บุคลิกตั้งต้นของ AI บนบอร์ด: Balanced เป็นค่าเริ่มต้นเพื่อคอยป่วนช่องรางวัล, Hunter จะใช้เมื่อผู้เล่นเลือดต่ำ")]
    [SerializeField] private BoardAIPersonality personality = BoardAIPersonality.Balanced;
    [SerializeField] private bool logDecisionScores = true;
    [SerializeField] private float randomScoreNoise = 0f;

    [Header("Path Planning")]
    [Tooltip("จำนวนก้าวสูงสุดที่ AI จะจำลองล่วงหน้าด้วย graph search เพื่อเลือกทางแยก โดยไม่เปลี่ยน flow การเดินจริง")]
    [SerializeField, Range(1, 12)] private int maxLookAheadSteps = 6;
    [Tooltip("น้ำหนักคะแนนอนาคตจากการจำลองเส้นทาง ยิ่งสูง AI ยิ่งมองปลายทางมากขึ้น")]
    [SerializeField, Range(0f, 1f)] private float futurePathScoreWeight = 0.35f;
    [Tooltip("จำกัดจำนวน node ที่ AI สำรวจต่อการเลือกทาง เพื่อกันกราฟที่มีลูปทำให้ค้นหามากเกินจำเป็น")]
    [SerializeField, Range(8, 256)] private int maxLookAheadNodes = 64;

    [Header("Nuisance AI Tuning")]
    [Tooltip("เปิดให้ AI เปลี่ยนเป็น Hunter อัตโนมัติเมื่อมีผู้เล่นมนุษย์ HP น้อยกว่าหรือเท่ากับ Hunter Health Threshold")]
    [SerializeField] private bool autoSwitchPersonality = true;
    [SerializeField, Range(0.01f, 1f)] private float hunterHealthThreshold = 0.6f;

    private PlayerState myState;
    private RouteManager routeManager;
    private BoardAIPersonality activePersonality;
    private PlayerState currentHunterTarget;

    private void Awake()
    {
        myState = GetComponent<PlayerState>();
        activePersonality = personality;
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
        return ChoosePath(choices, 1);
    }

    // อธิบายกับอาจารย์ได้ว่าเมธอดนี้คือ decision point ของ AI ตอนเจอทางแยก:
    // 1) ให้คะแนนช่องถัดไปทันที 2) มองเส้นทางอนาคตในกราฟตามก้าวที่เหลือ 3) เลือกคะแนนรวมสูงสุด
    public Transform ChoosePath(List<Transform> choices, int stepsRemaining)
    {
        if (TryGetSimplePathChoice(choices, out Transform simpleChoice))
            return simpleChoice;

        BoardAIPersonality decisionPersonality = GetDecisionPersonality();
        int lookAheadSteps = Mathf.Clamp(stepsRemaining, 1, maxLookAheadSteps);
        PathChoiceScore bestScore = ScorePathChoice(choices[0], decisionPersonality, lookAheadSteps);
        LogPathChoiceScore(bestScore, decisionPersonality, lookAheadSteps);

        for (int i = 1; i < choices.Count; i++)
        {
            PathChoiceScore candidateScore = ScorePathChoice(choices[i], decisionPersonality, lookAheadSteps);
            LogPathChoiceScore(candidateScore, decisionPersonality, lookAheadSteps);

            if (candidateScore.TotalScore > bestScore.TotalScore)
                bestScore = candidateScore;
        }

        Debug.Log($"🤖 {name} [{decisionPersonality}] chose path {bestScore.Choice.name} with score {bestScore.TotalScore:0.##} using graph look-ahead ({lookAheadSteps} step(s))");
        return bestScore.Choice;
    }

    private bool TryGetSimplePathChoice(List<Transform> choices, out Transform selectedChoice)
    {
        selectedChoice = null;

        if (choices == null || choices.Count == 0)
            return true;

        if (choices.Count == 1)
        {
            selectedChoice = choices[0];
            return true;
        }

        if (ResolveRouteManager() != null)
            return false;

        selectedChoice = choices[Random.Range(0, choices.Count)];
        Debug.LogWarning($"🤖 {name} cannot find RouteManager. Fallback random path: {selectedChoice.name}");
        return true;
    }

    private PathChoiceScore ScorePathChoice(Transform choice, BoardAIPersonality decisionPersonality, int lookAheadSteps)
    {
        float immediateScore = EvaluatePathChoice(choice, decisionPersonality, true);
        float futureScore = EstimateFuturePathScore(choice, lookAheadSteps - 1, decisionPersonality);
        float totalScore = immediateScore + futureScore * futurePathScoreWeight;
        NodeConnection nodeData = GetNodeData(choice);
        string tileType = nodeData != null ? nodeData.type.ToString() : "Unknown";

        return new PathChoiceScore(choice, tileType, immediateScore, futureScore, totalScore);
    }

    private void LogPathChoiceScore(PathChoiceScore score, BoardAIPersonality decisionPersonality, int lookAheadSteps)
    {
        if (!logDecisionScores)
            return;

        Debug.Log($"🤖 {name} [{decisionPersonality}] evaluates {score.Choice.name} ({score.TileType}) immediate={score.ImmediateScore:0.##}, future={score.FutureScore:0.##}, total={score.TotalScore:0.##}, lookAhead={lookAheadSteps}");
    }

    private float EvaluatePathChoice(Transform choice, BoardAIPersonality decisionPersonality, bool includeNoise = false)
    {
        NodeConnection nodeData = GetNodeData(choice);
        if (nodeData == null)
            return -999f;

        float score = GetBaseTileScore(nodeData.type);
        score += GetPlayerTargetScore(nodeData.tileID, decisionPersonality);
        score += GetPersonalityModifier(nodeData.type, decisionPersonality);

        if (includeNoise && randomScoreNoise > 0f)
            score += Random.Range(-randomScoreNoise, randomScoreNoise);

        return score;
    }

    // BFS-style look-ahead แบบ bounded: ใช้ Queue ไล่ node ตาม depth แต่จำกัดทั้ง depth และ node budget
    // เพื่อให้เหมาะกับเกมบอร์ดที่มีทางวน โดยไม่ต้องเพิ่ม A* หรือระบบ pathfinding ใหญ่เกินความจำเป็น
    private float EstimateFuturePathScore(Transform startNode, int remainingSteps, BoardAIPersonality decisionPersonality)
    {
        if (startNode == null || remainingSteps <= 0)
            return 0f;

        Queue<PathSearchStep> frontier = new Queue<PathSearchStep>();
        HashSet<PathSearchStep> visitedSteps = new HashSet<PathSearchStep>();
        PathSearchStep startStep = new PathSearchStep(startNode, 0);
        frontier.Enqueue(startStep);
        visitedSteps.Add(startStep);

        float bestTerminalScore = float.MinValue;
        int exploredNodes = 0;
        int nodeBudget = Mathf.Max(1, maxLookAheadNodes);

        while (frontier.Count > 0 && exploredNodes < nodeBudget)
        {
            PathSearchStep current = frontier.Dequeue();
            exploredNodes++;

            if (current.Depth >= remainingSteps)
            {
                bestTerminalScore = Mathf.Max(bestTerminalScore, EvaluatePathChoice(current.Node, decisionPersonality));
                continue;
            }

            List<Transform> nextNodes = routeManager.GetAllConnectedNodes(current.Node);
            if (nextNodes.Count == 0)
            {
                bestTerminalScore = Mathf.Max(bestTerminalScore, EvaluatePathChoice(current.Node, decisionPersonality));
                continue;
            }

            foreach (Transform nextNode in nextNodes)
            {
                if (nextNode == null)
                    continue;

                PathSearchStep nextStep = new PathSearchStep(nextNode, current.Depth + 1);
                if (visitedSteps.Contains(nextStep))
                    continue;

                visitedSteps.Add(nextStep);
                frontier.Enqueue(nextStep);
            }
        }

        if (frontier.Count > 0 && logDecisionScores)
            Debug.Log($"🤖 {name} stopped graph look-ahead at {exploredNodes}/{nodeBudget} nodes to keep the board AI lightweight.");

        return bestTerminalScore > float.MinValue ? bestTerminalScore : 0f;
    }

    private struct PathChoiceScore
    {
        public PathChoiceScore(Transform choice, string tileType, float immediateScore, float futureScore, float totalScore)
        {
            Choice = choice;
            TileType = tileType;
            ImmediateScore = immediateScore;
            FutureScore = futureScore;
            TotalScore = totalScore;
        }

        public Transform Choice;
        public string TileType;
        public float ImmediateScore;
        public float FutureScore;
        public float TotalScore;
    }

    private struct PathSearchStep
    {
        public PathSearchStep(Transform node, int depth)
        {
            Node = node;
            Depth = depth;
        }

        public Transform Node;
        public int Depth;
    }

    private BoardAIPersonality GetDecisionPersonality()
    {
        currentHunterTarget = null;

        if (!autoSwitchPersonality)
        {
            if (personality == BoardAIPersonality.Hunter)
                currentHunterTarget = GetLowestHealthHumanTarget();

            return SwitchPersonality(personality, "manual personality");
        }

        currentHunterTarget = GetLowestHealthHumanTargetUnderThreshold();
        if (currentHunterTarget != null)
            return SwitchPersonality(BoardAIPersonality.Hunter, $"low HP target {currentHunterTarget.name}");

        return SwitchPersonality(BoardAIPersonality.Balanced, "default board nuisance");
    }

    private BoardAIPersonality SwitchPersonality(BoardAIPersonality nextPersonality, string reason)
    {
        if (activePersonality != nextPersonality && logDecisionScores)
            Debug.Log($"🤖 {name} switches board personality {activePersonality} -> {nextPersonality} ({reason})");

        activePersonality = nextPersonality;
        return activePersonality;
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
            case TileType.Normal:
            case TileType.Start:
                return 2f;
            case TileType.Teleport:
            case TileType.Event:
            case TileType.Minigame:
                return 4f;
            case TileType.Trap:
            case TileType.iceeffect:
                return -6f;
            case TileType.Lava:
                return -10f;
            case TileType.Heal:
                return -12f;
            case TileType.Monster:
            case TileType.Boss:
            case TileType.SpecialBoss:
                return -18f;
            default:
                return 0f;
        }
    }

    private float GetPlayerTargetScore(int choiceTileID, BoardAIPersonality decisionPersonality)
    {
        if (decisionPersonality != BoardAIPersonality.Hunter || currentHunterTarget == null)
            return 0f;

        PlayerPathWalker targetWalker = currentHunterTarget.GetComponent<PlayerPathWalker>();
        if (targetWalker == null)
            return 0f;

        int distance = GetGraphDistance(choiceTileID, targetWalker.currentNodeID);
        if (distance < 0)
            return 0f;

        if (distance == 0)
            return 120f;

        return Mathf.Clamp(90f - distance * 15f, 0f, 90f);
    }

    private float GetPersonalityModifier(TileType tileType, BoardAIPersonality decisionPersonality)
    {
        switch (decisionPersonality)
        {
            case BoardAIPersonality.Balanced:
                switch (tileType)
                {
                    case TileType.Star:
                    case TileType.Treasure:
                        return 28f;
                    case TileType.Shop:
                    case TileType.Draw:
                        return 22f;
                    case TileType.Teleport:
                        return 10f;
                    case TileType.Event:
                    case TileType.Minigame:
                        return 8f;
                    case TileType.Heal:
                        return -10f;
                    case TileType.Trap:
                    case TileType.Lava:
                    case TileType.iceeffect:
                        return -6f;
                }
                break;

            case BoardAIPersonality.Hunter:
                switch (tileType)
                {
                    case TileType.Normal:
                    case TileType.Start:
                        return 8f;
                    case TileType.Teleport:
                        return 14f;
                    case TileType.Star:
                    case TileType.Treasure:
                    case TileType.Shop:
                    case TileType.Draw:
                        return -14f;
                    case TileType.Heal:
                        return -18f;
                    case TileType.Monster:
                    case TileType.Boss:
                    case TileType.SpecialBoss:
                        return -12f;
                }
                break;
        }

        return 0f;
    }

    private PlayerState GetLowestHealthHumanTargetUnderThreshold()
    {
        return GetLowestHealthHumanTarget(hunterHealthThreshold);
    }

    private PlayerState GetLowestHealthHumanTarget(float maxHealthRatio = 1f)
    {
        PlayerState[] players = FindObjectsOfType<PlayerState>();
        PlayerState lowestHealthTarget = null;
        float lowestHealthRatio = float.MaxValue;

        foreach (PlayerState player in players)
        {
            if (player == null || player.isAI || player.MaxHealth <= 0)
                continue;

            float healthRatio = Mathf.Clamp01((float)player.PlayerHealth / player.MaxHealth);
            if (healthRatio > maxHealthRatio || healthRatio >= lowestHealthRatio)
                continue;

            lowestHealthRatio = healthRatio;
            lowestHealthTarget = player;
        }

        return lowestHealthTarget;
    }

    // ใช้ BFS หา graph distance แบบไม่ถ่วงน้ำหนัก เหมาะกับกระดานที่แต่ละ edge คือ 1 ก้าวเท่ากัน
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
