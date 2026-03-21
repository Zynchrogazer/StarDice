using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SkillUnlocker : MonoBehaviour
{
    public Button[] skillButtons = new Button[7]; // ปุ่มสกิล 7
    public GameObject randomPanel; // Panel ที่มีปุ่มสุ่ม
    public Button[] randomButtons = new Button[3]; // ปุ่มสุ่ม 3 ปุ่ม

    void Start()
    {
        // เริ่มต้นล็อคสกิล 3-10
        for (int i = 0; i < skillButtons.Length; i++)
        {
            LockSkillButton(skillButtons[i]);
        }

        // กำหนดให้ปุ่มสุ่มแต่ละปุ่มกดแล้วเรียก RandomUnlockSkill
        for(int i = 0; i < randomButtons.Length; i++)
    {
            if (randomButtons[i] != null) // ✅ เช็ค null เสมอ
            {
                randomButtons[i].onClick.AddListener(() => RandomUnlockSkill());
            }
        }
    }

    void LockSkillButton(Button btn)
    {
        btn.interactable = false;
        ColorBlock cb = btn.colors;
        cb.normalColor = Color.gray;
        cb.highlightedColor = Color.gray;
        cb.pressedColor = Color.gray;
        cb.selectedColor = Color.gray;
        btn.colors = cb;
    }

    void UnlockSkillButton(Button btn)
    {
        btn.interactable = true;
        ColorBlock cb = btn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = Color.white;
        cb.pressedColor = Color.white;
        cb.selectedColor = Color.white;
        btn.colors = cb;
    }

    void RandomUnlockSkill()
    {
        // สุ่มเลข 3-10 จำนวน 3 ตัวไม่ซ้ำ
        List<int> indices = new List<int>();
        while (indices.Count < 3)
        {
            int rand = Random.Range(0, 6); // 2-9 = ปุ่ม 3-10
            if (!indices.Contains(rand))
                indices.Add(rand);
        }

        // ปลดล็อคปุ่มที่สุ่มได้
        foreach (int i in indices)
        {
            UnlockSkillButton(skillButtons[i]);
            Debug.Log($"ปลดล็อคสกิลปุ่ม {i + 1}");
        }

        // ปิด panel ปุ่มสุ่มทันที
        randomPanel.SetActive(false);
    }
}
