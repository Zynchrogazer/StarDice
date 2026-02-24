using UnityEngine;

public class GameSystem : MonoBehaviour
{
    public static GameSystem Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject); // ❌ ลบตัวเก่า
        }

        Instance = this;
        // ❌ ไม่ต้อง DontDestroyOnLoad เพราะอยากให้เปลี่ยนไปตาม Scene
    }
}
