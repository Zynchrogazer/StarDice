using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeSceneButton : MonoBehaviour
{
    public string sceneToLoad;

    public void GoToScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
