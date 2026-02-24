using UnityEngine;
using UnityEngine.UI;

public class ItemButton : MonoBehaviour
{
    [Header("ข้อมูลไอเท็ม")]
    public EquipmentData itemData;
    public Image iconImage;
    public EquipmentPanelManager panelManager;

    [Header("ตั้งค่าช่องสวมใส่")]
    public int targetSlotIndex = 0;

    [Header("UI แสดงสถานะ")]
    // *** 1. เพิ่มตัวแปรนี้ ให้ลาก Overlay ที่สร้างเมื่อกี้มาใส่ ***
    public GameObject equippedOverlay; 

    void OnEnable()
    {
        if (itemData != null)
        {
            // 1. ตั้งรูปไอคอน
            if (iconImage != null) iconImage.sprite = itemData.icon;

            // 2. เช็คสถานะ
            bool isOwned = itemData.isOwned;
            bool isEquipped = PlayerDataManager.Instance.IsItemEquipped(itemData);

            Button btn = GetComponent<Button>();
            
            // ดึงค่าสีปัจจุบันของปุ่มออกมาเพื่อแก้ไข
            ColorBlock colors = btn.colors;

           if (isEquipped)
            {
                // --- กรณี: ใส่อยู่ (Equipped) ---
                btn.interactable = false; 

                // แนะนำ: ใช้สีเขียวอ่อนๆ (R=0.7, G=1.0, B=0.7) จะดูเหมือน "ไฟสถานะเปิดอยู่"
                // และปรับค่าตัวสุดท้าย (Alpha) ให้เต็ม 1.0f จะได้ดูชัดๆ ไม่จางเหมือนของที่ไม่มี
                //colors.disabledColor = new Color(0.7f, 0.1f, 0.7f, 1f); 
                
                // หรือถ้าอยากได้โทนขาวๆ แต่ให้ต่างจริงๆ ให้ใช้ "สีเทาเงิน" (Silver)
            // เขียวทึบ (R=0.3, G=0.7, B=0.3)
            colors.disabledColor = new Color(0.3f, 1f, 0.3f, 1f);
            }
            else if (!isOwned)
            {
                // --- กรณี: ไม่มีของ (Unusable) ---
                btn.interactable = false; 
                
                // สีเทาเข้ม และจางๆ (Alpha 0.5)
                colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f); 
            }
            else
            {
                // --- กรณี: มีของ (Owned) ---
                btn.interactable = true; 
                // ปกติปุ่มจะเป็นสีขาว (Color.white) โดยอัตโนมัติ
            }

            // *** สำคัญมาก: ต้องยัดค่าสีกลับเข้าไปในปุ่ม ***
            btn.colors = colors;
        }
    }

    public void OnClick()
    {
        // ... (โค้ดเดิม)
        PlayerDataManager.Instance.EquipItem(itemData, targetSlotIndex);
       // if (panelManager != null) panelManager.ClosePanel();
        EquippedItemDisplay display = FindObjectOfType<EquippedItemDisplay>();
        if (display != null) display.UpdateUI();
    }
}