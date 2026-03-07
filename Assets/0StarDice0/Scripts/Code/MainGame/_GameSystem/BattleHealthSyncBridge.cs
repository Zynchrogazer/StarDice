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

        int syncedHealth = Mathf.Clamp(currentPlayer.PlayerHealth, 0, Mathf.Max(1, currentPlayer.MaxHealth));

        foreach (var behaviour in Object.FindObjectsOfType<MonoBehaviour>(true))
        {
            if (behaviour == null || behaviour.gameObject.scene != scene) continue;

            FieldInfo hpField = behaviour.GetType().GetField(PlayerHpFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (hpField == null || hpField.FieldType != typeof(int)) continue;

            hpField.SetValue(behaviour, syncedHealth);
            TrySyncSelectedPlayerStats(behaviour, currentPlayer, syncedHealth);
            TryUpdateHpBar(behaviour, currentPlayer, syncedHealth);
            TryInvokeHpUiRefresh(behaviour);
        }

        Debug.Log($"[BattleHealthSyncBridge] Synced board HP ({syncedHealth}) into battle scene '{scene.name}'.");
    }

    private static void OnSceneUnloaded(Scene scene)
    {
        if (!IsBattleScene(scene)) return;

        PlayerState currentPlayer = ResolveBoardPlayerState();
        if (currentPlayer == null) return;

        int? observedBattleHp = TryReadBattleHp(scene);
        if (!observedBattleHp.HasValue) return;

        int clamped = Mathf.Clamp(observedBattleHp.Value, 0, Mathf.Max(1, currentPlayer.MaxHealth));
        currentPlayer.PlayerHealth = clamped;

        if (GameData.Instance?.selectedPlayer != null)
        {
            GameData.Instance.selectedPlayer.SetHealth(clamped);
        }

        Debug.Log($"[BattleHealthSyncBridge] Synced battle HP ({clamped}) back to board state from scene '{scene.name}'.");
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


    private static void TrySyncSelectedPlayerStats(MonoBehaviour behaviour, PlayerState currentPlayer, int syncedHealth)
    {
        FieldInfo selectedPlayerField = behaviour.GetType().GetField(SelectedPlayerFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (selectedPlayerField == null || !typeof(PlayerData).IsAssignableFrom(selectedPlayerField.FieldType)) return;

        PlayerData selectedPlayerData = selectedPlayerField.GetValue(behaviour) as PlayerData;
        if (selectedPlayerData == null)
        {
            selectedPlayerData = GameData.Instance?.selectedPlayer;
            if (selectedPlayerData == null) return;
            selectedPlayerField.SetValue(behaviour, selectedPlayerData);
        }

        int syncedMaxHealth = Mathf.Max(1, currentPlayer.MaxHealth);
        selectedPlayerData.maxHealth = syncedMaxHealth;
        selectedPlayerData.maxHP = syncedMaxHealth;
        selectedPlayerData.attackDamage = Mathf.Max(0, currentPlayer.CurrentAttack);
        selectedPlayerData.def = Mathf.Max(0, selectedPlayerData.def);
        selectedPlayerData.speed = Mathf.Max(0, selectedPlayerData.speed);
        selectedPlayerData.level = Mathf.Max(1, currentPlayer.PlayerLevel);
        selectedPlayerData.currentExp = Mathf.Max(0, currentPlayer.CurrentExp);
        selectedPlayerData.maxExp = Mathf.Max(1, currentPlayer.MaxExp);
        selectedPlayerData.SetHealth(Mathf.Clamp(syncedHealth, 0, syncedMaxHealth));
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
        string sceneName = scene.name.ToLowerInvariant();
        return sceneName.Contains("fight") || sceneName.Contains("boss");
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
