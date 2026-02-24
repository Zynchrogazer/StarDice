using UnityEngine;
using UnityEngine.SceneManagement;

public class BoardGameGroup : MonoBehaviour
{
    public static BoardGameGroup Instance { get; private set; }

    // ชื่อ Scene หลักที่เป็นบอร์ดเกม (ตั้งให้ตรงกับชื่อไฟล์ Scene ของคุณ)
    public string boardSceneName = "MainGame";

    private void Awake()
    {
        // 🛡️ Logic ความเป็นอมตะ
        if (Instance != null && Instance != this)
        {
            // ถ้ามีของเก่าอยู่แล้ว (คือตัวที่เราเล่นค้างไว้)
            // ให้ทำลายตัวใหม่ทิ้งซะ (ตัวใหม่ที่เพิ่งโหลดมาพร้อม Scene)
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // ห้ามทำลายก้อนนี้
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // เช็คชื่อ Scene
        if (scene.name == boardSceneName)
        {
            // 🏠 ถ้ากลับมาบ้าน (Board Game) -> ให้แสดงตัว
            Debug.Log("[BoardSystem] Welcome Home! Showing Board.");
            ShowBoard(true);
        }
        else
        {
            // ⚔️ ถ้าไปที่อื่น (Battle / Minigame) -> ให้ซ่อนตัว
            Debug.Log($"[BoardSystem] Entering {scene.name}. Hiding Board.");
            ShowBoard(false);
        }
    }

    public void ShowBoard(bool show)
    {
        // เปิด/ปิด ลูกๆ ทั้งหมดในก้อนนี้
        // เราไม่ใช้ gameObject.SetActive(false) กับตัวเองตรงๆ 
        // เพราะเดี๋ยว Script นี้จะหยุดทำงานไปด้วย

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(show);
        }
    }
}