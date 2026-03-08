using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeSceneButton : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;
    [Header("State Reset")]
    [SerializeField] private bool resetAllStateBeforeLoad = false;
    [SerializeField] private string autoResetSceneName = "InterMission";

    public void GoToScene()
    {
        if (resetAllStateBeforeLoad || ShouldAutoResetForTargetScene())
        {
            ResetAllRuntimeState();
        }

        SceneManager.LoadScene(sceneToLoad);
    }

    private bool ShouldAutoResetForTargetScene()
    {
        return !string.IsNullOrEmpty(autoResetSceneName)
               && string.Equals(sceneToLoad, autoResetSceneName);
    }

    private void ResetAllRuntimeState()
    {
        if (NormaSystem.Instance != null)
        {
            NormaSystem.Instance.ResetForNewBoardSession();
        }

        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.ResetForNewBoardSession();
        }

        if (GameTurnManager.Instance != null)
        {
            GameTurnManager.Instance.ResetForNewBoardSession();
        }

        PlayerState[] players = FindObjectsOfType<PlayerState>(true);
        foreach (PlayerState player in players)
        {
            player?.ResetForNewBoardSession();
        }

        PlayerStartSpawner.LastKnownPositions.Clear();
    }
}
