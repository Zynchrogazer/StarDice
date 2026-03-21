using UnityEngine;
using UnityEngine.UI;

public class OpenSkillPanelButton : MonoBehaviour
{
    public GameObject panel0;
    public GameObject panel1;
    public GameObject panel2;

    public Button openButton;
    public GameObject thisPanel; // Panel ที่ปุ่มนี้จะเปิด

    void Start()
    {
        openButton.onClick.AddListener(() =>
        {
            // ถ้า Panel อื่นเปิดอยู่ → ไม่เปิด Panel นี้
            if (panel0.activeSelf || panel1.activeSelf || panel2.activeSelf)
            {
                Debug.Log("Panel อื่นกำลังเปิดอยู่ ไม่สามารถเปิด Panel นี้ได้");
                return;
            }

            // อัปเดตปุ่มก่อนเปิด
            var skillUI = thisPanel.GetComponent<SkillSelectUI>();
            if (skillUI != null)
            {
                skillUI.RefreshSkillButtons();
            }

            // เปิด Panel
            thisPanel.SetActive(true);
        });
    }
}
