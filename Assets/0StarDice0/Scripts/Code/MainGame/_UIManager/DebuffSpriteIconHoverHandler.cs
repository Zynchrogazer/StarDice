using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DebuffSpriteIconHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [SerializeField] private Image targetImage;

    private GameObject tooltipRoot;
    private TMP_Text tooltipLabel;
    private string tooltipMessage;
    private bool isPointerInside;

    public Image TargetImage => targetImage;

    private void Awake()
    {
        CacheImageReference();
    }

    public void CacheImageReference()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();
    }

    public void Bind(GameObject root, TMP_Text label, string message)
    {
        tooltipRoot = root;
        tooltipLabel = label;
        tooltipMessage = message ?? string.Empty;

        if (tooltipRoot != null)
            tooltipRoot.SetActive(false);
    }

    public void SetSprite(Sprite sprite)
    {
        CacheImageReference();
        if (targetImage == null)
            return;

        targetImage.sprite = sprite;
        targetImage.enabled = sprite != null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerInside = true;
        UpdateTooltipAtPointer(eventData);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (!isPointerInside)
            return;

        UpdateTooltipAtPointer(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerInside = false;
        HideTooltip();
    }

    private void UpdateTooltipAtPointer(PointerEventData eventData)
    {
        if (tooltipRoot == null || tooltipLabel == null || string.IsNullOrEmpty(tooltipMessage))
            return;

        tooltipLabel.text = tooltipMessage;
        tooltipRoot.SetActive(true);

        RectTransform tooltipRect = tooltipRoot.transform as RectTransform;
        RectTransform canvasRect = tooltipRoot.GetComponentInParent<Canvas>()?.transform as RectTransform;
        if (tooltipRect != null && canvasRect != null)
        {
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, eventData.pressEventCamera, out localPoint))
                tooltipRect.anchoredPosition = localPoint + new Vector2(20f, -20f);
        }
    }

    private void HideTooltip()
    {
        if (tooltipRoot != null)
            tooltipRoot.SetActive(false);
    }
}
