using UnityEngine;

/// <summary>
/// กล้องควบคุมมุมแบบเกม 100% Orange Juice (Pan, Zoom, Rotate)
/// แนบไว้กับกล้องหลัก (Camera) หรือ empty parent ของ Camera
/// </summary>
public class BoardCameraController : MonoBehaviour
{
    [Header("Pan")]
    public float panSpeed = 10f;      // ความเร็วการเลื่อนกล้อง
    public float panBorder = 30f;     // ขอบหน้าจอที่ trigger pan

    [Header("Zoom")]
    public float zoomSpeed = 30f;
    public float minZoom = 10f;
    public float maxZoom = 60f;

    [Header("Rotate")]
    public float rotateSpeed = 120f;

    [Header("Limit")]
    public Vector2 panLimitMin = new Vector2(-30, -30);
    public Vector2 panLimitMax = new Vector2(30, 30);

    private Vector3 dragOrigin;
    private bool isDragging = false;

    void Update()
    {
        HandlePan();
        HandleRotate();
        HandleZoom();
    }

    void HandlePan()
    {
        Vector3 pos = transform.position;

        // ลากด้วยเมาส์กลาง (หรือขวา)
        if (Input.GetMouseButtonDown(2))
        {
            dragOrigin = Input.mousePosition;
            isDragging = true;
        }
        if (Input.GetMouseButtonUp(2)) isDragging = false;

        if (isDragging)
        {
            Vector3 delta = Input.mousePosition - dragOrigin;
            // เลื่อนกล้องตามแนวนอนและแนวตั้ง (บนแกน x,z)
            pos -= transform.right * delta.x * panSpeed * Time.deltaTime / 10f;
            pos -= transform.forward * delta.y * panSpeed * Time.deltaTime / 10f;
            pos.y = transform.position.y; // รักษาระยะสูงเดิม
            dragOrigin = Input.mousePosition;
        }

        // หรือจะเปิดขอบจอ (mouse to edge) แบบเกม RTS ก็ได้
        // if (Input.mousePosition.x >= Screen.width - panBorder) pos += transform.right * panSpeed * Time.deltaTime;
        // if (Input.mousePosition.x <= panBorder) pos -= transform.right * panSpeed * Time.deltaTime;
        // if (Input.mousePosition.y >= Screen.height - panBorder) pos += transform.forward * panSpeed * Time.deltaTime;
        // if (Input.mousePosition.y <= panBorder) pos -= transform.forward * panSpeed * Time.deltaTime;

        pos.x = Mathf.Clamp(pos.x, panLimitMin.x, panLimitMax.x);
        pos.z = Mathf.Clamp(pos.z, panLimitMin.y, panLimitMax.y);
        transform.position = pos;
    }

    void HandleRotate()
    {
        // หมุนด้วยปุ่ม Q/E
        float rot = 0f;
        if (Input.GetKey(KeyCode.Q)) rot += 1f;
        if (Input.GetKey(KeyCode.E)) rot -= 1f;

        if (rot != 0f)
        {
            transform.Rotate(Vector3.up, rot * rotateSpeed * Time.deltaTime, Space.World);
        }
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
            {
                cam.fieldOfView -= scroll * zoomSpeed;
                cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, minZoom, maxZoom);
            }
            else // หากกล้องอยู่ที่ object นี้เอง
            {
                Camera myCam = GetComponent<Camera>();
                if (myCam != null)
                {
                    myCam.fieldOfView -= scroll * zoomSpeed;
                    myCam.fieldOfView = Mathf.Clamp(myCam.fieldOfView, minZoom, maxZoom);
                }
            }
        }
    }
}