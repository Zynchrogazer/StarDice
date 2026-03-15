using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Text;

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
    [Tooltip("Text สำหรับแสดง icon debuff แบบเรียงซ้าย->ขวา")]
    public TMP_Text debuffText;
    [Tooltip("กล่อง tooltip ที่จะโชว์ตอน hover icon debuff (optional)")]
    public GameObject debuffTooltipRoot;
    [Tooltip("ข้อความในกล่อง tooltip debuff (optional)")]
    public TMP_Text debuffTooltipText;

    [Header("Debuff Sprite Tags (TMP Rich Text)")]
    [Tooltip("ตัวอย่าง: <sprite name=\"burn\"> ถ้าเว้นว่างจะ fallback เป็น emoji")]
    public string burnSpriteTag = "<sprite name=\"burn\">";
    [Tooltip("ตัวอย่าง: <sprite name=\"ice\"> ถ้าเว้นว่างจะ fallback เป็น emoji")]
    public string iceSpriteTag = "<sprite name=\"ice\">";


    // ตัวแปรสำหรับจำตัวละครที่เป็น "คนเล่น" (Human)
    private PlayerState myPlayer;
    private ElementButtonManager elementButtonManager;
    private Transform boundStatusRoot;
    private DebuffTooltipHoverHandler debuffTooltipHoverHandler;
    private bool hasAttemptedTooltipAutoBind;

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

        if (debuffText != null)
        {
            EnsureDebuffTooltipHandler();
            List<DebuffUIEntry> debuffEntries = BuildDebuffEntries();
            debuffText.text = BuildDebuffIconRichText(debuffEntries);

            if (debuffTooltipHoverHandler != null)
                debuffTooltipHoverHandler.SetEntries(debuffEntries);
        }
    }

    private List<DebuffUIEntry> BuildDebuffEntries()
    {
        List<DebuffUIEntry> entries = new List<DebuffUIEntry>(2);
        if (myPlayer == null)
            return entries;

        if (myPlayer.DebuffBurn && myPlayer.DebuffBurnTurnsRemaining > 0)
        {
            entries.Add(new DebuffUIEntry(
                "burn",
                ResolveDebuffIconMarkup(burnSpriteTag, "🔥"),
                myPlayer.BurnDebuffAppliedOrder,
                $"Burn: รับความเสียหายตอนเริ่มเทิร์น\nคงเหลือ: {myPlayer.DebuffBurnTurnsRemaining} เทิร์น"));
        }

        if (myPlayer.hasIceEffect)
        {
            entries.Add(new DebuffUIEntry(
                "ice",
                ResolveDebuffIconMarkup(iceSpriteTag, "❄️"),
                myPlayer.IceDebuffAppliedOrder,
                "Ice: ทอยเต๋าครั้งถัดไปจะเหลือครึ่งหนึ่ง\nคงเหลือ: 1 ครั้ง"));
        }

        entries.Sort((left, right) =>
        {
            int leftOrder = left.Order <= 0 ? int.MaxValue : left.Order;
            int rightOrder = right.Order <= 0 ? int.MaxValue : right.Order;
            return leftOrder.CompareTo(rightOrder);
        });

        return entries;
    }

    private static string ResolveDebuffIconMarkup(string spriteTag, string fallbackEmoji)
    {
        if (!string.IsNullOrWhiteSpace(spriteTag) && spriteTag.Contains("<sprite"))
            return spriteTag;

        return fallbackEmoji;
    }

    private static string BuildDebuffIconRichText(List<DebuffUIEntry> entries)
    {
        if (entries == null || entries.Count == 0)
            return "-";

        StringBuilder sb = new StringBuilder(32);
        for (int i = 0; i < entries.Count; i++)
        {
            DebuffUIEntry entry = entries[i];
            sb.Append("<link=");
            sb.Append(entry.Key);
            sb.Append('>');
            sb.Append(entry.Icon);
            sb.Append("</link>");

            if (i < entries.Count - 1)
                sb.Append("  ");
        }

        return sb.ToString();
    }

    private void EnsureDebuffTooltipHandler()
    {
        if (debuffText == null)
            return;

        if (debuffTooltipHoverHandler == null)
            debuffTooltipHoverHandler = debuffText.GetComponent<DebuffTooltipHoverHandler>();

        if (debuffTooltipHoverHandler == null)
            debuffTooltipHoverHandler = debuffText.gameObject.AddComponent<DebuffTooltipHoverHandler>();

        debuffTooltipHoverHandler.Bind(debuffText, debuffTooltipRoot, debuffTooltipText);
    }

    public readonly struct DebuffUIEntry
    {
        public DebuffUIEntry(string key, string icon, int order, string tooltip)
        {
            Key = key;
            Icon = icon;
            Order = order;
            Tooltip = tooltip;
        }

        public string Key { get; }
        public string Icon { get; }
        public int Order { get; }
        public string Tooltip { get; }
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
        // KISS: credit ให้ยึด PlayerData ก่อน เพื่อไม่ให้ค่ารีเซ็ตเมื่อ unload scene
        // (PlayerState เป็น scene-bound)
        if (GameData.Instance != null && GameData.Instance.selectedPlayer != null)
            return Mathf.Max(0, GameData.Instance.selectedPlayer.Credit);

        if (myPlayer != null && myPlayer.selectedPlayerPreset != null)
            return Mathf.Max(0, myPlayer.selectedPlayerPreset.Credit);

        // fallback กันกรณี scene เทส/ข้อมูลยังไม่ถูกฉีดเข้ามา
        if (myPlayer != null)
            return Mathf.Max(0, myPlayer.PlayerCredit);

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

        if (!hasAttemptedTooltipAutoBind && debuffText != null)
        {
            hasAttemptedTooltipAutoBind = true;

            if (debuffTooltipRoot == null)
                debuffTooltipRoot = FindUIObjectByName("debufftooltip");

            if (debuffTooltipText == null)
                debuffTooltipText = FindUITextByName("debufftooltip");
        }

        if (hpText != null && creditText != null && starText != null && winText != null && levelText != null
            && atkText != null && spdText != null && defText != null && debuffText != null)
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
            && atkText != null && spdText != null && defText != null && debuffText != null)
            return;

        // fallback: ค่อยค้นทั้ง scene แต่ยัง prefer เฉพาะ object ที่ active
        TMP_Text[] activeSceneTexts = FindObjectsByType<TMP_Text>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        AssignTextsByName(activeSceneTexts, preferActiveOnly: true);

        if (hpText != null && creditText != null && starText != null && winText != null && levelText != null
            && atkText != null && spdText != null && defText != null && debuffText != null)
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
        debuffText = null;
        debuffTooltipHoverHandler = null;
        hasAttemptedTooltipAutoBind = false;
    }

    private Transform ResolveActiveStatusRoot()
    {
        if (elementButtonManager == null || !elementButtonManager.gameObject.scene.IsValid())
            elementButtonManager = ResolvePreferredElementButtonManagerInstance();

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

    private ElementButtonManager ResolvePreferredElementButtonManagerInstance()
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

    private static GameObject FindUIObjectByName(string keyword)
    {
        if (string.IsNullOrEmpty(keyword))
            return null;

        Transform[] transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform t = transforms[i];
            if (t == null) continue;
            if (t.name.ToLower().Contains(keyword.ToLower()))
                return t.gameObject;
        }

        return null;
    }

    private static TMP_Text FindUITextByName(string keyword)
    {
        if (string.IsNullOrEmpty(keyword))
            return null;

        TMP_Text[] texts = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < texts.Length; i++)
        {
            TMP_Text txt = texts[i];
            if (txt == null) continue;
            if (txt.name.ToLower().Contains(keyword.ToLower()))
                return txt;
        }

        return null;
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
            else if (debuffText == null && (n.Contains("debuff") || (n.Contains("status") && n.Contains("icon")))) debuffText = txt;
        }
    }

}
