using UnityEngine;
using UnityEngine.UI;

public class SkillConnection : MonoBehaviour
{
    [SerializeField] private SkillManager skillManager;
    [Header("Link")]
    public PassiveSkillData fromSkill; // ต้นทาง (สกิล A)
    public PassiveSkillData toSkill;   // ปลายทาง (สกิล B)

    [Header("Visual")]
    public Image lineImage; // ลากรูปเส้นมาใส่ (ควรเป็นสีขาว)
    public Color activeColor = Color.yellow; // สีตอนไฟวิ่งผ่านแล้ว
    public Color inactiveColor = Color.gray; // สีตอนยังไม่ผ่าน

    private void Start()
    {
        if (ResolveSkillManager() != null)
        {
            ResolveSkillManager().OnSkillTreeUpdated += UpdateLine;
        }
        UpdateLine();
    }

    private void OnDestroy()
    {
        if (ResolveSkillManager() != null)
        {
            ResolveSkillManager().OnSkillTreeUpdated -= UpdateLine;
        }
    }

    private SkillManager ResolveSkillManager()
    {
        if (skillManager == null)
            skillManager = FindFirstObjectByType<SkillManager>();

        return skillManager;
    }

    // เส้นจะสว่าง ก็ต่อเมื่อ "ต้นทาง" ถูกปลดล็อคแล้ว
    // หรือจะเอาแบบ "ปลายทาง" ปลดแล้วค่อยสว่างก็ได้ แล้วแต่ดีไซน์
    void UpdateLine()
    {
        if (fromSkill == null || toSkill == null) return;

        // Logic: ถ้าสกิลต้นทางปลดแล้ว เส้นจะสว่างเพื่อบอกว่า "ทางนี้ไปได้นะ"
        if (ResolveSkillManager() == null || lineImage == null) return;

        bool isPathActive = ResolveSkillManager().IsUnlocked(fromSkill);
        lineImage.color = isPathActive ? activeColor : inactiveColor;
    }
}   