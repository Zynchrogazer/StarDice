using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class ElementButtonManager : MonoBehaviour
{
    public Button[] buttons; // Array ของปุ่ม
    public PlayerData selectedPlayer;

    private bool hasInitialized;

    void Start()
    {
        UpdateButtons();
    }

    private void OnEnable()
    {
        // กันกรณี scene โหลดช้ากว่า GameData/PlayerData
        if (!hasInitialized)
        {
            UpdateButtons();
        }
    }

    void UpdateButtons()
    {
        EnsureButtonReferences();
        HideAllButtons();

        selectedPlayer = ResolveSelectedPlayer();

        if (selectedPlayer == null)
        {
            Debug.LogWarning("ยังไม่ได้เลือกตัวละครจาก GameData!");
            return;
        }

        int index = GetButtonIndexByElement(selectedPlayer.element);
        if (index < 0 || buttons == null || index >= buttons.Length || buttons[index] == null)
        {
            Debug.LogWarning($"หา status button ของธาตุ {selectedPlayer.element} ไม่เจอ");
            return;
        }

        buttons[index].gameObject.SetActive(true);
        hasInitialized = true;
    }

    private PlayerData ResolveSelectedPlayer()
    {
        if (selectedPlayer != null)
            return selectedPlayer;

        if (GameData.Instance != null && GameData.Instance.selectedPlayer != null)
            return GameData.Instance.selectedPlayer;

        // fallback: เข้า TestMain ตรงจากหน้าเลือกตัวแรก จะเก็บชื่อไว้ใน PlayerPrefs
        string selectedName = PlayerPrefs.GetString("SelectedMonster", string.Empty);
        if (!string.IsNullOrWhiteSpace(selectedName))
        {
            PlayerData loaded = Resources.Load<PlayerData>($"PlayerData/{selectedName}");
            if (loaded != null)
            {
                if (GameData.Instance != null)
                    GameData.Instance.selectedPlayer = loaded;

                return loaded;
            }
        }

        return null;
    }

    private int GetButtonIndexByElement(ElementType element)
    {
        switch (element)
        {
            case ElementType.Fire: return 0;
            case ElementType.Water: return 1;
            case ElementType.Wind: return 2;
            case ElementType.Earth: return 3;
            case ElementType.Light: return 4;
            case ElementType.Dark: return 5;
            default: return -1;
        }
    }

    private void HideAllButtons()
    {
        if (buttons == null) return;

        foreach (var btn in buttons)
        {
            if (btn != null)
                btn.gameObject.SetActive(false);
        }
    }

    private void EnsureButtonReferences()
    {
        if (buttons == null || buttons.Length < 6)
            buttons = new Button[6];

        bool needsFill = false;
        for (int i = 0; i < 6; i++)
        {
            if (buttons[i] == null)
            {
                needsFill = true;
                break;
            }
        }

        if (!needsFill) return;

        var allButtons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "fire", 0 },
            { "water", 1 },
            { "wind", 2 },
            { "earth", 3 },
            { "light", 4 },
            { "dark", 5 }
        };

        foreach (var btn in allButtons)
        {
            if (btn == null) continue;

            string n = btn.name.ToLowerInvariant();
            if (!n.Contains("openstatus")) continue;

            foreach (var kv in map)
            {
                if (n.Contains(kv.Key) && buttons[kv.Value] == null)
                {
                    buttons[kv.Value] = btn;
                    break;
                }
            }
        }
    }

    // อนาตจะทำ panel choose skill ในนี้ โดย if(selectedPlayer.element == ElementType.Fire && level 10 || level 20 ... level 50) โดยจะทำการสุ่มสกิลที่ยังไม่ปลดล็อคท้ังหมด ละให้เลือกอันเดียว
}
