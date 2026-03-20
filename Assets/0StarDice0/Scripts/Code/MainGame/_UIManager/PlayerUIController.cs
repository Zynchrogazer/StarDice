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
    [Tooltip("Legacy Text สำหรับแสดง icon debuff แบบเรียงซ้าย->ขวา")]
    public TMP_Text debuffText;
    [Tooltip("Parent สำหรับสร้าง icon debuff แบบ Sprite Image (แนะนำให้ใช้) ")]
    public Transform debuffIconContainer;
    [Tooltip("Prefab ของ icon debuff (optional) ถ้าไม่ใส่จะสร้าง Image ให้อัตโนมัติ")]
    public GameObject debuffIconPrefab;
    [Tooltip("ขนาด icon debuff กรณีสร้างอัตโนมัติ")]
    public Vector2 debuffIconSize = new Vector2(32f, 32f);
    [Tooltip("Sprite icon สำหรับ Burn debuff")]
    public Sprite burnDebuffSprite;
    [Tooltip("Sprite icon สำหรับ Ice debuff")]
    public Sprite iceDebuffSprite;
    [Tooltip("กล่อง tooltip ที่จะโชว์ตอน hover icon debuff (optional)")]
    public GameObject debuffTooltipRoot;
    [Tooltip("ข้อความในกล่อง tooltip debuff (optional)")]
    public TMP_Text debuffTooltipText;

    // ตัวแปรสำหรับจำตัวละครที่เป็น "คนเล่น" (Human)
    private PlayerState myPlayer;
    private ElementButtonManager elementButtonManager;
    private Transform boundStatusRoot;
    private DebuffTooltipHoverHandler debuffTooltipHoverHandler;
    private bool hasAttemptedTooltipAutoBind;
    private readonly List<DebuffSpriteIconHoverHandler> debuffIconHandlers = new List<DebuffSpriteIconHoverHandler>();

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

        // -------------------------------------------------------------------
        // 🟢 แก้ไขตรงนี้: ไปดึงข้อมูลเควสจาก NormaSystem มาโชว์ด้วย
        // -------------------------------------------------------------------
