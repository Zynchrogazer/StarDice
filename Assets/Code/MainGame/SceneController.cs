using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController instance; // ตัวแปร static เพื่อให้เข้าถึงได้จากทุกที่

    void Awake()
    {
        // นี่คือ Singleton Pattern: ตรวจสอบว่ามี GameManager ตัวอื่นอยู่แล้วหรือยัง
        if (instance == null)
        {
            instance = this; // ถ้ายังไม่มี ให้ตัวนี้เป็นตัวหลัก
            DontDestroyOnLoad(gameObject); // สั่งไม่ให้ทำลาย Object นี้เมื่อเปลี่ยนซีน
        }
        else
        {
            Destroy(gameObject); // ถ้ามีตัวหลักอยู่แล้ว ให้ทำลายตัวเองทิ้งไป
        }
    }

    // ฟังก์ชันสำหรับเริ่มการต่อสู้
    public void StartBattle()
    {
        // โหลดซีนต่อสู้แบบซ้อนทับเข้ามา
        SceneManager.LoadSceneAsync("BattleScene", LoadSceneMode.Additive);
    }

    // ฟังก์ชันสำหรับจบการต่อสู้
    public void EndBattle()
    {
        // เอาซีนต่อสู้ออกไป
        SceneManager.UnloadSceneAsync("BattleScene");
    }
}