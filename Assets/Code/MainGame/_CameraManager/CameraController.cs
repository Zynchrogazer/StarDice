using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [Header("Settings (Isometric View)")]
    // ค่ามาตรฐาน: X=-8, Y=12, Z=-8
    public Vector3 offset = new Vector3(-8f, 12f, -8f);
    public float smoothSpeed = 5f;

    [Header("Zoom Settings")]
    public float zoomSpeed = 4f;   // ความไวในการซูม
    public float minZoom = 0.5f;   // ซูมเข้าได้ใกล้สุดแค่ไหน (ค่าน้อย = ใกล้)
    public float maxZoom = 2.0f;   // ซูมออกได้ไกลสุดแค่ไหน (ค่ามาก = ไกล)
    private float currentZoomMultiplier = 1f; // ตัวคูณระยะปัจจุบัน (1 = ปกติ)

    [Header("Angle Settings")]
    [Range(0, 90)] public float rotationX = 45f; // มุมก้ม
    [Range(0, 360)] public float rotationY = 45f; // มุมหันข้าง

    [Header("State")]
    public Transform target;
    private bool isBattleScene = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RouteManager boardSystem = FindObjectOfType<RouteManager>();
        bool isBoardGameScene = (boardSystem != null);

        Camera myCam = GetComponent<Camera>();
        AudioListener myAudio = GetComponent<AudioListener>();

        if (isBoardGameScene)
        {
            isBattleScene = false;
            if (myCam != null) myCam.enabled = true;
            if (myAudio != null) myAudio.enabled = true;
            FindTarget();
        }
        else
        {
            isBattleScene = true;
            target = null;
            if (myCam != null) myCam.enabled = false;
            if (myAudio != null) myAudio.enabled = false;
        }
    }

    private void Update()
    {
        // ✨ รับค่าการ Zoom จาก Mouse Scroll Wheel
        // (ทำใน Update เพื่อความลื่นไหลของการรับ Input)
        if (!isBattleScene && target != null)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");

            // ถ้ามีการเลื่อนลูกกลิ้ง
            if (scroll != 0f)
            {
                // Scroll Up (ค่า +) = Zoom In (ลดตัวคูณ)
                // Scroll Down (ค่า -) = Zoom Out (เพิ่มตัวคูณ)
                currentZoomMultiplier -= scroll * zoomSpeed;

                // จำกัดค่าไม่ให้ซูมใกล้/ไกลเกินไป
                currentZoomMultiplier = Mathf.Clamp(currentZoomMultiplier, minZoom, maxZoom);
            }
        }
    }

    private void LateUpdate()
    {
        if (isBattleScene) return;

        if (target == null)
        {
            FindTarget();
            return;
        }

        // 1. คำนวณตำแหน่ง (Position) พร้อมคูณค่า Zoom เข้าไป
        // ✨ สูตร: ระยะทางเดิม * ตัวคูณ Zoom
        Vector3 finalOffset = offset * currentZoomMultiplier;

        Vector3 desiredPosition = target.position + finalOffset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        // 2. คำนวณการหมุน (Rotation)
        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0);
    }

    public void FindTarget()
    {
        if (GameTurnManager.Instance != null && GameTurnManager.CurrentPlayer != null)
        {
            target = GameTurnManager.CurrentPlayer.transform;
        }
        else
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) target = p.transform;
        }
    }
}