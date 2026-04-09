using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Flow")]
    [SerializeField] private string runtimeHubSceneName = "RuntimeHub";
   

    [Header("New Game Defaults")]
    [SerializeField] private int resetCreditValue = 0;

    private static readonly string[] MonsterUnlockKeys =
    {
        "MonsterWater", "MonsterEarth", "MonsterWind", "MonsterLight", "MonsterDark", "MonsterFire"
    };

    private bool isRequestingFlowScene;

    private void Awake()
    {
    }

    public void OnNewGameClicked()
    {
        Debug.Log("[MainMenu] Starting New Game. Resetting runtime/profile state.");

        ResetRuntimeState();
        ResetProgressForRuntimeHubStart();

        StartCoroutine(RequestRuntimeHubScene());
    }

    public void OnContinueClicked()
    {
        StartCoroutine(RequestRuntimeHubScene());
    }

    private void ResetProgressForRuntimeHubStart()
    {
        foreach (string monsterKey in MonsterUnlockKeys)
        {
            PlayerPrefs.SetInt(monsterKey, 0);
        }

        PlayerPrefs.DeleteKey("SelectedMonster");
        PlayerPrefs.SetInt("HasChosenMainCharacter", 0);

        PlayerPrefs.SetInt("levelReached", 1);

        ResetCardAvailabilityToCommonOnly();
        PlayerPrefs.DeleteKey("CurrentDeckData");
        ResetEquippedItemsForNewGame();

        // ---------------------------------------------------------
        // เพิ่มบรรทัดนี้เข้าไป เพื่อเรียกคำสั่งรีเซ็ต Equipment ให้ isOwned เป็น false
        EquipmentManager.ClearSavedOwnershipStates();
        // ---------------------------------------------------------

        if (RunSessionStore.TryGet(out var runSessionStore))
        {
            runSessionStore.ClearRunState();
        }

        if (GameData.Instance != null)
        {
            GameData.Instance.SetSelectedPlayer(null);
            GameData.Instance.SetSelectedCards(new List<CardData>());
        }

        ResetAllPlayerCredits();
        ResetSharedProgressForNewGame();
        PlayerPrefs.Save();
    }
    private void ResetSharedProgressForNewGame()
    {
        SkillManager.ClearSavedUnlockedSkills();
        PassiveSkillManager.ClearSavedProgress();
    }

    private void ResetEquippedItemsForNewGame()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.ClearEquippedItemsAndSave();
            return;
        }

        PlayerDataManager.ClearSavedEquipSlots();
    }

    private void ResetCardAvailabilityToCommonOnly()
    {
        Dictionary<string, bool> resolvedCardStates = new Dictionary<string, bool>(StringComparer.Ordinal);
        Dictionary<string, List<CardData>> cardsByKey = new Dictionary<string, List<CardData>>(StringComparer.Ordinal);

        if (DeckManager.TryGet(out var deckManager) && deckManager.allCards != null)
        {
            CollectCardDefaults(deckManager.allCards, resolvedCardStates, cardsByKey);

            if (deckManager.cardUse != null)
            {
                for (int i = 0; i < deckManager.cardUse.Length; i++)
                {
                    deckManager.cardUse[i] = null;
                }
            }

            deckManager.UpdateUseCardUI();
            deckManager.SortAndRefreshCards();
        }

        CardData[] resourceCardData = Resources.LoadAll<CardData>(string.Empty);
        CollectCardDefaults(resourceCardData, resolvedCardStates, cardsByKey);

        CardData[] loadedCardData = Resources.FindObjectsOfTypeAll<CardData>();
        CollectCardDefaults(loadedCardData, resolvedCardStates, cardsByKey);
        ApplyCollectedCardDefaults(resolvedCardStates, cardsByKey);
    }

    private static void CollectCardDefaults(
        IEnumerable<CardData> cards,
        Dictionary<string, bool> resolvedCardStates,
        Dictionary<string, List<CardData>> cardsByKey)
    {
        if (cards == null)
        {
            return;
        }

        foreach (CardData card in cards)
        {
            if (card == null || string.IsNullOrWhiteSpace(card.cardName))
            {
                continue;
            }

            bool isCommonCard = card.rarity == CardRarity.Common;
            if (resolvedCardStates.TryGetValue(card.cardName, out bool hasCommon))
            {
                resolvedCardStates[card.cardName] = hasCommon || isCommonCard;
            }
            else
            {
                resolvedCardStates[card.cardName] = isCommonCard;
            }

            if (!cardsByKey.TryGetValue(card.cardName, out List<CardData> sameKeyCards))
            {
                sameKeyCards = new List<CardData>();
                cardsByKey[card.cardName] = sameKeyCards;
            }

            sameKeyCards.Add(card);
        }
    }

    private static void ApplyCollectedCardDefaults(
        Dictionary<string, bool> resolvedCardStates,
        Dictionary<string, List<CardData>> cardsByKey)
    {
        foreach (KeyValuePair<string, bool> kv in resolvedCardStates)
        {
            string cardName = kv.Key;
            bool shouldBeUsable = kv.Value;

            PlayerPrefs.SetInt("CardState_" + cardName, shouldBeUsable ? 1 : 0);

            if (!cardsByKey.TryGetValue(cardName, out List<CardData> cards))
            {
                continue;
            }

            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i] != null)
                {
                    cards[i].isUsable = shouldBeUsable;
                }
            }
        }
    }

    private void ResetAllPlayerCredits()
    {
        int clampedCredit = Mathf.Max(0, resetCreditValue);
        HashSet<PlayerData> players = new HashSet<PlayerData>();

        PlayerData[] resourcePlayers = Resources.LoadAll<PlayerData>("PlayerData");
        for (int i = 0; i < resourcePlayers.Length; i++)
        {
            if (resourcePlayers[i] != null)
            {
                players.Add(resourcePlayers[i]);
            }
        }

        PlayerData[] loadedPlayers = Resources.FindObjectsOfTypeAll<PlayerData>();
        for (int i = 0; i < loadedPlayers.Length; i++)
        {
            if (loadedPlayers[i] != null)
            {
                players.Add(loadedPlayers[i]);
            }
        }

        foreach (PlayerData player in players)
        {
            if (player == null)
            {
                continue;
            }

            PlayerProgressService.ResetProgressToDefaults(player, clampedCredit);
        }

        if (GameData.Instance != null)
        {
            GameData.Instance.EnsureSelectedPlayerProgressLoaded();
        }
    }

    private void ResetRuntimeState()
    {
        // Scene-bound flow: managers live in RuntimeHub and are recreated by loading scenes,
        // so we only need to clear static transient trackers here.
        PlayerStartSpawner.LastKnownPositions.Clear();
    }

    private IEnumerator RequestRuntimeHubScene()
    {
        if (isRequestingFlowScene)
        {
            yield break;
        }

        if (string.IsNullOrWhiteSpace(runtimeHubSceneName))
        {
            yield break;
        }

        isRequestingFlowScene = true;

        if (Application.CanStreamedLevelBeLoaded(runtimeHubSceneName))
        {
            SceneManager.LoadScene(runtimeHubSceneName, LoadSceneMode.Single);
        }
        else
        {
            Debug.LogError($"[MainMenu] Cannot load RuntimeHub scene '{runtimeHubSceneName}'. Check Build Profiles.");
        }

        isRequestingFlowScene = false;
    }
 
}
