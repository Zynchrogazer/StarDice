using UnityEngine;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    public GameObject tooltipPanel;
    public TextMeshProUGUI headerText;
    public TextMeshProUGUI descriptionText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        tooltipPanel.SetActive(false);
    }

    private void Update()
    {
        // ให้ Tooltip วิ่งตามเมาส์
        if (tooltipPanel.activeSelf)
        {
            transform.position = Input.mousePosition;
        }
    }

    public void ShowTooltip(string header, string description)
    {
        headerText.text = header;

        // เช็คว่ามี Description ไหม (เผื่อเป็นสกิลที่ไม่มีคำอธิบาย)
        if (string.IsNullOrEmpty(description))
        {
            descriptionText.gameObject.SetActive(false);
        }
        else
        {
            descriptionText.gameObject.SetActive(true);
            descriptionText.text = description;
        }

        tooltipPanel.SetActive(true);
    }

    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
        headerText.text = "";
        descriptionText.text = "";
    }
}