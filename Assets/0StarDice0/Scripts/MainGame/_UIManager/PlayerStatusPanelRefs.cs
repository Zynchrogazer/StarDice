using System.Collections.Generic;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerStatusPanelRefs : MonoBehaviour
{
    [Header("Optional search roots")]
    [Tooltip("ถ้า UI ของ panel นี้แตกเป็นหลาย root (เช่น Status + HUD + Debuff) ให้ใส่ root เพิ่มตรงนี้เพื่อช่วย auto-bind เฉพาะส่วนที่ยังว่างอยู่")]
    public Transform[] additionalSearchRoots;

    [Header("Section 1: Status Button")]
    [Tooltip("ค่า Max HP / Base HP ของตัวละครในหน้าสถานะ")]
    public TMP_Text statusMaxHpText;
    [Tooltip("ค่า ATK รวมหลังคำนวณอุปกรณ์ / passive / runtime modifier")]
    public TMP_Text statusAttackText;
    [Tooltip("ค่า SPD รวมหลังคำนวณอุปกรณ์ / passive / runtime modifier")]
    public TMP_Text statusSpeedText;
    [Tooltip("ค่า DEF รวมหลังคำนวณอุปกรณ์ / passive / runtime modifier")]
    public TMP_Text statusDefenseText;

    [Header("Section 2: HUD")]
    [Tooltip("Current HP / Max HP เช่น 72/96")]
    public TMP_Text hudCurrentHpText;
    [Tooltip("Credit หลักของ HUD")]
    public TMP_Text hudCreditText;
    [Tooltip("Level หลักของ HUD")]
    public TMP_Text hudLevelText;

    [Header("Section 3: Debuff")]
    [Tooltip("Legacy TMP text สำหรับ debuff icon แบบ rich text")]
    public TMP_Text debuffLegacyText;
    public Transform debuffIconContainer;
    public GameObject debuffTooltipRoot;
    public TMP_Text debuffTooltipText;

    public bool HasCoreBindings()
    {
        return statusMaxHpText != null
            && hudCreditText != null
            && hudLevelText != null
            && statusAttackText != null
            && statusSpeedText != null
            && statusDefenseText != null
            && (debuffLegacyText != null || debuffIconContainer != null);
    }

    public void BindFromRoot(Transform searchRoot)
    {
        BindFromSingleRoot(searchRoot);

        if (additionalSearchRoots == null)
            return;

        for (int i = 0; i < additionalSearchRoots.Length; i++)
        {
            BindFromSingleRoot(additionalSearchRoots[i]);
        }
    }

    private void BindFromSingleRoot(Transform searchRoot)
    {
        if (searchRoot == null)
            return;

        TMP_Text[] texts = searchRoot.GetComponentsInChildren<TMP_Text>(true);
        AssignTextsByName(texts);
        if (debuffIconContainer == null)
            debuffIconContainer = FindTransformByKeyword(searchRoot, "debuff", "icon", "container");

        if (debuffTooltipRoot == null)
            debuffTooltipRoot = FindGameObjectByKeyword(searchRoot, "debufftooltip");

        if (debuffTooltipText == null)
            debuffTooltipText = FindTextByKeyword(searchRoot, "debufftooltip");
    }

    private void AssignTextsByName(TMP_Text[] texts)
    {
        if (texts == null)
            return;

        foreach (TMP_Text txt in texts)
        {
            if (txt == null)
                continue;

            string lowered = txt.name.ToLowerInvariant();

            if (hudCurrentHpText == null && lowered.Contains("hp") && (lowered.Contains("max") || lowered.Contains("full") || lowered.Contains("slash") || lowered.Contains("detail") || lowered.Contains("current")))
                hudCurrentHpText = txt;
            else if (statusMaxHpText == null && lowered.Contains("hp"))
                statusMaxHpText = txt;
            else if (lowered.Contains("credit"))
            {
                if (hudCreditText == null)
                    hudCreditText = txt;
            }
            else if (hudLevelText == null && (lowered.Contains("level") || lowered.Contains("lv")))
                hudLevelText = txt;
            else if (statusAttackText == null && lowered.Contains("atk"))
                statusAttackText = txt;
            else if (statusSpeedText == null && (lowered.Contains("spd") || lowered.Contains("speed")))
                statusSpeedText = txt;
            else if (statusDefenseText == null && lowered.Contains("def"))
                statusDefenseText = txt;
            else if (debuffLegacyText == null && (lowered.Contains("debuff") || (lowered.Contains("status") && lowered.Contains("icon"))))
                debuffLegacyText = txt;
        }
    }

    private static Transform FindTransformByKeyword(Transform root, params string[] keywords)
    {
        if (root == null)
            return null;

        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform candidate = transforms[i];
            if (candidate == null || candidate == root)
                continue;

            string lowered = candidate.name.ToLowerInvariant();
            if (ContainsAllKeywords(lowered, keywords))
                return candidate;
        }

        return null;
    }

    private static GameObject FindGameObjectByKeyword(Transform root, params string[] keywords)
    {
        Transform found = FindTransformByKeyword(root, keywords);
        return found != null ? found.gameObject : null;
    }

    private static TMP_Text FindTextByKeyword(Transform root, params string[] keywords)
    {
        TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            TMP_Text candidate = texts[i];
            if (candidate == null)
                continue;

            if (ContainsAllKeywords(candidate.name.ToLowerInvariant(), keywords))
                return candidate;
        }

        return null;
    }

    private static bool ContainsAllKeywords(string text, IReadOnlyList<string> keywords)
    {
        if (string.IsNullOrEmpty(text) || keywords == null)
            return false;

        for (int i = 0; i < keywords.Count; i++)
        {
            string keyword = keywords[i];
            if (string.IsNullOrEmpty(keyword))
                continue;

            if (!text.Contains(keyword.ToLowerInvariant()))
                return false;
        }

        return true;
    }
}
