using UnityEngine;
using UnityEditor;

public class RenameChildNodes : MonoBehaviour
{
    [MenuItem("Tools/Rename Child Nodes in Order")]
    private static void RenameChildren()
    {
        if (Selection.activeTransform == null)
        {
            Debug.LogWarning("Please select a parent GameObject that has children.");
            return;
        }

        Transform parent = Selection.activeTransform;

        // รวบรวมลูกทั้งหมด
        Transform[] children = new Transform[parent.childCount];
        for (int i = 0; i < parent.childCount; i++)
        {
            children[i] = parent.GetChild(i);
        }

        // เรียงตามตำแหน่งในโลก (World Position) หรืออื่น ๆ ได้ถ้าต้องการ

        // 🔤 เรียงตามชื่อก่อนก็ได้ (ถ้าต้องการ)
        // System.Array.Sort(children, (a, b) => string.Compare(a.name, b.name));

        // ✅ Rename เป็น Node_01, Node_02 ...
        for (int i = 0; i < children.Length; i++)
        {
            children[i].name = $"Node_{(i + 1).ToString("D2")}";
        }

        Debug.Log("Renamed all child nodes in order.");
    }
}
