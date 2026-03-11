using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeSceneButton : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;

    [Header("Core system references (preferred)")]
    [SerializeField] private NormaSystem normaSystem;
    [SerializeField] private GameEventManager gameEventManager;
    [SerializeField] private GameTurnManager gameTurnManager;

    [Header("State Reset")]
    [SerializeField] private bool resetAllStateBeforeLoad = false;
    [SerializeField] private string autoResetSceneName = "InterMission";
    [SerializeField] private string[] autoResetSceneNames = { "ShopIntermission" };
    [SerializeField] private bool autoDestroyBoardCoreSystemsOnIntermission = true;
    [SerializeField] private bool hardResetRuntimeOnIntermissionExit = true;

    public void GoToScene()
    {
        Time.timeScale = 1f;

        bool shouldResetForTargetScene = resetAllStateBeforeLoad || ShouldAutoResetForTargetScene();

        if (shouldResetForTargetScene && hardResetRuntimeOnIntermissionExit)
        {
            HardResetRuntimeStateKeepPersistentCredit();
        }
        else
        {
            if (ShouldDestroyBoardCoreSystemsForTargetScene())
            {
                DestroyBoardCoreSystems();
            }

            if (shouldResetForTargetScene)
            {
                ResetAllRuntimeState();
            }
        }

        if (!SceneFlowController.TryRequestScene(sceneToLoad))
        {
            if (Application.CanStreamedLevelBeLoaded(sceneToLoad))
            {
                SceneManager.LoadScene(sceneToLoad);
            }
            else
            {
                Debug.LogError($"[ChangeSceneButton] Cannot load scene '{sceneToLoad}'. Check Build Profiles.");
            }
        }
    }

    private bool ShouldAutoResetForTargetScene()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(autoResetSceneName)
            && string.Equals(sceneToLoad, autoResetSceneName, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (autoResetSceneNames != null)
        {
            foreach (string sceneName in autoResetSceneNames)
            {
                if (!string.IsNullOrEmpty(sceneName)
                    && string.Equals(sceneToLoad, sceneName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return sceneToLoad.IndexOf("intermission", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private bool ShouldDestroyBoardCoreSystemsForTargetScene()
    {
        if (!autoDestroyBoardCoreSystemsOnIntermission)
        {
            return false;
        }

        return ShouldAutoResetForTargetScene();
    }

    private void HardResetRuntimeStateKeepPersistentCredit()
    {
        PersistHumanPlayerCredit();

        PlayerPrefs.SetInt(GameTurnManager.PendingBattleReturnKey, 0);
        PlayerPrefs.DeleteKey(GameEventManager.LastBoardSceneKey);
        PlayerPrefs.Save();

        PlayerStartSpawner.LastKnownPositions.Clear();

        DestroyRuntimeSystem<PlayerState>();
        DestroyRuntimeSystem<GameTurnManager>();
        DestroyRuntimeSystem<GameEventManager>();
        DestroyRuntimeSystem<NormaSystem>();
        DestroyRuntimeSystem<BoardGameGroup>();
        DestroyRuntimeSystem<GameSystem>();
    }

    private void PersistHumanPlayerCredit()
    {
        PlayerState[] players = FindObjectsByType<PlayerState>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        for (int i = 0; i < players.Length; i++)
        {
            PlayerState player = players[i];
            if (player == null || player.isAI)
            {
                continue;
            }

            if (GameData.Instance != null && GameData.Instance.selectedPlayer != null)
            {
                GameData.Instance.selectedPlayer.SetCredit(player.PlayerCredit);
            }

            break;
        }
    }

    private void DestroyBoardCoreSystems()
    {
        DestroyRuntimeSystem<GameTurnManager>();
        DestroyRuntimeSystem<GameSystem>();
    }

    private static void DestroyRuntimeSystem<T>() where T : MonoBehaviour
    {
        T[] systems = FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < systems.Length; i++)
        {
            if (systems[i] != null)
            {
                Destroy(systems[i].gameObject);
            }
        }
    }

    private void ResetAllRuntimeState()
    {
        if (ResolveNormaSystem(out var resolvedNormaSystem))
        {
            resolvedNormaSystem.ResetForNewBoardSession();
        }

        if (ResolveGameEventManager(out var resolvedGameEventManager))
        {
            resolvedGameEventManager.ResetForNewBoardSession();
        }

        if (ResolveGameTurnManager(out var resolvedGameTurnManager))
        {
            resolvedGameTurnManager.ResetForNewBoardSession();
        }

        PlayerState[] players = FindObjectsOfType<PlayerState>(true);
        foreach (PlayerState player in players)
        {
            player?.ResetForNewBoardSession();
        }

        PlayerStartSpawner.LastKnownPositions.Clear();
    }

    private bool ResolveNormaSystem(out NormaSystem resolved)
    {
        if (normaSystem != null)
        {
            resolved = normaSystem;
            return true;
        }

        if (NormaSystem.TryGet(out resolved))
        {
            normaSystem = resolved;
            return true;
        }

        return false;
    }

    private bool ResolveGameEventManager(out GameEventManager resolved)
    {
        if (gameEventManager != null)
        {
            resolved = gameEventManager;
            return true;
        }

        if (GameEventManager.TryGet(out resolved))
        {
            gameEventManager = resolved;
            return true;
        }

        return false;
    }

    private bool ResolveGameTurnManager(out GameTurnManager resolved)
    {
        if (gameTurnManager != null)
        {
            resolved = gameTurnManager;
            return true;
        }

        if (GameTurnManager.TryGet(out resolved))
        {
            gameTurnManager = resolved;
            return true;
        }

        return false;
    }
}
