using UnityEngine;
using UnityEngine.SceneManagement;

public class BoardGameGroup : MonoBehaviour
{
    public static BoardGameGroup Instance { get; private set; }

    // ใช้เป็น fallback สำหรับ scene เก่าที่อาจไม่มี RouteManager
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
        // ✅ เช็คจาก scene ที่โหลดเข้ามา "โดยตรง" และรวม inactive object
        // แก้เคสที่บอร์ดถูกซ่อนอยู่ก่อนแล้ว ทำให้ FindObjectOfType หา RouteManager ไม่เจอ
        bool isBoardScene = SceneHasRouteManager(scene);

        // fallback: เผื่อ scene เก่าที่ยังไม่ได้วาง RouteManager
        if (!isBoardScene && scene.name == boardSceneName)
            isBoardScene = true;

        if (isBoardScene)
        {
            Debug.Log($"[BoardSystem] Welcome Home ({scene.name})! Showing Board.");
            ShowBoard(true);
        }
        else
        {
            Debug.Log($"[BoardSystem] Entering {scene.name}. Hiding Board.");
            ShowBoard(false);
        }
    }

    private bool SceneHasRouteManager(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded)
            return false;

        var roots = scene.GetRootGameObjects();
        foreach (var root in roots)
        {
            if (root.GetComponentInChildren<RouteManager>(true) != null)
                return true;
        }

        return false;
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
