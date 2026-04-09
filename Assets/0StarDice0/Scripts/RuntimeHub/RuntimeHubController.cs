using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.EventSystems;

public class RuntimeHubController : MonoBehaviour
{
    [Header("UI References to Hide")]
    [Tooltip("ใส่ Canvas หรือ Panel ทั้งหมดที่ต้องการซ่อนตอนย้ายฉากลงในนี้")]
    // ✅ เปลี่ยนจาก GameObject ธรรมดา เป็น GameObject[] (Array)
    public GameObject[] uiElementsToHide; 

    private bool isTransitioning = false;

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

        // โหลดฉากใหม่แบบ Additive
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Additive);
        yield return loadOperation;

        // ตั้งฉากใหม่เป็น Active Scene
        Scene loadedScene = SceneManager.GetSceneByName(nextScene);
        if (loadedScene.IsValid() && loadedScene.isLoaded)
        {
            SceneManager.SetActiveScene(loadedScene);
        }

        // 🎯 [KISS] วนลูปปิด UI ทุกตัวที่คุณลากมาใส่ใน Inspector ทิ้งไปตรงๆ
        if (uiElementsToHide != null)
        {
            foreach (GameObject ui in uiElementsToHide)
            {
                if (ui != null) 
                {
                    ui.SetActive(false);
                }
            }
        }

        EnsureSingleEventSystemAndAudioListener(loadedScene);
        isTransitioning = false;
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
}
