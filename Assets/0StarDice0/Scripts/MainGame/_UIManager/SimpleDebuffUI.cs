using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SimpleDebuffUI : MonoBehaviour
{
    [Header("อ้างอิง Player State")]
    public PlayerState playerState;

    [Header("ช่องภาพทั้ง 5 ช่อง (Image 1 ถึง 5)")]
    public Image[] debuffSlots; 

    [Header("รูปภาพ (0=Burn, 1=Freeze, 2=Curse, 3=Poison, 4=Sleep)")]
    public Sprite[] debuffSprites;

    [Header("หน้าต่าง Tooltip")]
    public GameObject tooltipPanel;  
    public TMP_Text tooltipText;     

    private int[] slotDebuffTypes = new int[5] { -1, -1, -1, -1, -1 };

    private void Start()
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }

    private void Update()
    {
        if (playerState == null)
        {
            FindPlayer();
            return; 
        }

        if (debuffSlots == null || debuffSlots.Length == 0 || debuffSprites == null || debuffSprites.Length < 5) return;

        // 1. จัดคิว Debuff ที่ติดอยู่
        List<int> activeDebuffs = new List<int>();
        if (playerState.DebuffBurn && playerState.DebuffBurnTurnsRemaining > 0) activeDebuffs.Add(0);
        if (playerState.hasIceEffect) activeDebuffs.Add(1);
        if (playerState.backwardCurseTurns > 0) activeDebuffs.Add(2);
        if (playerState.poisonDebuffTurns > 0) activeDebuffs.Add(3);
        if (playerState.sleepDebuffTurns > 0) activeDebuffs.Add(4);

        // 2. อัปเดตภาพลงในช่องให้เรียบร้อย
        for (int i = 0; i < debuffSlots.Length; i++)
        {
            if (i < activeDebuffs.Count)
            {
                int debuffType = activeDebuffs[i];
                debuffSlots[i].sprite = debuffSprites[debuffType];
                debuffSlots[i].enabled = true;
                slotDebuffTypes[i] = debuffType; 
            }
            else
            {
                debuffSlots[i].enabled = false;
                slotDebuffTypes[i] = -1;
            }
        }

        // 3. 🟢 ระบบจับเมาส์แบบยิงตรง (ทะลวงทุกบัคของ Unity)
        CheckMouseHoverDirectly();
    }

    // 🟢 ฟังก์ชันคำนวณว่า "เมาส์" เข้าไปเหยียบ "กรอบรูปภาพ" ไหนอยู่
    private void CheckMouseHoverDirectly()
    {
        if (tooltipPanel == null || tooltipText == null) return;

        int hoveredSlotIndex = -1;

        // วนเช็คกรอบรูปภาพทีละอัน
        for (int i = 0; i < debuffSlots.Length; i++)
        {
            // เช็คเฉพาะช่องที่เปิดใช้งานอยู่เท่านั้น
            if (debuffSlots[i] != null && debuffSlots[i].enabled) 
            {
                // ใช้การคำนวณแกน X Y บนหน้าจอตรงๆ ไม่สนว่าจะโดนอะไรบังอยู่
                if (RectTransformUtility.RectangleContainsScreenPoint(debuffSlots[i].rectTransform, Input.mousePosition, null))
                {
                    hoveredSlotIndex = i;
                    break; // เจอแล้วว่าชี้ช่องไหนอยู่ ให้หยุดหาทันที
                }
            }
        }

        // ถ้าพบว่าเมาส์เหยียบอยู่บนช่องใดช่องหนึ่ง
        if (hoveredSlotIndex != -1)
        {
            int debuffType = slotDebuffTypes[hoveredSlotIndex];
            if (debuffType != -1)
            {
                tooltipText.text = GetDebuffDescription(debuffType);
                tooltipPanel.SetActive(true);
            }
        }
        else
        {
            // ถ้าไม่ได้ชี้ช่องไหนเลย ให้ปิด Tooltip
            tooltipPanel.SetActive(false);
        }
    }

    private string GetDebuffDescription(int index)
    {
        if (playerState == null) return "";

        switch (index)
        {
            case 0: return $"<color=#FF5555>Burn</color>\nTake damage at the start of your turn.\nRemaining: {playerState.DebuffBurnTurnsRemaining} turn(s)";
            case 1: return "<color=#55FFFF>Freeze</color>\nNext dice roll is halved.\nRemaining: 1 time";
            case 2: return $"<color=#B555FF>Curse</color>\nForced to move backwards.\nRemaining: {playerState.backwardCurseTurns} turn(s)";
            case 3: return $"<color=#55FF55>Poison</color>\nTake damage equal to dice roll x2.\nRemaining: {playerState.poisonDebuffTurns} turn(s)";
            case 4: return $"<color=#AAAAFF>Sleep</color>\nSkip your turn.\nRemaining: {playerState.sleepDebuffTurns} turn(s)";
            default: return "";
        }
    }

    private void FindPlayer()
    {
        PlayerState[] allPlayers = FindObjectsByType<PlayerState>(FindObjectsSortMode.None);
        foreach (var p in allPlayers)
        {
            if (!p.isAI) { playerState = p; break; }
        }
    }
}