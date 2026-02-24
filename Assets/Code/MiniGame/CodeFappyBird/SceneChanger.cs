using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public string sceneToLoad; // กำหนดชื่อ Scene ที่จะย้ายไป

    public void ChangeScene()
    {
        SceneManager.LoadScene(0);
    }
}
