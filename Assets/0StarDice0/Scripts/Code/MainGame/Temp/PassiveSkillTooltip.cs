using UnityEngine;
using UnityEngine.UI;
using TMPro; // ใช้ TextMeshPro

public class PassiveSkillTooltip : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;

    private void Awake()
    {
        HideTooltip(); // ซ่อนตอนเริ่มเกม
    }

    private void Update()
    {
        // ขยับตามเมาส์
        if (tooltipPanel != null && tooltipPanel.activeSelf)
        {
            Vector2 mousePosition = Input.mousePosition;
            transform.position = mousePosition + new Vector2(15, -15);
        }
    }

    public void ShowTooltip(string skillName, string skillDesc)
    {
        if (tooltipPanel == null) return;
        tooltipPanel.SetActive(true);
        if (nameText != null) nameText.text = skillName;
        if (descriptionText != null) descriptionText.text = skillDesc;
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }
}
