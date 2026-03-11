using System.Collections.Generic;
using UnityEngine;

public class RunSessionStore : MonoBehaviour
{
    [Header("Current Run")]
    [SerializeField] private string selectedMonsterId;
    [SerializeField] private List<string> selectedDeckIds = new List<string>();
    [SerializeField] private int runRoundIndex;
    [SerializeField] private bool hydrateFromPlayerPrefsOnAwake = true;

    private const string SelectedMonsterKey = "SelectedMonster";

    private static RunSessionStore cached;

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
        selectedMonsterId = monsterId;
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
    }

    public void HydrateFromPlayerPrefs()
    {
        selectedMonsterId = PlayerPrefs.GetString(SelectedMonsterKey, selectedMonsterId);
    }
}
