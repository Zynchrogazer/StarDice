using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadSceneByName(string sceneName)
    {
        if (!SceneFlowController.TryRequestScene(sceneName))
        {
            SceneManager.LoadScene(sceneName);
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
