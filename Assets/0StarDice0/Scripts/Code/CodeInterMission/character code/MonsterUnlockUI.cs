using UnityEngine;
using UnityEngine.UI;

public class MonsterUnlockUI : MonoBehaviour
{
    public Button waterButton;
    public Button earthButton;
    public Button windButton;
    public Button lightButton;
    public Button darkButton;
    public Button fireButton;

    void Start()
    {
        // ตั้งค่าเริ่มต้นให้ล็อคไว้ก่อน
        waterButton.interactable = false;
        earthButton.interactable = false;
        windButton.interactable = false;
        lightButton.interactable = false;
        darkButton.interactable = false;
        fireButton.interactable = false;

        // ปลดล็อคถ้าเคยสุ่มได้
        if (PlayerPrefs.GetInt("MonsterWater", 0) == 1) waterButton.interactable = true;
        if (PlayerPrefs.GetInt("MonsterEarth", 0) == 1) earthButton.interactable = true;
        if (PlayerPrefs.GetInt("MonsterWind", 0) == 1) windButton.interactable = true;
        if (PlayerPrefs.GetInt("MonsterLight", 0) == 1) lightButton.interactable = true;
        if (PlayerPrefs.GetInt("MonsterDark", 0) == 1) darkButton.interactable = true;
        if (PlayerPrefs.GetInt("MonsterFire", 0) == 1) fireButton.interactable = true;
    }
}
