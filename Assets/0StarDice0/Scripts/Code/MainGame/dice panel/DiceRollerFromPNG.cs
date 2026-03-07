using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class DiceRollerFromPNG : MonoBehaviour
{
    public static DiceRollerFromPNG Instance { get; private set; }

    [Header("UI References (Optional)")]
    public Image diceImage;

    [Header("Settings")]
    public float rollDuration = 1.0f;

    [Header("Audio Settings")]
    public AudioSource sfxSource;
    public AudioClip rollTickSound;
    public AudioClip rollResultSound;

    private Button rollButton;
    private GameObject[] dicePanels = new GameObject[6];
    private bool isRolling = false;
    private int pendingMultiplier = 1;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (sfxSource == null) sfxSource = GetComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start() { RefreshReferences(); }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        // ✅ ลงทะเบียนฟังข่าว OnBoardSceneReady ด้วย
        GameEventManager.OnBoardSceneReady += RefreshReferences;

        if (gameObject.activeInHierarchy)
        {
            RefreshReferences();
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        GameEventManager.OnBoardSceneReady -= RefreshReferences;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "TestFight" || scene.name == "Shop" || scene.name.Contains("Minigame")) return;
        RefreshReferences();
    }

    private IEnumerator CheckStateOnEnable()
    {
        yield return new WaitForSeconds(0.2f); // รอให้ Manager ตั้งสติแป๊บนึง
        if (GameTurnManager.Instance != null &&
            GameTurnManager.Instance.currentState == GameState.WaitingForRoll &&
            !GameTurnManager.CurrentPlayer.isAI)
        {
            SetRollButtonActive(true);
            Debug.Log("<color=lime>[DiceRoller] กลับมาที่ซีนหลักและเปิดปุ่มให้ใหม่แล้ว</color>");
        }
    }

    private void RefreshReferences()
    {
        isRolling = false;
        rollButton = null;

        // ✅ ตรวจสอบว่า GameObject เปิดใช้งานอยู่หรือไม่ก่อนรัน Coroutine
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(RetryFindButton());
            FindAndSetupDicePanels();
            StartCoroutine(ApplyInitialState());
        }
        else
        {
            Debug.LogWarning($"[DiceRoller] {gameObject.name} ปิดอยู่! ข้ามการรัน Coroutine");
        }
    }

    public void RollDice()
    {   
        // ✅ เช็ค State: ต้องเป็นตาคนเล่น (WaitingForRoll) เท่านั้นถึงจะกดได้
        if (isRolling || GameTurnManager.Instance.currentState != GameState.WaitingForRoll) return;
        StartCoroutine(RollCoroutine());
        
    }

    public void RollDiceForAI()
    {
        // ✅ เช็ค State: ต้องเป็นตา AI (Rolling) เท่านั้น
        if (isRolling || GameTurnManager.Instance.currentState != GameState.Rolling) return;
        StartCoroutine(RollCoroutine());
    }

    public void RollDiceWithResult(int forcedResult)
    {
        if (isRolling) return;
        StartCoroutine(RollCoroutine(forcedResult));
    }

    private IEnumerator RollCoroutine(int forcedResult = -1)
    {
        float elapsed = 0f;
        int finalIndex = 0;

        if (dicePanels == null || dicePanels.Length == 0 || dicePanels[0] == null)
        {
            FindAndSetupDicePanels();
            if (dicePanels[0] == null) { elapsed = rollDuration; }
        }

        isRolling = true;
        SetRollButtonActive(false);

        // --- ช่วง Animation หมุน ---
        while (elapsed < rollDuration)
        {
            finalIndex = Random.Range(0, dicePanels.Length);
            ShowOnlyPanel(finalIndex + 1);
            if (sfxSource != null && rollTickSound != null) sfxSource.PlayOneShot(rollTickSound, 0.7f);
            elapsed += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        // --- ช่วงสรุปผล ---
        int finalResult = (forcedResult != -1) ? forcedResult : Random.Range(1, dicePanels.Length + 1);

        PlayerState currentPlayer = GameTurnManager.CurrentPlayer;
        if (currentPlayer != null && currentPlayer.TryConsumeIceDebuff())
        {
            finalResult = Mathf.Max(1, finalResult / 2);
            Debug.Log($"<color=cyan>❄️ Ice Debuff activated! Dice result reduced to {finalResult}</color>");
        }

        ShowOnlyPanel(finalResult);
        if (sfxSource != null && rollResultSound != null) sfxSource.PlayOneShot(rollResultSound, 1.0f);

        Debug.Log($"🎲 ลูกเต๋าออก {finalResult} (Multiplier: x{pendingMultiplier})");
        yield return new WaitForSeconds(0.5f);
        int finalnubmber = finalResult* pendingMultiplier;
        pendingMultiplier = 1;

        HideAllPanels();
        isRolling = false;

        // ✅ จุดสำคัญ: ส่งไม้ต่อให้ GameTurnManager เพื่อเปลี่ยน State เป็น Moving
        if (GameTurnManager.Instance != null)
        {
            GameTurnManager.Instance.OnDiceRolled(finalnubmber);
        }
    }

    public void SetPendingMultiplier(int multiplier) { pendingMultiplier = multiplier; }

    public void EnableRollButton() { SetRollButtonActive(true); }

    private void ShowOnlyPanel(int value)
    {
        for (int i = 0; i < dicePanels.Length; i++)
        {
            if (dicePanels[i] != null) dicePanels[i].SetActive((i + 1) == value);
        }
    }

    private void HideAllPanels()
    {
        foreach (var panel in dicePanels) if (panel != null) panel.SetActive(false);
    }

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
        // 1. พยายามหาด้วย Tag ก่อน (วิธีที่เร็วที่สุด)
        GameObject rollButtonObject = GameObject.FindWithTag("RollButton");

        // 2. ถ้าหาไม่เจอ อาจเป็นเพราะปุ่มถูกปิดอยู่ หรือชื่อไม่ตรง
        if (rollButtonObject == null)
        {
            // ค้นหาในกลุ่ม Object ทั้งหมด (รวมตัวที่ปิดอยู่ด้วย - ถ้าใช้ Unity เวอร์ชั่นใหม่)
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
                // Debug.Log("<color=cyan>[DiceRoller] ✅ เชื่อมต่อปุ่ม RollButton สำเร็จ!</color>");
            }
        }
        else
        {
            // 🚨 ถ้าหาไม่เจอจริงๆ ให้พิมพ์ Error ออกมาดู
            Debug.LogError("<color=red>[DiceRoller] ❌ หา RollButton ไม่เจอ! ตรวจสอบว่าปุ่มอยู่ใน Scene และตั้ง Tag 'RollButton' หรือยัง</color>");
        }
    }

    private void FindAndSetupDicePanels()
    {
        GameObject container = GameObject.Find("DicePanelsContainer");
        if (container != null)
        {
            container.SetActive(true);
            for (int i = 0; i < 6; i++)
            {
                if (i < container.transform.childCount)
                {
                    dicePanels[i] = container.transform.GetChild(i).gameObject;
                    dicePanels[i].SetActive(false);
                }
            }
        }
    }

    public void ForceEnableButton()
    {
        isRolling = false;

        // ✅ แก้ไข: พยายามหาปุ่มใหม่ทุกครั้งที่ถูกเรียก ถ้ายังไม่มี Reference
        if (rollButton == null)
        {
            FindAndSetupRollButton();
        }

        if (rollButton != null)
        {
            SetRollButtonActive(true);
            Debug.Log("<color=lime>[DiceRoller] 🔨 UI Restored Success!</color>");
        }
        else
        {
            // 🚨 ถ้ายังหาไม่เจอ ให้ลองหาแบบครอบคลุม (รวม Object ที่ปิดอยู่)
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
            Debug.LogError("<color=red>[DiceRoller] ❌ หา RollButton ไม่เจอจริงๆ ตรวจสอบ Tag ใน Scene ด้วย!</color>");
        }
    }

    private IEnumerator RetryFindButton()
    {
        int retryCount = 0;
        while (rollButton == null && retryCount < 10) // พยายามหา 10 ครั้ง (ประมาณ 1 วินาที)
        {
            FindAndSetupRollButton();
            if (rollButton != null) break;

            retryCount++;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator ApplyInitialState()
    {
        yield return new WaitForSeconds(0.3f); // รอให้การค้นหาปุ่มใน Retry เสร็จก่อน

        if (GameTurnManager.Instance != null &&
            GameTurnManager.Instance.currentState == GameState.WaitingForRoll &&
            !GameTurnManager.CurrentPlayer.isAI)
        {
            SetRollButtonActive(true);
            Debug.Log("<color=lime>[DiceRoller] UI Restored & Active!</color>");
        }
    }
}