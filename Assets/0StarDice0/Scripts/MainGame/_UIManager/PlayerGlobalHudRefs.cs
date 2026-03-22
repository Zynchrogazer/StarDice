using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerGlobalHudRefs : MonoBehaviour
{
    [Header("Shared HUD")]
    public TMP_Text currentHpText;
    public TMP_Text creditText;
    public TMP_Text levelText;

    [Header("Shared Debuff HUD")]
    public TMP_Text debuffLegacyText;
    public Transform debuffIconContainer;
    public GameObject debuffTooltipRoot;
    public TMP_Text debuffTooltipText;
}
