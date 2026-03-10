using UnityEngine;
using TMPro; // ถ้าใช้ TextMeshPro
using UnityEngine.UI;
using System.Collections;

public class TurnAnnouncementUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panelObj;      // ตัว Panel ที่จะให้เด้ง (Image พื้นหลัง)
    public TextMeshProUGUI turnText; // ตัวหนังสือที่จะเปลี่ยนข้อความ

    [Header("Settings")]
    public float showDuration = 1.5f; // โชว์นานกี่วิ (ควรน้อยกว่าเวลารอใน Manager นิดหน่อย)

    private void Start()
    {
        // เริ่มมาซ่อนก่อน
        if (panelObj != null) panelObj.SetActive(false);

        // 👂 ดักฟัง GameTurnManager
        if (GameTurnManager.TryGet(out var gameTurnManager))
        {
            gameTurnManager.OnTurnChanged += ShowTurnAnnouncement;
        }
    }

    private void OnDestroy()
    {
        // เลิกฟังเมื่อถูกทำลาย (สำคัญมาก ป้องกัน Error)
        if (GameTurnManager.TryGet(out var gameTurnManager))
        {
            gameTurnManager.OnTurnChanged -= ShowTurnAnnouncement;
        }
    }

    private void ShowTurnAnnouncement(bool isAI)
    {
        // 1. ตั้งข้อความ
        if (turnText != null)
        {
            if (isAI)
            {
                turnText.text = "Enemy Turn";
                turnText.color = Color.red; // (Optional) เปลี่ยนสีตัวหนังสือ
            }
            else
            {
                turnText.text = "Player Turn";
                turnText.color = Color.green;
            }
        }

        // 2. เปิด Panel และเริ่มนับถอยหลังปิด
        StartCoroutine(PanelRoutine());
    }

    IEnumerator PanelRoutine()
    {
        if (panelObj != null) panelObj.SetActive(true);

        // (Optional) ใส่เสียงประกาศเทิร์นตรงนี้ได้

        yield return new WaitForSeconds(showDuration);

        if (panelObj != null) panelObj.SetActive(false);
    }
}