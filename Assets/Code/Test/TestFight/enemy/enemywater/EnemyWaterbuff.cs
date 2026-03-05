 using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class EnemyWaterbuff : MonoBehaviour
{
    [Header("Player Setup")]
    public PlayerData selectedPlayer;
    public Image playerImage;
    public Slider playerHPBar;
    public TMP_Text playerHPText;

    [Header("Enemy Setup")]
    private int enemyHP = 160;
    public Slider enemyHPBar;
    public TMP_Text enemyHPText;

    [Header("UI")]
    public Button attackButton;
    public Button[] skillButtons;
    int EnemyStunPlayer = 0;
    private int playerHP;

       [Header("Sound Collection")]
    public AudioClip[] sfxList;
    
    private int enemySkill1Cooldown = 0;
    private int enemySkill2Cooldown = 0;

    private Dictionary<SkillData, int> skillCooldowns = new Dictionary<SkillData, int>();
    private bool isPlayerTurn = true; // เริ่มเกมเป็นเทิร์นผู้เล่น

    private int burnTurnsLeft = 0;

    public TMP_Text playerturntext;
    public TMP_Text enemyturntext;
    private int SuperburnTurnsLeft = 0;
    public ElementType elementType;
    public ElementType enemyElement = ElementType.Earth; // <--เปลียนธาตุศัตรูตรนี้
    private bool isDamageBoosted = false;
    private int enemySkill3Cooldown = 0;
    private int playerStunTurns = 0;
    [Header("Result Panel")]
    public GameObject winPanel;
    public GameObject losePanel;
    public TMP_Text resultText;

    [Header("Card System")]

    public Button[] cardButtons;
    private int reduceEnemyDamageTurns = 0; // นับจำนวนเทิร์นที่ลดดาเมจศัตรู
    private HashSet<CardData> usedCards = new HashSet<CardData>(); // กันใช้ซ้ำ
    private int silenceEnemyTurns = 0; // เทิร์นที่ห้ามศัตรูใช้สกิล
    private bool reflectNextAttack = false;
    private bool isIgnoreElementCardActive = false;

    private int enemyDamageReductionTurns = 0;
    private bool isEnemyDamageReduced = false;
    private bool reflectNextAttackWind = false;

    private bool isShieldActive = false;
    private int shieldTurnsLeft = 0;
    int EnemyBuffLight = 0;
    private bool isEnemyStunned = false;
    private int enemyStunTurnsLeft = 0;
    private int evadeTurnsLeft = 0;
    private int lightBuffTurnsLeft = 0;
    private int lightShieldTurnsLeft = 0;
    private int EnemydirtStunPlayer = 0;
    public TMP_Text[] cooldownTexts;
    private bool hasUsedCardThisTurn = false;
    int doubleAttackTurnsLeft = 0; // เทิร์นที่ยังเหลือสำหรับบัฟโจมตี x2
    bool isDelayedHealActive = false;
    int delayedHealTurnsLeft = 0;
    float delayedHealPercent = 0f;
    bool isEnemyAttackReducedPermanently = false;
    int regenTurnsLeft = 0; // จำนวนเทิร์นที่เหลือในการฟื้น HP
    int regenAmountPerTurn = 0; // ฟื้น HP ต่อเทิร์น
    int passiveAttackBonusTurns = 0;
    int passiveAttackBonusPerTurn = 0;
    bool isHealingEveryTurn = false;
    int healPerTurnAmount = 1; // ค่า default
    bool isDodgeActive = false;
    float dodgeChance = 0f;

    bool isElementalAttackBoosted = false;
    int isElementalAttackBoostedx2 = 0;
    float isElementalAttackBoostedRandom = 0f;
    int ElementalAttackBoostedRandom = 0;
    int SuperBoostAttackTurn = 0;
    int AttackBoosted = 0;
    int RandomBootDamageTurn = 0;
    private bool isElementalBuffPerTurnActive = false;
    private float elementalBuffPerTurnValue = 0.5f;
    private int doubleAttackandloseTurnsLeft = 0;
    private bool isHalfDamageAll = false;
    private int originalMaxHP; // เก็บไว้เผื่ออยากคืนค่าเดิม
    private float attackMultiplier = 1f;
    private bool attackMultiplierTurn = false;
    int poisonEnemyTurnsLeft = 0;
    bool isPoisonEnemy = false;

    private int enemyMaxHP;
    private bool checkReduceAttackHealFull = false;
    int healOverTimeTurnsLeft = 0;
    int confuseHitSelfTurns = 0;

    bool isHealingOverTime = false;

    int healOnAttackTurnsLeft = 0;
    int confuseEnemyTurns = 0;
    public GameObject[] skillEffectObjects;
        public GameObject[] CardEffectObjects;
    bool AttackBoostRandomRange = false;
    int isEnemyDamageReducedHalf = 0;
    private bool isCursedAttack = false;
    private bool hasPreventDeathEffect = false;
    private int ultraDamageTurns = 0;
    private int enemySpeed = 30;
    private int enemyDef = 40; //def ของศัตรู

    private int finaldamgedef = 1;
    private int damegeEnemydef = 1;
    public RectTransform enemyImage;        // UI หรือ Sprite ศัตรูที่ต้องการให้สั่น
    public TMP_Text damageTextUI;
    public RectTransform[] playerRects;          // UI รูปผู้เล่น (RectTransform)
    public TMP_Text playerDamageTextUI;            // UI Text สำหรับดาเมจผู้เล่น

    bool SuperSmashFire = false;
    public List<Transform> spriteSlots; // GameObjects ที่จะใช้แสดงภาพ (ต้องมี SpriteRenderer)
    int regenwater = 0;
    int superreduce = 0;
    int ReduceWater = 0;
    int WaterBuffx2 = 0;
    int ReduceFire = 0;
    int ReduceWind = 0;
    int ReduceLight = 0;
    int EarthBootDef = 0;
    int EarthNerfDamage = 0;
    int SuperSmashEarthLeft = 0;
    int SuperevadeTurnsLeft = 0;
    int BootDamageDark = 0;
    int Buffx3Light = 0;
    int superreducelight = 0;
    int NerfEnemyDamgelight = 0;
    bool NerfEarth = false;
    bool EarthNerfSPD = false;
    bool EarthSmashEarthLeft = false;
    bool NerfDark = false;
    private HashSet<SkillData> usedOnceSkills = new HashSet<SkillData>();
    int EnemyShieldEarth = 0;
    int EnemyShieldWater = 0;
    int doublereflectNextAttackWind = 0;
    int EnemyDarkWalk = 0;
    int EnemyFireBuff = 0;
    int EnemyReflectWind = 0;
    int EnemyShieldWater100 = 0;
    private List<CardData> selectedCards = new List<CardData>();
     void Start()
    { Debug.Log(">>> BattleSystem เริ่มทำงานแล้วนะ! <<<");
  GameEventManager.Instance.AddCount2(1);
       ApplyEquippedItems();
        List<CardData> myHand = BattleCardHandResolver.GetOpeningHand(4);
        if (myHand.Count > 0)
        {
            LoadSelectedCards(myHand);
        }
        else
        {
            Debug.LogWarning("[BattleSystem] ไม่พบการ์ดสำหรับใช้งานในฉากต่อสู้");
        }
        selectedPlayer = GameData.Instance.selectedPlayer;
        SetupPlayer();
        SetupEnemy();
        SetupButtons();
        UpdateSkillButtons();
        SetupCardsUI();

        if (selectedPlayer.speed >= enemySpeed)
        {
            isPlayerTurn = true;
            UpdateSkillButtons();
            playerturntext.gameObject.SetActive(true);
            enemyturntext.gameObject.SetActive(false);
            Debug.Log("ผู้เล่นมีสปีดมากกว่า เริ่มเทิร์นก่อน");
        }
        else
        {
            isPlayerTurn = false;
            hasUsedCardThisTurn = true;
            playerturntext.gameObject.SetActive(false);
            enemyturntext.gameObject.SetActive(true);
            UpdateSkillButtons();
            Debug.Log("ศัตรูมีสปีดมากกว่า เริ่มเทิร์นก่อน");
            StartCoroutine(DelayedEnemyTurn());
        }

    }

    public void ApplyEquippedItems()
    {
        // 1. ดึง Array ของไอเท็ม 2 ชิ้นมาจาก Manager
        EquipmentData[] items = PlayerDataManager.Instance.equippedItems;

        // 2. วนลูปเช็คทีละชิ้น (ทั้งช่อง 0 และช่อง 1)
        foreach (EquipmentData item in items)
        {
            // เช็คว่าช่องนั้นมีของใส่จริงไหม (กัน Error)
            if (item != null)
            {
                ApplyEffect(item.itemID);
            }
        }
    }
    public void ShowSkillEffectOnce(int index)
    {
        StartCoroutine(ShowAndHideEffect(skillEffectObjects[index]));
    }
       public void ShowCardEffectOnce(int index)
    {
        StartCoroutine(ShowAndHideEffect(CardEffectObjects[index]));
    }


    IEnumerator ShowAndHideEffect(GameObject effect)
    {
        effect.SetActive(true);  // ✅ แสดง
        yield return new WaitForSeconds(3f);
        effect.SetActive(false); // ✅ ซ่อน
    }
        void PlaySoundEffect(int index)
{
    // เช็คทีเดียวตรงนี้เลย ปลอดภัย ไม่ต้องเขียนซ้ำ
    if (index < sfxList.Length && sfxList[index] != null)
    {
        BattleAudioUtility.PlaySfx(this, sfxList, index);
    }
}


    public void LoadSelectedCards(List<CardData> cards)
    {
        selectedCards = new List<CardData>(cards);
        SetupCardsUI();
    }


    void SetupCardsUI()
    {
        for (int i = 0; i < cardButtons.Length; i++)
        {
            if (i < selectedCards.Count)
            {
                var card = selectedCards[i];
                cardButtons[i].gameObject.SetActive(true);
                cardButtons[i].interactable = isPlayerTurn;

                cardButtons[i].GetComponentInChildren<TMP_Text>().text = card.cardName;
                cardButtons[i].GetComponentInChildren<Image>().sprite = card.icon;

                int index = i;
                cardButtons[i].onClick.RemoveAllListeners();
                cardButtons[i].onClick.AddListener(() => UseCard(card,index));
            }
            else
            {
                cardButtons[i].gameObject.SetActive(false);
                cardButtons[i].interactable = false;

            }

        }
    }

    void LoadSelectedCharacter()
    {
        string selectedName = PlayerPrefs.GetString("SelectedCharacter");
        Debug.Log("ชื่อที่เลือกจาก PlayerPrefs: " + selectedName);

        selectedPlayer = Resources.Load<PlayerData>($"PlayerData/{selectedName}");




        if (selectedPlayer == null)
            Debug.LogError("ไม่พบ PlayerData ที่เลือกจาก Resources!");
    }

    void SetupPlayer()
    {
        playerHP = selectedPlayer.maxHP;
        playerHPBar.maxValue = selectedPlayer.maxHP;
        playerHPBar.value = playerHP;
        UpdatePlayerHPUI();

        if (selectedPlayer != null && selectedPlayer.playerName == "MonsterEarth")
        {
            spriteSlots[0].gameObject.SetActive(true);

            Debug.Log("ตัวละครที่เลือกคือ EarthMonster (จาก field playerName)");
        }
        if (selectedPlayer != null && selectedPlayer.playerName == "FireMonster")
        {
            spriteSlots[1].gameObject.SetActive(true);

            Debug.Log("ตัวละครที่เลือกคือ FireMonster (จาก field playerName)");
        }
        if (selectedPlayer != null && selectedPlayer.playerName == "MonsterWater")
        {
            spriteSlots[2].gameObject.SetActive(true);

            Debug.Log("ตัวละครที่เลือกคือ WaterMonster (จาก field playerName)");
        }
        if (selectedPlayer != null && selectedPlayer.playerName == "MonsterWind")
        {
            spriteSlots[3].gameObject.SetActive(true);

            Debug.Log("ตัวละครที่เลือกคือ WindMonster (จาก field playerName)");
        }
        if (selectedPlayer != null && selectedPlayer.playerName == "MonsterDark")
        {
            spriteSlots[4].gameObject.SetActive(true);

            Debug.Log("ตัวละครที่เลือกคือ WindMonster (จาก field playerName)");
        }
        if (selectedPlayer != null && selectedPlayer.playerName == "MonsterLight")
        {
            spriteSlots[5].gameObject.SetActive(true);

            Debug.Log("ตัวละครที่เลือกคือ WindMonster (จาก field playerName)");
        }




        for (int i = 0; i < skillButtons.Length; i++)
        {
            if (i < selectedPlayer.skills.Length)
            {
                var skill = selectedPlayer.skills[i];
                skillButtons[i].GetComponentInChildren<TMP_Text>().text = skill.skillName;
                skillButtons[i].GetComponentInChildren<Image>().sprite = skill.icon;
                int index = i;
                skillButtons[i].onClick.AddListener(() => UseSkill(skill));
            }
            else
            {
                skillButtons[i].gameObject.SetActive(false);
            }
        }
    }

    void SetupEnemy()
    {
        enemyHP = 160;
        enemyMaxHP = enemyHP; // เก็บ maxHP ไว้ใช้ในภายหลัง

        enemyHPBar.maxValue = enemyMaxHP;
        enemyHPBar.value = enemyHP;

        Debug.Log("ธาตุศัตรูคือ: " + enemyElement);
        UpdateEnemyHPUI();
    }


    void SetupButtons()
    {
        attackButton.onClick.AddListener(() =>
        {
            if (isPlayerTurn)
            {
                DoBasicAttack();
            }
            else
            {
                Debug.Log("ยังไม่ถึงเทิร์นของคุณ!");
            }
        });

        for (int i = 0; i < skillButtons.Length; i++)
        {
            int index = i; // เก็บ index สำหรับ Lambda
            skillButtons[i].onClick.AddListener(() =>
            {
                if (isPlayerTurn)
                {
                    UseSkill(selectedPlayer.skills[index]);
                }
                else
                {
                    Debug.Log("ยังไม่ถึงเทิร์นของคุณ!");
                }
            });
        }


    }

    void UpdateSkillButtons()
    {
        
        for (int i = 0; i < skillButtons.Length; i++)
        {
            if (i < selectedPlayer.skills.Length)
            {
                var skill = selectedPlayer.skills[i];
                bool canUse = isPlayerTurn;

                if (skillCooldowns.ContainsKey(skill) && skillCooldowns[skill] > 0)
                {
                    canUse = false;
                    cooldownTexts[i].text = skillCooldowns[skill].ToString(); // แสดงเลขคูลดาวน์
                    cooldownTexts[i].gameObject.SetActive(true);
                }
                else
                {
                    cooldownTexts[i].text = "";
                    cooldownTexts[i].gameObject.SetActive(false); // ซ่อนเลขถ้าไม่มีคูลดาวน์
                }

                skillButtons[i].interactable = canUse;
            }
            else
            {
                skillButtons[i].interactable = false;
                cooldownTexts[i].gameObject.SetActive(false);
            }
        }
        attackButton.interactable = isPlayerTurn;
    }


    void DoBasicAttack()
    {
        StartCoroutine(MyDelay());
        isPlayerTurn = false;  // ผู้เล่นใช้เทิร์นนี้แล้ว
        ShowSkillEffectOnce(8);
         UpdateSkillButtons();
        DamageNormalEnemy(selectedPlayer.attackDamage);
        StartCoroutine(DelayedEnemyTurn());
    }

  void UseSkill(SkillData skill)
    {
        StartCoroutine(MyDelay());
        if (!skillCooldowns.ContainsKey(skill))
            skillCooldowns[skill] = 0;

        if (skillCooldowns[skill] > 0)
        {
            Debug.Log($"{skill.skillName} ยังไม่พร้อมใช้! ต้องรออีก {skillCooldowns[skill]} เทิร์น");
            return;
        }
        if (usedOnceSkills.Contains(skill))
        {
            Debug.Log($"{skill.skillName} ใช้ได้ครั้งเดียว และถูกใช้ไปแล้ว");
            return;
        }

        ////Monster Fire///////////////////////////////

        // >>> สกิล 3 (บัฟดาเมจ) ธาตุไฟ
        if (skill.effectType == SkillData.SkillEffectType.BuffFire && selectedPlayer.element == ElementType.Fire)
        {
            DamagePlayer(10); // ลดเลือดตัวเอง
            isDamageBoosted = true; // เปิดบัฟ
            skillCooldowns[skill] = 7;
            Debug.Log("ใช้สกิล 3 ลดเลือดตัวเอง 10 ดาเมจโจมตีจะคูณ 2 ในเทิร์นนี้");
            PlaySoundEffect(3);
            ShowSkillEffectOnce(7);

            UpdateSkillButtons();
            return; // ไม่ส่งเทิร์น ให้ใช้สกิลอื่นต่อ
        }

        int finalDamage = skill.power;

        // เช็คธาตุศัตรูสำหรับ skill 1 และ 2 เท่านั้น
        if (skill.effectType == SkillData.SkillEffectType.DamageFire || skill.effectType == SkillData.SkillEffectType.Burn || skill.effectType == SkillData.SkillEffectType.RandomDamgeFire || skill.effectType == SkillData.SkillEffectType.SuperSuperFire || skill.effectType == SkillData.SkillEffectType.SuperFire)
        {
            if (enemyElement == ElementType.Water)
            {
                finalDamage /= 2;
                Debug.Log("ศัตรูธาตุน้ำ ดาเมจหาร 2");
            }
            else if (enemyElement == ElementType.Wind)
            {
                finalDamage *= 2;
                Debug.Log("ศัตรูธาตุลม ดาเมจคูณ 2");
            }
            if (isElementalAttackBoosted)
            {
                finalDamage += finalDamage / 2;
                Debug.Log("ใช้การ์ดบัฟโจมตีธาตุแรงขึ้น 0.5 เท่า");
            }
            if (isElementalAttackBoostedx2 > 0)
            {
                finalDamage *= 2;
                Debug.Log("ใช้การ์ดบัฟโจมตีธาตุแรงขึ้น 2 เท่า");
            }
            if (ElementalAttackBoostedRandom > 0)
            {
                finalDamage = Mathf.RoundToInt(finalDamage * isElementalAttackBoostedRandom);
                ElementalAttackBoostedRandom--;
                Debug.Log($"ใช้การ์ดบัฟโจมตีธาตุ {isElementalAttackBoostedRandom} เท่า");
            }
        }
        else
        {
            Debug.Log("ข้ามการคำนวณธาตุเพราะใช้การ์ด Ignore Element");
            isIgnoreElementCardActive = false;  // ✅ ใช้ครั้งเดียว
        }

        // >>> สกิล 1 (โจมตี) ธาตุไฟ
        if (skill.effectType == SkillData.SkillEffectType.DamageFire && selectedPlayer.element == ElementType.Fire)
        {
            skillCooldowns[skill] = 3;
            ShowSkillEffectOnce(0);

            if (isDamageBoosted)
            {
                finalDamage *= 2;
                isDamageBoosted = false;
                Debug.Log("บัฟ x2 ทำงานกับสกิล 1 ดาเมจรวม = " + finalDamage);
            }
            isPlayerTurn = false;
            DamageEnemy(finalDamage);
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }


        // >>> สกิล 2 (ไฟเผา) ธาตุไฟ
        else if (skill.effectType == SkillData.SkillEffectType.Burn && selectedPlayer.element == ElementType.Fire)
        {
            isPlayerTurn = false;
            burnTurnsLeft = 3;
            ShowSkillEffectOnce(4);
            PlaySoundEffect(7);
            skillCooldowns[skill] = 6;
            Debug.Log("ศัตรูติดสถานะไฟ 3 เทิร์น!");
             UpdateSkillButtons();
              StartCoroutine(DelayedEnemyTurn());
            // ไม่ใช้ isDamageBoosted กับสกิลนี้
        }

        else if (skill.effectType == SkillData.SkillEffectType.SuperFire && selectedPlayer.element == ElementType.Fire)
        {
            skillCooldowns[skill] = 5;
            ShowSkillEffectOnce(25);
            PlaySoundEffect(7);
            if (isDamageBoosted)
            {
                finalDamage *= 2;
                isDamageBoosted = false;
                Debug.Log("บัฟ x2 ทำงานกับสกิล 1 ดาเมจรวม = " + finalDamage);
            }
            isPlayerTurn = false;
            DamageEnemy(finalDamage);
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

        else if (skill.effectType == SkillData.SkillEffectType.PaintoHeal && selectedPlayer.element == ElementType.Fire)
        {
            int lostHP = Mathf.FloorToInt(selectedPlayer.maxHP * 0.1f);
            playerHP -= lostHP;
            playerHP = Mathf.Clamp(playerHP, 0, selectedPlayer.maxHP);
            UpdatePlayerHPUI();
            ShowSkillEffectOnce(24);
            StartCoroutine(ShakePlayer());
            ShowPlayerDamageNumber($"-{lostHP}");
            Debug.Log($"เสีย HP 10% ทันที: -{lostHP}");
            delayedHealTurnsLeft = 3;
            delayedHealPercent = 0.3f; // 30%
            isDelayedHealActive = true;
            Debug.Log("ฟื้นฟู 30% ของ HP ในอีก 3 เทิร์น");
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

        else if (skill.effectType == SkillData.SkillEffectType.RandomDamgeFire && selectedPlayer.element == ElementType.Fire)
        {

            int randomDamage = Random.Range(20, 51); // สุ่ม 20 ถึง 50
            finalDamage = randomDamage;

            if (isDamageBoosted)
            {
                finalDamage *= 2;
                isDamageBoosted = false;
                Debug.Log("บัฟ x2 ทำงานกับสกิล 1 ดาเมจรวม = " + finalDamage);
            }

            DamageEnemy(finalDamage);
            skillCooldowns[skill] = 5;
            ShowSkillEffectOnce(25);
            PlaySoundEffect(7);
            Debug.Log($"ใช้สกิล (ธาตุไฟ) โจมตีแบบสุ่ม ดาเมจที่ออกคือ {finalDamage}");
            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }
        else if (skill.effectType == SkillData.SkillEffectType.SuperBurn && selectedPlayer.element == ElementType.Fire)
        {
            isPlayerTurn = false;
            SuperburnTurnsLeft = 3;
            ShowSkillEffectOnce(4);
            PlaySoundEffect(7);
            skillCooldowns[skill] = 6;
            Debug.Log("ศัตรูติดสถานะไฟ 3 เทิร์น!");
            // ไม่ใช้ isDamageBoosted กับสกิลนี้
             UpdateSkillButtons();
             StartCoroutine(DelayedEnemyTurn());
        }
        else if (skill.effectType == SkillData.SkillEffectType.SuperSuperFire && selectedPlayer.element == ElementType.Fire)
        {
            skillCooldowns[skill] = 6;
            ShowSkillEffectOnce(25);
            PlaySoundEffect(7);
            if (isDamageBoosted)
            {
                finalDamage *= 2;
                isDamageBoosted = false;
                Debug.Log("บัฟ x2 ทำงานกับสกิล 1 ดาเมจรวม = " + finalDamage);
            }
            isPlayerTurn = false;
            DamageEnemy(finalDamage);
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;

        }

        else if (skill.effectType == SkillData.SkillEffectType.SuperSmashFire && selectedPlayer.element == ElementType.Fire)
        {

            SuperSmashFire = true;
            isPlayerTurn = false;
            skillCooldowns[skill] = 7;
              ShowSkillEffectOnce(23);
              PlaySoundEffect(3);
            Debug.Log("ผู้เล่นโจมตีธรรมดาแรงขึ้น 5 เท่า");
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

        else if (skill.effectType == SkillData.SkillEffectType.ReduceWater100 && selectedPlayer.element == ElementType.Fire)
        {
            skillCooldowns[skill] = 8;
            if (enemyElement == ElementType.Water)
            {
                ReduceWater = 3;
            }
              ShowSkillEffectOnce(26);
            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }
        //////////////////////////Monster Water//////////////////////

        if (skill.effectType == SkillData.SkillEffectType.DamageWater && selectedPlayer.element == ElementType.Water) // สกิล 1 ธาตุน้ำ
        {
            if (enemyElement == ElementType.Fire)
            {
                finalDamage *= 2;
                Debug.Log("ศัตรูธาตุไฟ ดาเมจน้ำคูณ 2");
            }
            else if (enemyElement == ElementType.Earth)
            {
                finalDamage /= 2;
                Debug.Log("ศัตรูธาตุดิน ดาเมจน้ำหาร 2");
            }

            if (isElementalAttackBoosted)
            {
                finalDamage += finalDamage / 2;
                Debug.Log("ใช้การ์ดบัฟโจมตีธาตุแรงขึ้น 0.5");
            }
            if (isElementalAttackBoostedx2 > 0)
            {
                finalDamage *= 2;
                isElementalAttackBoostedx2--;
                Debug.Log("ใช้การ์ดบัฟโจมตีธาตุแรงขึ้น 2 เท่า");
            }
            if (ElementalAttackBoostedRandom > 0)
            {
                finalDamage = Mathf.RoundToInt(finalDamage * isElementalAttackBoostedRandom);
                ElementalAttackBoostedRandom--;
                Debug.Log($"ใช้การ์ดบัฟโจมตีธาตุ {isElementalAttackBoostedRandom} เท่า");
            }
            skillCooldowns[skill] = 3;
            isPlayerTurn = false;
            ShowSkillEffectOnce(27);
            PlaySoundEffect(8);
            DamageEnemy(finalDamage); // ✅ ตีศัตรู
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

        if (skill.effectType == SkillData.SkillEffectType.HealWater && selectedPlayer.element == ElementType.Water) //สกิล 2 ธาตุน้ำ
        {
            playerHP += 20;
            playerHP = Mathf.Clamp(playerHP, 0, selectedPlayer.maxHP);
            UpdatePlayerHPUI();

            skillCooldowns[skill] = 3; // คูลดาวน์ 3 เทิร์น
            ShowSkillEffectOnce(2);
            PlaySoundEffect(2);
            Debug.Log("ใช้สกิล Heal รักษา HP 20");
            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }
        if (skill.effectType == SkillData.SkillEffectType.WaterReducted && selectedPlayer.element == ElementType.Water) // สกิล 3 ธาตุน้ำ
        {
            enemyDamageReductionTurns = 3; // ลดดาเมจศัตรูครึ่งนึง 3 เทิร์น
            isEnemyDamageReduced = true;

            skillCooldowns[skill] = 5; // คูลดาวน์ 5 เทิร์น
            ShowSkillEffectOnce(10);
            StartCoroutine(ShakeEnemy());
            Debug.Log("ใช้สกิล 3 ลดดาเมจศัตรูครึ่งนึง 3 เทิร์น");
            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

        if (skill.effectType == SkillData.SkillEffectType.SuperWater && selectedPlayer.element == ElementType.Water)
        {
            if (enemyElement == ElementType.Fire)
            {
                finalDamage *= 2;
                Debug.Log("ศัตรูธาตุไฟ ดาเมจน้ำคูณ 2");
            }
            else if (enemyElement == ElementType.Earth)
            {
                finalDamage /= 2;
                Debug.Log("ศัตรูธาตุดิน ดาเมจน้ำหาร 2");
            }

            if (isElementalAttackBoosted)
            {
                finalDamage += finalDamage / 2;
                Debug.Log("ใช้การ์ดบัฟโจมตีธาตุแรงขึ้น 0.5");
            }
            if (isElementalAttackBoostedx2 > 0)
            {
                finalDamage *= 2;
                isElementalAttackBoostedx2--;
                Debug.Log("ใช้การ์ดบัฟโจมตีธาตุแรงขึ้น 2 เท่า");
            }
            if (ElementalAttackBoostedRandom > 0)
            {
                finalDamage = Mathf.RoundToInt(finalDamage * isElementalAttackBoostedRandom);
                ElementalAttackBoostedRandom--;
                Debug.Log($"ใช้การ์ดบัฟโจมตีธาตุ {isElementalAttackBoostedRandom} เท่า");
            }
            skillCooldowns[skill] = 5;
            isPlayerTurn = false;
            ShowSkillEffectOnce(28);
            PlaySoundEffect(8);
            DamageEnemy(finalDamage); // ✅ ตีศัตรู
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }


        if (skill.effectType == SkillData.SkillEffectType.SuperHealWater && selectedPlayer.element == ElementType.Water)
        {
            playerHP += 40;
            playerHP = Mathf.Clamp(playerHP, 0, selectedPlayer.maxHP);
            UpdatePlayerHPUI();

            skillCooldowns[skill] = 5; // คูลดาวน์ 5 เทิร์น
            ShowSkillEffectOnce(2);
            PlaySoundEffect(2);
            Debug.Log("ใช้สกิล Heal รักษา HP 40");
            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }


        if (skill.effectType == SkillData.SkillEffectType.WaterMaxHP && selectedPlayer.element == ElementType.Water)
        {
            selectedPlayer.maxHP += 50;
            playerHP = Mathf.Clamp(playerHP, 0, selectedPlayer.maxHP);
            UpdatePlayerHPUI();

            skillCooldowns[skill] = 15; // คูลดาวน์ 15 เทิร์น
            ShowSkillEffectOnce(2);
            PlaySoundEffect(2);
            Debug.Log("ใช้สกิลเพิ่ม maxHP 50");
            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }
        if (skill.effectType == SkillData.SkillEffectType.SuperWaterReduce100 && selectedPlayer.element == ElementType.Water)

        {
            superreduce = 2;

            UpdatePlayerHPUI();

            skillCooldowns[skill] = 8; // คูลดาวน์ 8 เทิร์น
            ShowSkillEffectOnce(10); 
            Debug.Log("กันดาเมจ 100% 2 เทิร์น");
            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

        if (skill.effectType == SkillData.SkillEffectType.WaterRegen && selectedPlayer.element == ElementType.Water)
        {
            playerHP += 10;
            playerHP = Mathf.Clamp(playerHP, 0, selectedPlayer.maxHP);
            UpdatePlayerHPUI();
            ShowSkillEffectOnce(2);
            regenwater = 5;

            skillCooldowns[skill] = 12; // คูลดาวน์ 5 เทิร์น

            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

        if (skill.effectType == SkillData.SkillEffectType.WaterBuffx2 && selectedPlayer.element == ElementType.Water)
        {

            WaterBuffx2 = 3;

            skillCooldowns[skill] = 8; // คูลดาวน์ 5 เทิร์น
             ShowSkillEffectOnce(29);
             PlaySoundEffect(3);
            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

       if (skill.effectType == SkillData.SkillEffectType.NerfEarth && selectedPlayer.element == ElementType.Water)
        {
            usedOnceSkills.Add(skill);
            if (enemyElement == ElementType.Earth)
            {
                NerfEarth = true;
            }
             ShowSkillEffectOnce(30);
             PlaySoundEffect(5);
            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }



        //////////////////Monster Wind///////////////////


        if (skill.effectType == SkillData.SkillEffectType.DamageWind && selectedPlayer.element == ElementType.Wind)//สกิล 1 ลม
        {

            if (enemyElement == ElementType.Earth)
            {
                finalDamage *= 2;
                Debug.Log("ศัตรูธาตุดิน โดนลมแรง ดาเมจคูณ 2");
            }
            else if (enemyElement == ElementType.Fire)
            {
                finalDamage /= 2;
                Debug.Log("ศัตรูธาตุไฟ ทนลมได้ ดาเมจหาร 2");
            }
            if (isElementalAttackBoosted)
            {
                finalDamage += finalDamage / 2;
                Debug.Log("ใช้การ์ดบัฟโจมตีธาตุแรงขึ้น 0.5");
            }
            if (isElementalAttackBoostedx2 > 0)
            {
                finalDamage *= 2;
                isElementalAttackBoostedx2--;
                Debug.Log("ใช้การ์ดบัฟโจมตีธาตุแรงขึ้น 2 เท่า");
            }
            if (ElementalAttackBoostedRandom > 0)
            {
                finalDamage = Mathf.RoundToInt(finalDamage * isElementalAttackBoostedRandom);
                ElementalAttackBoostedRandom--;
                Debug.Log($"ใช้การ์ดบัฟโจมตีธาตุ {isElementalAttackBoostedRandom} เท่า");
            }
            skillCooldowns[skill] = 3;
            ShowSkillEffectOnce(19);
            PlaySoundEffect(9);

            DamageEnemy(finalDamage);
            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

        if (skill.effectType == SkillData.SkillEffectType.DoubleWindDamage && selectedPlayer.element == ElementType.Wind)//สกิล 2 ลม
        {

            if (enemyElement == ElementType.Earth)
            {
                finalDamage *= 2;
                Debug.Log("ศัตรูธาตุดิน โดนลมแรง ดาเมจคูณ 2");
            }
            else if (enemyElement == ElementType.Fire)
            {
                finalDamage /= 2;
                Debug.Log("ศัตรูธาตุไฟ ทนลมได้ ดาเมจหาร 2");
            }
            if (isElementalAttackBoosted)
            {
                finalDamage += finalDamage / 2;
                Debug.Log("ใช้การ์ดบัฟโจมตีธาตุแรงขึ้น 0.5");
            }
            if (isElementalAttackBoostedx2 > 0)
            {
                finalDamage *= 2;
                isElementalAttackBoostedx2--;
                Debug.Log("ใช้การ์ดบัฟโจมตีธาตุแรงขึ้น 2 เท่า");
            }
            if (ElementalAttackBoostedRandom > 0)
            {
                finalDamage = Mathf.RoundToInt(finalDamage * isElementalAttackBoostedRandom);
                ElementalAttackBoostedRandom--;
                Debug.Log($"ใช้การ์ดบัฟโจมตีธาตุ {isElementalAttackBoostedRandom} เท่า");
            }

            skillCooldowns[skill] = 5;
            DamageEnemy(finalDamage);
            ShowSkillEffectOnce(1);
            PlaySoundEffect(9);
            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }
        if (skill.effectType == SkillData.SkillEffectType.ReflectWind && selectedPlayer.element == ElementType.Wind)//สกิล 3 ลม
        {
            reflectNextAttackWind = true;
            skillCooldowns[skill] = 5;
            ShowSkillEffectOnce(11);
            Debug.Log("ใช้สกิลลม: เตรียมสะท้อนดาเมจในเทิร์นถัดไป โดยไม่เสียเลือด");
            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

        if (skill.effectType == SkillData.SkillEffectType.SuperWind && selectedPlayer.element == ElementType.Wind)
        {

            if (enemyElement == ElementType.Earth)
            {
                finalDamage *= 2;
                Debug.Log("ศัตรูธาตุดิน โดนลมแรง ดาเมจคูณ 2");
            }
            else if (enemyElement == ElementType.Fire)
            {
                finalDamage /= 2;
                Debug.Log("ศัตรูธาตุไฟ ทนลมได้ ดาเมจหาร 2");
            }
            if (isElementalAttackBoosted)
            {
                finalDamage += finalDamage / 2;
                Debug.Log("ใช้การ์ดบัฟโจมตีธาตุแรงขึ้น 0.5");
            }
            if (isElementalAttackBoostedx2 > 0)
            {
                finalDamage *= 2;
                isElementalAttackBoostedx2--;
                Debug.Log("ใช้การ์ดบัฟโจมตีธาตุแรงขึ้น 2 เท่า");
            }
            if (ElementalAttackBoostedRandom > 0)
            {
                finalDamage = Mathf.RoundToInt(finalDamage * isElementalAttackBoostedRandom);
                ElementalAttackBoostedRandom--;
                Debug.Log($"ใช้การ์ดบัฟโจมตีธาตุ {isElementalAttackBoostedRandom} เท่า");
            }
            skillCooldowns[skill] = 5;
            ShowSkillEffectOnce(31);
            PlaySoundEffect(9);
            DamageEnemy(finalDamage);
            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

        if (skill.effectType == SkillData.SkillEffectType.SuperSuperWind && selectedPlayer.element == ElementType.Wind)
        {

            if (enemyElement == ElementType.Earth)
            {
                finalDamage *= 2;
                Debug.Log("ศัตรูธาตุดิน โดนลมแรง ดาเมจคูณ 2");
            }
            else if (enemyElement == ElementType.Fire)
            {
                finalDamage /= 2;
                Debug.Log("ศัตรูธาตุไฟ ทนลมได้ ดาเมจหาร 2");
            }
            if (isElementalAttackBoosted)
            {
                finalDamage += finalDamage / 2;
                Debug.Log("ใช้การ์ดบัฟโจมตีธาตุแรงขึ้น 0.5");
            }
            if (isElementalAttackBoostedx2 > 0)
            {
                finalDamage *= 2;
                isElementalAttackBoostedx2--;
                Debug.Log("ใช้การ์ดบัฟโจมตีธาตุแรงขึ้น 2 เท่า");
            }
            if (ElementalAttackBoostedRandom > 0)
            {
                finalDamage = Mathf.RoundToInt(finalDamage * isElementalAttackBoostedRandom);
                ElementalAttackBoostedRandom--;
                Debug.Log($"ใช้การ์ดบัฟโจมตีธาตุ {isElementalAttackBoostedRandom} เท่า");
            }
            skillCooldowns[skill] = 6;
            ShowSkillEffectOnce(32);
            PlaySoundEffect(9);
            DamageEnemy(finalDamage);
            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

        if (skill.effectType == SkillData.SkillEffectType.SuperSuperSuperWind && selectedPlayer.element == ElementType.Wind)
        {

            if (enemyElement == ElementType.Earth)
            {
                finalDamage *= 2;
                Debug.Log("ศัตรูธาตุดิน โดนลมแรง ดาเมจคูณ 2");
            }
            else if (enemyElement == ElementType.Fire)
            {
                finalDamage /= 2;
                Debug.Log("ศัตรูธาตุไฟ ทนลมได้ ดาเมจหาร 2");
            }
            if (isElementalAttackBoosted)
            {
                finalDamage += finalDamage / 2;
                Debug.Log("ใช้การ์ดบัฟโจมตีธาตุแรงขึ้น 0.5");
            }
            if (isElementalAttackBoostedx2 > 0)
            {
                finalDamage *= 2;
                isElementalAttackBoostedx2--;
                Debug.Log("ใช้การ์ดบัฟโจมตีธาตุแรงขึ้น 2 เท่า");
            }
            if (ElementalAttackBoostedRandom > 0)
            {
                finalDamage = Mathf.RoundToInt(finalDamage * isElementalAttackBoostedRandom);
                ElementalAttackBoostedRandom--;
                Debug.Log($"ใช้การ์ดบัฟโจมตีธาตุ {isElementalAttackBoostedRandom} เท่า");
            }
            skillCooldowns[skill] = 8;
            ShowSkillEffectOnce(33);
            PlaySoundEffect(9);
            DamageEnemy(finalDamage);
            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

        if (skill.effectType == SkillData.SkillEffectType.DoubleReflectWind && selectedPlayer.element == ElementType.Wind)
        {
            doublereflectNextAttackWind = 2;
            skillCooldowns[skill] = 6;
            ShowSkillEffectOnce(11);
            Debug.Log("ใช้สกิลลม: เตรียมสะท้อนดาเมจ2เทิร์น โดยไม่เสียเลือด");
            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

        if (skill.effectType == SkillData.SkillEffectType.WindBootSPD && selectedPlayer.element == ElementType.Wind)
        {
            usedOnceSkills.Add(skill);
            ShowSkillEffectOnce(34);
            PlaySoundEffect(6);
            selectedPlayer.speed *= 2;

            Debug.Log($"ใช้สกิล ความเร็ว {selectedPlayer.speed}");
            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

        if (skill.effectType == SkillData.SkillEffectType.WindWalk && selectedPlayer.element == ElementType.Wind)
        {
            evadeTurnsLeft = 3;
            skillCooldowns[skill] = 8;

            Debug.Log("ใช้สกิลที่ 3: หลบการโจมตี 50% เป็นเวลา 3 เทิร์น");
            isPlayerTurn = false;
            ShowSkillEffectOnce(34);
            PlaySoundEffect(6);
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }


        else if (skill.effectType == SkillData.SkillEffectType.ReduceFire100 && selectedPlayer.element == ElementType.Wind)
        {
            skillCooldowns[skill] = 8;
            if (enemyElement == ElementType.Fire)
            {
                ReduceFire = 3;
            }
            isPlayerTurn = false;
            ShowSkillEffectOnce(11);
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }


        ////////////////////////////Monster Earth//////////////////////

        if (skill.effectType == SkillData.SkillEffectType.DamageEarth && selectedPlayer.element == ElementType.Earth)//สกิล 1 ดิน
        {
            if (enemyElement == ElementType.Water)
            {
                finalDamage *= 2;
                Debug.Log("ศัตรูธาตุน้ำ โดนดินแรง ดาเมจคูณ 2");
            }
            else if (enemyElement == ElementType.Wind)
            {
                finalDamage /= 2;
                Debug.Log("ศัตรูธาตุลม ทนดินได้ ดาเมจหาร 2");
            }
            if (isElementalAttackBoosted)
            {
                finalDamage += finalDamage / 2;
                Debug.Log("ใช้การ์ดบัฟโจมตีธาตุแรงขึ้น 0.5");
            }
            if (isElementalAttackBoostedx2 > 0)
            {
                finalDamage *= 2;
                isElementalAttackBoostedx2--;
                Debug.Log("ใช้การ์ดบัฟโจมตีธาตุแรงขึ้น 2 เท่า");
            }
            if (ElementalAttackBoostedRandom > 0)
            {
                finalDamage = Mathf.RoundToInt(finalDamage * isElementalAttackBoostedRandom);
                ElementalAttackBoostedRandom--;
                Debug.Log($"ใช้การ์ดบัฟโจมตีธาตุ {isElementalAttackBoostedRandom} เท่า");
            }

            skillCooldowns[skill] = 3;
            DamageEnemy(finalDamage);
            ShowSkillEffectOnce(22);
            PlaySoundEffect(10);
            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }
        if (skill.effectType == SkillData.SkillEffectType.ShieldEarth && selectedPlayer.element == ElementType.Earth)//สกิล 2 ดิน
        {
            isShieldActive = true;
            shieldTurnsLeft = 3;

            skillCooldowns[skill] = 8; // คูลดาวน์ 8 เทิร์น

            Debug.Log("ใช้สกิลโล่ดิน: ป้องกันดาเมจทุกประเภท 3 เทิร์น");
            ShowSkillEffectOnce(20);
            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }
        if (skill.effectType == SkillData.SkillEffectType.StunEarth && selectedPlayer.element == ElementType.Earth)//สกิล 3 ดิน
        {
            isEnemyStunned = true;
            enemyStunTurnsLeft = 3;

            skillCooldowns[skill] = 9; // คูลดาวน์ 6 เทิร์น

            Debug.Log("ใช้สกิล 3: สตั๊นศัตรู 3 เทิร์น!");
            isPlayerTurn = false;
            ShowSkillEffectOnce(21);
            PlaySoundEffect(5);
            StartCoroutine(ShakeEnemy());
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn()); // ไปเทิร์นศัตรู
            return;
        }

        if (skill.effectType == SkillData.SkillEffectType.SuperEath && selectedPlayer.element == ElementType.Earth)
        {
            if (enemyElement == ElementType.Water)
            {
                finalDamage *= 2;
                Debug.Log("ศัตรูธาตุน้ำ โดนดินแรง ดาเมจคูณ 2");
            }
            else if (enemyElement == ElementType.Wind)
            {
                finalDamage /= 2;
                Debug.Log("ศัตรูธาตุลม ทนดินได้ ดาเมจหาร 2");
            }
            if (isElementalAttackBoosted)
            {
                finalDamage += finalDamage / 2;
                Debug.Log("ใช้การ์ดบัฟโจมตีธาตุแรงขึ้น 0.5");
            }
            if (isElementalAttackBoostedx2 > 0)
            {
                finalDamage *= 2;
                isElementalAttackBoostedx2--;
                Debug.Log("ใช้การ์ดบัฟโจมตีธาตุแรงขึ้น 2 เท่า");
            }
            if (ElementalAttackBoostedRandom > 0)
            {
                finalDamage = Mathf.RoundToInt(finalDamage * isElementalAttackBoostedRandom);
                ElementalAttackBoostedRandom--;
                Debug.Log($"ใช้การ์ดบัฟโจมตีธาตุ {isElementalAttackBoostedRandom} เท่า");
            }

            skillCooldowns[skill] = 5;
            DamageEnemy(finalDamage);
            ShowSkillEffectOnce(22);
            ShowSkillEffectOnce(35);
            PlaySoundEffect(10);
            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

        if (skill.effectType == SkillData.SkillEffectType.EarthNerfDamage && selectedPlayer.element == ElementType.Earth)
        {
            EarthNerfDamage = 3;

            skillCooldowns[skill] = 8;
            ShowSkillEffectOnce(36);
            PlaySoundEffect(5);
            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

        if (skill.effectType == SkillData.SkillEffectType.EarthBootDef && selectedPlayer.element == ElementType.Earth)
        {
            EarthBootDef = 1;

            if (EarthBootDef > 0)
            {
                selectedPlayer.def *= 2;
                EarthBootDef = 0;
            }
            usedOnceSkills.Add(skill);
            ShowSkillEffectOnce(37);
            PlaySoundEffect(4);
            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

        if (skill.effectType == SkillData.SkillEffectType.EarthNerfSPD && selectedPlayer.element == ElementType.Earth)
        {
            EarthNerfSPD = true;

            if (EarthNerfSPD)
            {
                enemySpeed = Mathf.RoundToInt(enemySpeed * 0.5f);
                EarthNerfSPD = false;
            }
            usedOnceSkills.Add(skill);
            ShowSkillEffectOnce(38);
            PlaySoundEffect(5);
            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

        if (skill.effectType == SkillData.SkillEffectType.SuperSmashEarth && selectedPlayer.element == ElementType.Earth)
        {
            SuperSmashEarthLeft = 3;

            skillCooldowns[skill] = 8;
            ShowSkillEffectOnce(37);
            PlaySoundEffect(3);
            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

        if (skill.effectType == SkillData.SkillEffectType.EarthSmashEarth && selectedPlayer.element == ElementType.Earth)
        {
            EarthSmashEarthLeft = true;

            skillCooldowns[skill] = 6;
            ShowSkillEffectOnce(37);
            PlaySoundEffect(3);
            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

        if (skill.effectType == SkillData.SkillEffectType.ReduceWind100 && selectedPlayer.element == ElementType.Earth)
        {
            skillCooldowns[skill] = 8;
            if (enemyElement == ElementType.Wind)
            {
                ReduceWind = 3;
            }
            isPlayerTurn = false;
             ShowSkillEffectOnce(20);
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

        ////////////////////Monster Dark//////////////////////
        if (skill.effectType == SkillData.SkillEffectType.DamageDark && selectedPlayer.element == ElementType.Dark)//สกิล 1 มืด
        {
            if (enemyElement == ElementType.Light)
            {
                finalDamage *= 2;
                Debug.Log("ศัตรูธาตุแสง โดนธาตุมืดแรง ดาเมจคูณ 2");
            }
            if (isElementalAttackBoosted)
            {
                finalDamage += finalDamage / 2;
                Debug.Log("ใช้การ์ดบัฟโจมตีธาตุแรงขึ้น 0.5");
            }
            if (isElementalAttackBoostedx2 > 0)
            {
                finalDamage *= 2;
                isElementalAttackBoostedx2--;
                Debug.Log("ใช้การ์ดบัฟโจมตีธาตุแรงขึ้น 2 เท่า");
            }
            if (ElementalAttackBoostedRandom > 0)
            {
                finalDamage = Mathf.RoundToInt(finalDamage * isElementalAttackBoostedRandom);
                ElementalAttackBoostedRandom--;
                Debug.Log($"ใช้การ์ดบัฟโจมตีธาตุ {isElementalAttackBoostedRandom} เท่า");
            }

            skillCooldowns[skill] = 3;
            DamageEnemy(finalDamage);
            ShowSkillEffectOnce(15);
            PlaySoundEffect(11);
            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

        if (skill.effectType == SkillData.SkillEffectType.DamageRandomDark && selectedPlayer.element == ElementType.Dark) // สกิลที่ 2 ของธาตุมืด
        {
            int randomDamage = Random.Range(20, 61); // สุ่ม 20 ถึง 60
            finalDamage = randomDamage;

            if (enemyElement == ElementType.Light)
            {
                finalDamage *= 2;
                Debug.Log("ศัตรูธาตุแสง โดนดาเมจธาตุมืดคูณ 2");
            }
            if (isElementalAttackBoosted)
            {
                finalDamage += finalDamage / 2;
                Debug.Log("ใช้การ์ดบัฟโจมตีธาตุแรงขึ้น 0.5");
            }
            if (isElementalAttackBoostedx2 > 0)
            {
                finalDamage *= 2;
                isElementalAttackBoostedx2--;
                Debug.Log("ใช้การ์ดบัฟโจมตีธาตุแรงขึ้น 2 เท่า");
            }
            if (ElementalAttackBoostedRandom > 0)
            {
                finalDamage = Mathf.RoundToInt(finalDamage * isElementalAttackBoostedRandom);
                ElementalAttackBoostedRandom--;
                Debug.Log($"ใช้การ์ดบัฟโจมตีธาตุ {isElementalAttackBoostedRandom} เท่า");
            }
            DamageEnemy(finalDamage);
            skillCooldowns[skill] = 5;
            ShowSkillEffectOnce(18);
            PlaySoundEffect(11);
            Debug.Log($"ใช้สกิลที่ 2 (ธาตุมืด) โจมตีแบบสุ่ม ดาเมจที่ออกคือ {finalDamage}");
            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }
        if (skill.effectType == SkillData.SkillEffectType.EyeDark && selectedPlayer.element == ElementType.Dark) // สกิล 3 ธาตุมืด
        {
            evadeTurnsLeft = 5;
            skillCooldowns[skill] = 8;

            Debug.Log("ใช้สกิลที่ 3: หลบการโจมตี 50% เป็นเวลา 5 เทิร์น");
            isPlayerTurn = false;
            ShowSkillEffectOnce(16);
            PlaySoundEffect(6);
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

        if (skill.effectType == SkillData.SkillEffectType.SuperDark && selectedPlayer.element == ElementType.Dark)
        {
            if (enemyElement == ElementType.Light)
            {
                finalDamage *= 2;
                Debug.Log("ศัตรูธาตุแสง โดนธาตุมืดแรง ดาเมจคูณ 2");
            }
            if (isElementalAttackBoosted)
            {
                finalDamage += finalDamage / 2;
                Debug.Log("ใช้การ์ดบัฟโจมตีธาตุแรงขึ้น 0.5");
            }
            if (isElementalAttackBoostedx2 > 0)
            {
                finalDamage *= 2;
                isElementalAttackBoostedx2--;
                Debug.Log("ใช้การ์ดบัฟโจมตีธาตุแรงขึ้น 2 เท่า");
            }
            if (ElementalAttackBoostedRandom > 0)
            {
                finalDamage = Mathf.RoundToInt(finalDamage * isElementalAttackBoostedRandom);
                ElementalAttackBoostedRandom--;
                Debug.Log($"ใช้การ์ดบัฟโจมตีธาตุ {isElementalAttackBoostedRandom} เท่า");
            }

            skillCooldowns[skill] = 5;
            DamageEnemy(finalDamage);
            ShowSkillEffectOnce(15);
            ShowSkillEffectOnce(39);
            PlaySoundEffect(11);
            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

        if (skill.effectType == SkillData.SkillEffectType.SuperRandomDark && selectedPlayer.element == ElementType.Dark)
        {
            int randomDamage = Random.Range(30, 81); // สุ่ม 30 ถึง 80
            finalDamage = randomDamage;

            if (enemyElement == ElementType.Light)
            {
                finalDamage *= 2;
                Debug.Log("ศัตรูธาตุแสง โดนดาเมจธาตุมืดคูณ 2");
            }
            if (isElementalAttackBoosted)
            {
                finalDamage += finalDamage / 2;
                Debug.Log("ใช้การ์ดบัฟโจมตีธาตุแรงขึ้น 0.5");
            }
            if (isElementalAttackBoostedx2 > 0)
            {
                finalDamage *= 2;
                isElementalAttackBoostedx2--;
                Debug.Log("ใช้การ์ดบัฟโจมตีธาตุแรงขึ้น 2 เท่า");
            }
            if (ElementalAttackBoostedRandom > 0)
            {
                finalDamage = Mathf.RoundToInt(finalDamage * isElementalAttackBoostedRandom);
                ElementalAttackBoostedRandom--;
                Debug.Log($"ใช้การ์ดบัฟโจมตีธาตุ {isElementalAttackBoostedRandom} เท่า");
            }
            DamageEnemy(finalDamage);
            skillCooldowns[skill] = 7;
            ShowSkillEffectOnce(18);
            ShowSkillEffectOnce(40);
            PlaySoundEffect(11);
            Debug.Log($"ใช้สกิลที่ 2 (ธาตุมืด) โจมตีแบบสุ่ม ดาเมจที่ออกคือ {finalDamage}");
            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

        if (skill.effectType == SkillData.SkillEffectType.LuckyBadLuckDark && selectedPlayer.element == ElementType.Dark)
        {
            int randomDamage = Random.Range(10, 101); // สุ่ม 10 ถึง 100
            finalDamage = randomDamage;

            if (enemyElement == ElementType.Light)
            {
                finalDamage *= 2;
                Debug.Log("ศัตรูธาตุแสง โดนดาเมจธาตุมืดคูณ 2");
            }
            if (isElementalAttackBoosted)
            {
                finalDamage += finalDamage / 2;
                Debug.Log("ใช้การ์ดบัฟโจมตีธาตุแรงขึ้น 0.5");
            }
            if (isElementalAttackBoostedx2 > 0)
            {
                finalDamage *= 2;
                isElementalAttackBoostedx2--;
                Debug.Log("ใช้การ์ดบัฟโจมตีธาตุแรงขึ้น 2 เท่า");
            }
            if (ElementalAttackBoostedRandom > 0)
            {
                finalDamage = Mathf.RoundToInt(finalDamage * isElementalAttackBoostedRandom);
                ElementalAttackBoostedRandom--;
                Debug.Log($"ใช้การ์ดบัฟโจมตีธาตุ {isElementalAttackBoostedRandom} เท่า");
            }
            DamageEnemy(finalDamage);
            skillCooldowns[skill] = 7;
            ShowSkillEffectOnce(18);
             ShowSkillEffectOnce(15);
             PlaySoundEffect(11);
            Debug.Log($"ใช้สกิลที่ 2 (ธาตุมืด) โจมตีแบบสุ่ม ดาเมจที่ออกคือ {finalDamage}");
            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

        if (skill.effectType == SkillData.SkillEffectType.SuperEyeDark && selectedPlayer.element == ElementType.Dark)
        {
            SuperevadeTurnsLeft = 5;
            skillCooldowns[skill] = 8;

            Debug.Log("ใช้สกิลที่ 3: หลบการโจมตี 50% เป็นเวลา 5 เทิร์น");
            isPlayerTurn = false;
            ShowSkillEffectOnce(16);
            PlaySoundEffect(6);
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

        if (skill.effectType == SkillData.SkillEffectType.BootDarkSpeed && selectedPlayer.element == ElementType.Dark)
        {
            usedOnceSkills.Add(skill);
            ShowSkillEffectOnce(11);

            selectedPlayer.speed *= 2;

            Debug.Log($"ใช้สกิล ความเร็ว {selectedPlayer.speed}");
            isPlayerTurn = false;
            UpdateSkillButtons();
            ShowSkillEffectOnce(41);
            PlaySoundEffect(3);
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

        if (skill.effectType == SkillData.SkillEffectType.BootDamageDark && selectedPlayer.element == ElementType.Dark)
        {
            skillCooldowns[skill] = 7;
            ShowSkillEffectOnce(42);
            PlaySoundEffect(3);
            BootDamageDark = 2;


            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

        if (skill.effectType == SkillData.SkillEffectType.ReduceLight50 && selectedPlayer.element == ElementType.Dark)
        {
            skillCooldowns[skill] = 8;
            if (enemyElement == ElementType.Light)
            {
                ReduceLight = 3;
            }
            isPlayerTurn = false;
             ShowSkillEffectOnce(43);
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

        //////////////////////////Monster Light/////////////////////


        if (skill.effectType == SkillData.SkillEffectType.DamageLight && selectedPlayer.element == ElementType.Light)//สกิล 1 แสง
        {
            if (enemyElement == ElementType.Dark)
            {
                finalDamage *= 2;
                Debug.Log("ศัตรูธาตุแสง โดนธาตุมืดแรง ดาเมจคูณ 2");
            }
            if (isElementalAttackBoosted)
            {
                finalDamage += finalDamage / 2;
                Debug.Log("ใช้การ์ดบัฟโจมตีแรงขึ้น 0.5");
            }
            if (isElementalAttackBoostedx2 > 0)
            {
                finalDamage *= 2;
                isElementalAttackBoostedx2--;
                Debug.Log("ใช้การ์ดบัฟโจมตีแรงขึ้น 2 เท่า");
            }
            if (ElementalAttackBoostedRandom > 0)
            {
                finalDamage = Mathf.RoundToInt(finalDamage * isElementalAttackBoostedRandom);
                ElementalAttackBoostedRandom--;
                Debug.Log($"ใช้การ์ดบัฟโจมตี {isElementalAttackBoostedRandom} เท่า");
            }

            skillCooldowns[skill] = 3;
            DamageEnemy(finalDamage);
            ShowSkillEffectOnce(44);
            PlaySoundEffect(12);
            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }
        if (skill.effectType == SkillData.SkillEffectType.Buffx2Light && selectedPlayer.element == ElementType.Light) // สกิล 2 ธาตุแสง
        {
            lightBuffTurnsLeft = 3;
            skillCooldowns[skill] = 8;
            ShowSkillEffectOnce(13);
            PlaySoundEffect(3);
            Debug.Log("ใช้สกิล 2: บัฟดาเมจคูณ 2 เป็นเวลา 3 เทิร์น");
            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }
        if (skill.effectType == SkillData.SkillEffectType.HealandShield && selectedPlayer.element == ElementType.Light) // สกิล 3 ธาตุแสง
        {
            // Heal 30% ของ maxHP
            int healAmount = Mathf.FloorToInt(selectedPlayer.maxHP * 0.3f);
            playerHP += healAmount;
            playerHP = Mathf.Clamp(playerHP, 0, selectedPlayer.maxHP);
            UpdatePlayerHPUI();
            Debug.Log($"ใช้สกิล 3 ฮีลเลือด {healAmount} HP");
            ShowSkillEffectOnce(12);
            PlaySoundEffect(2);
            // เปิดโล่ ลดดาเมจ 50% เป็นเวลา 3 เทิร์น
            lightShieldTurnsLeft = 3;
            Debug.Log("เปิดโล่ป้องกัน 50% เป็นเวลา 3 เทิร์น");

            skillCooldowns[skill] = 9;
            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

        if (skill.effectType == SkillData.SkillEffectType.SuperLight && selectedPlayer.element == ElementType.Light)
        {
            if (enemyElement == ElementType.Dark)
            {
                finalDamage *= 2;
                Debug.Log("ศัตรูธาตุแสง โดนธาตุมืดแรง ดาเมจคูณ 2");
            }
            if (isElementalAttackBoosted)
            {
                finalDamage += finalDamage / 2;
                Debug.Log("ใช้การ์ดบัฟโจมตีแรงขึ้น 0.5");
            }
            if (isElementalAttackBoostedx2 > 0)
            {
                finalDamage *= 2;
                isElementalAttackBoostedx2--;
                Debug.Log("ใช้การ์ดบัฟโจมตีแรงขึ้น 2 เท่า");
            }
            if (ElementalAttackBoostedRandom > 0)
            {
                finalDamage = Mathf.RoundToInt(finalDamage * isElementalAttackBoostedRandom);
                ElementalAttackBoostedRandom--;
                Debug.Log($"ใช้การ์ดบัฟโจมตี {isElementalAttackBoostedRandom} เท่า");
            }

            skillCooldowns[skill] = 3;
            DamageEnemy(finalDamage);
            ShowSkillEffectOnce(44);
            PlaySoundEffect(12);
            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

        if (skill.effectType == SkillData.SkillEffectType.HealLight && selectedPlayer.element == ElementType.Light)
        {
            // Heal 30% ของ maxHP
            int healAmount = Mathf.FloorToInt(selectedPlayer.maxHP * 0.5f);
            playerHP += healAmount;
            playerHP = Mathf.Clamp(playerHP, 0, selectedPlayer.maxHP);
            UpdatePlayerHPUI();
            Debug.Log($"ใช้สกิล ฮีลเลือด {healAmount} HP");
            ShowSkillEffectOnce(12);
            PlaySoundEffect(2);
            skillCooldowns[skill] = 15;
            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

        if (skill.effectType == SkillData.SkillEffectType.SuperHealLight && selectedPlayer.element == ElementType.Light)
        {
            // Heal 100% ของ maxHP
            int healAmount = Mathf.FloorToInt(selectedPlayer.maxHP * 1);
            playerHP += healAmount;
            playerHP = Mathf.Clamp(playerHP, 0, selectedPlayer.maxHP);
            UpdatePlayerHPUI();
            Debug.Log($"ใช้สกิล ฮีลเลือด {healAmount} HP");
            ShowSkillEffectOnce(12);
            PlaySoundEffect(2);
            usedOnceSkills.Add(skill);
            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

        if (skill.effectType == SkillData.SkillEffectType.bootDamageLight && selectedPlayer.element == ElementType.Light)
        {

            Buffx3Light = 2;

            skillCooldowns[skill] = 8;
            isPlayerTurn = false;
             ShowSkillEffectOnce(13);
             PlaySoundEffect(3);
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

        if (skill.effectType == SkillData.SkillEffectType.SuperShieldLight && selectedPlayer.element == ElementType.Light)

        {
            superreducelight = 3;

            UpdatePlayerHPUI();

            skillCooldowns[skill] = 9; // คูลดาวน์ 9 เทิร์น
            
            Debug.Log("กันดาเมจ 100% 3 เทิร์น");
            isPlayerTurn = false;
             ShowSkillEffectOnce(12);
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }


        if (skill.effectType == SkillData.SkillEffectType.NerfEnemyDamge && selectedPlayer.element == ElementType.Light)

        {
            NerfEnemyDamgelight = 2;

            UpdatePlayerHPUI();

            skillCooldowns[skill] = 8; // คูลดาวน์ 9 เทิร์น
            ShowSkillEffectOnce(45); 
            PlaySoundEffect(5);
            isPlayerTurn = false;
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }
        
        if (skill.effectType == SkillData.SkillEffectType.NerfEnemyDark && selectedPlayer.element == ElementType.Light)
        {
            usedOnceSkills.Add(skill);
            if (enemyElement == ElementType.Dark)
            {
                NerfDark = true;
            }
            isPlayerTurn = false;
            ShowSkillEffectOnce(46); 
            PlaySoundEffect(5);
            UpdateSkillButtons();
            StartCoroutine(DelayedEnemyTurn());
            return;
        }

    }

    void DamageNormalEnemy(int damagenormal)
    {
         playerturntext.gameObject.SetActive(false);
    enemyturntext.gameObject.SetActive(true);
        int damageN = damagenormal;

        if (SuperSmashFire)
        {
            damageN *= 5;
            SuperSmashFire = false;
         }

        if (SuperSmashEarthLeft > 0)
        {
            damageN *= 2;
            SuperSmashEarthLeft--;
         }

        if (EarthSmashEarthLeft)
        {
            damageN *= 5;
            EarthSmashEarthLeft = false;
        }
        
      PlaySoundEffect(0);

         finaldamgedef = Mathf.RoundToInt(damageN * (100f / (100f + enemyDef)));
        damageN = finaldamgedef;
        enemyHP -= damageN;
        //enemyHP -= finalWaterDamage;
        enemyHP = Mathf.Clamp(enemyHP, 0, enemyMaxHP);
        ShowSkillEffectOnce(8);
        StartCoroutine(ShakeEnemy()); // ศัตรูสั่น
        ShowDamageNumber($"-{damageN}"); // แสดงเลขดาเมจ
        UpdateEnemyHPUI();

        if (enemyHP <= 0)
        {
            GiveExpToPlayer();
            OpenChest();
            Debug.Log("ศัตรูแพ้แล้ว!");
            ShowResultPanelVictory("Victory!");
        }
    }
public PlayerState player;
    public void GiveExpToPlayer()
    {
        int expReward = 700; // จำนวน EXP ที่ต้องการให้
        
        if (player != null)
        {
            player.GainExp(expReward);
            Debug.Log($"ได้รับ EXP {expReward} หน่วย!");
        }
    }

    void DamageEnemy(int damage) //<-- ดาเมจผู้เล่น
    {
           playerturntext.gameObject.SetActive(false);
    enemyturntext.gameObject.SetActive(true);
        int finalDamage = damage;

         playerdamageItems = 0;
        if (knightswords)
        {
            playerdamageItems += Mathf.RoundToInt(finalDamage* 0.05f);
        }
        if (lightrings)
        {
            playerdamageItems += Mathf.RoundToInt(finalDamage* 0.05f);
        }
        if (lightspears)
        {
            playerdamageItems += Mathf.RoundToInt(finalDamage* 0.1f);
        }
        if (firedaggers)
        {
            playerdamageItems += Mathf.RoundToInt(finalDamage* 0.07f);
        }
        if (firelegendaryswords)
        {
            playerdamageItems += Mathf.RoundToInt(finalDamage* 0.1f);
        }
        if (windswords)
        {
            playerdamageItems += Mathf.RoundToInt(finalDamage* 0.07f);
        }
        if (windspears)
        {
             playerdamageItems += Mathf.RoundToInt(finalDamage* 0.1f);
        }
        if (darkdaggers)
        {
              playerdamageItems += Mathf.RoundToInt(finalDamage* 0.1f);
        }
        if (darkrings)
        {
            int healAmount = Mathf.RoundToInt(finalDamage* 0.1f);
            playerHP += healAmount;
            playerHP = Mathf.Clamp(playerHP, 0, selectedPlayer.maxHP);
            UpdatePlayerHPUI();
        }
        if (darklegendaryrings)
        {
            int healAmount = Mathf.RoundToInt(finalDamage* 0.3f);
            playerHP += healAmount;
            playerHP = Mathf.Clamp(playerHP, 0, selectedPlayer.maxHP);
            UpdatePlayerHPUI();
        }
        if (darklegendarydaggers)
        {
             playerdamageItems += Mathf.RoundToInt(finalDamage* 0.2f);
        }

        finalDamage += playerdamageItems;
        ///card and item
        //int finalWaterDamage = damage;
         if (lightBuffTurnsLeft > 0)//สกิล 2 แสง
    {
        finalDamage *= 2;
        Debug.Log("บัฟดาเมจธาตุแสงทำงาน! ดาเมจ x2 = " + finalDamage);
    }

        if (isDamageBoosted) //สกิล 3 ไฟ
        {
            finalDamage *= 2;
            Debug.Log("บัฟดาเมจคูณ 2 ถูกใช้! ดาเมจรวม: " + finalDamage);
        }
        if (attackMultiplierTurn)//การ์ดเพิ่มดาเมจ *1.5 แต่ลดเลือดครึ่งนึง
        {
            finalDamage = Mathf.RoundToInt(finalDamage * attackMultiplier);
             Debug.Log("บัฟดาเมจคูณ 2 ถูกใช้!แต่ลดเลือดครึ่งนึง");
        }
        if (AttackBoostRandomRange)
        {
            int bonus = Random.Range(10, 51);
            finalDamage += bonus;
            Debug.Log($"การ์ดเพิ่มพลังโจมตีแบบสุ่ม +{bonus}");
        }
          if (doubleAttackTurnsLeft > 0)//การ์ด *2 โจมตีธรรมดา
        {
            doubleAttackTurnsLeft--;
            Debug.Log("บัฟโจมตีธรรมดาคูณ 2 ทำงาน! ดาเมจรวม = " + selectedPlayer.attackDamage);
        }
    
        if (SuperBoostAttackTurn > 0)//การ์ดเพิ่มพลังโจมตีแบบสุ่ม 3-5 เท่า 1 เทิร์น
        {
            finalDamage *= AttackBoosted;
            SuperBoostAttackTurn--;
            Debug.Log("บัฟโจมตีแบบสุ่มทำงาน! ดาเมจ =" + finalDamage);
        }
        if (RandomBootDamageTurn > 0)// การ์ดสุุ่มเทิร์นดาเมจเพิ่ม *0.5
        {
            finalDamage += finalDamage / 2;
            RandomBootDamageTurn--;
             Debug.Log("บัฟโจมตีแบบสุ่มเทิร์นทำงาน!เทิร์นที่เหลือ ="+ RandomBootDamageTurn);
     }
        if (isElementalBuffPerTurnActive)//การ์ดเพิ่มดาเมจ 0.5 ทุกเทิร์น 
        {
            finalDamage = Mathf.RoundToInt(finalDamage + elementalBuffPerTurnValue);
            Debug.Log($"บัฟพลังโจมตีสะสมทำงาน!ดาเมจ = {finalDamage}");
        }

        if (doubleAttackandloseTurnsLeft > 0)//การ์ดโจมตี 2 เท่าแต่ลดเลือด 10%
        {
            finalDamage *= 2;
            int loss = Mathf.RoundToInt(selectedPlayer.maxHP * 0.1f);
            playerHP -= loss;
            playerHP = Mathf.Clamp(playerHP, 0, selectedPlayer.maxHP);
            Debug.Log($"เสีย HP 10% จาก maxHP = {loss}");
            UpdatePlayerHPUI();
            doubleAttackandloseTurnsLeft--;
        }
        if (isHalfDamageAll)//การ์ดลดดาเมจครึ่งนึงแต่เพิ่มเลือด 2 เท่า
            {
                finalDamage /= 2;
                Debug.Log("บัฟลดดาเมจทุกชนิดครึ่งนึง ทำงาน! ดาเมจใหม่: " + finalDamage);
            }
        if (checkReduceAttackHealFull)//การ์ด
        {
            finalDamage -= Mathf.RoundToInt(finalDamage * 0.1f);
            Debug.Log("เพิ่มเลือดจนเต็มแต่ลดดาเมจเหลือ : " + finalDamage);
        }
        if (healOnAttackTurnsLeft > 0)//การ์ด
        {
            finalDamage = Mathf.RoundToInt(finalDamage * 0.5f);
            int AttackhealAmount = Mathf.RoundToInt(selectedPlayer.maxHP * 0.1f);
            playerHP += AttackhealAmount;
            playerHP = Mathf.Clamp(playerHP, 0, selectedPlayer.maxHP);
            ShowSkillEffectOnce(2); 
            UpdatePlayerHPUI();
            Debug.Log($"ฟื้น HP {AttackhealAmount} จากการ์ดเมื่อโจมตี");

            healOnAttackTurnsLeft--;

        }
        if (confuseEnemyTurns > 0)//การ์ด
        {
            finalDamage *= 2;
            confuseEnemyTurns--;
        }
        if (ultraDamageTurns > 0)
        {
            finalDamage *= 5;
            ultraDamageTurns--;
        }
        if (isCursedAttack)
        {
            int cursehp = Mathf.RoundToInt(selectedPlayer.maxHP * 0.05f);
            playerHP -= cursehp;
            playerHP = Mathf.Clamp(playerHP, 0, selectedPlayer.maxHP);
            UpdatePlayerHPUI();
            finalDamage *= 2;
        }
        if (WaterBuffx2 > 0)
        {
            finalDamage *= 2;
            WaterBuffx2--;
        }
        if (BootDamageDark > 0)
        {
            finalDamage *= 2;
            BootDamageDark--;
        }

        if (Buffx3Light > 0)
        {
            finalDamage *= 3;
            Buffx3Light--;
        }
        if (EnemyShieldWater > 0)
        {
            finalDamage = Mathf.RoundToInt(finalDamage * 0.5f);
            EnemyShieldWater--;
        }
        if (EnemyShieldEarth > 0)
        {
            finalDamage = 0;
            EnemyShieldEarth--;
        }
        if (EnemyDarkWalk > 0)
        {
            int roll = Random.Range(0, 100);

            if (roll < 50)
            {
                Debug.Log("Player โจมตีพลาด! (หลบสำเร็จ)");
                return;
            }
            else
            {
                Debug.Log("Player โจมตีโดน! (ไม่หลบ)");
            }

        }
        if (EnemyReflectWind > 0)
        {
            DamagePlayer(finalDamage);
            return;
        }
        if (EnemyShieldWater100 > 0)
        {
            EnemyShieldWater100--;
            finalDamage = 0;

        }

        finaldamgedef = Mathf.RoundToInt(finalDamage * (100f / (100f + enemyDef)));
        finalDamage = finaldamgedef;
        enemyHP -= finalDamage;
        //enemyHP -= finalWaterDamage;
        enemyHP = Mathf.Clamp(enemyHP, 0, enemyMaxHP);
        StartCoroutine(ShakeEnemy()); // ศัตรูสั่น
        ShowDamageNumber($"-{finalDamage}"); // แสดงเลขดาเมจ
        UpdateEnemyHPUI();
         StartCoroutine(MyDelay());

        if (enemyHP <= 0)
        {
            GiveExpToPlayer();
            OpenChest();
            Debug.Log("ศัตรูแพ้แล้ว!");
            ShowResultPanelVictory("Victory!");
        }
    }
     public Sprite[] itemImages; 
    public Image showImage;
     public ItemID WaterArmor = ItemID.WaterArmor; // สมมติกล่องนี้ดรอปดาบไฟ
        public ItemID RegenRing = ItemID.RegenRing;
    public ItemID WaterNeckless = ItemID.WaterNeckless ;

    public void OpenChest()
    {
        // เรียกใช้คำสั่งปลดล็อก
         int roll = Random.Range(1, 101);
            
            if (roll < 21)
            {
                 EquipmentManager.Instance.UnlockItem(WaterArmor);
                 showImage.sprite = itemImages[0]; 
                 showImage.gameObject.SetActive(true);
            }
            else if (roll < 41 && roll > 20)
        {
             EquipmentManager.Instance.UnlockItem(RegenRing);
             showImage.sprite = itemImages[1]; 
                 showImage.gameObject.SetActive(true);
        }
          else if (roll < 61 && roll > 40)
        {
             EquipmentManager.Instance.UnlockItem(WaterNeckless);
             showImage.sprite = itemImages[2]; 
                 showImage.gameObject.SetActive(true);
        }

       
    }
    void DamagePlayer(int damage) //<-- ดาเมจศัตรู
    {
              playerturntext.gameObject.SetActive(true);
    enemyturntext.gameObject.SetActive(false);
       
        if (reflectNextAttackWind)//สกิล 3 ลม
        {
            reflectNextAttackWind = false;
            ShowSkillEffectOnce(11);
            int reflectedDamage = damage;
            DamageEnemy(reflectedDamage);

            Debug.Log($"สะท้อนดาเมจลม {reflectedDamage} กลับใส่ศัตรู และเราไม่เสียเลือด");
            return; // ✅ ไม่เสียเลือด
        }

        if (doublereflectNextAttackWind > 0)
        {
            doublereflectNextAttackWind--;
            ShowSkillEffectOnce(11);
            int reflectedDamage = damage;
            DamageEnemy(reflectedDamage);

            Debug.Log($"สะท้อนดาเมจลม {reflectedDamage} กลับใส่ศัตรู และเราไม่เสียเลือด");
            return; // ✅ ไม่เสียเลือด
        }

        if (isShieldActive)//สกิล 2 ดิน
        {
            ShowSkillEffectOnce(20);
            Debug.Log("โล่ดินป้องกันดาเมจ! ไม่เสียเลือด");
            return; // ✅ ไม่เสียเลือด
        }

        if (lightShieldTurnsLeft > 0) //สกิล 3 แสง
        {
            ShowSkillEffectOnce(12);
            damage = Mathf.FloorToInt(damage * 0.5f);
            Debug.Log($"โล่แสงลดดาเมจ 50%! ดาเมจที่ได้รับ = {damage}");
        }


        if (evadeTurnsLeft > 0 && isDodgeActive)//สกิล 3 มืด และการ์ดหลบหลีกทำงาน
        {
            int roll = Random.Range(0, 100);
           
            if (roll < 50)
            {
                Debug.Log("ศัตรูโจมตีพลาด! (หลบสำเร็จ)");
                return;
            }
            else
            {
                Debug.Log("ศัตรูโจมตีโดน! (ไม่หลบ)");
            }
        }
           else if (!isDodgeActive && evadeTurnsLeft > 0)//สกิล 3 มืด
    {
        int roll = Random.Range(0, 100);
           
            if (roll < 60)
            {
                Debug.Log("ศัตรูโจมตีพลาด! (หลบสำเร็จ)");
                return;
            }
            else
            {
                Debug.Log("ศัตรูโจมตีโดน! (ไม่หลบ)");
            }
    }
     else if (isDodgeActive && evadeTurnsLeft  <= 0)//การ์ดหลบหลีกศัตรู 10%
    {
        float roll = Random.Range(0f, 1f);
        if (roll < dodgeChance)
            {
                Debug.Log("หลบการโจมตีของศัตรูได้!");
                return; // ✅ ไม่เสียเลือด
            }
    }

        if (SuperevadeTurnsLeft > 0)
        {
            int roll = Random.Range(0, 100);
            if (roll < 70)
            {
                Debug.Log("ศัตรูโจมตีพลาด! (หลบสำเร็จ)");
                SuperevadeTurnsLeft--;
                return;
            }
            else
            {
                Debug.Log("ศัตรูโจมตีโดน! (ไม่หลบ)");
                 SuperevadeTurnsLeft--;
            }
            
    }
        if (isEnemyDamageReducedHalf > 0)//การ์ดมีโอกาสที่ศัตรูจะลดดาเมจมากสุด 80% แต่ก็มีโอกาสที่ศัตรูเพิ่มดาเมจ 30%
        {
            float randomreduced = Random.Range(0.2f, 1.3f);
            damage = Mathf.RoundToInt(damage * randomreduced);
            Debug.Log($"การ์ดแห่งโชคดาเมจศัตรู :{damage}");
        } 


         if (reflectNextAttack)//การ์ดสะท้อนดาเมจ *2
        {
            reflectNextAttack = false;

            int reflectedDamage = damage * 2;
            DamageEnemy(reflectedDamage);

            Debug.Log($"สะท้อนดาเมจ x2 = {reflectedDamage} ใส่ศัตรู และป้องกันดาเมจที่ได้รับ");

            return; // ✅ ไม่เสียเลือด
        }
        
      if (isEnemyDamageReduced)//การ์ดทำให้ศัตรูดาเมจลดครึ่งนึง
        {
            damage /= 2;
            Debug.Log($"สถานะลดดาเมจศัตรูลดดาเมจลงครึ่งนึง เหลือ {damage}");
        }
         if (isEnemyAttackReducedPermanently)//การ์ดลดดาเมจศัตรู 30%
    {
        damage = Mathf.RoundToInt(damage * 0.7f);
        Debug.Log($"ศัตรูโดนลดดาเมจถาวร: ดาเมจเหลือ {damage}");
    }
    if (confuseEnemyTurns > 0)//การ์ด
        {
            damage =  Mathf.RoundToInt(damage * 0.5f);
        }

        if (confuseHitSelfTurns>0)//การ์ดศัตรูตีตัวเอง
        {
            int roll = Random.Range(0, 100); // 0-99
            if (roll < 70)
            {
                // ตีตัวเอง
                enemyHP -= damage;
                enemyHP = Mathf.Clamp(enemyHP, 0, enemyMaxHP);
                Debug.Log($"ศัตรูสับสน! โจมตีตัวเอง {damage} ดาเมจ");
                UpdateEnemyHPUI();
                damage = 0;
            }
            else
            {
                damage = Mathf.RoundToInt(damage * 1.3f);
                Debug.Log("ศัตรูสับสนแต่โจมตีผู้เล่นแรงขึ้น 30%");
            }
            confuseHitSelfTurns--;
        }

        if (ReduceWater > 0)
        {
            damage = 0;
            ReduceWater--;
        }

        if (superreduce > 0)
        {
            damage = 0;
            superreduce--;
        }
        if (NerfEarth)
        {
            damage = Mathf.RoundToInt(damage * 0.8f);
        }
        if (ReduceFire > 0)
        {
            damage = 0;
            ReduceFire--;
        }
        if (EarthNerfDamage > 0)
        {
            damage = Mathf.RoundToInt(damage * 0.5f);
            EarthNerfDamage--;
        }

        if (ReduceWind > 0)
        {
            damage = 0;
            ReduceWind --;
        }

        if (superreducelight > 0)
        {
            damage = 0;
            superreducelight--;
        }

        if (ReduceLight > 0)
        {
            damage = Mathf.RoundToInt(damage * 0.5f);
            ReduceLight--;
        }
        if (NerfEnemyDamgelight > 0)
        {
            damage = Mathf.RoundToInt(damage / 3);
            NerfEnemyDamgelight--;
        }
        if (NerfDark)
        {
            damage = Mathf.RoundToInt(damage * 0.8f);
        }

        if (EnemyBuffLight > 0)
        {
            damage *= 3;
            EnemyBuffLight--;
        }
        if(EnemyFireBuff > 0)
        {
            damage *= 2;
            EnemyFireBuff--;
        }
        if( EnemydirtStunPlayer > 0)
        {
           Debug.Log("ผู้เล่นติดสตั๊น! ข้ามเทิร์นนี้");
                    
            EnemydirtStunPlayer--; // ลดเทิร์นสตั๊น
            damage = 10;
// ปิดปุ่มทั้งหมด (กันเหนียว)
            attackButton.interactable = false;
        foreach (var btn in skillButtons)
            btn.interactable = false;
                foreach (var btn in cardButtons) // ปิดปุ่มการ์ดด้วย
            btn.interactable = false;
            // สั่งให้กลับไปเป็นเทิร์นศัตรูทันที
 isPlayerTurn = false; 
StartCoroutine(DelayedEnemyTurn()); 

 return; // ⭐️ สำคัญมาก: ออกจากฟังก์ชันนี้ทันที (ไม่ไปเปิดปุ่มด้านล่าง)
            
        }
        damegeEnemydef = Mathf.RoundToInt(damage * (100f / (100f + selectedPlayer.def)));
        damage = damegeEnemydef;
        playerHP -= damage;
        playerHP = Mathf.Clamp(playerHP, 0, selectedPlayer.maxHP);
        StartCoroutine(ShakePlayer());
        ShowPlayerDamageNumber($"-{damage}");
        UpdatePlayerHPUI();
           StartCoroutine(MyDelay());

        if (playerHP <= 0 && hasPreventDeathEffect)
        {
            playerHP = 1;
            hasPreventDeathEffect = false;
            UpdatePlayerHPUI();
            Debug.Log("ผู้เล่นรอดตายด้วยการ์ดหัวใจที่ไม่ยอมแพ้! เหลือ 1 HP");
            return;
        }

        else if (playerHP <= 0)
        {
            Debug.Log("ผู้เล่นแพ้แล้ว!");
            ShowResultPanelLose("You Lose!");
        }
    }

    void ReduceCooldowns() //<-- ระบบคูลดาวน์
    {
        List<SkillData> keys = new List<SkillData>(skillCooldowns.Keys);
        foreach (var key in keys)
        {
            if (skillCooldowns[key] > 0)
                skillCooldowns[key]--;
        }
        UpdateSkillButtons();
    }

    

    void EnemyTurn()
    {
  GameEventManager.Instance.AddCount1(1);
        playerturntext.gameObject.SetActive(false);
        enemyturntext.gameObject.SetActive(true);

        Debug.Log("ธาตุศัตรูตอนนี้คือ: " + enemyElement);
        // ลด HP จากสถานะไฟก่อนศัตรูโจมตี

        if (isShieldActive) //สกิล 2 ดิน
        {
            shieldTurnsLeft--;
            if (shieldTurnsLeft <= 0)
            {
                isShieldActive = false;
                Debug.Log("โล่ดินหมดอายุแล้ว!");
            }
        }

        if (lightBuffTurnsLeft > 0)//สกิล 2 แสง
        {
            lightBuffTurnsLeft--;
        }
        if (lightShieldTurnsLeft > 0)//สกิล 3 แสง
        {
            lightShieldTurnsLeft--;
        }
        if (burnTurnsLeft > 0) //สกิล 2 ไฟ
        {
            int burnDamage = 10;
            ShowSkillEffectOnce(4);
            if (enemyElement == ElementType.Water)
                burnDamage /= 2;
            else if (enemyElement == ElementType.Wind)
                burnDamage *= 2;

            DamageEnemy(burnDamage);
            burnTurnsLeft--;
            Debug.Log($"ศัตรูติดไฟโดน {burnDamage} ดาเมจ เหลือ {burnTurnsLeft} เทิร์น");
        }
        if (SuperburnTurnsLeft > 0) //สกิลไฟ
        {
            int burnDamage = 15;
            ShowSkillEffectOnce(4);
            if (enemyElement == ElementType.Water)
                burnDamage /= 2;
            else if (enemyElement == ElementType.Wind)
                burnDamage *= 2;

            DamageEnemy(burnDamage);
            burnTurnsLeft--;
            Debug.Log($"ศัตรูติดไฟโดน {burnDamage} ดาเมจ เหลือ {SuperburnTurnsLeft} เทิร์น");
        }
        if (evadeTurnsLeft > 0)//สกิล 3 มืด
        {
            evadeTurnsLeft--;
        }

        int damage = 10;
        if (reduceEnemyDamageTurns > 0) //การ์ดศัตรูตีเบาลงครึ่งนึง
        {
            damage /= 2;
            reduceEnemyDamageTurns--;
            Debug.Log($"ศัตรูโจมตีเบาลงเหลือ {damage} (เหลือ {reduceEnemyDamageTurns} เทิร์น)");
        }
        if (doubleAttackTurnsLeft > 0)//การ์ด *2 ตีธรรมดา
        {
            doubleAttackTurnsLeft--;
        }

        if (silenceEnemyTurns > 0)//การ์ด 
        {
            Debug.Log("ศัตรูถูกผนึกสกิล ใช้สกิลไม่ได้");
            silenceEnemyTurns--; // ลดเทิร์นทุกครั้งที่ศัตรูถึงเทิร์น
            DamagePlayer(damage); // โจมตีธรรมดาแทน

            isPlayerTurn = true;
            UpdateSkillButtons(); // ✅ คืนสิทธิ์ให้ผู้เล่น
            return;
        }
        if (isDelayedHealActive)//การ์ดเพิ่มเลือด -10% & +30%
        {
            delayedHealTurnsLeft--;

            if (delayedHealTurnsLeft <= 0)
            {
                int healAmount = Mathf.FloorToInt(selectedPlayer.maxHP * delayedHealPercent);
                ShowSkillEffectOnce(2);
                playerHP += healAmount;
                playerHP = Mathf.Clamp(playerHP, 0, selectedPlayer.maxHP);
                PlaySoundEffect(2);
                UpdatePlayerHPUI();

                Debug.Log($"ฟื้นฟู HP 30% (+{healAmount}) หลังจากดีเลย์ 3 เทิร์น");

                isDelayedHealActive = false;
            }
        }


        ChooseAndExecuteBestEnemyMove(damage);


            // ถ้า AI ใช้สกิล 1, 2, หรือโจมตีปกติ
            // ให้เรียก Coroutine ใหม่ที่รอ 5 วิของเรา
            StartCoroutine(FinalizeTurnAfterDelay());
        
        

    }
    IEnumerator FinalizeTurnAfterDelay()
    {
        // 1. รอ 5 วินาที ตามที่คุณต้องการ
        Debug.Log("AI: กำลังรอ 5 วินาที...");
        yield return new WaitForSeconds(3f); // ⬅️ นี่คือ MyDelay() ของคุณ
        Debug.Log("AI: รอครบ 5 วิแล้ว, เริ่มเทิร์นผู้เล่น");

        // 2. ย้ายโค้ด "จบเทิร์น" ทั้งหมดจาก EnemyTurn() มาไว้ที่นี่
        if (enemySkill1Cooldown > 0) enemySkill1Cooldown--;
        if (enemySkill2Cooldown > 0) enemySkill2Cooldown--;
        if (enemySkill3Cooldown > 0) enemySkill3Cooldown--;

        if (playerStunTurns > 0) playerStunTurns--;

        if (isDamageBoosted)
        {
            isDamageBoosted = false;
            Debug.Log("บัฟดาเมจคูณ 2 หายไปแล้ว");
        }
        
        ReduceCooldowns();
        hasUsedCardThisTurn = false;
        
        isPlayerTurn = true; // ⬅️ ปลดล็อคเทิร์น
        UpdateSkillButtons(); // ⬅️ ปลดล็อคปุ่ม
    }
    
    // ฟังก์ชันนี้คือ "สมอง" ที่ตัดสินใจ
    void ChooseAndExecuteBestEnemyMove(int basicAttackDamage)
    {
        // 1. คำนวณ "คะแนน" ของแต่ละท่า
        float scoreSkill1 = CalculateSkill1Score();
        float scoreSkill2 = CalculateSkill2Score();
        float scoreSkill3 = CalculateSkill3Score();
        float scoreBasicAttack = CalculateBasicAttackScore(basicAttackDamage);

        // 2. หาว่าท่าไหนมีคะแนนสูงสุด
        float maxScore = Mathf.Max(scoreSkill1, scoreSkill2, scoreSkill3, scoreBasicAttack);

        // 3. เลือกใช้ท่านั้น
        // (สำคัญ: เช็คท่าที่คะแนนสูงก่อน เช่น สกิล 3 > สกิล 2 > สกิล 1)
        if (maxScore == scoreSkill3 && scoreSkill3 > -999) // -999 คือใช้ไม่ได้ (ติด Cooldown)
        {
            Debug.Log("AI เลือก: Skill 3 (water)");
            EnemySkill3();
        }
        else if (maxScore == scoreSkill2 && scoreSkill2 > -999)
        {
            Debug.Log("AI เลือก: Skill 2 (buff)");
            EnemySkill2();
        }
        else if (maxScore == scoreSkill1 && scoreSkill1 > -999)
        {
            Debug.Log("AI เลือก: Skill 1 (buff)");
            EnemyUseSkill1();
        }
        else
        {
            Debug.Log("AI เลือก: โจมตีปกติ");
            ShowSkillEffectOnce(9);
            DamagePlayer(basicAttackDamage);
        }
    }

    // --- นี่คือส่วนที่คุณต้อง "จูน" ความฉลาดของ AI ---
    // (ฟังก์ชันให้คะแนนของแต่ละท่า)

    float CalculateSkill1Score() // สกิล 1: โจมตี (Damage)
    {
        if (enemySkill1Cooldown > 0) return -1000; // ใช้ไม่ได้

        float score = 30; // ค่าพื้นฐานของดาเมจ

        // ถ้าผู้เล่นแพ้ทาง (ธาตุ wind) ให้คะแนนเยอะๆ
        if (selectedPlayer.elementType == ElementType.Fire)
            score *= 2; 
        
        else if (selectedPlayer.elementType == ElementType.Earth)
            score /= 2; 
        

        // ถ้าผู้เล่นมีโล่ (isShieldActive) หรือสะท้อน (reflectNextAttackWind)
        // การโจมตีจะได้ผลน้อยมาก หรือโดนสวน
        if (isShieldActive || reflectNextAttackWind || doublereflectNextAttackWind > 0 || reflectNextAttack || shieldTurnsLeft > 0 || ReduceWater > 0|| superreduce >0 || isDodgeActive || isShieldActive || EarthBootDef >0 || lightShieldTurnsLeft >0 || superreducelight > 0|| EarthNerfDamage >0 || NerfEnemyDamgelight >0  || ReduceWater>0)
            score = 2; // ให้คะแนนน้อยสุดๆ (แต่ยังดีกว่าติดลบ)

        // ถ้าสกิลนี้สามารถฆ่าผู้เล่นได้ ให้คะแนนโบนัสสูงสุด
        if (playerHP <= score)
            score += 500;

        return score;
    }

    float CalculateSkill2Score() // สกิล 2 
    {
        if (enemySkill2Cooldown > 0) return -1000; // ใช้ไม่ได้

        float score = 40; 

        if (enemySkill1Cooldown <= 1 )
            score *= 2;

        return score;
    }

    float CalculateSkill3Score() 
    {
         if (enemySkill3Cooldown > 0) return -1000; // ใช้ไม่ได้

        float score = 50; 

        if (enemySkill1Cooldown <= 1 )
            score *= 2;

        return score;
    }

    float CalculateBasicAttackScore(int basicAttackDamage)
    {
        float score = basicAttackDamage; // คะแนนเท่าดาเมจ

        // (ใส่เงื่อนไขเหมือน Skill 1)
        if (isShieldActive || reflectNextAttackWind || doublereflectNextAttackWind > 0 || reflectNextAttack || shieldTurnsLeft > 0 || ReduceWater > 0|| superreduce >0 || isDodgeActive || isShieldActive || EarthBootDef >0 || lightShieldTurnsLeft >0 || superreducelight > 0|| EarthNerfDamage >0 || NerfEnemyDamgelight >0 || ReduceWater>0)
            score = 1;

        if (playerHP <= score)
            score += 500; // โจมตีปิดเกมได้

        return score;
    }


    void UpdatePlayerHPUI()
    {
        playerHPBar.maxValue = selectedPlayer.maxHP;
        playerHPBar.value = playerHP;
        playerHPText.text = $"HP: {playerHP}/{selectedPlayer.maxHP}";
    }

    void UpdateEnemyHPUI()
    {
        enemyHPBar.maxValue = 160;
        enemyHPBar.value = enemyHP;
        enemyHPText.text = $"HP: {enemyHP}/160";
    }

    IEnumerator DelayedEnemyTurn()
    {
          //item
        if (recoverrings)
        {
            playerHP+= 2;
            playerHP = Mathf.Clamp(playerHP, 0, selectedPlayer.maxHP);
            UpdatePlayerHPUI();
        }
        if (regenrings)
        {
             playerHP+= 5;
            playerHP = Mathf.Clamp(playerHP, 0, selectedPlayer.maxHP);
            UpdatePlayerHPUI();
        }
        if (watergodarmors)
        {
             playerHP+= 5;
            playerHP = Mathf.Clamp(playerHP, 0, selectedPlayer.maxHP);
            UpdatePlayerHPUI();
        }
        //skill and card
        if (regenTurnsLeft > 0)//การ์ดเลือดเพิ่ม 10 ถึง 10 turn
        {
            playerHP += regenAmountPerTurn;
            playerHP = Mathf.Clamp(playerHP, 0, selectedPlayer.maxHP);
            regenTurnsLeft--;
            Debug.Log($"ฟื้นฟู HP +{regenAmountPerTurn} (เหลืออีก {regenTurnsLeft} เทิร์น)");
            UpdatePlayerHPUI();
        }
        if (regenwater > 0)
        {
            playerHP += 10;
            playerHP = Mathf.Clamp(playerHP, 0, selectedPlayer.maxHP);
            regenwater--;
            UpdatePlayerHPUI();
            ShowSkillEffectOnce(2);
            PlaySoundEffect(2);
        }
        if (passiveAttackBonusPerTurn > 0)//การ์ดเพิ่มโจมตีธรรมดา 1 ทุกเทิร์น
            {
                selectedPlayer.attackDamage += passiveAttackBonusPerTurn;
                Debug.Log($"+{passiveAttackBonusPerTurn} พลังโจมตีปกติสะสมในเทิร์นนี้! รวม = {selectedPlayer.attackDamage}");
            }
         if (isHealingEveryTurn)//การ์ดเพิ่มเลือด 1 ทุกเทิร์น
    {
        ShowSkillEffectOnce(2); 
        playerHP += healPerTurnAmount;
        playerHP = Mathf.Clamp(playerHP, 0, selectedPlayer.maxHP);
        Debug.Log($"ฟื้นฟู +{healPerTurnAmount} HP อัตโนมัติ! ตอนนี้ HP = {playerHP}");
        UpdatePlayerHPUI();
    }
        if (isHealingOverTime && healOverTimeTurnsLeft > 0)//การ์ดเพิ่มเลือด 10% สุ่มเทิร์น
        {
            ShowSkillEffectOnce(2); 
            int healAmountrandomturn = Mathf.RoundToInt(selectedPlayer.maxHP * 0.1f);
            playerHP += healAmountrandomturn;
            playerHP = Mathf.Clamp(playerHP, 0, selectedPlayer.maxHP);
            UpdatePlayerHPUI();

            Debug.Log($"ฟื้นฟู HP {healAmountrandomturn} จากการ์ด Heal ต่อเนื่อง");

            healOverTimeTurnsLeft--;
            if (healOverTimeTurnsLeft == 0)
            {
                isHealingOverTime = false;
                Debug.Log("จบสถานะ Heal ต่อเนื่อง");
            }
        }


        if (isPoisonEnemy && poisonEnemyTurnsLeft > 0)//การ์ดพิษ
        {
            int poisonDamage = Mathf.RoundToInt(enemyMaxHP * 0.05f);
            enemyHP -= poisonDamage;
            enemyHP = Mathf.Clamp(enemyHP, 0, enemyMaxHP);
            UpdateEnemyHPUI();
            Debug.Log($"ศัตรูได้รับดาเมจจากพิษ {poisonDamage} HP");

            poisonEnemyTurnsLeft--;
            if (poisonEnemyTurnsLeft == 0)
            {
                isPoisonEnemy = false;
                Debug.Log("สถานะพิษสิ้นสุด");
            }
        }

        yield return new WaitForSeconds(3f); // รอ 3 วินาที
         if (isEnemyStunned)//สกิล 3 ดิน
    {
        enemyStunTurnsLeft--;

        Debug.Log("ศัตรูติดสตั๊น! เทิร์นนี้ไม่ได้โจมตี");

        if (enemyStunTurnsLeft <= 0)
        {
            isEnemyStunned = false;
            Debug.Log("ศัตรูหลุดจากสตั๊นแล้ว");
        }

        yield return new WaitForSeconds(3f);
          isPlayerTurn = true;
            // ✅ ข้ามเทิร์นศัตรู กลับมาที่ผู้เล่นทันที
        
        UpdateSkillButtons();
        yield break;
    }
        EnemyTurn();
    }

    
 void EnemyUseSkill1() //<--สกิล 1 ศัตรู
    {
        int damage = 30;

        if (selectedPlayer.elementType == ElementType.Earth)
        {
            damage /= 2;
            Debug.Log("ผู้เล่นเป็นธาตุไฟ ดาเมจถูกลดครึ่งหนึ่ง");
        }
        else if (selectedPlayer.elementType == ElementType.Fire)
        {
            damage *= 2;
            Debug.Log("ผู้เล่นเป็นธาตุดิน ดาเมจคูณ 2");
        }

        if (reduceEnemyDamageTurns > 0)//การ์ดลดดาเมจศัตรุ 50%
        {
            damage /= 2;
            Debug.Log($"ดาเมจของศัตรูจากสกิลลดลงครึ่งหนึ่ง เหลือ {damage}");
        }
        //ShowSkillEffectOnce(5); 
        DamagePlayer(damage);
        ShowSkillEffectOnce(47);
        PlaySoundEffect(8);
        enemySkill1Cooldown = 3;
        Debug.Log("ศัตรูใช้ Wind ทำดาเมจ " + damage);
    }

    void EnemySkill2() //<--สกิล 2 ศัตรู
    {
         EnemyFireBuff = 2;
        enemySkill3Cooldown = 5;
        ShowSkillEffectOnce(48);
        PlaySoundEffect(3);
    }

    void EnemySkill3() //<--สกิล 3 ศัตรู
    {

        EnemyBuffLight = 1;
        enemySkill3Cooldown = 6;
        ShowSkillEffectOnce(48);
        PlaySoundEffect(3);
    }


    IEnumerator EnemyTripleSkillTurn()
    {
        for (int i = 1; i <= 1; i++)
        {
            Debug.Log($"ศัตรูใช้สกิลแบบสุ่มครั้งที่ {i}");

            bool usedSkill = false;

            if (enemySkill1Cooldown == 0)
            {
                EnemyUseSkill1();
                usedSkill = true;
            }
            else if (enemySkill2Cooldown == 0)
            {
                EnemySkill2();
                usedSkill = true;
            }

            if (!usedSkill)
            {

                if (reduceEnemyDamageTurns > 0)
                {
                    DamagePlayer(8);
                    reduceEnemyDamageTurns--;
                    Debug.Log($"ศัตรูโจมตีเบาลงเหลือ 5 (เหลือ {reduceEnemyDamageTurns} เทิร์น)");
                }

                else { DamagePlayer(15);
                        ShowSkillEffectOnce(9);
                    Debug.Log("ศัตรูโจมตีปกติ");
                }
            }

            yield return new WaitForSeconds(2f);
        }

        ReduceCooldowns();

        if (EnemyStunPlayer > 0)
        {
            playerStunTurns--;
            Debug.Log("ผู้เล่นยังติดสถานะหยุด: เหลือ " + playerStunTurns + " เทิร์น");
        }

        // ✅ เงื่อนไขนี้คือ ถ้าผู้เล่นไม่ติดสถานะหยุดแล้ว ก็ให้เริ่มเทิร์นผู้เล่น
        if (playerStunTurns <= 0)
        {
            isPlayerTurn = true;
            UpdateSkillButtons();
            Debug.Log("ถึงตาผู้เล่นแล้ว ปุ่มเปิดเรียบร้อย");
        }
        else
        {
            Debug.Log("ผู้เล่นยังติดสถานะหยุด ไม่เปิดปุ่ม");
        }
    }

    void ShowResultPanelVictory(string message)
    {
        winPanel.SetActive(true);

        // ปิดปุ่มต่าง ๆ เพื่อไม่ให้กดต่อ
        attackButton.interactable = false;
        foreach (var btn in skillButtons)
            btn.interactable = false;
    }

    void ShowResultPanelLose(string message)
    {
        losePanel.SetActive(true);
        resultText.text = message;

        // ปิดปุ่มต่าง ๆ เพื่อไม่ให้กดต่อ
        attackButton.interactable = false;
        foreach (var btn in skillButtons)
            btn.interactable = false;
    }
    IEnumerator ShakeEnemy(float duration = 0.5f, float magnitude = 10f)
    {
        
        Vector3 originalPos = enemyImage.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float offsetX = Random.Range(-magnitude, magnitude);
            float offsetY = Random.Range(-magnitude, magnitude);
            enemyImage.localPosition = originalPos + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        enemyImage.localPosition = originalPos;
    }

public void ShowDamageNumber(string text)
{
        damageTextUI.text = text;
    damageTextUI.gameObject.SetActive(true);

    StartCoroutine(HideAfterSeconds(2f));
}

IEnumerator HideAfterSeconds(float seconds)
{
    yield return new WaitForSeconds(seconds);
    damageTextUI.gameObject.SetActive(false);
}

IEnumerator ShakePlayer(float duration = 0.5f, float magnitude = 10f)
{
    // เก็บตำแหน่งเริ่มต้นของทุก RectTransform
        Vector3[] originalPositions = new Vector3[playerRects.Length];
    for (int i = 0; i < playerRects.Length; i++)
    {
        originalPositions[i] = playerRects[i].localPosition;
    }

    float elapsed = 0f;

    while (elapsed < duration)
    {
        for (int i = 0; i < playerRects.Length; i++)
        {
            float offsetX = Random.Range(-magnitude, magnitude);
            float offsetY = Random.Range(-magnitude, magnitude);
            playerRects[i].localPosition = originalPositions[i] + new Vector3(offsetX, offsetY, 0f);
        }

        elapsed += Time.deltaTime;
        yield return null;
    }

    // รีเซ็ตตำแหน่งเดิมให้ทุกตัว
    for (int i = 0; i < playerRects.Length; i++)
    {
        playerRects[i].localPosition = originalPositions[i];
    }
}
public void ShowPlayerDamageNumber(string text)
{
        playerDamageTextUI.text = text;
    playerDamageTextUI.gameObject.SetActive(true);

    StartCoroutine(HidePlayerDamageAfterSeconds(2f));
}

    IEnumerator HidePlayerDamageAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        playerDamageTextUI.gameObject.SetActive(false);
    }

IEnumerator MyDelay()
    {
        Debug.Log("ก่อนรอ");
        yield return new WaitForSeconds(3f); // รอ 5 วินาที
        Debug.Log("หลังรอ");
    }


    
// Method สำหรับปิดปุ่มและเปลี่ยนสีให้มืด
void DisableCardButton(int index)
{
    // เช็คว่า index ที่ส่งมาถูกต้องไหม (กัน Error Array out of range)
    if (index >= 0 && index < cardButtons.Length)
    {
        // 1. ปิดไม่ให้กดซ้ำ
        cardButtons[index].interactable = false;

        // 2. ดึงรูปภาพของปุ่มมาเปลี่ยนสี
        Image btnImage = cardButtons[index].GetComponent<Image>();
        if (btnImage != null)
        {
            // ใช้ new Color(แดง, เขียว, น้ำเงิน, ความทึบ)
            // 0.5f = เทามาตรฐาน (มืด)
            // 0.75f = เทาอ่อน (สว่างขึ้น) <-- แนะนำค่านี้
            // 1.0f = ขาวปกติ (สว่างสุด)
            // เปลี่ยนเป็นสีเทา (มืดลง)

            btnImage.color = new Color(0.75f, 0.75f, 0.75f, 1f);
            
            // หรือถ้าอยากให้มืดกว่านี้ ใช้ new Color(0.5f, 0.5f, 0.5f, 1f); ก็ได้
        }
    }
}

     void UseCard(CardData card,int buttonIndex)
{
playerturntext.gameObject.SetActive(false);
    enemyturntext.gameObject.SetActive(true);
StartCoroutine(MyDelay());

        if (hasUsedCardThisTurn)
        {
            Debug.Log("คุณใช้การ์ดไปแล้วในเทิร์นนี้!");
            return;
        }

    if (usedCards.Contains(card))
        {
            Debug.Log("การ์ดนี้ถูกใช้ไปแล้ว");
            return;
        }

        Debug.Log($"ใช้การ์ด: {card.cardName}");
        usedCards.Add(card); // ✅ ใช้ครั้งเดียวแล้วลบ
        hasUsedCardThisTurn = true;
        DisableCardButton(buttonIndex);
        switch (card.effectType)
        {
            case CardEffectType.ReduceEnemyDamage:
                reduceEnemyDamageTurns = card.value; // เช่น 3 เทิร์น
                Debug.Log($"ศัตรูจะโจมตีเบาลงครึ่งหนึ่ง {card.value} เทิร์น");
                ShowCardEffectOnce(0);
                PlaySoundEffect(5);
            if (buttonIndex >= 0 && buttonIndex < cardButtons.Length)
    {
        cardButtons[buttonIndex].interactable = false; // ปิดปุ่มกดไม่ได้
        
        var img = cardButtons[buttonIndex].GetComponent<Image>();
        if (img != null) 
        {
            img.color = Color.gray; // เปลี่ยนเป็นสีมืด (เทา)
        }
    }
                if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                 isPlayerTurn = false;
                break;
            case CardEffectType.SilenceEnemy:
                silenceEnemyTurns = card.value;
                Debug.Log($"ศัตรูถูกผนึกสกิล {card.value} เทิร์น");
               ShowCardEffectOnce(1); 
               PlaySoundEffect(5);
               if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                 isPlayerTurn = false;
                break;
            case CardEffectType.ReflectDamage:
                reflectNextAttack = true;
                if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                Debug.Log("เปิดการ์ดสะท้อนดาเมจ ศัตรูจะโดนดาเมจ x2 และเราไม่เสียเลือดในครั้งต่อไป");
                 isPlayerTurn = false;
                  ShowCardEffectOnce(2);
                break;
            case CardEffectType.IgnoreElement:
                isIgnoreElementCardActive = true;
                if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                Debug.Log("ใช้การ์ด Ignore Element — การโจมตีครั้งถัดไปจะไม่สนธาตุศัตรู!");
                 isPlayerTurn = false;
                break;
            case CardEffectType.PermanentAttackBoost:
                int bonus = Random.Range(5, 21); // สุ่ม 5-20
                selectedPlayer.attackDamage += bonus;
                if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                Debug.Log($"เพิ่มพลังโจมตีปกติถาวร +{bonus}. พลังใหม่ = {selectedPlayer.attackDamage}");
                
                 isPlayerTurn = false;
                 
                  ShowCardEffectOnce(3);
                  PlaySoundEffect(3);
                break;
            case CardEffectType.PermanentAttackBoost05:
                selectedPlayer.attackDamage *= 1 / 2;
                if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                 isPlayerTurn = false;
                 ShowCardEffectOnce(3);
                 PlaySoundEffect(3);
                break;
            case CardEffectType.DoubleNormalAttack:
                doubleAttackTurnsLeft = card.value;
                selectedPlayer.attackDamage = Mathf.RoundToInt(selectedPlayer.attackDamage * 2 );
                Debug.Log($"เปิดบัฟโจมตีธรรมดาคูณ 2 เป็นเวลา {card.value} เทิร์น");
                if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                 isPlayerTurn = false;
                 ShowCardEffectOnce(3);
                 PlaySoundEffect(3);
                break;
            case CardEffectType.PermanentAttackBoostRandom:
                float bonusRandom = Random.Range(0.5f, 3.5f);
                selectedPlayer.attackDamage = Mathf.RoundToInt(selectedPlayer.attackDamage * bonusRandom);
                Debug.Log($"เพิ่มพลังโจมตีปกติถาวร +{bonusRandom}. พลังใหม่ = {selectedPlayer.attackDamage}");
                if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                 isPlayerTurn = false;
                 ShowCardEffectOnce(3);
                 PlaySoundEffect(3);
                break;
            case CardEffectType.HealRandomHP:
                int healAmount = Random.Range(10, 101); // 10-100
                ShowSkillEffectOnce(2); 
                PlaySoundEffect(2);
                 playerHP += healAmount;
                 playerHP = Mathf.Clamp(playerHP, 0, selectedPlayer.maxHP);
                 UpdatePlayerHPUI();
                Debug.Log($"ฟื้นฟู HP แบบสุ่ม: +{healAmount} หน่วย (HP ปัจจุบัน: {playerHP})");
                if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                 isPlayerTurn = false;
                   break;
            case CardEffectType.DelayedHeal:
                int lostHP = Mathf.FloorToInt(selectedPlayer.maxHP * 0.1f);
                playerHP -= lostHP;
                playerHP = Mathf.Clamp(playerHP, 0, selectedPlayer.maxHP);
                UpdatePlayerHPUI();
                 Debug.Log($"เสีย HP 10% ทันที: -{lostHP}");
                 delayedHealTurnsLeft = 3;
                delayedHealPercent = 0.3f; // 30%
                 isDelayedHealActive = true;
                Debug.Log("ฟื้นฟู 30% ของ HP ในอีก 3 เทิร์น");
                if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                 isPlayerTurn = false;
                break;
            case CardEffectType.PermanentEnemyAttackReduction:
                isEnemyAttackReducedPermanently = true;
                Debug.Log("เปิดการ์ดลดดาเมจศัตรูถาวร 30%");
                ShowCardEffectOnce(0);
                PlaySoundEffect(5);
                if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                isPlayerTurn = false;
                break;
            case CardEffectType.FullHeal:
                int healedAmount = selectedPlayer.maxHP - playerHP;
                ShowSkillEffectOnce(2); 
                PlaySoundEffect(2);
                 playerHP = selectedPlayer.maxHP;
                UpdatePlayerHPUI();
                Debug.Log($"ฟื้นฟูเลือดจนเต็ม (+{healedAmount} HP)");
                if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                 isPlayerTurn = false;
                break;
            case CardEffectType.RegenerateHPOverTime:
                regenTurnsLeft = 10;
                regenAmountPerTurn = 10;
                PlaySoundEffect(2);
                Debug.Log("เปิดการ์ดฟื้นฟู HP 10 ต่อเทิร์น นาน 10 เทิร์น");
                if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                 isPlayerTurn = false;
                break;
            case CardEffectType.AddAttackPerTurn:
                passiveAttackBonusPerTurn = 1; // บวกทีละ 1 ทุกเทิร์น
                PlaySoundEffect(3);
                Debug.Log("การ์ดเปิดใช้งาน: +1 พลังโจมตีปกติทุกเทิร์น (ถาวร)");
                ShowCardEffectOnce(3);
                if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                 isPlayerTurn = false;
                break;
            case CardEffectType.HealEveryTurn:
                isHealingEveryTurn = true;
                healPerTurnAmount = 1;
                PlaySoundEffect(2);
                Debug.Log("เปิดการ์ดฮีล: จะฟื้น 1 HP ทุกเทิร์น");
                if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                 isPlayerTurn = false;
                break;
            case CardEffectType.PermanentDodge:
                isDodgeActive = true;
                dodgeChance = 0.1f; // 10% หลบ
                Debug.Log("การ์ดหลบหลีกเปิดใช้งาน: หลบได้ 10% ตลอดทั้งเกม");
                ShowCardEffectOnce(3);
                PlaySoundEffect(6);
                if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                 isPlayerTurn = false;
                break;
            case CardEffectType.DrainLifePercent:
                 float drainPercent = Random.Range(0.1f, 0.21f); // 10% - 20%
                int drainAmount = Mathf.RoundToInt(enemyHP * drainPercent);
                drainAmount = Mathf.Min(drainAmount, enemyHP - 1);

                enemyHP -= drainAmount;
                enemyHP = Mathf.Clamp(enemyHP, 0, 100); // ปรับตาม max HP ศัตรู
                StartCoroutine(ShakeEnemy());

                playerHP += drainAmount;
                playerHP = Mathf.Clamp(playerHP, 0, selectedPlayer.maxHP);

                Debug.Log($"ดูดเลือดศัตรู {drainAmount} หน่วย (คิดเป็น {Mathf.RoundToInt(drainPercent * 100)}%)");
                ShowCardEffectOnce(4);
                PlaySoundEffect(5);
                if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                UpdateEnemyHPUI();
                UpdatePlayerHPUI();
                 isPlayerTurn = false;
                break;
            case CardEffectType.InstantEnemyHPPercentDamage:
                float percent = 0.3f;
                int damageAmount = Mathf.RoundToInt(enemyMaxHP * percent);

                enemyHP -= damageAmount;
                enemyHP = Mathf.Clamp(enemyHP, 0, enemyMaxHP);

                Debug.Log($"ศัตรูเสียเลือดทันที {damageAmount} หน่วย (30% ของ max HP)");
                if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                ShowCardEffectOnce(5);
                PlaySoundEffect(0);
                UpdateEnemyHPUI();
                 isPlayerTurn = false;
                break;
            case CardEffectType.PermanentElementalBoost:
                 isElementalAttackBoosted = true;
                 if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                  isPlayerTurn = false;
                  ShowCardEffectOnce(3);
                  PlaySoundEffect(3);
                break;
            case CardEffectType.PermanentElementalBoostx2:
                 isElementalAttackBoostedx2 = 2;
                 if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                  isPlayerTurn = false;
                  ShowCardEffectOnce(3);
                  PlaySoundEffect(3);
                break;
            case CardEffectType.PermanentElementalBoostRandom:
             float BootElementPercent = Random.Range(0.5f, 3.5f);
                ElementalAttackBoostedRandom = 3;
                 isElementalAttackBoostedRandom = BootElementPercent;
                 if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                  isPlayerTurn = false;
                  ShowCardEffectOnce(3);PlaySoundEffect(3);
                break;
            case CardEffectType.AttackSuperBoost:
             SuperBoostAttackTurn = card.value;
             int BootAttackDamage = Random.Range(3 ,5);
                 AttackBoosted = BootAttackDamage;
                 if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                  isPlayerTurn = false;
                  ShowCardEffectOnce(3);PlaySoundEffect(3);
                break;
             case CardEffectType.AttackBoostTurnRandom:
             RandomBootDamageTurn = Random.Range(1 ,5);
             if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
              isPlayerTurn = false;
              ShowCardEffectOnce(3);PlaySoundEffect(3);
                break;
            case CardEffectType.DamageBuffIncreaseEachTurn:
                isElementalBuffPerTurnActive = true;
                Debug.Log("เปิดการ์ดเพิ่มพลังโจมตี +0.5 ทุกเทิร์น");
                if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                isPlayerTurn = false;
                ShowCardEffectOnce(3);PlaySoundEffect(3);
                break;
            case CardEffectType.DoubleAttackAndLoseHP3Turns:
                doubleAttackandloseTurnsLeft = 3;
                Debug.Log("เปิดการ์ด บัฟโจมตี x2 และเสีย HP 10% ต่อเทิร์น เป็นเวลา 3 เทิร์น");
                if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                isPlayerTurn = false;
                ShowCardEffectOnce(3);PlaySoundEffect(3);
                break;
            case CardEffectType.HalfDamageAndDoubleMaxHP:
                isHalfDamageAll = true;

                originalMaxHP = selectedPlayer.maxHP;
                selectedPlayer.maxHP *= 2;
                playerHP = selectedPlayer.maxHP;

                UpdatePlayerHPUI(); // ปรับแถบ HP ตามใหม่

                Debug.Log("เปิดการ์ด ลดดาเมจทุกอย่างครึ่งนึง และเพิ่ม Max HP x2 พร้อมฟื้นเต็ม");
                ShowSkillEffectOnce(2);PlaySoundEffect(2);
                if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                isPlayerTurn = false;
                break;
            case CardEffectType.HalfHP_AndBoostAttack15x:
                // ลด HP เหลือครึ่งของ Max
                playerHP = selectedPlayer.maxHP / 2;
                playerHP = Mathf.Clamp(playerHP, 0, selectedPlayer.maxHP);

                // เพิ่มพลังโจมตี
                attackMultiplier = 1.5f;

                Debug.Log("ใช้การ์ด: ลดเลือดเหลือครึ่งหนึ่ง และเพิ่มพลังโจมตี x1.5");
                StartCoroutine(ShakePlayer());
                 ShowPlayerDamageNumber($"-{selectedPlayer.maxHP / 2}");
                ShowCardEffectOnce(3);
                PlaySoundEffect(3);
                UpdatePlayerHPUI();
                attackMultiplierTurn = true;
                if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                 isPlayerTurn = false;
                break;
            case CardEffectType.Heal20PercentMaxHP:
                int healAmount20Percent = Mathf.RoundToInt(selectedPlayer.maxHP * 0.2f);
                ShowSkillEffectOnce(2); 
                PlaySoundEffect(2);
                playerHP += healAmount20Percent;
                playerHP = Mathf.Clamp(playerHP, 0, selectedPlayer.maxHP);

                Debug.Log($"ฟื้นฟู HP {healAmount20Percent} (20% ของ Max HP)");
                if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                UpdatePlayerHPUI();
                isPlayerTurn = false;
                break;
            case CardEffectType.HealFullAndReduceAttack:
                playerHP = selectedPlayer.maxHP; // เพิ่มเลือดจนเต็ม
                ShowSkillEffectOnce(2);
                PlaySoundEffect(2);
                checkReduceAttackHealFull = true;
                if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                UpdatePlayerHPUI(); isPlayerTurn = false;
                break;
            case CardEffectType.Poison:
                poisonEnemyTurnsLeft = Random.Range(2, 6); // สุ่ม 2–5 เทิร์น
                isPoisonEnemy = true;
                Debug.Log($"ศัตรูติดพิษ {poisonEnemyTurnsLeft} เทิร์น ลดเลือด 5% ของ Max HP ต่อเทิร์น");
                if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                isPlayerTurn = false;
                ShowCardEffectOnce(6);
                PlaySoundEffect(11);
                break;
            case CardEffectType.HealOverTimePercent:
                healOverTimeTurnsLeft = Random.Range(1, 6); // 1-5 เทิร์น
                isHealingOverTime = true;
                PlaySoundEffect(2);
                Debug.Log($"เพิ่มเลือด 10% ของ Max HP ทุกเทิร์น เป็นเวลา {healOverTimeTurnsLeft} เทิร์น");
                if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                isPlayerTurn = false;
                break;
            case CardEffectType.HalfPlayerDamage_HealOnAttack:
                 healOnAttackTurnsLeft = Random.Range(1, 6); // 1–5 เทิร์น
                 PlaySoundEffect(3);
               Debug.Log($"ดาเมจผู้เล่นถูกลดครึ่ง และจะฟื้น HP 10% ทุกครั้งที่โจมตีศัตรู เป็นเวลา {healOnAttackTurnsLeft} เทิร์น");
               if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                isPlayerTurn = false;
                break;
            case CardEffectType.ConfuseEnemy:
                confuseEnemyTurns = 2;
                Debug.Log("ศัตรูติดสถานะมึนงง: โจมตีเบาลงครึ่ง และโดนดาเมจแรงขึ้น 2 เท่า เป็นเวลา 2 เทิร์น");
                if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                ShowCardEffectOnce(7);
                PlaySoundEffect(5);
                 isPlayerTurn = false;
                break;
            case CardEffectType.ConfuseEnemyHitSelf:
                confuseHitSelfTurns = 4;
                Debug.Log("ศัตรูติดสถานะสับสน 4 เทิร์น — อาจโจมตีตัวเองหรือโจมตีเราแรงขึ้น");
                if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                 isPlayerTurn = false;
                ShowCardEffectOnce(8);
                PlaySoundEffect(5);
                break;
            case CardEffectType.ReduceEnemyDamageHalf:
                isEnemyDamageReducedHalf = 3;
                if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                 isPlayerTurn = false;
                  ShowCardEffectOnce(0);
                  PlaySoundEffect(5);
                break;
            case CardEffectType.AttackBoostRandomRange:
                AttackBoostRandomRange = true;
                if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                 isPlayerTurn = false;
                  ShowCardEffectOnce(3);
                  PlaySoundEffect(3);
                break;
            case CardEffectType.SacrificeHPForMassiveDamage:
                int hpLost = Mathf.FloorToInt(playerHP * 0.7f);
                playerHP -= hpLost;
                playerHP = Mathf.Clamp(playerHP, 0, selectedPlayer.maxHP);
                UpdatePlayerHPUI();
                 StartCoroutine(ShakePlayer());
                ShowCardEffectOnce(3);
                PlaySoundEffect(3);
                  ShowPlayerDamageNumber($"-{hpLost}");
                Debug.Log($"เสียเลือด {hpLost} HP เพื่อบัฟดาเมจ x5 เป็นเวลา 3 เทิร์น");
                ultraDamageTurns = 3;
                if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                 isPlayerTurn = false;
                break;
            case CardEffectType.ReduceEnemyHPBy20Percent:
                 float twentypercent = 0.2f;
                int damageAmounttwenty = Mathf.RoundToInt(enemyMaxHP * twentypercent);
                enemyHP -= damageAmounttwenty;
                enemyHP = Mathf.Clamp(enemyHP, 0, enemyMaxHP);
                UpdateEnemyHPUI();
                  ShowCardEffectOnce(5);
                  PlaySoundEffect(0);
                 StartCoroutine(ShakeEnemy());
                ShowDamageNumber($"-{damageAmounttwenty}");
               Debug.Log($"ใช้การ์ดลด HP ศัตรูทันที {damageAmounttwenty} หน่วย (20% ของ Max HP)");
               if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                isPlayerTurn = false;
                break;
            case CardEffectType.ReduceEnemyHPBy15Percent:
                float fifteenpercent = 0.15f;
                int damageAmountfifty = Mathf.RoundToInt(enemyMaxHP * fifteenpercent);
                enemyHP -= damageAmountfifty;
                enemyHP = Mathf.Clamp(enemyHP, 0, enemyMaxHP);
                UpdateEnemyHPUI();
                ShowCardEffectOnce(5);
                PlaySoundEffect(0);
                 StartCoroutine(ShakeEnemy());
                ShowDamageNumber($"-{damageAmountfifty}");
               Debug.Log($"ใช้การ์ดลด HP ศัตรูทันที {damageAmountfifty} หน่วย (15% ของ Max HP)");
               if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                isPlayerTurn = false;
                break;
            case CardEffectType.CurseAttackUpLoseHPPerTurn:
                isCursedAttack = true;
                PlaySoundEffect(5);
                Debug.Log("คำสาปพลังคลั่ง: เพิ่มดาเมจ 2 เท่า ถาวร แต่จะเสีย HP 5% ของ Max ทุกเทิร์น");
                if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                 isPlayerTurn = false;
                ShowCardEffectOnce(9);
                break;
            case CardEffectType.PreventDeathOnce:
                hasPreventDeathEffect = true;
                Debug.Log("ใช้การ์ดหัวใจที่ไม่ยอมแพ้! ถ้า HP หมด จะรอดตาย 1 ครั้งด้วย HP 1");
                if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                ShowCardEffectOnce(10);
                PlaySoundEffect(2);
                 isPlayerTurn = false;
                break;


              case CardEffectType.ElementAttack_Fire:
                {
                    int damage = card.value;
                    if (enemyElement == ElementType.Wind)
                    {
                        damage *= 2;
                        Debug.Log("ศัตรูธาตุลม โดนไฟคูณ 2");
                    }
                    else if (enemyElement == ElementType.Water)
                    {
                        damage /= 2;
                        Debug.Log("ศัตรูธาตุน้ำ โดนไฟลดครึ่ง");
                    }

                    DamageEnemy(damage);
                    ShowSkillEffectOnce(0);
                    PlaySoundEffect(7);
                    Debug.Log($"ใช้การ์ดโจมตีไฟ ดาเมจ: {damage}");
                    if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                    isPlayerTurn = false;
                    break;
                }
            case CardEffectType.ElementAttack_Water:
                {
                    int damage = card.value;
                    if (enemyElement == ElementType.Fire)
                    {
                        damage *= 2;
                        Debug.Log("ศัตรูธาตุไฟ โดนน้ำคูณ 2");
                    }
                    else if (enemyElement == ElementType.Earth)
                    {
                        damage /= 2;
                        Debug.Log("ศัตรูธาตุดิน โดนน้ำลดครึ่ง");
                    }

                    DamageEnemy(damage);
                    ShowSkillEffectOnce(27);
                    PlaySoundEffect(8);
                    Debug.Log($"ใช้การ์ดโจมตีน้ำ ดาเมจ: {damage}");
                    if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                    isPlayerTurn = false;
                    break;
                }
            case CardEffectType.ElementAttack_Wind:
                {
                    int damage = card.value;
                    if (enemyElement == ElementType.Earth)
                    {
                        damage *= 2;
                        Debug.Log("ศัตรูธาตุดิน โดนลมคูณ 2");
                    }
                    else if (enemyElement == ElementType.Fire)
                    {
                        damage /= 2;
                        Debug.Log("ศัตรูธาตุไฟ โดนลมลดครึ่ง");
                    }

                    DamageEnemy(damage);
                      ShowSkillEffectOnce(19);
                      PlaySoundEffect(9);
                    Debug.Log($"ใช้การ์ดโจมตีลม ดาเมจ: {damage}");
                    if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                    isPlayerTurn = false;
                    break;
                }
            case CardEffectType.ElementAttack_Earth:
                {
                    int damage = card.value;
                    if (enemyElement == ElementType.Water)
                    {
                        damage *= 2;
                        Debug.Log("ศัตรูธาตุน้ำ โดนดินคูณ 2");
                    }
                    else if (enemyElement == ElementType.Wind)
                    {
                        damage /= 2;
                        Debug.Log("ศัตรูธาตุลม โดนดินลดครึ่ง");
                    }

                    DamageEnemy(damage);
                     ShowSkillEffectOnce(22);
                     PlaySoundEffect(10);
                    Debug.Log($"ใช้การ์ดโจมตีดิน ดาเมจ: {damage}");
                    if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                    isPlayerTurn = false;
                    break;
                }
            case CardEffectType.ElementAttack_Dark:
                {
                    int damage = card.value;
                    if (enemyElement == ElementType.Light)
                    {
                        damage *= 2;
                        Debug.Log("ศัตรูธาตุแสง โดนมืดคูณ 2");
                    }

                    DamageEnemy(damage);
                    ShowSkillEffectOnce(15);
                    PlaySoundEffect(11);
                    Debug.Log($"ใช้การ์ดโจมตีมืด ดาเมจ: {damage}");
                    if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                    isPlayerTurn = false;
                    break;
                }
            case CardEffectType.ElementAttack_Light:
                {
                    int damage = card.value;
                    if (enemyElement == ElementType.Dark)
                    {
                        damage *= 2;
                        Debug.Log("ศัตรูธาตุมืด โดนแสงคูณ 2");
                    }

                    DamageEnemy(damage);
                    ShowSkillEffectOnce(44);
                    PlaySoundEffect(12);
                    Debug.Log($"ใช้การ์ดโจมตีแสง ดาเมจ: {damage}");
                    if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                    isPlayerTurn = false;
                    break;
                }
            case CardEffectType.ElementAttack_Physical:
                {
                    int damage = card.value;
                    DamageEnemy(damage);
                    PlaySoundEffect(0);
                    Debug.Log($"ใช้การ์ดโจมตีกายภาพ ดาเมจ: {damage}");
                    if (DeckManager.Instance != null)
            {
                DeckManager.Instance.LockCard(card);
            }
                    isPlayerTurn = false;
                    ShowSkillEffectOnce(8);
                    break;
                }
                
    
    
    

        // (อื่น ๆ เหมือนเดิม)
        }
        
    

    // ปิดปุ่มการ์ดหลังใช้
        int index = selectedCards.IndexOf(card);
    if (index >= 0 && index < cardButtons.Length)
    {
        cardButtons[index].interactable = false;
    }

    UpdateSkillButtons();
    StartCoroutine(DelayedEnemyTurn()); // จบเทิร์น
}

int playerdamageItems = 0;
bool knightswords = false;
bool lightrings = false;
bool lightspears  =false;
bool firedaggers = false;
bool firelegendaryswords = false;
bool windswords = false;
bool  windspears = false;
bool darkdaggers  =false;
bool darkrings = false;
 bool darklegendaryrings = false;
bool darklegendarydaggers = false;

//recover
bool recoverrings =false;
bool regenrings = false;
bool watergodarmors = false;
void ApplyEffect(ItemID id)
    {
        switch (id)
        {

            //Normal
            case ItemID.Sword:
                selectedPlayer.attackDamage += 5; // เพิ่มพลังกายภาพโจมตี 5
                Debug.Log("Effect: ใส่ดาบ เพิ่มโจมตี +5");
                break;

            case ItemID.Armor:
                selectedPlayer.def += 20; // เพิ่มป้องกัน 20
                Debug.Log("Effect: ใส่เกราะ เพิ่มป้องกัน +20");
                break;
            case ItemID.DawnRign:
                int healAmount = 20;
                  playerHP += healAmount;
                 playerHP = Mathf.Clamp(playerHP, 0, selectedPlayer.maxHP);
                 UpdatePlayerHPUI();
                 break;
            case ItemID.WhiteFeather:
                selectedPlayer.speed +=20;
                break;
            case ItemID.RecoverRing: // ADD Method RecoverItem
                recoverrings= true;
                break;
            case ItemID.HearthNeckless:
                   int healAmount20Percent = Mathf.RoundToInt(selectedPlayer.maxHP * 0.2f);
                 playerHP = healAmount20Percent;
                  playerHP = Mathf.Clamp(playerHP, 0, selectedPlayer.maxHP);
                 UpdatePlayerHPUI();
                 break;
            case ItemID.KnightSword: //ADD Method ItemDamage
                 knightswords = true;
                 break;
            case ItemID.KnightArmor:
                 selectedPlayer.def += Mathf.RoundToInt(selectedPlayer.def*0.05f);
                 break;
            case ItemID.KnightShoes:
                selectedPlayer.speed += Mathf.RoundToInt(selectedPlayer.speed*0.1f);
                break;

                //light

            case ItemID.LightArmor:
                if(enemyElement == ElementType.Dark)
                {
                    selectedPlayer.def += Mathf.RoundToInt(selectedPlayer.def*0.1f);
                }
                break;
            case ItemID.LightNeckless:
                 healAmount =  50;
                  playerHP += healAmount;
                 playerHP = Mathf.Clamp(playerHP, 0, selectedPlayer.maxHP);
                 UpdatePlayerHPUI();
                 break;
            case ItemID.LightRing:  //ADD Method ItemDamage
                lightrings = true;
                break;
            case ItemID.LightSpear://ADD Method ItemDamage
                if (enemyElement == ElementType.Dark)
                {
                    lightspears = true;
                }
                break;
            case ItemID.GodArmor:
                selectedPlayer.def += Mathf.RoundToInt(selectedPlayer.def*0.15f);
                break;

                //Fire

            case ItemID.FireDagger: //ADD Method ItemDamage
                firedaggers = true;
                break;
            case ItemID.FireAxe:
            int roll = Random.Range(1, 101);
                if(roll <= 10)
                {
                    int damage = 10;
                    if (enemyElement == ElementType.Wind)
                    {
                        damage *= 2;
                        Debug.Log("ศัตรูธาตุลม โดนไฟคูณ 2");
                    }
                    else if (enemyElement == ElementType.Water)
                    {
                        damage /= 2;
                        Debug.Log("ศัตรูธาตุน้ำ โดนไฟลดครึ่ง");
                    }

                    DamageEnemy(damage);
                    ShowSkillEffectOnce(0);
                    PlaySoundEffect(7);
                }
                break;
            case ItemID.FireArmor:
                if (enemyElement == ElementType.Wind)
                {
                    selectedPlayer.def += Mathf.RoundToInt(selectedPlayer.def*0.10f);
                }
                break;
            case ItemID.FireLegendarySword: //ADD Method ItemDamage
                if (enemyElement == ElementType.Wind)
                {
                    firelegendaryswords = true;
                }
                break;
            case ItemID.FireSword:
            
             int rollburn = Random.Range(1, 101);
                if(rollburn <= 20)
                {
                    int damage = 10;
                    if (enemyElement == ElementType.Wind)
                    {
                        damage *= 2;
                        Debug.Log("ศัตรูธาตุลม โดนไฟคูณ 2");
                    }
                    else if (enemyElement == ElementType.Water)
                    {
                        damage /= 2;
                        Debug.Log("ศัตรูธาตุน้ำ โดนไฟลดครึ่ง");
                    }

                    DamageEnemy(damage);
                    ShowSkillEffectOnce(0);
                    PlaySoundEffect(7);
                }
            
                break;
            
                //water
                case ItemID.WaterArmor:
                    selectedPlayer.def += Mathf.RoundToInt(selectedPlayer.def*0.10f);
                    break;
                case ItemID.RegenRing: //ADD Method RecoverItem
                    regenrings = true;
                    break;
                case ItemID.WaterNeckless:
                     healAmount =  70;
                    playerHP += healAmount;
                    playerHP = Mathf.Clamp(playerHP, 0, selectedPlayer.maxHP);
                    UpdatePlayerHPUI();
                    break;
                case ItemID.WaterLegendaryArmor:
                if (enemyElement == ElementType.Fire)
                {
                    selectedPlayer.def+=  Mathf.RoundToInt(selectedPlayer.def*0.15f);
                }
                break;
                case ItemID.WaterGodArmor: //ADD Method RecoverItem
                    selectedPlayer.def+=  Mathf.RoundToInt(selectedPlayer.def*0.15f);
                    watergodarmors = true;
                    break;
                
                //Wind

                case ItemID.WindShoes:
                    selectedPlayer.speed +=  Mathf.RoundToInt(selectedPlayer.speed*0.2f);
                    break;
                case ItemID.WindEye:
                    isDodgeActive = true;
                    dodgeChance += 0.1f; // 10% หลบ
                    break;
                case ItemID.WindSword: //ADD Method ItemDamage
                    windswords = true;
                    break;
                case ItemID.WindSpear://ADD Method ItemDamage
                    if(enemyElement == ElementType.Earth)
                {
                    windspears = true;
                }
                break;
                case ItemID.WindLegendaryEye:
                    isDodgeActive = true;
                    dodgeChance += 0.3f; // 30% หลบ
                    break;

                //Earth

                case ItemID.EarthArmor:
                    selectedPlayer.def+=  Mathf.RoundToInt(selectedPlayer.def*0.1f);
                    break;
                case ItemID.EarthHammer:
                  int rollhammer = Random.Range(1, 101);
                if(rollhammer <= 10)
                {
                    enemyStunTurnsLeft +=1;
                }
                    break;
                case ItemID.EarthRing:
                  int rollearthring = Random.Range(1, 101);
                if(rollearthring <= 7)
                { superreduce += 1;
                }
                break;
                case ItemID.EarthLegendaryArmor:
                if (enemyElement == ElementType.Water)
                {
                    selectedPlayer.def += Mathf.RoundToInt(selectedPlayer.def*0.15f);
                }
                break;
                case ItemID.EarthLegendaryHammer:
              int rolllehammer = Random.Range(1, 101);
                if(rolllehammer <= 20)
                {
                    enemyStunTurnsLeft +=1;
                }
                    break;

                //Dark

                case ItemID.DarkDagger: //ADD Method ItemDamage
                    if (enemyElement == ElementType.Light) 
                {
                    darkdaggers =true;
                }
                break;
                case ItemID.DarkShoes:
                     isDodgeActive = true;
                    dodgeChance += 0.15f; // 15% หลบ
                    break;
                case ItemID.DarkRing: //ADD Method ItemDamage
                    darkrings = true;
                    break;
                case ItemID.DarkLegendaryRing://ADD Method ItemDamage
                    darklegendaryrings = true;
                    break;
                case ItemID.DarkLegendaryDagger://ADD Method ItemDamage
                    darklegendarydaggers = true;
                    break;

            case ItemID.None:
            default:
                break;
        }
    }




}






