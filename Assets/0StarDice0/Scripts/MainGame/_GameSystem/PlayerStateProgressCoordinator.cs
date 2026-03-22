using UnityEngine;

public static class PlayerStateProgressCoordinator
{
    public static PlayerProgress ResolveSelectedProgress(PlayerState playerState)
    {
        if (playerState == null || GameData.Instance == null)
        {
            return null;
        }

        PlayerProgress progress = PlayerProgressService.EnsureSelectedPlayerProgress(GameData.Instance);
        if (progress != null && playerState.selectedPlayerPreset == null && GameData.Instance.SelectedPlayer != null)
        {
            playerState.BindSelectedPlayerPreset(GameData.Instance.SelectedPlayer);
        }

        return progress;
    }

    public static void CaptureSnapshot(PlayerState playerState, PlayerData sourceData, PlayerProgressSnapshot snapshot)
    {
        if (playerState == null || snapshot == null)
        {
            return;
        }

        PlayerProgress progress = ResolveSelectedProgress(playerState);
        if (progress != null)
        {
            snapshot.Set(progress.Level, progress.CurrentExp, progress.MaxExp);
            return;
        }

        int level = sourceData != null ? sourceData.startingLevel : 1;
        int currentExp = sourceData != null ? sourceData.startingCurrentExp : 0;
        int maxExp = sourceData != null ? sourceData.startingMaxExp : 100;
        snapshot.Set(level, currentExp, maxExp);
    }

    public static void RestoreSnapshot(PlayerProgressSnapshot snapshot)
    {
        if (snapshot == null || GameData.Instance == null)
        {
            return;
        }

        GameData.Instance.SetSelectedPlayerLevelProgress(snapshot.Level, snapshot.CurrentExp, snapshot.MaxExp);
    }

    public static void SyncPersistentCredit(PlayerState playerState)
    {
        if (playerState == null || GameData.Instance == null)
        {
            return;
        }

        GameData.Instance.SetSelectedPlayerCredit(playerState.PlayerCredit);
    }

    public static void ApplyPersistentProgressToRuntime(PlayerState playerState, PlayerData sourceData)
    {
        if (playerState == null)
        {
            return;
        }

        PlayerProgress progress = ResolveSelectedProgress(playerState);
        if (progress != null)
        {
            playerState.PlayerLevel = progress.Level;
            playerState.CurrentExp = progress.CurrentExp;
            playerState.MaxExp = Mathf.Max(1, progress.MaxExp);
            return;
        }

        if (sourceData != null)
        {
            playerState.PlayerLevel = sourceData.startingLevel;
            playerState.CurrentExp = sourceData.startingCurrentExp;
            playerState.MaxExp = sourceData.startingMaxExp > 0 ? sourceData.startingMaxExp : 100;
            return;
        }

        playerState.PlayerLevel = 1;
        playerState.CurrentExp = 0;
        playerState.MaxExp = Mathf.Max(playerState.MaxExp, 100);
    }
}
