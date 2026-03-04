using UnityEngine;
using UnityEngine.SceneManagement;

public class BoardGameGroup : MonoBehaviour
{
    public static BoardGameGroup Instance { get; private set; }

    private bool shouldResetOnNextBoardEntry = false;

    [Header("Legacy fallback")]
    public string boardSceneName = "MainGame";

    [Header("Board scene names (recommended)")]
    public string[] boardSceneNames = new string[]
    {
        "MainGame",
        "TestMain",
        "MainLight",
        "MainWater",
        "MainWind",
        "MainEarth",
        "MainDark"
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool isBoardScene = IsBoardScene(scene);

        if (isBoardScene)
        {
            Debug.Log($"[BoardSystem] Welcome Home ({scene.name})! Showing Board.");
            ShowBoard(true);

            if (shouldResetOnNextBoardEntry)
            {
                ResetBoardSessionState();
                shouldResetOnNextBoardEntry = false;
            }
        }
        else
        {
            Debug.Log($"[BoardSystem] Entering {scene.name}. Hiding Board.");
            ShowBoard(false);

            if (scene.name == "InterMission")
            {
                shouldResetOnNextBoardEntry = true;
            }
        }
    }

    private bool IsBoardScene(Scene scene)
    {
        // 1) เช็คจากชื่อ scene ก่อน (กันกรณี RouteManager หาไม่เจอชั่วคราว)
        if (IsBoardName(scene.name))
            return true;

        // 2) เช็คจาก RouteManager ใน scene ที่โหลดมา (รวม inactive)
        if (SceneHasRouteManager(scene))
            return true;

        return false;
    }

    private bool IsBoardName(string sceneName)
    {
        if (!string.IsNullOrEmpty(boardSceneName) && sceneName == boardSceneName)
            return true;

        if (boardSceneNames != null)
        {
            for (int i = 0; i < boardSceneNames.Length; i++)
            {
                if (!string.IsNullOrEmpty(boardSceneNames[i]) && sceneName == boardSceneNames[i])
                    return true;
            }
        }

        return false;
    }

    private bool SceneHasRouteManager(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded)
            return false;

        var roots = scene.GetRootGameObjects();
        foreach (var root in roots)
        {
            if (root.GetComponentInChildren<RouteManager>(true) != null)
                return true;
        }

        return false;
    }


    private void ResetBoardSessionState()
    {
        Debug.Log("[BoardSystem] ♻️ Reset board session state for fresh run.");

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
    }

    public void ShowBoard(bool show)
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(show);
        }
    }
}
