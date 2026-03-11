using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectMenu : MonoBehaviour
{
    [Header("Character Buttons")]
    public Button waterButton;
    public Button earthButton;
    public Button windButton;
    public Button lightButton;
    public Button darkButton;
    public Button fireButton;

    [Header("PlayerData References")]
    public PlayerData waterPlayerData;
    public PlayerData earthPlayerData;
    public PlayerData windPlayerData;
    public PlayerData lightPlayerData;
    public PlayerData darkPlayerData;
    public PlayerData firePlayerData;

    private const string SELECTED_MONSTER_KEY = "SelectedMonster";
    private const string HAS_CHOSEN_KEY = "HasChosenMainCharacter";
    void Start()
    {
        UpdateAllButtons();
        SyncRuntimeSelectionFromPrefs();
    }

    private void OnEnable()
    {
        SyncRuntimeSelectionFromPrefs();
    }

    private void SyncRuntimeSelectionFromPrefs()
    {
        string selectedFromPrefs = NormalizeElementId(PlayerPrefs.GetString(SELECTED_MONSTER_KEY, string.Empty));
        if (string.IsNullOrEmpty(selectedFromPrefs))
        {
            return;
        }

        if (RunSessionStore.TryGet(out var sessionStore))
        {
            sessionStore.SetSelectedMonster(selectedFromPrefs);
        }
        else
        {
            Debug.LogWarning("[CharacterSelectMenu] RunSessionStore not found while syncing selected monster.");
        }

        PlayerData resolved = ResolvePlayerDataByElement(selectedFromPrefs);
        if (resolved != null && GameData.Instance != null)
        {
            GameData.Instance.SetSelectedPlayer(resolved);
        }
    }

    public void UpdateAllButtons()
    {
        string currentSelected = NormalizeElementId(PlayerPrefs.GetString(SELECTED_MONSTER_KEY, ""));

        // เช็คว่าเคยเลือกตัวเริ่มต้นไปหรือยัง? (0 = ยังไม่เคย, 1 = เคยแล้ว)
        bool isNewPlayer = PlayerPrefs.GetInt(HAS_CHOSEN_KEY, 0) == 0;

        // ถ้าเป็นผู้เล่นใหม่ ให้ส่ง true ไปเพื่อบังคับเปิดทุกปุ่ม
        UpdateButtonState(waterButton, "MonsterWater", currentSelected, isNewPlayer);
        UpdateButtonState(earthButton, "MonsterEarth", currentSelected, isNewPlayer);
        UpdateButtonState(windButton, "MonsterWind", currentSelected, isNewPlayer);
        UpdateButtonState(lightButton, "MonsterLight", currentSelected, isNewPlayer);
        UpdateButtonState(darkButton, "MonsterDark", currentSelected, isNewPlayer);
        UpdateButtonState(fireButton, "MonsterFire", currentSelected, isNewPlayer);
    }

    private void UpdateButtonState(Button btn, string monsterKey, string currentSelected, bool isNewPlayer)
    {
        // 1. เช็คว่ามีตัวนี้ไหม
        bool isOwned = PlayerPrefs.GetInt(monsterKey, 0) == 1;

        // เงื่อนไข: ต้อง "มีตัวนี้" หรือ "เป็นผู้เล่นใหม่(เลือกฟรี)"
        if (isOwned || isNewPlayer)
        {
            // เปิดให้กดได้ตลอด (ไม่ล็อคแล้ว)
            btn.interactable = true;

            // --- เปลี่ยนสีแทนการล็อค ---
            // ถ้าไม่ใช่ผู้เล่นใหม่ และตัวนี้ถูกเลือกอยู่ ให้เปลี่ยนสี (เช่น สีเขียว)
            if (!isNewPlayer && currentSelected == GetElementFromKey(monsterKey))
            {
                // เปลี่ยนสีปุ่มเป็นสีเขียว (บอกว่าใส่อยู่)
                btn.image.color = Color.green;
            }
            else
            {
                // เปลี่ยนสีปุ่มเป็นสีขาวปกติ (ไม่ได้เลือก)
                btn.image.color = Color.white;
            }
        }
        else
        {
            // ถ้าไม่มี และไม่ใช่ผู้เล่นใหม่ -> ล็อคเหมือนเดิม (กดไม่ได้เพราะไม่มีของ)
            btn.interactable = false;
            btn.image.color = Color.gray; // ทำเป็นสีเทาๆ
        }
    }

    private string GetElementFromKey(string key)
    {
        return key.Replace("Monster", "");
    }

    // ใส่ใน OnClick ของปุ่ม (ส่งค่า Water, Fire, etc.)
    public void SelectCharacter(string element)
    {
        element = NormalizeElementId(element);
        Debug.Log("เลือก: " + element);

        // --- ส่วนแจกฟรี ---
        // ถ้าเป็นผู้เล่นใหม่ กดปุ๊บ ได้ตัวนั้นเป็นของตัวเองทันที
        if (PlayerPrefs.GetInt(HAS_CHOSEN_KEY, 0) == 0)
        {
            PlayerPrefs.SetInt("Monster" + element, 1); // ปลดล็อคตัวนี้
            PlayerPrefs.SetInt(HAS_CHOSEN_KEY, 1);      // จบสถานะผู้เล่นใหม่
            PlayerPrefs.Save();
            Debug.Log("🎉 ได้รับ " + element + " เป็นตัวเริ่มต้น!");
        }

        // --- ส่วนบันทึกการเลือก (single point of truth update path) ---
        ApplySelectedMonsterState(element);

        // รีเฟรชปุ่ม
        UpdateAllButtons();
    }

    private void ApplySelectedMonsterState(string element)
    {
        // 1) persist สำหรับ continue/restore
        string normalizedElement = NormalizeElementId(element);
        PlayerPrefs.SetString(SELECTED_MONSTER_KEY, normalizedElement);
        PlayerPrefs.Save();

        // 2) runtime additive session
        if (RunSessionStore.TryGet(out var sessionStore))
        {
            sessionStore.SetSelectedMonster(normalizedElement);
        }
        else
        {
            Debug.LogWarning("[CharacterSelectMenu] RunSessionStore not found while applying selected monster.");
        }

        // 3) primary selection pointer used by current gameplay systems
        PlayerData resolved = ResolvePlayerDataByElement(normalizedElement);
        if (resolved != null && GameData.Instance != null)
        {
            GameData.Instance.SetSelectedPlayer(resolved);
        }
    }

    private static string NormalizeElementId(string rawElement)
    {
        if (string.IsNullOrWhiteSpace(rawElement))
        {
            return string.Empty;
        }

        string normalized = rawElement.Trim();
        if (normalized.StartsWith("Monster", System.StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized.Substring("Monster".Length);
        }

        return normalized;
    }

    private PlayerData ResolvePlayerDataByElement(string element)
    {
        if (string.IsNullOrEmpty(element)) return null;

        switch (element.Trim().ToLowerInvariant())
        {
            case "water": return waterPlayerData;
            case "earth": return earthPlayerData;
            case "wind": return windPlayerData;
            case "light": return lightPlayerData;
            case "dark": return darkPlayerData;
            case "fire": return firePlayerData;
            default: return null;
        }
    }
}
