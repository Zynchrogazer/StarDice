using UnityEngine;

public class ExitGameHandler : MonoBehaviour
{
    [Header("Confirm Exit UI")]
    [SerializeField] private GameObject confirmExitPanel;

    private void Awake()
    {
        HideConfirmPanel();
    }

    // ผูกกับปุ่ม Exit หลัก
    public void OnExitButtonClicked()
    {
        ShowConfirmPanel();
    }

    // ผูกกับปุ่ม Yes ใน panel
    public void OnConfirmExitYes()
    {
        Debug.Log("👋 Quit Game Confirmed");
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // ผูกกับปุ่ม No ใน panel
    public void OnConfirmExitNo()
    {
        HideConfirmPanel();
    }

    // backward compatible สำหรับปุ่มเดิมที่ผูก QuitGame ไว้
    public void QuitGame()
    {
        OnExitButtonClicked();
    }

    private void ShowConfirmPanel()
    {
        if (confirmExitPanel != null)
        {
            confirmExitPanel.SetActive(true);
        }
    }

    private void HideConfirmPanel()
    {
        if (confirmExitPanel != null)
        {
            confirmExitPanel.SetActive(false);
        }
    }
}
