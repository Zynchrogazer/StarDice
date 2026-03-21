using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RunSessionStore : MonoBehaviour
{
    [Header("Current Run")]
    [SerializeField] private string selectedMonsterId;
    [SerializeField] private List<string> selectedDeckIds = new List<string>();
    [SerializeField] private int runRoundIndex;
    [SerializeField] private bool hydrateFromPlayerPrefsOnAwake = true;
    [SerializeField] private float monsterSyncIntervalSeconds = 0.2f;

    private const string SelectedMonsterKey = "SelectedMonster";
    private const string SelectedDeckKey = "CurrentDeckData";

    private static RunSessionStore cached;
    private float nextMonsterSyncAt;

    public string SelectedMonsterId => selectedMonsterId;
    public IReadOnlyList<string> SelectedDeckIds => selectedDeckIds;
    public int RunRoundIndex => runRoundIndex;

    private void Awake()
    {
        if (cached != null && cached != this)
        {
            Destroy(gameObject);
            return;
        }

        cached = this;
        DontDestroyOnLoad(gameObject);

        if (hydrateFromPlayerPrefsOnAwake)
        {
            HydrateFromPlayerPrefs();
        }
    }

    private void OnDestroy()
    {
        if (cached == this)
        {
            cached = null;
        }
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
        if (!hydrateFromPlayerPrefsOnAwake)
        {
            return;
        }

        // รองรับเคสที่มีสคริปต์อื่นเขียน PlayerPrefs โดยไม่ได้เรียก SetSelectedMonster ตรง ๆ
        SyncMonsterFromPlayerPrefs();
    }


    private void Update()
    {
        if (!hydrateFromPlayerPrefsOnAwake)
        {
            return;
        }

        if (Time.unscaledTime < nextMonsterSyncAt)
        {
            return;
        }

        nextMonsterSyncAt = Time.unscaledTime + Mathf.Max(0.05f, monsterSyncIntervalSeconds);
        SyncMonsterFromPlayerPrefs();
    }

    private void SyncMonsterFromPlayerPrefs()
    {
        string prefsMonsterId = NormalizeMonsterId(PlayerPrefs.GetString(SelectedMonsterKey, string.Empty));
        if (!string.Equals(selectedMonsterId, prefsMonsterId, System.StringComparison.Ordinal))
        {
            selectedMonsterId = prefsMonsterId;
        }
    }


    public static bool TryGet(out RunSessionStore store)
    {
        store = cached;
        if (store != null)
        {
            return true;
        }

        store = FindFirstObjectByType<RunSessionStore>(FindObjectsInactive.Include);
        cached = store;
        return store != null;
    }

    public void SetSelectedMonster(string monsterId)
    {
        selectedMonsterId = NormalizeMonsterId(monsterId);

        if (string.IsNullOrEmpty(selectedMonsterId))
        {
            PlayerPrefs.DeleteKey(SelectedMonsterKey);
        }
        else
        {
            PlayerPrefs.SetString(SelectedMonsterKey, selectedMonsterId);
        }

        PlayerPrefs.Save();
    }

    public void SetSelectedDeck(IEnumerable<string> deckIds)
    {
        selectedDeckIds.Clear();
        if (deckIds == null)
        {
            return;
        }

        foreach (string id in deckIds)
        {
            if (!string.IsNullOrEmpty(id))
            {
                selectedDeckIds.Add(id);
            }
        }
    }

    public void SetRunRoundIndex(int roundIndex)
    {
        runRoundIndex = Mathf.Max(0, roundIndex);
    }

    public void ClearRunState()
    {
        selectedMonsterId = string.Empty;
        selectedDeckIds.Clear();
        runRoundIndex = 0;

        PlayerPrefs.DeleteKey(SelectedMonsterKey);
        PlayerPrefs.DeleteKey(SelectedDeckKey);
        PlayerPrefs.Save();
    }

    private static string NormalizeMonsterId(string rawMonsterId)
    {
        if (string.IsNullOrWhiteSpace(rawMonsterId))
        {
            return string.Empty;
        }

        string normalized = rawMonsterId.Trim();
        if (normalized.StartsWith("Monster", System.StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized.Substring("Monster".Length);
        }

        return normalized;
    }

    public void HydrateFromPlayerPrefs()
    {
        SyncMonsterFromPlayerPrefs();

        string deckSnapshot = PlayerPrefs.GetString(SelectedDeckKey, string.Empty);
        if (string.IsNullOrEmpty(deckSnapshot))
        {
            return;
        }

        string[] splitNames = deckSnapshot.Split(',');
        selectedDeckIds.Clear();
        foreach (string deckId in splitNames)
        {
            if (string.IsNullOrEmpty(deckId) || deckId == "EMPTY")
            {
                continue;
            }

            selectedDeckIds.Add(deckId);
        }
    }
}
