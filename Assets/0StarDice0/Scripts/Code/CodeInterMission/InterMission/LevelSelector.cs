using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // จำเป็นสำหรับการเปลี่ยน Scene

public class LevelSelector : MonoBehaviour
{
    // ลากปุ่มทั้ง 4 มาใส่ใน Inspector
    public Button[] levelButtons;
    
    // ลากรูป LockIcon ของแต่ละปุ่มมาใส่ (เรียงลำดับให้ตรงกับปุ่ม)
    public GameObject[] lockIcons;

    void Start()
    {
        // ดึงข้อมูลว่าเล่นถึงด่านไหนแล้ว (ถ้าไม่มีข้อมูล ให้เริ่มที่ 1)
        int levelReached = PlayerPrefs.GetInt("levelReached", 1);

        for (int i = 0; i < levelButtons.Length; i++)
        {
            // เช็คว่าปุ่มนี้ (ด่าน i+1) ควรจะปลดล็อคไหม
            if (i + 1 > levelReached)
            {
                // --- กรณีล็อค (Locked) ---
                levelButtons[i].interactable = false; // กดไม่ได้
                levelButtons[i].image.color = Color.gray; // ทำปุ่มสีมืดๆ
                
                if(lockIcons[i] != null) 
                    lockIcons[i].SetActive(true); // โชว์รูปกุญแจ
            }
            else
            {
                // --- กรณีปลดล็อค (Unlocked) ---
                levelButtons[i].interactable = true; // กดได้
                levelButtons[i].image.color = Color.white; // สีปกติ
                
                if(lockIcons[i] != null) 
                    lockIcons[i].SetActive(false); // ซ่อนรูปกุญแจ
            }
        }
    }

    // ฟังก์ชันสำหรับให้ปุ่มกดเรียกใช้เพื่อเข้าด่าน
    public void SelectLevel(string levelName)
    {
        if (!SceneFlowController.TryRequestScene(levelName))
        {
            SceneManager.LoadScene(levelName);
        }
    }
}