using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class ButtonTextColorChange : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI targetText; // ข้อความที่จะเปลี่ยนสี
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;

    private void Start()
    {
        if (targetText != null)
            targetText.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetText != null)
            targetText.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (targetText != null)
            targetText.color = normalColor;
    }
}
