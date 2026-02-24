using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [Header("Settings (Isometric View)")]
    // เราจะใช้ offset เพื่อหา "ระยะห่างเริ่มต้น" เท่านั้น ไม่ได้ใช้กำหนดทิศทางแล้ว
    public Vector3 offset = new Vector3(-8f, 12f, -8f);
    public float smoothSpeed = 5f;

    [Header("Zoom Settings")]
    public float zoomSpeed = 4f;
    public float minZoom = 0.5f;
    public float maxZoom = 2.0f;
    private float currentZoomMultiplier = 1f;

    [Header("Angle Settings")]
    [Range(0, 90)] public float rotationX = 45f;
    [Range(0, 360)] public float rotationY = 45f;

    // ✨ [เพิ่ม] ความไวในการหมุนกล้อง
    [Header("Rotation Settings")]
    public float rotateSpeed = 5f;

    [Header("State")]
    public Transform target;
    private bool isBattleScene = false;
    
    // ✨ [เพิ่ม] ตัวแปรเก็บระยะห่างพื้นฐาน (คำนวณจาก offset เดิม)
    private float defaultDistance; 

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // ✨ คำนวณระยะห่างเริ่มต้นจากค่า Offset ที่ตั้งไว้
        defaultDistance = offset.magnitude;
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
        if (!isBattleScene && target != null)
        {
            // ---------------------------------------------------------
            // 1. Logic การ Zoom (เหมือนเดิม)
            // ---------------------------------------------------------
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0f)
            {
                currentZoomMultiplier -= scroll * zoomSpeed;
                currentZoomMultiplier = Mathf.Clamp(currentZoomMultiplier, minZoom, maxZoom);
            }

            // ---------------------------------------------------------
            // 2. ✨ [เพิ่ม] Logic การหมุนกล้อง (Orbit) เมื่อกดคลิกขวาค้าง
            // ---------------------------------------------------------
            if (Input.GetMouseButton(1)) // 0=ซ้าย, 1=ขวา, 2=กลาง
            {
                float mouseX = Input.GetAxis("Mouse X") * rotateSpeed;
                float mouseY = Input.GetAxis("Mouse Y") * rotateSpeed;

                // หมุนแนวนอน (Y-Axis)
                rotationY += mouseX;

                // หมุนแนวตั้ง (X-Axis) - ลบออกเพื่อให้ลากลงแล้วเงยหน้า (Invert) หรือบวกตามความถนัด
                rotationX -= mouseY; 
                
                // จำกัดมุมก้มเงย ไม่ให้ทะลุพื้น หรือตีลังกา
                rotationX = Mathf.Clamp(rotationX, 10f, 85f);
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

        // ---------------------------------------------------------
        // 3. ✨ [แก้ไข] การคำนวณตำแหน่งใหม่ (Orbit Calculation)
        // ---------------------------------------------------------
        
        // ก. คำนวณระยะทางปัจจุบัน (ระยะเริ่มต้น * ตัวคูณซูม)
        float currentDist = defaultDistance * currentZoomMultiplier;

        // ข. สร้าง Rotation จากค่า rotationX, rotationY ที่เราปรับได้
        Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0);

        // ค. คำนวณตำแหน่งที่กล้องควรอยู่
        // สูตร: ตำแหน่งเป้าหมาย - (ทิศทางตามมุมหมุน * ระยะห่าง)
        // การคูณ Quaternion * Vector3.forward คือการหาทิศทางข้างหน้าของมุมนั้นๆ
        // เราใช้ -Vector3.forward (คือถอยหลัง) เพื่อวางกล้องไว้ข้างหลังเป้าหมายตามระยะห่าง
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -currentDist);
        Vector3 desiredPosition = target.position + rotation * negDistance;

        // ง. Smooth Movement
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        // จ. บังคับให้กล้องหันหน้ามองเป้าหมายเสมอ (หรือใช้ transform.rotation = rotation ก็ได้)
        transform.LookAt(target);
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