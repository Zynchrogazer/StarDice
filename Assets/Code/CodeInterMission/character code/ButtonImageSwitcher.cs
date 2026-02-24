using UnityEngine;
using UnityEngine.UI;

public class ButtonImageSwitcher : MonoBehaviour
{
    public Button[] buttons;
    public Image targetImage;

    void Start()
    {
        // โหลด index ที่เคยเลือกไว้
        int savedIndex = PlayerPrefs.GetInt(gameObject.name + "_LastSelectedIndex", -1);

        if (savedIndex >= 0 && savedIndex < buttons.Length)
        {
            Image buttonImg = buttons[savedIndex].GetComponent<Image>();
            if (buttonImg != null && targetImage != null)
            {
                targetImage.sprite = buttonImg.sprite;
            }
        }

        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            buttons[i].onClick.AddListener(() => OnButtonClick(index));
        }
    }

    void OnButtonClick(int buttonIndex)
    {
        Image buttonImg = buttons[buttonIndex].GetComponent<Image>();
        if (buttonImg != null && targetImage != null)
        {
            targetImage.sprite = buttonImg.sprite;

            // บันทึก index ของปุ่มที่เลือก (key ผูกกับชื่อ GameObject เพื่อไม่ให้ชนกันถ้ามีหลายตัว)
            PlayerPrefs.SetInt(gameObject.name + "_LastSelectedIndex", buttonIndex);
            PlayerPrefs.Save();
        }
    }
}
