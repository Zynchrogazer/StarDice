//using UnityEngine;
//using UnityEngine.UI;
//using System.Collections;

//public class DiceController : MonoBehaviour
//{
//    public static DiceController Instance { get; private set; }

//    [Header("UI แสดงหน้าลูกเต๋า")]
//    public GameObject[] dicePanels;       // index: 0 = หน้าลูกเต๋า 1, ..., 5 = ลูกเต๋า 6
//    public Image diceImage;
//    public Sprite[] diceSprites;          // index: 0 = sprite หน้า 1, ..., 5 = หน้า 6

//    [Header("Dependencies")]
//    public PlayerPathWalker player;
//    public Button rollButton;

//    [Header("Settings")]
//    public float rollDuration = 1.0f;

//    private bool isRolling = false;

//    private void Awake()
//    {
//        if (Instance != null && Instance != this)
//        {
//            Destroy(gameObject);
//        }
//        else
//        {
//            Instance = this;
//        }
//    }

//    private void Start()
//    {
//        if (rollButton != null)
//        {
//            rollButton.onClick.AddListener(RollDiceAndMove);
//        }
//        else
//        {
//            Debug.LogError("[DiceController] Roll Button is not assigned!");
//        }
//    }

//    public void RollDiceAndMove()
//    {
//        if (isRolling || player.IsExecutingTurn) return;

//        rollButton.interactable = false;
//        StartCoroutine(RollDiceCoroutine());
//    }

//    private IEnumerator RollDiceCoroutine()
//    {
//        isRolling = true;
//        float elapsed = 0f;

//        // หมุนเต๋าแบบสุ่ม
//        while (elapsed < rollDuration)
//        {
//            int randIndex = Random.Range(0, diceSprites.Length); // 0 - 5
//            diceImage.sprite = diceSprites[randIndex];
//            elapsed += 0.1f;
//            yield return new WaitForSeconds(0.3f);
//        }

//        // สุ่มผลลัพธ์สุดท้าย
//        int result = Random.Range(1, 7); // 1 - 6
//        diceImage.sprite = diceSprites[result - 1];
//        Debug.Log($"🎲 ทอยได้: {result}");

//        yield return new WaitForSeconds(0.3f);

//        ShowOnlyPanel(result);
//        player.ExecuteMove(result);

//        isRolling = false;
//    }

//    private void ShowOnlyPanel(int value)
//    {
//        for (int i = 0; i < dicePanels.Length; i++)
//        {
//            dicePanels[i].SetActive((i + 1) == value);
//        }
//    }

//    public void EnableRollButton()
//    {
//        if (rollButton != null)
//        {
//            rollButton.interactable = true;
//            Debug.Log("<color=yellow>[DiceController] Enable Roll Button</color>");
//        }
//    }

//    private void OnDestroy()
//    {
//        if (rollButton != null)
//        {
//            rollButton.onClick.RemoveListener(RollDiceAndMove);
//        }
//    }
//}
