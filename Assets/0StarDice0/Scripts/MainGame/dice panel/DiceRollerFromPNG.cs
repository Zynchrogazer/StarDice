using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class DiceRollerFromPNG : MonoBehaviour
{
    private static DiceRollerFromPNG cachedManager;

    public static bool TryGet(out DiceRollerFromPNG manager)
    {
        if (cachedManager != null) { manager = cachedManager; return true; }
        manager = FindFirstObjectByType<DiceRollerFromPNG>();
        if (manager != null) { cachedManager = manager; }
        return manager != null;
    }

    [Header("UI References (Player & AI)")]
    // 🟢 1. ช่องใหม่สำหรับลากโฟลเดอร์ UI มาใส่แยกกัน!
    [Tooltip("ลาก GameObject ตัวแม่ที่คลุมเต๋าทั้ง 6 หน้าของผู้เล่นมาใส่ตรงนี้")]
    public GameObject playerDiceContainer; 
    
    [Tooltip("ลาก GameObject ตัวแม่ที่คลุมเต๋าทั้ง 6 หน้าของ AI มาใส่ตรงนี้")]
    public GameObject aiDiceContainer;     

    [Header("Settings")]
    public float rollDuration = 1.0f;
    public float resultDisplayTime = 1.5f; // เวลาโชว์ผลลัพธ์

    [Header("Audio Settings")]
    public AudioSource sfxSource;
    public AudioClip rollTickSound;
    public AudioClip rollResultSound;

    private Button rollButton;
    private bool isRolling = false;
    private int pendingMultiplier = 1;

    // 🟢 2. แยกความจำลูกเต๋าเป็น 2 ชุด
    private GameObject[] playerDicePanels = new GameObject[6];
    private GameObject[] aiDicePanels = new GameObject[6];
    
    // ตัวแปรชี้เป้าว่าตอนนี้ตาใครทอย จะได้โชว์ภาพถูกชุด
    private GameObject[] currentActivePanels;
    private GameObject currentActiveContainer;

    private void Awake()
    {
        DiceRollerFromPNG[] rollers = FindObjectsByType<DiceRollerFromPNG>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (rollers.Length > 1) { Destroy(gameObject); return; }
        cachedManager = this;
        if (sfxSource == null) sfxSource = GetComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start() { RefreshReferences(); }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        GameEventManager.OnBoardSceneReady += RefreshReferences;
        if (gameObject.activeInHierarchy) RefreshReferences();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        GameEventManager.OnBoardSceneReady -= RefreshReferences;
    }

    private void OnDestroy() { if (cachedManager == this) cachedManager = null; }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "TestFight" || scene.name == "Shop" || scene.name.Contains("Minigame")) return;
        RefreshReferences();
    }

    private void RefreshReferences()
    {
        isRolling = false;
        rollButton = null;
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(RetryFindButton());
            SetupDicePanels(); // 🟢 สั่งดึงหน้าลูกเต๋าทั้ง 2 ชุด
            StartCoroutine(ApplyInitialState());
        }
    }

    // ==========================================
    // 🟢 โซนกำหนดว่าตาใครทอย
    // ==========================================
    public void RollDice()
    {   
        if (!GameTurnManager.TryGet(out var gameTurnManager) || isRolling || gameTurnManager.currentState != GameState.WaitingForRoll) return;
        
        // บอกระบบว่าใช้ UI ของผู้เล่นนะ!
        currentActivePanels = playerDicePanels;
        currentActiveContainer = playerDiceContainer;
        
        StartCoroutine(RollCoroutine());
    }

    public void RollDiceForAI()
    {
        if (!GameTurnManager.TryGet(out var gameTurnManager) || isRolling || gameTurnManager.currentState != GameState.Rolling) return;
        
        // บอกระบบว่าใช้ UI ของ AI นะ!
        currentActivePanels = aiDicePanels;
        currentActiveContainer = aiDiceContainer;
        
        StartCoroutine(RollCoroutine());
    }

    public void RollDiceWithResult(int forcedResult)
    {
        if (isRolling) return;
        // กรณีโดนบังคับทอย ให้เช็คว่าเป็นตาใคร
        if (GameTurnManager.TryGet(out var gameTurnManager) && GameTurnManager.CurrentPlayer != null)
        {
            currentActivePanels = GameTurnManager.CurrentPlayer.isAI ? aiDicePanels : playerDicePanels;
            currentActiveContainer = GameTurnManager.CurrentPlayer.isAI ? aiDiceContainer : playerDiceContainer;
        }
        StartCoroutine(RollCoroutine(forcedResult));
    }

    private IEnumerator RollCoroutine(int forcedResult = -1)
    {
        float elapsed = 0f;
        int finalIndex = 0;

        // ถ้าหาไม่เจอจริงๆ ให้ดึงชุดของผู้เล่นมากันบัคค้าง
        if (currentActivePanels == null || currentActivePanels.Length == 0) 
        {
            currentActivePanels = playerDicePanels;
            currentActiveContainer = playerDiceContainer;
        }

        isRolling = true;
        SetRollButtonActive(false);
        if (currentActiveContainer != null) currentActiveContainer.SetActive(true); // โชว์กล่องของคนที่กำลังทอย

        // --- ช่วง Animation หมุน ---
        while (elapsed < rollDuration)
        {
            finalIndex = Random.Range(0, currentActivePanels.Length);
            ShowOnlyPanel(finalIndex + 1);
            if (sfxSource != null && rollTickSound != null) sfxSource.PlayOneShot(rollTickSound, 0.7f);
            elapsed += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        // --- ช่วงสรุปผล ---
        int finalResult = (forcedResult != -1) ? forcedResult : Random.Range(1, currentActivePanels.Length + 1);

        PlayerState currentPlayer = GameTurnManager.CurrentPlayer;
        if (currentPlayer != null && currentPlayer.TryConsumeIceDebuff())
        {
            finalResult = Mathf.Max(1, finalResult / 2);
        }

        ShowOnlyPanel(finalResult);
        if (sfxSource != null && rollResultSound != null) sfxSource.PlayOneShot(rollResultSound, 1.0f);

        yield return new WaitForSeconds(resultDisplayTime); 

        int finalnubmber = finalResult * pendingMultiplier;
        pendingMultiplier = 1;

        HideAllPanels();
        isRolling = false;

        if (GameTurnManager.TryGet(out var gameTurnManager))
        {
            gameTurnManager.OnDiceRolled(finalnubmber);
        }
    }

    private void ShowOnlyPanel(int value)
    {
        for (int i = 0; i < currentActivePanels.Length; i++)
        {
            if (currentActivePanels[i] != null) 
                currentActivePanels[i].SetActive((i + 1) == value);
        }
    }

    private void HideAllPanels()
    {
        // สั่งปิดซ่อนโฟลเดอร์ของทั้งคู่เลย
        if (playerDiceContainer != null) playerDiceContainer.SetActive(false);
        if (aiDiceContainer != null) aiDiceContainer.SetActive(false);
    }

    // 🟢 ระบบดึงภาพหน้าลูกเต๋าแบบใหม่ 2 ชุด
    private void SetupDicePanels()
    {
        if (playerDiceContainer != null)
        {
            playerDiceContainer.SetActive(true);
            for (int i = 0; i < 6 && i < playerDiceContainer.transform.childCount; i++)
            {
                playerDicePanels[i] = playerDiceContainer.transform.GetChild(i).gameObject;
                playerDicePanels[i].SetActive(false);
            }
            playerDiceContainer.SetActive(false);
        }

        if (aiDiceContainer != null)
        {
            aiDiceContainer.SetActive(true);
            for (int i = 0; i < 6 && i < aiDiceContainer.transform.childCount; i++)
            {
                aiDicePanels[i] = aiDiceContainer.transform.GetChild(i).gameObject;
                aiDicePanels[i].SetActive(false);
            }
            aiDiceContainer.SetActive(false);
        }
    }

    // ==========================================
    // UI Button & System
    // ==========================================
    public void SetPendingMultiplier(int multiplier) { pendingMultiplier = multiplier; }
    public void EnableRollButton() { SetRollButtonActive(true); }

    public void SetRollButtonActive(bool isActive)
    {
        if (rollButton == null) FindAndSetupRollButton();
        if (rollButton != null)
        {
            rollButton.gameObject.SetActive(isActive);
            rollButton.interactable = isActive;
        }
    }

    private void FindAndSetupRollButton()
    {
        GameObject rollButtonObject = GameObject.FindWithTag("RollButton");
        if (rollButtonObject == null)
        {
            Button foundButton = Resources.FindObjectsOfTypeAll<Button>().Length > 0
                ? System.Array.Find(Resources.FindObjectsOfTypeAll<Button>(), b => b.name == "RollButton" || b.CompareTag("RollButton"))
                : null;
            if (foundButton != null) rollButtonObject = foundButton.gameObject;
        }

        if (rollButtonObject != null)
        {
            rollButton = rollButtonObject.GetComponent<Button>();
            if (rollButton != null)
            {
                rollButton.onClick.RemoveAllListeners();
                rollButton.onClick.AddListener(RollDice);
            }
        }
    }

    public void ForceEnableButton()
    {
        isRolling = false;
        if (rollButton == null) FindAndSetupRollButton();
        if (rollButton != null)
        {
            SetRollButtonActive(true);
        }
        else
        {
            Button[] allButtons = Resources.FindObjectsOfTypeAll<Button>();
            foreach (Button b in allButtons)
            {
                if (b.CompareTag("RollButton") || b.name == "RollButton")
                {
                    rollButton = b;
                    SetRollButtonActive(true);
                    return;
                }
            }
        }
    }

    private IEnumerator RetryFindButton()
    {
        int retryCount = 0;
        while (rollButton == null && retryCount < 10) 
        {
            FindAndSetupRollButton();
            if (rollButton != null) break;
            retryCount++;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator ApplyInitialState()
    {
        yield return new WaitForSeconds(0.3f); 
        if (GameTurnManager.TryGet(out var gameTurnManager) &&
            gameTurnManager.currentState == GameState.WaitingForRoll &&
            !GameTurnManager.CurrentPlayer.isAI)
        {
            SetRollButtonActive(true);
        }
    }
}