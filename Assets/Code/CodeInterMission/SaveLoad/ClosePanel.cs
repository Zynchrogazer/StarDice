using UnityEngine;

public class ClosePanel : MonoBehaviour
{
    public GameObject panelToClose; // ลาก Panel ที่ต้องการปิดมาใส่ในช่องนี้

    public void Close()
    {
        if (panelToClose != null)
        {
            panelToClose.SetActive(false);
        }
    }
}