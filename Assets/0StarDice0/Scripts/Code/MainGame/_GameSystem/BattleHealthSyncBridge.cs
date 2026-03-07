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

        PlayerState currentPlayer = GameTurnManager.CurrentPlayer;
        if (currentPlayer == null) return;

        int syncedHealth = Mathf.Clamp(currentPlayer.PlayerHealth, 0, Mathf.Max(1, currentPlayer.MaxHealth));

        foreach (var behaviour in Object.FindObjectsOfType<MonoBehaviour>(true))
        {
            if (behaviour == null || behaviour.gameObject.scene != scene) continue;

            FieldInfo hpField = behaviour.GetType().GetField(PlayerHpFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (hpField == null || hpField.FieldType != typeof(int)) continue;

            hpField.SetValue(behaviour, syncedHealth);
            TryUpdateHpBar(behaviour, currentPlayer, syncedHealth);
            TryInvokeHpUiRefresh(behaviour);
        }

        Debug.Log($"[BattleHealthSyncBridge] Synced board HP ({syncedHealth}) into battle scene '{scene.name}'.");
    }

    private static void OnSceneUnloaded(Scene scene)
    {
        if (!IsBattleScene(scene)) return;

        PlayerState currentPlayer = GameTurnManager.CurrentPlayer;
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

    private static void TryUpdateHpBar(MonoBehaviour behaviour, PlayerState currentPlayer, int syncedHealth)
    {
        FieldInfo hpBarField = behaviour.GetType().GetField(PlayerHpBarFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (hpBarField == null || !typeof(Slider).IsAssignableFrom(hpBarField.FieldType)) return;

        Slider hpBar = hpBarField.GetValue(behaviour) as Slider;
        if (hpBar == null) return;

        int maxHp = Mathf.Max(1, currentPlayer.MaxHealth);

        FieldInfo selectedPlayerField = behaviour.GetType().GetField(SelectedPlayerFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (selectedPlayerField != null && typeof(PlayerData).IsAssignableFrom(selectedPlayerField.FieldType))
        {
            if (selectedPlayerField.GetValue(behaviour) is PlayerData selectedPlayerData)
            {
                maxHp = Mathf.Max(1, selectedPlayerData.GetMaxHealth());
            }
        }

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
}
