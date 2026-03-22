using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerStatusPanelRefs : MonoBehaviour
{
    [Header("Section 1: Status Button")]
    [Tooltip("ค่า Max HP / Base HP ของตัวละครในหน้าสถานะ")]
    public TMP_Text statusMaxHpText;
    [Tooltip("ค่า ATK รวมหลังคำนวณอุปกรณ์ / passive / runtime modifier")]
    public TMP_Text statusAttackText;
    [Tooltip("ค่า SPD รวมหลังคำนวณอุปกรณ์ / passive / runtime modifier")]
    public TMP_Text statusSpeedText;
    [Tooltip("ค่า DEF รวมหลังคำนวณอุปกรณ์ / passive / runtime modifier")]
    public TMP_Text statusDefenseText;

    public bool HasCoreBindings()
    {
        return statusMaxHpText != null
            && statusAttackText != null
            && statusSpeedText != null
            && statusDefenseText != null;
    }

    public void BindFromRoot(Transform searchRoot)
    {
        BindFromSingleRoot(searchRoot);
    }

    private void BindFromSingleRoot(Transform searchRoot)
    {
        if (searchRoot == null)
            return;

        TMP_Text[] texts = searchRoot.GetComponentsInChildren<TMP_Text>(true);
        AssignTextsByName(texts);
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

            if (statusMaxHpText == null && lowered.Contains("hp"))
                statusMaxHpText = txt;
            else if (statusAttackText == null && lowered.Contains("atk"))
                statusAttackText = txt;
            else if (statusSpeedText == null && (lowered.Contains("spd") || lowered.Contains("speed")))
                statusSpeedText = txt;
            else if (statusDefenseText == null && lowered.Contains("def"))
                statusDefenseText = txt;
        }
    }
}
