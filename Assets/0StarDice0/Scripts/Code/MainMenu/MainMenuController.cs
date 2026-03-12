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

        ResetCardAvailabilityToCommonOnly();
        PlayerPrefs.DeleteKey("CurrentDeckData");
        ResetEquippedItemsForNewGame();

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
        PlayerPrefs.Save();
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
        HashSet<string> appliedCardKeys = new HashSet<string>(StringComparer.Ordinal);

        if (DeckManager.TryGet(out var deckManager) && deckManager.allCards != null)
        {
            ApplyCardDefaults(deckManager.allCards, appliedCardKeys);

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

        CardData[] loadedCardData = Resources.FindObjectsOfTypeAll<CardData>();
        ApplyCardDefaults(loadedCardData, appliedCardKeys);
    }

    private static void ApplyCardDefaults(IEnumerable<CardData> cards, HashSet<string> appliedCardKeys)
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

            if (!appliedCardKeys.Add(card.cardName))
            {
                continue;
            }

            bool isCommonCard = card.rarity == CardRarity.Common;
            card.isUsable = isCommonCard;
            PlayerPrefs.SetInt("CardState_" + card.cardName, isCommonCard ? 1 : 0);
        }
    }

    private void ResetAllPlayerCredits()
    {
        int clampedCredit = Mathf.Max(0, resetCreditValue);
        PlayerData[] players = Resources.FindObjectsOfTypeAll<PlayerData>();
        foreach (PlayerData player in players)
        {
            if (player == null)
            {
                continue;
            }

            player.SetCredit(clampedCredit);
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
