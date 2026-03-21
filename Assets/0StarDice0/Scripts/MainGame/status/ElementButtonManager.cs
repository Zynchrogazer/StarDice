using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;

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
        ApplyRuntimeStatsToStatusPanels();
        hasInitialized = true;
    }

    private void ApplyRuntimeStatsToStatusPanels()
    {
        PlayerState playerState = ResolveHumanPlayerState();
        if (playerState == null) return;

        if (buttons == null) return;

        foreach (Button btn in buttons)
        {
            if (btn == null) continue;

            TMP_Text[] texts = btn.GetComponentsInChildren<TMP_Text>(true);
            foreach (TMP_Text txt in texts)
            {
                if (txt == null) continue;

                string key = txt.name.ToLowerInvariant();
                if (key.Contains("hp"))
                {
                    txt.text = $"HP : {playerState.PlayerHealth}";
                }
                else if (key.Contains("atk"))
                {
                    txt.text = $"ATK : {playerState.CurrentAttack}";
                }
                else if (key.Contains("spd") || key.Contains("speed"))
                {
                    txt.text = $"SPD : {playerState.CurrentSpeed}";
                }
                else if (key.Contains("def"))
                {
                    txt.text = $"DEF : {playerState.CurrentDefense}";
                }
            }
        }
    }

    private static PlayerState ResolveHumanPlayerState()
    {
        if (GameTurnManager.TryGet(out var gameTurnManager) && gameTurnManager.allPlayers != null)
        {
            foreach (PlayerState p in gameTurnManager.allPlayers)
            {
                if (p != null && !p.isAI)
                    return p;
            }
        }

        PlayerState[] players = FindObjectsByType<PlayerState>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (PlayerState p in players)
        {
            if (p != null && !p.isAI)
                return p;
        }

        return null;
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
            PlayerData loaded = TryLoadSelectedPlayerData(selectedName);
            if (loaded != null)
            {
                if (GameData.Instance != null)
                    GameData.Instance.selectedPlayer = loaded;

                return loaded;
            }
        }

        return null;
    }


    private static PlayerData TryLoadSelectedPlayerData(string selectedName)
    {
        string trimmed = selectedName.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        PlayerData loaded = Resources.Load<PlayerData>($"PlayerData/{trimmed}");
        if (loaded != null)
        {
            return loaded;
        }

        if (trimmed.StartsWith("Monster", StringComparison.OrdinalIgnoreCase))
        {
            string withoutPrefix = trimmed.Substring("Monster".Length);
            if (!string.IsNullOrEmpty(withoutPrefix))
            {
                return Resources.Load<PlayerData>($"PlayerData/{withoutPrefix}");
            }
        }

        return Resources.Load<PlayerData>($"PlayerData/Monster{trimmed}");
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

    public Transform GetActiveStatusRoot()
    {
        if (buttons == null) return null;

        foreach (Button btn in buttons)
        {
            if (btn == null) continue;
            if (!btn.gameObject.activeInHierarchy) continue;
            return btn.transform;
        }

        return null;
    }

    // อนาตจะทำ panel choose skill ในนี้ โดย if(selectedPlayer.element == ElementType.Fire && level 10 || level 20 ... level 50) โดยจะทำการสุ่มสกิลที่ยังไม่ปลดล็อคท้ังหมด ละให้เลือกอันเดียว
}
