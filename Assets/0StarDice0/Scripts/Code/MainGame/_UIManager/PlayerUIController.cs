using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUIController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text hpText;
    public TMP_Text hpCurrentMaxText;
    public TMP_Text creditText;
    [Tooltip("รองรับกรณีมี Credit UI มากกว่า 1 จุด")]
    public TMP_Text secondaryCreditText;
    public TMP_Text starText;
    public TMP_Text winText;
    public TMP_Text levelText;
    public TMP_Text atkText;
    public TMP_Text spdText;
    public TMP_Text defText;

    // ตัวแปรสำหรับจำตัวละครที่เป็น "คนเล่น" (Human)
    private PlayerState myPlayer;
    private ElementButtonManager elementButtonManager;
    private Transform boundStatusRoot;

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

        if (hpCurrentMaxText != null)
            hpCurrentMaxText.text = $"HP: {myPlayer.PlayerHealth}/{myPlayer.MaxHealth}";

        int resolvedCredit = ResolvePersistentCredit();
        SetCreditUI(resolvedCredit);

        if (starText != null)
            starText.text = $"{myPlayer.PlayerStar}";

        if (winText != null)
            winText.text = $"{myPlayer.WinCount}";

        if (levelText != null)
            levelText.text = $"Lv. {myPlayer.PlayerLevel}";

        if (atkText != null)
            atkText.text = $"ATK: {myPlayer.CurrentAttack}";

        if (spdText != null)
            spdText.text = $"SPD: {myPlayer.CurrentSpeed}";

        if (defText != null)
            defText.text = $"DEF: {myPlayer.CurrentDefense}";
    }

    private void SetCreditUI(int credit)
    {
        string textValue = $"Credit: {credit}";

        if (creditText != null)
            creditText.text = textValue;

        if (secondaryCreditText != null)
            secondaryCreditText.text = textValue;
    }

    private int ResolvePersistentCredit()
    {
        // KISS: บน board ให้ยึดค่าจาก PlayerState ก่อน (source of truth ระหว่างเล่น)
        if (myPlayer != null)
            return Mathf.Max(0, myPlayer.PlayerCredit);

        if (GameData.Instance != null && GameData.Instance.selectedPlayer != null)
            return Mathf.Max(0, GameData.Instance.selectedPlayer.Credit);

        if (myPlayer != null && myPlayer.selectedPlayerPreset != null)
            return Mathf.Max(0, myPlayer.selectedPlayerPreset.Credit);

        // fallback กันพังตอนเทสฉากเดี่ยว
        return 0;
    }

    /// <summary>
    /// กันกรณีลืมผูก UI ใน Inspector ของแต่ละ Board Scene
    /// จะพยายามหา Text จากชื่อวัตถุอัตโนมัติแบบครั้งต่อครั้งจนกว่าจะครบ
    /// </summary>
    private void TryAutoAssignUIRefs()
    {
        Transform activeStatusRoot = ResolveActiveStatusRoot();
        RebindIfStatusRootChanged(activeStatusRoot);

        if (hpText != null && creditText != null && starText != null && winText != null && levelText != null
            && atkText != null && spdText != null && defText != null)
        {
            // secondaryCreditText เป็น optional
            return;
        }

        // KISS: ถ้ามี ElementButtonManager ให้ยึด status root ของธาตุที่เลือกโดยตรง
        if (activeStatusRoot != null)
        {
            TMP_Text[] statusTexts = activeStatusRoot.GetComponentsInChildren<TMP_Text>(true);
            AssignTextsByName(statusTexts, preferActiveOnly: false);
            return;
        }

        // KISS: หาใน scope ของตัวเองก่อน (ลดโอกาสไปจับ UI ของธาตุ/หน้าต่างอื่น)
        TMP_Text[] localTexts = GetComponentsInChildren<TMP_Text>(true);
        AssignTextsByName(localTexts, preferActiveOnly: true);

        if (hpText != null && creditText != null && starText != null && winText != null && levelText != null
            && atkText != null && spdText != null && defText != null)
            return;

        // fallback: ค่อยค้นทั้ง scene แต่ยัง prefer เฉพาะ object ที่ active
        TMP_Text[] activeSceneTexts = FindObjectsByType<TMP_Text>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        AssignTextsByName(activeSceneTexts, preferActiveOnly: true);

        if (hpText != null && creditText != null && starText != null && winText != null && levelText != null
            && atkText != null && spdText != null && defText != null)
            return;

        // fallback สุดท้าย: รวม inactive เผื่อบาง panel เปิดภายหลัง
        TMP_Text[] allTexts = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        AssignTextsByName(allTexts, preferActiveOnly: false);
    }

    private void RebindIfStatusRootChanged(Transform activeStatusRoot)
    {
        if (boundStatusRoot == activeStatusRoot)
            return;

        boundStatusRoot = activeStatusRoot;

        // status panel เปลี่ยน (รวมถึงหายไป) ให้ bind ใหม่ทั้งหมด เพื่อลดปัญหาค้าง ref ข้าม scene
        hpText = null;
        //hpCurrentMaxText = null;
        //creditText = null;
        //secondaryCreditText = null;
        //starText = null;
        //winText = null;
        //levelText = null;
        atkText = null;
        spdText = null;
        defText = null;
    }

    private Transform ResolveActiveStatusRoot()
    {
        if (elementButtonManager == null || !elementButtonManager.gameObject.scene.IsValid())
            elementButtonManager = FindPreferredElementButtonManager();

        if (elementButtonManager == null)
            return null;

        // KISS: เลือก panel ตาม selectedPlayer ก่อน (เหมือนแนวคิดเดียวกับ ElementButtonManager)
        Transform selectedRoot = ResolveStatusRootFromSelectedPlayer(elementButtonManager);
        if (selectedRoot != null)
            return selectedRoot;

        return elementButtonManager.GetActiveStatusRoot();
    }


    private static Transform ResolveStatusRootFromSelectedPlayer(ElementButtonManager manager)
    {
        if (manager == null || manager.buttons == null)
            return null;

        PlayerData selected = manager.selectedPlayer;
        if (selected == null && GameData.Instance != null)
            selected = GameData.Instance.selectedPlayer;

        if (selected == null)
            return null;

        int index = GetButtonIndexByElement(selected.element);
        if (index < 0 || index >= manager.buttons.Length)
            return null;

        Button selectedButton = manager.buttons[index];
        return selectedButton != null ? selectedButton.transform : null;
    }

    private static int GetButtonIndexByElement(ElementType element)
    {
        switch (element)
        {
            case ElementType.Fire: return 0;
            case ElementType.Water: return 1;
            case ElementType.Wind: return 2;
            case ElementType.Earth: return 3;
            case ElementType.Light: return 4;
            case ElementType.Dark: return 5;
            default: return -1;
        }
    }

    private ElementButtonManager FindPreferredElementButtonManager()
    {
        ElementButtonManager[] managers = FindObjectsByType<ElementButtonManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (managers == null || managers.Length == 0)
            return null;

        var myScene = gameObject.scene;

        foreach (var manager in managers)
        {
            if (manager == null) continue;
            if (manager.gameObject.scene == myScene)
                return manager;
        }

        return managers[0];
    }

    private void AssignTextsByName(TMP_Text[] texts, bool preferActiveOnly)
    {
        if (texts == null) return;

        foreach (var txt in texts)
        {
            if (txt == null) continue;
            if (preferActiveOnly && !txt.gameObject.activeInHierarchy) continue;

            string n = txt.name.ToLower();

            if (hpCurrentMaxText == null && n.Contains("hp") && (n.Contains("max") || n.Contains("full") || n.Contains("slash") || n.Contains("detail"))) hpCurrentMaxText = txt;
            else if (hpText == null && n.Contains("hp")) hpText = txt;
            else if (n.Contains("credit"))
            {
                if (creditText == null) creditText = txt;
                else if (secondaryCreditText == null && txt != creditText) secondaryCreditText = txt;
            }
            else if (starText == null && n.Contains("star")) starText = txt;
            else if (winText == null && n.Contains("win")) winText = txt;
            else if (levelText == null && (n.Contains("level") || n.Contains("lv"))) levelText = txt;
            else if (atkText == null && n.Contains("atk")) atkText = txt;
            else if (spdText == null && (n.Contains("spd") || n.Contains("speed"))) spdText = txt;
            else if (defText == null && n.Contains("def")) defText = txt;
        }
    }

}
