using UnityEngine;
using TMPro; 
using UnityEngine.UI;

public class SaveSlot : MonoBehaviour
{
    public int slotNumber; 
    public TextMeshProUGUI slotNameText;  
    public TextMeshProUGUI detailsText;   
    public Button deleteButton;           
    
    [Header("State Visuals (ลาก GameObject มาใส่)")]
    public GameObject noSaveState;  // หน้าตาตอน "ไม่มีเซฟ" (เช่น รูปกากบาท)
    public GameObject hasSaveState; // หน้าตาตอน "มีเซฟ" (เช่น รูปปกเกม)

    private BackupSaveManager manager; 

    public void Setup(BackupSaveManager managerRef)
    {
        manager = managerRef;
        if(deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(() => manager.OnDeleteFileClicked(slotNumber));
        }

        Button slotBtn = GetComponent<Button>();
        slotBtn.onClick.RemoveAllListeners();
        slotBtn.onClick.AddListener(() => manager.OnSlotClicked(slotNumber));
    }
}