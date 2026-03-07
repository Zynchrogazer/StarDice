using UnityEngine;
using UnityEngine.UI;
using TMPro; // ใช้ TextMeshPro

public class PassiveSkillTooltip : MonoBehaviour
{
    public static PassiveSkillTooltip Instance; // Singleton

    [Header("UI Components")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;

    private void Awake()
    {
        // ตั้งค่า Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }

        HideTooltip(); // ซ่อนตอนเริ่มเกม
    }

    private void Update()
    {
        // ขยับตามเมาส์
        if (tooltipPanel.activeSelf)
        {
            Vector2 mousePosition = Input.mousePosition;
            transform.position = mousePosition + new Vector2(15, -15);
        }
    }

    public void ShowTooltip(string skillName, string skillDesc)
    {
        tooltipPanel.SetActive(true);
        nameText.text = skillName;
        descriptionText.text = skillDesc;
    }

    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }
}