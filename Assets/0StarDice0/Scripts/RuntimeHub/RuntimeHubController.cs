using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.EventSystems;

public class RuntimeHubController : MonoBehaviour
{
    private static string initialAdditiveSceneAfterHubLoad;

    [Header("UI References to Hide")]
    [Tooltip("ใส่ Canvas หรือ Panel ทั้งหมดที่ต้องการซ่อนตอนย้ายฉากลงในนี้")]
    // ✅ เปลี่ยนจาก GameObject ธรรมดา เป็น GameObject[] (Array)
    public GameObject[] uiElementsToHide; 

    private bool isTransitioning = false;

    private void Awake()
    {
        if (!string.IsNullOrWhiteSpace(initialAdditiveSceneAfterHubLoad))
        {
            HideConfiguredUI();
        }
    }

    public static void RequestInitialAdditiveSceneAfterHubLoad(string sceneName)
    {
        initialAdditiveSceneAfterHubLoad = string.IsNullOrWhiteSpace(sceneName) ? null : sceneName.Trim();
    }

    private void Start()
    {
        if (!string.IsNullOrWhiteSpace(initialAdditiveSceneAfterHubLoad))
        {
            string sceneName = initialAdditiveSceneAfterHubLoad;
            initialAdditiveSceneAfterHubLoad = null;
            StartCoroutine(LoadInitialAdditiveSceneRoutine(sceneName));
        }
    }

    private IEnumerator LoadInitialAdditiveSceneRoutine(string sceneName)
    {
        yield return LoadSceneRoutine(sceneName);
    }

    public void ConfirmAndGoNextScene(string nextScene)
    {
        if (isTransitioning) return;

        // 1. สั่งเซฟเด็ค
        if (DeckManager.TryGet(out var deckManager))
        {
            deckManager.SaveCurrentDeck();
        }

        // 2. เริ่มโหลดฉากใหม่
        StartCoroutine(LoadSceneRoutine(nextScene));
    }

    private IEnumerator LoadSceneRoutine(string nextScene)
    {
        isTransitioning = true;

        if (string.IsNullOrWhiteSpace(nextScene))
        {
            Debug.LogError("[RuntimeHubController] nextScene is null or empty.");
            isTransitioning = false;
            yield break;
        }

        HideConfiguredUI();

        if (SceneFlowController.TryRequestScene(nextScene))
        {
            while (SceneFlowController.IsTransitioning)
            {
                yield return null;
            }

            isTransitioning = false;
            yield break;
        }

        // Fallback for builds that do not have a SceneFlowController-compatible scene entry.
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Additive);
        if (loadOperation == null)
        {
            Debug.LogError($"[RuntimeHubController] Failed to start additive load for scene '{nextScene}'.");
            isTransitioning = false;
            yield break;
        }

        yield return loadOperation;

        Scene loadedScene = SceneManager.GetSceneByName(nextScene);
        if (loadedScene.IsValid() && loadedScene.isLoaded)
        {
            SceneManager.SetActiveScene(loadedScene);
        }

        EnsureSingleEventSystemAndAudioListener(loadedScene);
        isTransitioning = false;
    }

    private void HideConfiguredUI()
    {
        if (uiElementsToHide == null)
        {
            return;
        }

        foreach (GameObject ui in uiElementsToHide)
        {
            if (ui != null)
            {
                ui.SetActive(false);
            }
        }
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
            if (candidate == null)
            {
                continue;
            }

            candidate.enabled = candidate == preferredEventSystem;
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
            if (candidate == null)
            {
                continue;
            }

            candidate.enabled = candidate == preferredAudioListener;
        }
    }

    // ---------------------------------------------------------
    // 🟢 1. ฟังก์ชันสำหรับเปิด UI ทั้งหมดกลับมา
    // ---------------------------------------------------------
   public void RestoreUI()
    {
        // 1. เปิด UI ทั้งหมดกลับมา
        if (uiElementsToHide != null)
        {
            foreach (GameObject ui in uiElementsToHide)
            {
                if (ui != null) 
                {
                    ui.SetActive(true);
                }
            }
        }

        // 🟢 2. สิ่งที่ต้องเพิ่ม: ตามหา EventSystem ที่หลับอยู่ แล้วปลุกมันขึ้นมา!
        EventSystem currentEventSystem = FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
        if (currentEventSystem != null)
        {
            currentEventSystem.enabled = true; // สั่งตื่น!
        }

        Debug.Log("[RuntimeHubController] เปิด UI และปลุก EventSystem กลับมาทำงานแล้ว!");
    }
    // ---------------------------------------------------------
    // 🟢 2. ให้ระบบดักฟังอัตโนมัติ ว่ามีฉากไหนถูกปิดไปหรือเปล่า
    // ---------------------------------------------------------
    private void OnEnable()
    {
        // สมัครรับแจ้งเตือนเมื่อมีการ Unload (ปิด) ฉากใดๆ
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        // ยกเลิกการรับแจ้งเตือนเมื่อสคริปต์นี้ถูกทำลาย
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneUnloaded(Scene unloadedScene)
    {
        // 1. เช็คว่าฉากที่เพิ่งปิดไป ต้องไม่ใช่ฉากที่สคริปต์นี้อยู่ (ป้องกันมันทำงานตอนเราตั้งใจปิด Hub ทิ้งจริงๆ)
        // 2. เช็คว่าฉากที่สคริปต์นี้ทำงานอยู่ ต้องชื่อ "InterMission" เท่านั้น!
        if (unloadedScene != gameObject.scene && gameObject.scene.name == "InterMission")
        {
            RestoreUI();
        }
        else
        {
            Debug.Log($"[RuntimeHubController] ไม่ได้อยู่ในฉาก InterMission (อยู่ฉาก {gameObject.scene.name}) เลยไม่เปิด UI กลับมาครับ");
        }
    }
}
