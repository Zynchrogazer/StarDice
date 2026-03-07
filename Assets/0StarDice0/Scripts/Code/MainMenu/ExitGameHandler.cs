using UnityEngine;

public class ExitGameHandler : MonoBehaviour
{
    // เอาฟังก์ชันนี้ไปผูกกับปุ่ม Exit
    public void QuitGame()
    {
        Debug.Log("👋 Quit Game Called!"); // เช็คใน Console ว่าปุ่มทำงานไหม

        // สำหรับ Build จริง (เกมที่ Compile แล้ว)
        Application.Quit();

        // สำหรับตอนเทสใน Unity Editor (สั่งหยุดเล่น)
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}