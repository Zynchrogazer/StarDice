using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PassiveSkillSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public PassiveSkillData passiveSkillData;
    public Image iconImage;
    public Image frameImage;

    [Header("Colors")]
    public Color lockedColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
    public Color unlockableColor = new Color(1f, 0.92f, 0.3f, 1f);
    public Color unlockedColor = Color.white;

    [Header("Frame Colors")]
    public Color lockedFrameColor = new Color(0.35f, 0.15f, 0.15f, 1f);
    public Color unlockableFrameColor = new Color(0.95f, 0.8f, 0.2f, 1f);
    public Color unlockedFrameColor = new Color(0.25f, 0.95f, 0.4f, 1f);

    [Header("Locked Overlay")]
    [Range(0f, 1f)] public float lockedOverlayAlpha = 0.5f;
    public GameObject lockedOverlayObject;

    private Image lockedOverlayImage;

    private void Awake()
    {
        if (frameImage == null)
        {
            frameImage = GetComponent<Image>();
        }

        EnsureLockedOverlay();
    }

    private void Start()
    {
        if (passiveSkillData != null && iconImage != null)
        {
            iconImage.sprite = passiveSkillData.icon;
            UpdateUI();
        }

        if (SkillManager.Instance != null)
            SkillManager.Instance.OnSkillTreeUpdated += UpdateUI;
    }

    private void OnDestroy()
    {
        if (SkillManager.Instance != null)
            SkillManager.Instance.OnSkillTreeUpdated -= UpdateUI;
    }

    public void UpdateUI()
    {
        if (passiveSkillData == null || iconImage == null || SkillManager.Instance == null) return;

        bool isUnlocked = SkillManager.Instance.IsUnlocked(passiveSkillData);
        bool canUnlock = SkillManager.Instance.CanUnlock(passiveSkillData);

        if (isUnlocked)
        {
            iconImage.color = unlockedColor;
            if (frameImage != null) frameImage.color = unlockedFrameColor;
            SetLockedOverlay(false);
        }
        else if (canUnlock)
        {
            iconImage.color = unlockableColor;
            if (frameImage != null) frameImage.color = unlockableFrameColor;
            SetLockedOverlay(true, lockedOverlayAlpha * 0.5f);
        }
        else
        {
            iconImage.color = lockedColor;
            if (frameImage != null) frameImage.color = lockedFrameColor;
            SetLockedOverlay(true, lockedOverlayAlpha);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (passiveSkillData != null && SkillManager.Instance != null)
        {
            if (SkillManager.Instance.TryUnlockSkill(passiveSkillData))
            {
                Debug.Log($"Upgrade {passiveSkillData.skillName} Success!");
            }
            else
            {
                Debug.Log("Cannot Unlock (Not enough points or requirements not met)");
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (passiveSkillData != null && PassiveSkillTooltip.Instance != null)
            PassiveSkillTooltip.Instance.ShowTooltip(passiveSkillData.skillName, passiveSkillData.description);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (PassiveSkillTooltip.Instance != null)
            PassiveSkillTooltip.Instance.HideTooltip();
    }

    private void EnsureLockedOverlay()
    {
        if (lockedOverlayObject == null)
        {
            Transform existing = transform.Find("LockedOverlay");
            if (existing != null)
            {
                lockedOverlayObject = existing.gameObject;
            }
            else
            {
                GameObject overlay = new GameObject("LockedOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                overlay.transform.SetParent(transform, false);

                RectTransform rect = overlay.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;

                lockedOverlayObject = overlay;
            }
        }

        if (lockedOverlayObject != null)
        {
            lockedOverlayImage = lockedOverlayObject.GetComponent<Image>();
            if (lockedOverlayImage != null)
            {
                lockedOverlayImage.raycastTarget = false;
                lockedOverlayImage.color = new Color(0f, 0f, 0f, lockedOverlayAlpha);
            }
        }
    }

    private void SetLockedOverlay(bool visible, float alphaOverride = -1f)
    {
        if (lockedOverlayObject == null)
        {
            EnsureLockedOverlay();
        }

        if (lockedOverlayObject == null) return;

        if (lockedOverlayImage != null)
        {
            float useAlpha = alphaOverride >= 0f ? alphaOverride : lockedOverlayAlpha;
            lockedOverlayImage.color = new Color(0f, 0f, 0f, Mathf.Clamp01(useAlpha));
        }

        lockedOverlayObject.SetActive(visible);
    }
}
