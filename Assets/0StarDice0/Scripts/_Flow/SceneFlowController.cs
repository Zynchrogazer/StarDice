using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class SceneFlowController : MonoBehaviour
{
    [Header("RuntimeHub")]
    [SerializeField] private string persistentSceneName = "RuntimeHub";

    [Header("Transition")]
    [SerializeField] private bool useAdditiveTransition = true;
    [SerializeField] private bool blockInputDuringTransition = true;
    [Tooltip("When enabled, only RuntimeHub and the target scene survive a normal SceneFlow transition.")]
    [SerializeField] private bool unloadAllNonPersistentScenes = true;

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
        DontDestroyOnLoad(gameObject);
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
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            return false;
        }

        if (!TryGetOrCreate(out var controller))
        {
            return false;
        }

        return controller.TryStartSceneRequest(sceneName, controller.unloadAllNonPersistentScenes, false);
    }

    public static bool TryRequestScene(int sceneIndex)
    {
        if (sceneIndex < 0 || sceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            return false;
        }

        if (!TryGetOrCreate(out var controller))
        {
            return false;
        }

        string sceneName = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(sceneIndex));
        return controller.TryStartSceneRequest(sceneName, controller.unloadAllNonPersistentScenes, false);
    }

    public static bool TryRequestSceneKeepingPersistent(string sceneName, bool notifyBoardReadyIfAlreadyLoaded = false)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            return false;
        }

        if (!TryGetOrCreate(out var controller))
        {
            return false;
        }

        return controller.TryStartSceneRequest(sceneName, true, notifyBoardReadyIfAlreadyLoaded);
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
        TryStartSceneRequest(sceneName, unloadAllNonPersistentScenes, false);
    }

    public void RequestScene(int sceneIndex)
    {
        if (sceneIndex < 0 || sceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            return;
        }

        string sceneName = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(sceneIndex));
        TryStartSceneRequest(sceneName, unloadAllNonPersistentScenes, false);
    }

    private static bool TryGetOrCreate(out SceneFlowController controller)
    {
        if (TryGet(out controller))
        {
            return true;
        }

        GameObject runner = new GameObject("[SceneFlowController]");
        controller = runner.AddComponent<SceneFlowController>();
        cached = controller;
        return true;
    }

    private bool TryStartSceneRequest(string requestedSceneName, bool unloadAllScenesExceptPersistentAndTarget, bool notifyBoardReadyIfAlreadyLoaded)
    {
        if (string.IsNullOrWhiteSpace(requestedSceneName) || isTransitioning)
        {
            return false;
        }

        if (!TryResolveSceneName(requestedSceneName, out string resolvedSceneName))
        {
            Debug.LogError($"[SceneFlow] Cannot resolve scene '{requestedSceneName}'. Check Build Profiles/scene name.");
            return false;
        }

        StartCoroutine(TransitionToScene(resolvedSceneName, unloadAllScenesExceptPersistentAndTarget, notifyBoardReadyIfAlreadyLoaded));
        return true;
    }

    private bool TryResolveSceneName(string requestedSceneName, out string resolvedSceneName)
    {
        resolvedSceneName = string.Empty;
        if (string.IsNullOrWhiteSpace(requestedSceneName))
        {
            return false;
        }

        string requested = requestedSceneName.Trim();
        string requestedFileName = Path.GetFileNameWithoutExtension(requested);

        if (!string.IsNullOrEmpty(shopAliasName)
            && string.Equals(requested, shopAliasName, StringComparison.OrdinalIgnoreCase)
            && Application.CanStreamedLevelBeLoaded(shopIntermissionSceneName))
        {
            resolvedSceneName = shopIntermissionSceneName;
            return true;
        }

        if (Application.CanStreamedLevelBeLoaded(requested))
        {
            resolvedSceneName = requestedFileName;
            return true;
        }

        int sceneCount = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < sceneCount; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string candidateName = Path.GetFileNameWithoutExtension(path);
            if (string.Equals(candidateName, requested, StringComparison.OrdinalIgnoreCase)
                || string.Equals(candidateName, requestedFileName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(path, requested, StringComparison.OrdinalIgnoreCase))
            {
                resolvedSceneName = candidateName;
                return true;
            }
        }

        return false;
    }

    private IEnumerator TransitionToScene(string nextSceneName, bool unloadAllScenesExceptPersistentAndTarget, bool notifyBoardReadyIfAlreadyLoaded)
    {
        isTransitioning = true;
        float startedAt = Time.unscaledTime;
        Scene currentActive = SceneManager.GetActiveScene();
        bool targetWasAlreadyLoaded = false;

        try
        {
            if (blockInputDuringTransition)
            {
                Cursor.lockState = CursorLockMode.None;
            }

            if (!useAdditiveTransition)
            {
                AsyncOperation singleLoadOp = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Single);
                if (singleLoadOp == null)
                {
                    Debug.LogError($"[SceneFlow] Failed to start single load for scene '{nextSceneName}'.");
                    yield break;
                }

                while (!singleLoadOp.isDone)
                {
                    yield return null;
                }

                yield break;
            }

            Scene nextScene = SceneManager.GetSceneByName(nextSceneName);
            targetWasAlreadyLoaded = nextScene.IsValid() && nextScene.isLoaded;

            if (!targetWasAlreadyLoaded)
            {
                AsyncOperation loadOp = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);
                if (loadOp == null)
                {
                    Debug.LogError($"[SceneFlow] Failed to start additive load for scene '{nextSceneName}'.");
                    yield break;
                }

                while (!loadOp.isDone)
                {
                    yield return null;
                }

                nextScene = SceneManager.GetSceneByName(nextSceneName);
            }

            if (!nextScene.IsValid() || !nextScene.isLoaded)
            {
                Debug.LogError($"[SceneFlow] Scene '{nextSceneName}' did not finish loading.");
                yield break;
            }

            ActivateScene(nextScene);
            EnsureSingleEventSystemAndAudioListener(nextScene);

            if (notifyBoardReadyIfAlreadyLoaded && targetWasAlreadyLoaded)
            {
                GameEventManager.NotifyBoardSceneReady(nextSceneName);
            }

            if (unloadAllScenesExceptPersistentAndTarget)
            {
                yield return UnloadAllNonPersistentScenesExcept(nextSceneName);
            }
            else
            {
                yield return UnloadPreviousActiveSceneIfNeeded(currentActive, nextSceneName);
            }
        }
        finally
        {
            Debug.Log($"[SceneFlow] {currentActive.name} -> {nextSceneName} done in {(Time.unscaledTime - startedAt):0.00}s");
            isTransitioning = false;
        }
    }

    private static void ActivateScene(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root != null)
            {
                root.SetActive(true);
            }
        }

        SceneManager.SetActiveScene(scene);
    }

    private static void EnsureSingleEventSystemAndAudioListener(Scene preferredScene)
    {
        EventSystem[] eventSystems = FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        EventSystem preferredEventSystem = null;
        for (int i = 0; i < eventSystems.Length; i++)
        {
            EventSystem candidate = eventSystems[i];
            if (candidate != null && candidate.gameObject.scene == preferredScene)
            {
                preferredEventSystem = candidate;
                break;
            }
        }

        if (preferredEventSystem == null && eventSystems.Length > 0)
        {
            preferredEventSystem = eventSystems[0];
        }

        for (int i = 0; i < eventSystems.Length; i++)
        {
            EventSystem candidate = eventSystems[i];
            if (candidate != null)
            {
                candidate.enabled = candidate == preferredEventSystem;
            }
        }

        AudioListener[] audioListeners = FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        AudioListener preferredAudioListener = null;
        for (int i = 0; i < audioListeners.Length; i++)
        {
            AudioListener candidate = audioListeners[i];
            if (candidate != null && candidate.gameObject.scene == preferredScene)
            {
                preferredAudioListener = candidate;
                break;
            }
        }

        if (preferredAudioListener == null && audioListeners.Length > 0)
        {
            preferredAudioListener = audioListeners[0];
        }

        for (int i = 0; i < audioListeners.Length; i++)
        {
            AudioListener candidate = audioListeners[i];
            if (candidate != null)
            {
                candidate.enabled = candidate == preferredAudioListener;
            }
        }
    }

    private IEnumerator UnloadPreviousActiveSceneIfNeeded(Scene currentActiveScene, string targetSceneName)
    {
        if (!ShouldUnloadScene(currentActiveScene, targetSceneName))
        {
            yield break;
        }

        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(currentActiveScene);
        while (unloadOp != null && !unloadOp.isDone)
        {
            yield return null;
        }
    }

    private IEnumerator UnloadAllNonPersistentScenesExcept(string targetSceneName)
    {
        List<Scene> scenesToUnload = new List<Scene>();
        int loadedCount = SceneManager.sceneCount;
        for (int i = 0; i < loadedCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (ShouldUnloadScene(scene, targetSceneName))
            {
                scenesToUnload.Add(scene);
            }
        }

        for (int i = 0; i < scenesToUnload.Count; i++)
        {
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(scenesToUnload[i]);
            while (unloadOp != null && !unloadOp.isDone)
            {
                yield return null;
            }
        }
    }

    private bool ShouldUnloadScene(Scene scene, string targetSceneName)
    {
        if (!scene.IsValid() || !scene.isLoaded)
        {
            return false;
        }

        return !string.Equals(scene.name, targetSceneName, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(scene.name, persistentSceneName, StringComparison.OrdinalIgnoreCase);
    }
}
