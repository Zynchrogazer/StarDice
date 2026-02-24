using UnityEngine;
using UnityEngine.UI;

public class MonsterUnlockUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button waterButton;
    public Button earthButton;
    public Button windButton;
    public Button lightButton;
    public Button darkButton;
    public Button fireButton; // 1. เพิ่มปุ่มธาตุไฟ

    void Start()
    {
        // รีเซ็ตปุ่ม: ล็อคทุกปุ่มก่อน แล้วค่อยเช็คว่าอันไหนควรเปิด
        LockAllButtonsInternal();

        // ตรวจสอบสถานะการปลดล็อคจาก PlayerPrefs (ถ้ามีค่าเป็น 1 คือปลดล็อคแล้ว)
        CheckUnlockStatus();
    }

    void CheckUnlockStatus()
    {
        // ถ้าเคยได้ตัวนั้นแล้ว ให้ปุ่มกดได้ (Interactable = true)
        if (PlayerPrefs.GetInt("MonsterWater", 0) == 1) waterButton.interactable = true;
        if (PlayerPrefs.GetInt("MonsterEarth", 0) == 1) earthButton.interactable = true;
        if (PlayerPrefs.GetInt("MonsterWind", 0) == 1) windButton.interactable = true;
        if (PlayerPrefs.GetInt("MonsterLight", 0) == 1) lightButton.interactable = true;
        if (PlayerPrefs.GetInt("MonsterDark", 0) == 1) darkButton.interactable = true;
        if (PlayerPrefs.GetInt("MonsterFire", 0) == 1) fireButton.interactable = true; // เช็คธาตุไฟ
    }

    // 2. ฟังก์ชันนี้จะถูกเรียกเมื่อกดปุ่ม (ต้องไปผูกใน Inspector)
    public void SelectMonster(string element)
    {
        Debug.Log("เลือกตัวละครธาตุ: " + element);

        // บันทึกตัวละครที่เลือกล่าสุดลงในเครื่อง
        PlayerPrefs.SetString("SelectedMonster", element);
        PlayerPrefs.Save();

        // (Option) ถ้าต้องการให้กดปุ่มนี้แล้ว เป็นการ "ปลดล็อค" ตัวละครนั้นด้วย (กรณีเลือกตัวเริ่มต้น)
        // PlayerPrefs.SetInt("Monster" + element, 1); 

        // 3. ล็อคปุ่มทั้งหมดทันทีเพื่อไม่ให้กดเลือกซ้ำ
        LockAllButtonsInternal();
        
        // ตรงนี้สามารถใส่โค้ดเปลี่ยน Scene ได้ เช่น SceneManager.LoadScene("GameScene");
    }

    // ฟังก์ชันสำหรับสั่งปิดปุ่มทั้งหมด
    private void LockAllButtonsInternal()
    {
        waterButton.interactable = false;
        earthButton.interactable = false;
        windButton.interactable = false;
        lightButton.interactable = false;
        darkButton.interactable = false;
        fireButton.interactable = false;
    }
}