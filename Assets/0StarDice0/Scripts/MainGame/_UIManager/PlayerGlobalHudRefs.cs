using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerGlobalHudRefs : MonoBehaviour
{
    [Header("Preferred Debuff HUD")]
    public SimpleDebuffUI simpleDebuffUI;

    public bool UseSimpleDebuffUI => ResolveSimpleDebuffUI() != null;

    public SimpleDebuffUI ResolveSimpleDebuffUI()
    {
        if (simpleDebuffUI == null)
            simpleDebuffUI = GetComponent<SimpleDebuffUI>();

        if (simpleDebuffUI == null)
            simpleDebuffUI = GetComponentInChildren<SimpleDebuffUI>(true);

        if (simpleDebuffUI != null && !simpleDebuffUI.isActiveAndEnabled)
            return null;

        return simpleDebuffUI;
    }

    [Header("Shared HUD")]
    public TMP_Text currentHpText;
    public TMP_Text creditText;
    public TMP_Text levelText;
}
