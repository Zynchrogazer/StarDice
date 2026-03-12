using UnityEngine;
using TMPro;

public class PlayerUIController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text hpText;
    public TMP_Text creditText;
    public TMP_Text starText;
    public TMP_Text winText;
    public TMP_Text levelText;

    // ตัวแปรสำหรับจำตัวละครที่เป็น "คนเล่น" (Human)
    private PlayerState myPlayer;

    private void Update()
    {
        TryAutoAssignUIRefs();

        // 1. ถ้ายังหาตัวคนเล่นไม่เจอ ให้ลองหาดู
        if (myPlayer == null)
        {
            FindHumanPlayer();
            return; // ยังไม่มีข้อมูล ให้ข้ามไปก่อน
        }

        // 2. ถ้าเจอแล้ว ให้อัปเดตค่าจาก "คนเล่น" เท่านั้น (ไม่สนใจว่าเทิร์นใคร)
        UpdateUI();
    }

    private void FindHumanPlayer()
    {
        if (GameTurnManager.TryGet(out var gameTurnManager) && gameTurnManager.allPlayers != null)
        {
            // วนหาในลิสต์ผู้เล่นทั้งหมด
            foreach (var p in gameTurnManager.allPlayers)
            {
                // เงื่อนไข: เอาตัวที่ไม่ใช่ null และ ไม่ใช่ AI
                if (p != null && !p.isAI)
                {
                    myPlayer = p;
                    Debug.Log($"[UI] 🔒 ล็อคการแสดงผลที่ผู้เล่น: {myPlayer.name}");
                    break; // เจอแล้วหยุดหาเลย
                }
            }
        }
    }

    private void UpdateUI()
    {
        // ใช้ข้อมูลจาก myPlayer ที่เราล็อคไว้ แทน GameTurnManager.CurrentPlayer
        if (hpText != null)
            hpText.text = $"HP: {myPlayer.PlayerHealth}";

        if (creditText != null)
            creditText.text = $"Credit: {ResolvePersistentCredit()}";

        if (starText != null)
            starText.text = $"{myPlayer.PlayerStar}";

        if (winText != null)
            winText.text = $"{myPlayer.WinCount}";

        if (levelText != null)
            levelText.text = $"Lv. {myPlayer.PlayerLevel}";
    }

    private int ResolvePersistentCredit()
    {
        // KISS: ใช้เครดิตจากข้อมูลถาวรเป็นหลัก เพื่อไม่ให้รีเซ็ตตาม runtime board state
        if (GameData.Instance != null && GameData.Instance.selectedPlayer != null)
            return GameData.Instance.selectedPlayer.Credit;

        if (myPlayer != null && myPlayer.selectedPlayerPreset != null)
            return myPlayer.selectedPlayerPreset.Credit;

        // fallback กันพังตอนเทสฉากเดี่ยว
        return myPlayer != null ? myPlayer.PlayerCredit : 0;
    }

    /// <summary>
    /// กันกรณีลืมผูก UI ใน Inspector ของแต่ละ Board Scene
    /// จะพยายามหา Text จากชื่อวัตถุอัตโนมัติแบบครั้งต่อครั้งจนกว่าจะครบ
    /// </summary>
    private void TryAutoAssignUIRefs()
    {
        if (hpText != null && creditText != null && starText != null && winText != null && levelText != null)
            return;

        TMP_Text[] texts = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var txt in texts)
        {
            if (txt == null) continue;
            string n = txt.name.ToLower();

            if (hpText == null && n.Contains("hp")) hpText = txt;
            else if (creditText == null && n.Contains("credit")) creditText = txt;
            else if (starText == null && n.Contains("star")) starText = txt;
            else if (winText == null && n.Contains("win")) winText = txt;
            else if (levelText == null && (n.Contains("level") || n.Contains("lv"))) levelText = txt;
        }
    }
}
