using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFlowController : MonoBehaviour
{
    [Header("RuntimeHub")]
    [SerializeField] private string persistentSceneName = "RuntimeHub";

    [Header("Transition")]
    [SerializeField] private bool useAdditiveTransition = true;
    [SerializeField] private bool blockInputDuringTransition = true;

    [Header("Scene aliases")]
    [SerializeField] private string shopAliasName = "Shop";
    [SerializeField] private string shopIntermissionSceneName = "ShopIntermission";

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
    }

    private void OnDestroy()
    {
        if (cached == this)
        {
            cached = null;
        }
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

        if (!controller.TryResolveSceneName(sceneName, out string resolvedSceneName))
        {
            Debug.LogError($"[SceneFlow] Cannot resolve scene '{sceneName}'. Check Build Profiles/scene name.");
            return false;
        }

        controller.RequestScene(resolvedSceneName);
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


        if (!TryResolveSceneName(sceneName, out string resolvedSceneName))
        {
            Debug.LogError($"[SceneFlow] Cannot resolve scene '{sceneName}'. Transition skipped.");
            return;
        }

        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.IsValid() && string.Equals(activeScene.name, resolvedSceneName, System.StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        StartCoroutine(TransitionToScene(resolvedSceneName));
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

    private bool TryResolveSceneName(string requestedSceneName, out string resolvedSceneName)
    {
        resolvedSceneName = string.Empty;
        if (string.IsNullOrWhiteSpace(requestedSceneName))
        {
            return false;
        }

        string requested = requestedSceneName.Trim();
        if (Application.CanStreamedLevelBeLoaded(requested))
        {
            resolvedSceneName = requested;
            return true;
        }

        if (!string.IsNullOrEmpty(shopAliasName)
            && string.Equals(requested, shopAliasName, System.StringComparison.OrdinalIgnoreCase)
            && Application.CanStreamedLevelBeLoaded(shopIntermissionSceneName))
        {
            resolvedSceneName = shopIntermissionSceneName;
            return true;
        }

        int sceneCount = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < sceneCount; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string candidateName = System.IO.Path.GetFileNameWithoutExtension(path);
            if (string.Equals(candidateName, requested, System.StringComparison.OrdinalIgnoreCase))
            {
                resolvedSceneName = candidateName;
                return true;
            }
        }

        return false;
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
