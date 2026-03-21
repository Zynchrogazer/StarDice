using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadSceneByName(string sceneName)
    {
        if (!SceneFlowController.TryRequestScene(sceneName))
        {
            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.LogError($"[SceneLoader] Cannot load scene '{sceneName}'. Check Build Profiles.");
            }
        }
    }

    public void LoadSceneByIndex(int sceneIndex)
    {
        if (!SceneFlowController.TryRequestScene(sceneIndex))
        {
            SceneManager.LoadScene(sceneIndex);
        }
    }
}
