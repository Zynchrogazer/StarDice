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
   
    // ⚠️ เปลี่ยนค่าเริ่มต้นตรงนี้เป็น 0 (ผู้เล่นใหม่จะยังไม่มีสิทธิ์กดด่าน 1)
    int levelReached = PlayerPrefs.GetInt("levelReached", 0);

    for (int i = 0; i < levelButtons.Length; i++)
    {
        // i = 0 คือปุ่มด่าน 1 (ดังนั้นเลขด่านของปุ่มคือ i + 1)
        // ถ้าเลขด่านของปุ่มนี้ (i + 1) มากกว่า สิทธิ์ที่ผู้เล่นมี (levelReached) ให้ล็อค
        if (i + 1 > levelReached) 
        {
            // --- กรณีล็อค (Locked) ---
            levelButtons[i].interactable = false;
            levelButtons[i].image.color = Color.gray;
            
            if(lockIcons[i] != null) 
                lockIcons[i].SetActive(true);
        }
        else
        {
            // --- กรณีปลดล็อค (Unlocked) ---
            levelButtons[i].interactable = true;
            levelButtons[i].image.color = Color.white;
            
            if(lockIcons[i] != null) 
                lockIcons[i].SetActive(false);
        }
    }
}
    // ฟังก์ชันสำหรับให้ปุ่มกดเรียกใช้เพื่อเข้าด่าน
    public void SelectLevel(string levelName)
    {
        if (!SceneFlowController.TryRequestScene(levelName))
        {
            if (Application.CanStreamedLevelBeLoaded(levelName))
            {
                SceneManager.LoadScene(levelName);
            }
            else
            {
                Debug.LogError($"[LevelSelector] Cannot load scene '{levelName}'. Check Build Profiles.");
            }
        }
    }
}