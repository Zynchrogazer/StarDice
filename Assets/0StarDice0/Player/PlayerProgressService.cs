using UnityEngine;

public static class PlayerProgressService
{
    public static PlayerProgress LoadForPlayer(PlayerData playerData)
    {
        if (playerData == null) return null;
        return PlayerProgress.Create(playerData);
    }

    public static string ResolvePlayerId(PlayerData playerData)
    {
        return PlayerProgress.ResolvePlayerId(playerData);
    }

    public static void ResetStoredProgress(PlayerData playerData)
    {
        PlayerProgress.ResetStoredProgress(playerData);
    }

    public static PlayerProgress EnsureSelectedPlayerProgress(GameData gameData)
    {
        if (gameData == null || gameData.SelectedPlayer == null)
        {
            return null;
        }

        PlayerProgress current = gameData.SelectedPlayerProgress;
        if (current != null && current.PlayerId == ResolvePlayerId(gameData.SelectedPlayer))
        {
            return current;
        }

        PlayerProgress loaded = LoadForPlayer(gameData.SelectedPlayer);
        gameData.SetSelectedPlayerProgressInternal(loaded);
        return loaded;
    }

    public static int GetSelectedPlayerCredit(GameData gameData, int fallback = 0)
    {
        PlayerProgress progress = EnsureSelectedPlayerProgress(gameData);
        return progress != null ? Mathf.Max(0, progress.Credit) : Mathf.Max(0, fallback);
    }

    public static void SetSelectedPlayerCredit(GameData gameData, int amount)
    {
        PlayerProgress progress = EnsureSelectedPlayerProgress(gameData);
        if (progress == null)
        {
            return;
        }

        progress.SetCredit(amount);
        NotifySelectedPlayerCreditChanged(gameData, progress.Credit);
    }

    public static void AddSelectedPlayerCredit(GameData gameData, int amount)
    {
        if (amount <= 0) return;

        PlayerProgress progress = EnsureSelectedPlayerProgress(gameData);
        if (progress == null)
        {
            return;
        }

        progress.AddCredit(amount);
        NotifySelectedPlayerCreditChanged(gameData, progress.Credit);
    }

    public static bool TrySpendSelectedPlayerCredit(GameData gameData, int amount)
    {
        if (amount <= 0) return true;

        PlayerProgress progress = EnsureSelectedPlayerProgress(gameData);
        if (progress == null)
        {
            return false;
        }

        bool spent = progress.TrySpendCredit(amount);
        if (spent)
        {
            NotifySelectedPlayerCreditChanged(gameData, progress.Credit);
        }

        return spent;
    }

    public static void SetSelectedPlayerLevelProgress(GameData gameData, int level, int currentExp, int maxExp)
    {
        PlayerProgress progress = EnsureSelectedPlayerProgress(gameData);
        if (progress == null)
        {
            return;
        }

        progress.SetLevelProgress(level, currentExp, maxExp);
    }

    public static void ResetProgressToDefaults(PlayerData playerData, int? creditOverride = null)
    {
        if (playerData == null) return;

        ResetStoredProgress(playerData);
        PlayerProgress progress = LoadForPlayer(playerData);
        if (progress == null) return;

        int credit = creditOverride.HasValue ? Mathf.Max(0, creditOverride.Value) : playerData.startingCredit;
        progress.SetCredit(credit);
        progress.SetLevelProgress(playerData.startingLevel, playerData.startingCurrentExp, playerData.startingMaxExp);
    }

    private static void NotifySelectedPlayerCreditChanged(GameData gameData, int newCredit)
    {
        if (gameData?.SelectedPlayer == null) return;
        gameData.SelectedPlayer.NotifyCreditChangedFromProgress(newCredit);
    }
}
