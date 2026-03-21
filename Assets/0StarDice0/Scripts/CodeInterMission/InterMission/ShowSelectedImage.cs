using UnityEngine;

public class ShowSelectedImage : MonoBehaviour
{
    public GameObject fireImage;
    public GameObject waterImage;
    public GameObject earthImage;
    public GameObject windImage;
    public GameObject lightImage;
    public GameObject darkImage;

    void Start()
    {
        HideAllImages();

        if (GameData.Instance == null || GameData.Instance.selectedPlayer == null)
        {
            Debug.LogError("ไม่มีข้อมูลตัวละครที่เลือก");
            return;
        }

        // ดึง PlayerData ที่เลือก
        PlayerData selectedPlayer = GameData.Instance.selectedPlayer;

        // แสดงภาพตามธาตุ (หนึ่งธาตุ = หนึ่งภาพ)
        switch (selectedPlayer.elementType)
        {
            case ElementType.Fire:
                fireImage.SetActive(true);
                break;
            case ElementType.Water:
                waterImage.SetActive(true);
                break;
            case ElementType.Earth:
                earthImage.SetActive(true);
                break;
            case ElementType.Wind:
                windImage.SetActive(true);
                break;
            case ElementType.Light:
                lightImage.SetActive(true);
                break;
            case ElementType.Dark:
                darkImage.SetActive(true);
                break;
        }
    }

    void HideAllImages()
    {
        fireImage.SetActive(false);
        waterImage.SetActive(false);
        earthImage.SetActive(false);
        windImage.SetActive(false);
        lightImage.SetActive(false);
        darkImage.SetActive(false);
    }
}
