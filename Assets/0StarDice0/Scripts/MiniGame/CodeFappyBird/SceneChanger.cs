using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeManager : MonoBehaviour
{
    //public string sceneToLoad; // กำหนดชื่อ Scene ที่จะย้ายไป

    public void GoToScene(string name)
{
    Time.timeScale = 1f;

    if (string.IsNullOrWhiteSpace(name) || name == "BOARD")
    {
        MiniGameRewardService.ReturnToBoardScene();
        return;
    }

    SceneManager.LoadScene(name);
}
}
