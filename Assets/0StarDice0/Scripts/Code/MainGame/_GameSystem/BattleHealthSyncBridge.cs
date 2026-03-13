using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Sync HP between board PlayerState and legacy battle controllers that still store HP in local fields.
/// </summary>
public static class BattleHealthSyncBridge
{
    private const string PlayerHpFieldName = "playerHP";
    private const string PlayerHpBarFieldName = "playerHPBar";
    private const string SelectedPlayerFieldName = "selectedPlayer";
    private static PlayerData runtimeBattlePlayerData;
    private static PlayerData previousSelectedPlayerData;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!IsBattleScene(scene)) return;

        PlayerState currentPlayer = ResolveBoardPlayerState();
        if (currentPlayer == null) return;

        CleanupRuntimeBattlePlayerData();
        runtimeBattlePlayerData = CreateRuntimeBattlePlayerData(currentPlayer);
        OverrideGlobalSelectedPlayerForBattle();
        int syncedHealth = Mathf.Clamp(currentPlayer.PlayerHealth, 0, Mathf.Max(1, currentPlayer.MaxHealth));

        foreach (var behaviour in Object.FindObjectsOfType<MonoBehaviour>(true))
        {
            if (behaviour == null || behaviour.gameObject.scene != scene) continue;

            // inject selectedPlayer runtime data ให้ทุกตัวที่รองรับก่อน
            // เพื่อให้สคริปต์ที่อ่านสถานะจาก selectedPlayer โดยตรงใช้ค่าจาก PlayerState
            TrySyncSelectedPlayerStats(behaviour);

            FieldInfo hpField = behaviour.GetType().GetField(PlayerHpFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (hpField == null || hpField.FieldType != typeof(int))
            {
                // บาง battle controller อาจไม่มี playerHP field แต่มี selectedPlayer อยู่แล้ว
                // จึงยังต้องปล่อยให้ทำงานต่อโดยไม่ถือว่าเป็น error
                continue;
            }

            hpField.SetValue(behaviour, syncedHealth);
            TryUpdateHpBar(behaviour, currentPlayer, syncedHealth);
            TryInvokeHpUiRefresh(behaviour);
        }

        Debug.Log($"[BattleHealthSyncBridge] Synced board HP ({syncedHealth}) into battle scene '{scene.name}'.");
    }

    private static void OnSceneUnloaded(Scene scene)
    {
        if (!IsBattleScene(scene)) return;

        try
        {
            PlayerState currentPlayer = ResolveBoardPlayerState();
            if (currentPlayer == null) return;

            int? observedBattleHp = TryReadBattleHp(scene);
            if (!observedBattleHp.HasValue)
            {
                Debug.LogWarning($"[BattleHealthSyncBridge] Could not read battle HP from scene '{scene.name}'. Keeping board HP as-is.");
                return;
            }

            int clamped = Mathf.Clamp(observedBattleHp.Value, 0, Mathf.Max(1, currentPlayer.MaxHealth));
            currentPlayer.PlayerHealth = clamped;

            Debug.Log($"[BattleHealthSyncBridge] Synced battle HP ({clamped}) back to board state from scene '{scene.name}'.");
        }
        finally
        {
            CleanupRuntimeBattlePlayerData();
        }
    }

    private static void CleanupRuntimeBattlePlayerData()
    {
        if (runtimeBattlePlayerData == null) return;

        PlayerData runtimeDataToCleanup = runtimeBattlePlayerData;
        RestoreGlobalSelectedPlayerAfterBattle(runtimeDataToCleanup);

        Object.Destroy(runtimeBattlePlayerData);
        runtimeBattlePlayerData = null;
    }

    private static void OverrideGlobalSelectedPlayerForBattle()
    {
        if (GameData.Instance == null || runtimeBattlePlayerData == null) return;

        if (previousSelectedPlayerData == null)
        {
            previousSelectedPlayerData = GameData.Instance.selectedPlayer;
        }

        GameData.Instance.selectedPlayer = runtimeBattlePlayerData;
    }

    private static void RestoreGlobalSelectedPlayerAfterBattle(PlayerData runtimeData)
    {
        if (GameData.Instance == null) return;
        if (runtimeData == null) return;
        if (GameData.Instance.selectedPlayer != runtimeData) return;

        GameData.Instance.selectedPlayer = previousSelectedPlayerData;
        previousSelectedPlayerData = null;
    }

    private static int? TryReadBattleHp(Scene scene)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour == null) continue;

                FieldInfo hpField = behaviour.GetType().GetField(PlayerHpFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (hpField == null || hpField.FieldType != typeof(int)) continue;

                return (int)hpField.GetValue(behaviour);
            }
        }

        return null;
    }


    private static void TrySyncSelectedPlayerStats(MonoBehaviour behaviour)
    {
        FieldInfo selectedPlayerField = behaviour.GetType().GetField(SelectedPlayerFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (selectedPlayerField == null || !typeof(PlayerData).IsAssignableFrom(selectedPlayerField.FieldType)) return;

        if (runtimeBattlePlayerData == null) return;
        selectedPlayerField.SetValue(behaviour, runtimeBattlePlayerData);
    }

    private static PlayerData CreateRuntimeBattlePlayerData(PlayerState currentPlayer)
    {
        PlayerData persistentPlayerData = GameData.Instance?.selectedPlayer;
        PlayerData source = persistentPlayerData != null ? persistentPlayerData : currentPlayer.selectedPlayerPreset;
        if (source == null) return null;

        PlayerData runtimeData = ScriptableObject.Instantiate(source);
        runtimeData.name = $"{source.name}_RuntimeBattle";

        int syncedMaxHealth = Mathf.Max(1, currentPlayer.MaxHealth);
        runtimeData.maxHealth = syncedMaxHealth;
        runtimeData.maxHP = syncedMaxHealth;
        runtimeData.attackDamage = Mathf.Max(0, currentPlayer.CurrentAttack);
        runtimeData.speed = Mathf.Max(0, currentPlayer.CurrentSpeed);
        runtimeData.def = Mathf.Max(0, currentPlayer.CurrentDefense);
        runtimeData.level = Mathf.Max(1, currentPlayer.PlayerLevel);
        runtimeData.currentExp = Mathf.Max(0, currentPlayer.CurrentExp);
        runtimeData.maxExp = Mathf.Max(1, currentPlayer.MaxExp);

        return runtimeData;
    }

    private static void TryUpdateHpBar(MonoBehaviour behaviour, PlayerState currentPlayer, int syncedHealth)
    {
        FieldInfo hpBarField = behaviour.GetType().GetField(PlayerHpBarFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (hpBarField == null || !typeof(Slider).IsAssignableFrom(hpBarField.FieldType)) return;

        Slider hpBar = hpBarField.GetValue(behaviour) as Slider;
        if (hpBar == null) return;

        int maxHp = Mathf.Max(1, currentPlayer.MaxHealth);
        hpBar.maxValue = maxHp;
        hpBar.value = Mathf.Clamp(syncedHealth, 0, maxHp);
    }

    private static void TryInvokeHpUiRefresh(MonoBehaviour behaviour)
    {
        MethodInfo updateMethod = behaviour.GetType().GetMethod("UpdatePlayerHPUI", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        updateMethod?.Invoke(behaviour, null);
    }

    private static bool IsBattleScene(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root == null) continue;

            if (string.Equals(root.name, "BattleSystem", System.StringComparison.OrdinalIgnoreCase))
                return true;

            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child == null) continue;
                if (string.Equals(child.name, "BattleSystem", System.StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }

        return false;
    }

    private static PlayerState ResolveBoardPlayerState()
    {
        PlayerState currentPlayer = GameTurnManager.CurrentPlayer;
        if (currentPlayer != null && !currentPlayer.isAI)
        {
            return currentPlayer;
        }

        PlayerState[] allPlayers = Object.FindObjectsOfType<PlayerState>(true);
        foreach (PlayerState player in allPlayers)
        {
            if (player == null || player.isAI) continue;

            // ผู้เล่นบนบอร์ดที่ DontDestroyOnLoad จะมี buildIndex == -1
            if (player.gameObject.scene.buildIndex == -1)
            {
                return player;
            }
        }

        return null;
    }
}
