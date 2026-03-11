using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFlowController : MonoBehaviour
{
    [Header("Bootstrap")]
    [SerializeField] private string persistentSceneName = "Bootstrap";
    [SerializeField] private bool autoLoadFirstGameplayScene = true;
    [SerializeField] private string firstGameplaySceneName = "Menu";

    [Header("Transition")]
    [SerializeField] private bool useAdditiveTransition = true;
    [SerializeField] private bool blockInputDuringTransition = true;

    private static SceneFlowController cached;
    private bool isTransitioning;

    public static bool IsTransitioning => cached != null && cached.isTransitioning;

    private void Awake()
    {
        if (cached != null && cached != this)
        {
            Destroy(gameObject);
            return;
        }

        cached = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (cached == this)
        {
            cached = null;
        }
    }

    private void Start()
    {
        if (!autoLoadFirstGameplayScene)
        {
            return;
        }

        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid())
        {
            return;
        }

        if (!string.Equals(activeScene.name, persistentSceneName, System.StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (string.IsNullOrEmpty(firstGameplaySceneName))
        {
            Debug.LogWarning("[SceneFlow] firstGameplaySceneName is empty. Auto-load skipped.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(firstGameplaySceneName))
        {
            Debug.LogWarning($"[SceneFlow] Cannot load first gameplay scene '{firstGameplaySceneName}'. Check Build Settings scene name.");
            return;
        }

        RequestScene(firstGameplaySceneName);
    }


    public static bool TryRequestScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            return false;
        }

        if (!TryGet(out var controller))
        {
            return false;
        }

        controller.RequestScene(sceneName);
        return true;
    }

    public static bool TryRequestScene(int sceneIndex)
    {
        if (!TryGet(out var controller))
        {
            return false;
        }

        controller.RequestScene(sceneIndex);
        return true;
    }

    public static bool TryGet(out SceneFlowController controller)
    {
        controller = cached;
        if (controller != null)
        {
            return true;
        }

        controller = FindFirstObjectByType<SceneFlowController>(FindObjectsInactive.Include);
        cached = controller;
        return controller != null;
    }

    public void RequestScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName) || isTransitioning)
        {
            return;
        }

        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.IsValid() && string.Equals(activeScene.name, sceneName, System.StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        StartCoroutine(TransitionToScene(sceneName));
    }

    public void RequestScene(int sceneIndex)
    {
        if (isTransitioning || sceneIndex < 0 || sceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            return;
        }

        string sceneName = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(sceneIndex));
        RequestScene(sceneName);
    }

    private IEnumerator TransitionToScene(string nextSceneName)
    {
        isTransitioning = true;
        float startedAt = Time.unscaledTime;

        if (blockInputDuringTransition)
        {
            Cursor.lockState = CursorLockMode.None;
        }

        Scene currentActive = SceneManager.GetActiveScene();

        if (useAdditiveTransition)
        {
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);
            while (!loadOp.isDone)
            {
                yield return null;
            }

            Scene nextScene = SceneManager.GetSceneByName(nextSceneName);
            if (nextScene.IsValid())
            {
                SceneManager.SetActiveScene(nextScene);
            }

            if (currentActive.IsValid() &&
                currentActive.isLoaded &&
                !string.Equals(currentActive.name, nextSceneName, System.StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(currentActive.name, persistentSceneName, System.StringComparison.OrdinalIgnoreCase))
            {
                AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(currentActive);
                while (unloadOp != null && !unloadOp.isDone)
                {
                    yield return null;
                }
            }
        }
        else
        {
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Single);
            while (!loadOp.isDone)
            {
                yield return null;
            }
        }

        Debug.Log($"[SceneFlow] {currentActive.name} -> {nextSceneName} done in {(Time.unscaledTime - startedAt):0.00}s");
        isTransitioning = false;
    }
}
