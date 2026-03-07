using UnityEngine;
using UnityEngine.UI;

public class ElementButtonManager : MonoBehaviour
{
    public Button[] buttons; // Array ของปุ่ม
    public PlayerData selectedPlayer;
    void Start()
    {
        UpdateButtons();
    }

    void UpdateButtons()
    {

        if (GameData.Instance != null)
        {
            selectedPlayer = GameData.Instance.selectedPlayer;
        }

        if (selectedPlayer == null)
        {
            Debug.LogWarning("ยังไม่ได้เลือกตัวละครจาก GameData!");
            return;
        }

        if (selectedPlayer.element == ElementType.Fire)
        {
            buttons[0].gameObject.SetActive(true);
        }

        else if (selectedPlayer.element == ElementType.Water)
        {
            buttons[1].gameObject.SetActive(true);
        }

        else if (selectedPlayer.element == ElementType.Wind)
        {
            buttons[2].gameObject.SetActive(true);
        }

        else if (selectedPlayer.element == ElementType.Earth)
        {
            buttons[3].gameObject.SetActive(true);
        }

        else if (selectedPlayer.element == ElementType.Light)
        {
            buttons[4].gameObject.SetActive(true);
        }
        
         else if (selectedPlayer.element == ElementType.Dark)
        {
            buttons[5].gameObject.SetActive(true);
        }
        
        
        // อนาตจะทำ panel choose skill ในนี้ โดย if(selectedPlayer.element == ElementType.Fire && level 10 || level 20 ... level 50) โดยจะทำการสุ่มสกิลที่ยังไม่ปลดล็อคท้ังหมด ละให้เลือกอันเดียว
    }
}
