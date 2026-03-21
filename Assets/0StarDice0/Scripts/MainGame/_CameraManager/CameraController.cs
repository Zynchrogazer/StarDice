using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour
{
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
    public float rotateSpeed = 120f; // ความไวตอนคลิกขวาค้างเพื่อหมุนกล้อง
    [Range(15, 85)] public float minRotationX = 25f;
    [Range(15, 85)] public float maxRotationX = 70f;

    [Header("Pan Settings")]
    public float middleMousePanSpeed = 0.03f; // ความไวลากกล้องด้วยเมาส์กลาง
    public float keyboardPanSpeed = 8f; // ความไวเลื่อนกล้องด้วยปุ่มลูกศร
    public float maxPanDistance = 10f; // ระยะที่เลื่อนได้สูงสุดจากตัวละครที่กำลังเล่น

    [Header("State")]
    public Transform target;
    private bool isBattleScene = false;
    private Vector3 panOffset = Vector3.zero;

    private void Awake()
    {
        if (FindObjectsOfType<CameraController>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

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
            panOffset = Vector3.zero;
            if (myCam != null) myCam.enabled = true;
            if (myAudio != null) myAudio.enabled = true;
            FindTarget();
        }
        else
        {
            isBattleScene = true;
            target = null;
            panOffset = Vector3.zero;
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

            // คลิกขวาค้าง + ลากเมาส์เพื่อหมุนมุมกล้อง
            if (Input.GetMouseButton(1))
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");

                rotationY += mouseX * rotateSpeed * Time.deltaTime;
                rotationX -= mouseY * rotateSpeed * Time.deltaTime;

                rotationY = Mathf.Repeat(rotationY, 360f);
                rotationX = Mathf.Clamp(rotationX, minRotationX, maxRotationX);
            }

            HandlePanInput();
        }
    }

    private void HandlePanInput()
    {
        Quaternion yawRotation = Quaternion.Euler(0f, rotationY, 0f);
        Vector3 right = yawRotation * Vector3.right;
        Vector3 forward = yawRotation * Vector3.forward;

        Vector3 panDelta = Vector3.zero;

        // คลิกเมาส์กลางค้าง + ลากเพื่อเลื่อนมุมมอง
        if (Input.GetMouseButton(2))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            panDelta += (-right * mouseX - forward * mouseY) * middleMousePanSpeed;
        }

        // ปุ่มลูกศร/WASD สำหรับเลื่อนกล้องบนระนาบบอร์ด
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");
        if (inputX != 0f || inputY != 0f)
        {
            panDelta += (right * inputX + forward * inputY) * keyboardPanSpeed * Time.deltaTime;
        }

        if (panDelta != Vector3.zero)
        {
            panOffset += panDelta;
            panOffset = Vector3.ClampMagnitude(panOffset, maxPanDistance);
        }
    }

    private void LateUpdate()
    {
        if (isBattleScene) return;

        // ซิงก์เป้าหมายกับเทิร์นปัจจุบันเสมอ
        // เพื่อให้กล้องสลับไปหา Player/AI ที่กำลังเล่นทันทีเมื่อเปลี่ยนเทิร์น
        if (GameTurnManager.CurrentPlayer != null && target != GameTurnManager.CurrentPlayer.transform)
        {
            target = GameTurnManager.CurrentPlayer.transform;
            panOffset = Vector3.zero;
        }

        if (target == null)
        {
            FindTarget();
            return;
        }

        // 1. คำนวณตำแหน่ง (Position) พร้อมคูณค่า Zoom เข้าไป
        // ✨ สูตร: ระยะทางเดิม * ตัวคูณ Zoom
        Vector3 finalOffset = offset * currentZoomMultiplier;

        Vector3 desiredPosition = target.position + finalOffset + panOffset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        // 2. คำนวณการหมุน (Rotation)
        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0);
    }

    public void FindTarget()
    {
        if (GameTurnManager.CurrentPlayer != null)
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
