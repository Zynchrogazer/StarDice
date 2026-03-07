using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// จัดการ UI สำหรับการเลือกทางแยกโดยการสร้าง Object ใน World Space
/// </summary>
public class ChoiceUIManager : MonoBehaviour
{
    [Header("World Space UI Dependencies")]
    [Tooltip("Prefab ของลูกศรที่จะใช้สร้างเป็นตัวเลือก")]
    public GameObject choiceArrowPrefab; // เปลี่ยนจาก Button เป็น GameObject

    [Tooltip("ตำแหน่งความสูงที่จะให้ลูกศรลอยเหนือช่อง")]
    public float yOffset = 0.2f;

    // List สำหรับเก็บลูกศรที่สร้างขึ้นมาทั้งหมด เพื่อใช้ในการลบทิ้ง
    private List<GameObject> spawnedArrows = new List<GameObject>();

    /// <summary>
    /// เมธอดสำหรับแสดงทางเลือกโดยการสร้างลูกศรบนโหนดเป้าหมาย
    /// </summary>
    public void DisplayChoices(List<Transform> choices, Action<Transform> onChoiceMade)
    {
        // เคลียร์ลูกศรเก่าทิ้งก่อนเสมอ
        HideChoices();

        // ตรวจสอบว่ามี Prefab หรือไม่
        if (choiceArrowPrefab == null)
        {
            Debug.LogError("Choice Arrow Prefab is not assigned in ChoiceUIManager!");
            return;
        }

        // สร้างลูกศรขึ้นมาใหม่ตามจำนวนทางเลือก
        foreach (Transform choiceNode in choices)
        {
            // คำนวณตำแหน่งที่จะวางลูกศร
            Vector3 spawnPosition = choiceNode.position + new Vector3(0, yOffset, 0);

            // สร้างลูกศรจาก Prefab ณ ตำแหน่งของโหนดที่เป็นทางเลือก
            GameObject arrowInstance = Instantiate(choiceArrowPrefab, spawnPosition, choiceArrowPrefab.transform.rotation);

            // ดึงสคริปต์ ChoiceArrow ออกมาเพื่อตั้งค่า
            ChoiceArrow arrowScript = arrowInstance.GetComponent<ChoiceArrow>();
            if (arrowScript != null)
            {
                // บอกให้ลูกศรจำว่าตัวเองเป็นตัวแทนของโหนดไหน
                arrowScript.TargetNode = choiceNode;

                // ส่วนที่สำคัญที่สุด: ผูก Action (onChoiceMade) ที่ได้รับมาจาก PlayerPathWalker
                // เข้ากับ Event 'OnArrowClicked' ของลูกศรแต่ละอัน
                arrowScript.OnArrowClicked = onChoiceMade;
            }

            // เพิ่มลูกศรที่สร้างใหม่เข้าไปใน List เพื่อการจัดการ
            spawnedArrows.Add(arrowInstance);
        }
    }

    /// <summary>
    /// เมธอดสำหรับซ่อนและทำลายลูกศรทั้งหมด
    /// </summary>
    public void HideChoices()
    {
        foreach (GameObject arrow in spawnedArrows)
        {
            Destroy(arrow);
        }
        spawnedArrows.Clear();
    }

}
