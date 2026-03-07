using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PassiveSkillSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public PassiveSkillData passiveSkillData;
    public Image iconImage;
    //public Image frameImage; // กรอบ (ถ้าอยากเปลี่ยนสีเลเวลกรอบด้วย)

    [Header("Colors")]
    public Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 1f); // สีมืดๆ (ยังอัปไม่ได้)
    public Color unlockableColor = new Color(0.7f, 0.7f, 0.7f, 1f); // สีเทาๆ (เงื่อนไขครบ แต่อัปได้)
    public Color unlockedColor = Color.white; // สีสว่าง (อัปแล้ว)

    private void Start()
    {
        if (passiveSkillData != null)
        {
            iconImage.sprite = passiveSkillData.icon;
            UpdateUI(); // เช็คสถานะตอนเริ่ม
        }

        // สมัครรับข่าวสาร: ถ้ามีการอัปสกิลที่อื่น ให้ฉันเช็คตัวเองใหม่ด้วย
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
        if (passiveSkillData == null) return;

        bool isUnlocked = SkillManager.Instance.IsUnlocked(passiveSkillData);
        bool canUnlock = SkillManager.Instance.CanUnlock(passiveSkillData);

        if (isUnlocked)
        {
            // ✅ ปลดแล้ว: สว่างเต็มที่
            iconImage.color = unlockedColor;
        }
        else if (canUnlock)
        {
            // 🟡 ยังไม่ปลด แต่เงื่อนไขครบ: สีกลางๆ (รอให้กด)
            iconImage.color = unlockableColor;
        }
        else
        {
            // 🔒 ล็อค: มืดตึ๊ดตื๋อ
            iconImage.color = lockedColor;
        }
    }

    // กดคลิกเพื่ออัปเกรด
    public void OnPointerClick(PointerEventData eventData)
    {
        if (passiveSkillData != null)
        {
            if (SkillManager.Instance.TryUnlockSkill(passiveSkillData))
            {
                Debug.Log($"Upgrade {passiveSkillData.skillName} Success!");
                // (ใส่เสียง Effect ตรงนี้ได้)
            }
            else
            {
                Debug.Log("Cannot Unlock (Not enough points or requirements not met)");
            }
        }
    }

    // (ส่วน Tooltip เหมือนเดิม)
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (passiveSkillData != null)
            PassiveSkillTooltip.Instance.ShowTooltip(passiveSkillData.skillName, passiveSkillData.description);
    }

    public void OnPointerExit(PointerEventData eventData)
        => PassiveSkillTooltip.Instance.HideTooltip();
}