if (starText != null || winText != null)
        {
            int reqStars = 999;
            int reqWins = 999;
            NormaType activeQuest = NormaType.Stars; // ตัวแปรเก็บว่าตอนนี้ทำเควสอะไรอยู่
            
            if (NormaSystem.TryGet(out var normaSystem))
            {
                int nextRank = normaSystem.currentNormaRank + 1;
                if (nextRank > normaSystem.maxNormaRank) nextRank = normaSystem.maxNormaRank;
                
                reqStars = normaSystem.GetRequirement(nextRank, NormaType.Stars);
                reqWins = normaSystem.GetRequirement(nextRank, NormaType.Wins);
                
                // ถามระบบว่าตอนนี้ผู้เล่นเลือกเควสอะไรไว้
                activeQuest = normaSystem.selectedNorma; 
            }

            // --- จัดการข้อความ ดาว ---
            if (starText != null)
            {
                if (activeQuest == NormaType.Stars)
                    starText.text = $"Star: {myPlayer.PlayerStar} / {reqStars}"; // ถ้าเลือกดาว ให้โชว์เป้าหมาย
                else
                    starText.text = $"Star: {myPlayer.PlayerStar}"; // ถ้าไม่ได้เลือก โชว์แค่ดาวที่มี
            }

            // --- จัดการข้อความ ชนะต่อสู้ ---
            if (winText != null)
            {
                if (activeQuest == NormaType.Wins)
                    winText.text = $"Battle: {myPlayer.WinCount} / {reqWins}"; // ถ้าเลือกต่อสู้ ให้โชว์เป้าหมาย
                else
                    winText.text = $"Battle: {myPlayer.WinCount}"; // ถ้าไม่ได้เลือก โชว์แค่จำนวนที่ชนะ
            }
        }
        // -------------------------------------------------------------------

        if (levelText != null)
            levelText.text = $"Lv. {myPlayer.PlayerLevel}";

        if (atkText != null)
            atkText.text = $"ATK: {myPlayer.CurrentAttack}";

        if (spdText != null)
            spdText.text = $"SPD: {myPlayer.CurrentSpeed}";

        if (defText != null)
            defText.text = $"DEF: {myPlayer.CurrentDefense}";

        List<DebuffUIEntry> debuffEntries = BuildDebuffEntries();
        RefreshDebuffSpriteIcons(debuffEntries);
        RefreshDebuffTextFallback(debuffEntries);
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
                "🔥",
                burnDebuffSprite,
                myPlayer.BurnDebuffAppliedOrder,
                $"Burn: รับความเสียหายตอนเริ่มเทิร์น\nคงเหลือ: {myPlayer.DebuffBurnTurnsRemaining} เทิร์น"));
        }

        if (myPlayer.hasIceEffect)
        {
            entries.Add(new DebuffUIEntry(
                "ice",
                "❄️",
                iceDebuffSprite,
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

    private void RefreshDebuffSpriteIcons(List<DebuffUIEntry> entries)
    {
        if (debuffIconContainer == null)
            return;

        EnsureSpriteIconPool(entries != null ? entries.Count : 0);

        int entryCount = entries != null ? entries.Count : 0;
        for (int i = 0; i < debuffIconHandlers.Count; i++)
        {
            bool shouldShow = i < entryCount;
            GameObject iconObject = debuffIconHandlers[i] != null ? debuffIconHandlers[i].gameObject : null;
            if (iconObject == null)
                continue;

            if (!shouldShow)
            {
                iconObject.SetActive(false);
                continue;
            }

            DebuffUIEntry entry = entries[i];
            debuffIconHandlers[i].Bind(debuffTooltipRoot, debuffTooltipText, entry.Tooltip);
            debuffIconHandlers[i].SetSprite(entry.IconSprite);
            iconObject.name = $"DebuffIcon_{entry.Key}";
            iconObject.SetActive(entry.IconSprite != null);
        }

        if (entryCount == 0)
            HideSpriteTooltipIfNeeded();
    }

    private void RefreshDebuffTextFallback(List<DebuffUIEntry> entries)
    {
        if (debuffText == null)
            return;

        bool isUsingSpriteIcons = debuffIconContainer != null;
        debuffText.gameObject.SetActive(!isUsingSpriteIcons);
        if (isUsingSpriteIcons)
            return;

        EnsureDebuffTooltipHandler();
        debuffText.text = BuildDebuffIconRichText(entries);

        if (debuffTooltipHoverHandler != null)
            debuffTooltipHoverHandler.SetEntries(entries);
    }

    private void EnsureSpriteIconPool(int requiredCount)
    {
        while (debuffIconHandlers.Count < requiredCount)
        {
            DebuffSpriteIconHoverHandler handler = CreateDebuffSpriteIcon();
            if (handler == null)
                break;

            debuffIconHandlers.Add(handler);
        }
    }

    private DebuffSpriteIconHoverHandler CreateDebuffSpriteIcon()
    {
        GameObject iconObject;
        if (debuffIconPrefab != null)
        {
            iconObject = Instantiate(debuffIconPrefab, debuffIconContainer);
        }
        else
        {
            iconObject = new GameObject("DebuffIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
            iconObject.transform.SetParent(debuffIconContainer, false);
            RectTransform rectTransform = iconObject.transform as RectTransform;
            if (rectTransform != null)
                rectTransform.sizeDelta = debuffIconSize;

            LayoutElement layoutElement = iconObject.GetComponent<LayoutElement>();
            if (layoutElement != null)
            {
                layoutElement.preferredWidth = debuffIconSize.x;
                layoutElement.preferredHeight = debuffIconSize.y;
                layoutElement.minWidth = debuffIconSize.x;
                layoutElement.minHeight = debuffIconSize.y;
            }

            Image image = iconObject.GetComponent<Image>();
            if (image != null)
                image.preserveAspect = true;
        }

        DebuffSpriteIconHoverHandler handler = iconObject.GetComponent<DebuffSpriteIconHoverHandler>();
        if (handler == null)
            handler = iconObject.AddComponent<DebuffSpriteIconHoverHandler>();

        if (handler.TargetImage == null)
            handler.CacheImageReference();

        iconObject.SetActive(false);
        return handler;
    }

    private void HideSpriteTooltipIfNeeded()
    {
        if (debuffTooltipRoot != null)
            debuffTooltipRoot.SetActive(false);
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
            sb.Append(entry.LegacyIconText);
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
        public DebuffUIEntry(string key, string legacyIconText, Sprite iconSprite, int order, string tooltip)
        {
            Key = key;
            LegacyIconText = legacyIconText;
            IconSprite = iconSprite;
            Order = order;
            Tooltip = tooltip;
        }

        public string Key { get; }
        public string LegacyIconText { get; }
        public Sprite IconSprite { get; }
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

        if (!hasAttemptedTooltipAutoBind && (debuffText != null || debuffIconContainer != null))
        {
            hasAttemptedTooltipAutoBind = true;

            if (debuffTooltipRoot == null)
                debuffTooltipRoot = FindUIObjectByName("debufftooltip");

            if (debuffTooltipText == null)
                debuffTooltipText = FindUITextByName("debufftooltip");
        }

        if (hpText != null && creditText != null && starText != null && winText != null && levelText != null
            && atkText != null && spdText != null && defText != null && (debuffText != null || debuffIconContainer != null))
        {
            // secondaryCreditText เป็น optional
            return;
        }

        // KISS: ถ้ามี ElementButtonManager ให้ยึด status root ของธาตุที่เลือกโดยตรง
        if (activeStatusRoot != null)
        {
            TMP_Text[] statusTexts = activeStatusRoot.GetComponentsInChildren<TMP_Text>(true);
            AssignTextsByName(statusTexts, preferActiveOnly: false);
            AutoAssignDebuffIconContainer(activeStatusRoot);
            return;
        }

        // KISS: หาใน scope ของตัวเองก่อน (ลดโอกาสไปจับ UI ของธาตุ/หน้าต่างอื่น)
        TMP_Text[] localTexts = GetComponentsInChildren<TMP_Text>(true);
        AssignTextsByName(localTexts, preferActiveOnly: true);
        AutoAssignDebuffIconContainer(transform);

        if (hpText != null && creditText != null && starText != null && winText != null && levelText != null
            && atkText != null && spdText != null && defText != null && (debuffText != null || debuffIconContainer != null))
            return;

        // fallback: ค่อยค้นทั้ง scene แต่ยัง prefer เฉพาะ object ที่ active
        TMP_Text[] activeSceneTexts = FindObjectsByType<TMP_Text>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        AssignTextsByName(activeSceneTexts, preferActiveOnly: true);
        if (debuffIconContainer == null)
            debuffIconContainer = FindTransformByName("debufficoncontainer", includeInactive: false);

        if (hpText != null && creditText != null && starText != null && winText != null && levelText != null
            && atkText != null && spdText != null && defText != null && (debuffText != null || debuffIconContainer != null))
            return;

        // fallback สุดท้าย: รวม inactive เผื่อบาง panel เปิดภายหลัง
        TMP_Text[] allTexts = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        AssignTextsByName(allTexts, preferActiveOnly: false);
        if (debuffIconContainer == null)
            debuffIconContainer = FindTransformByName("debufficoncontainer", includeInactive: true);
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
        debuffIconContainer = null;
        debuffTooltipHoverHandler = null;
        hasAttemptedTooltipAutoBind = false;
        debuffIconHandlers.Clear();
    }

    private void AutoAssignDebuffIconContainer(Transform searchRoot)
    {
        if (debuffIconContainer != null || searchRoot == null)
            return;

        Transform[] transforms = searchRoot.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform candidate = transforms[i];
            if (candidate == null || candidate == searchRoot)
                continue;

            string loweredName = candidate.name.ToLower();
            if (loweredName.Contains("debuff") && loweredName.Contains("icon") && loweredName.Contains("container"))
            {
                debuffIconContainer = candidate;
                return;
            }
        }
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

    private static Transform FindTransformByName(string keyword, bool includeInactive)
    {
        if (string.IsNullOrEmpty(keyword))
            return null;

        Transform[] transforms = FindObjectsByType<Transform>(includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform t = transforms[i];
            if (t == null) continue;
            if (t.name.ToLower().Contains(keyword.ToLower()))
                return t;
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
