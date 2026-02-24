using UnityEngine;

public class TileClickable : MonoBehaviour
{
    [Header("Settings")]
    public Renderer targetRenderer;

    [Header("Highlight Config")]
    public Color highlightColor = Color.green; // เลือกสี (เช่น สีเขียว)
    [Range(0f, 1f)] 
    public float transparency = 0.5f; // 0.0 = หายไปเลย, 0.5 = โปร่งครึ่งนึง, 1.0 = ทึบตัน
    
    // ตัวแปรภายใน
    private Material originalMat;
    private Material highlightMat;
    private bool isSelectable = false;

    void Start()
    {
        // 1. หา Renderer
        if (targetRenderer == null) targetRenderer = GetComponent<Renderer>();
        if (targetRenderer == null) targetRenderer = GetComponentInChildren<Renderer>();

        if (targetRenderer != null)
        {
            // 2. จำ Material เดิม
            originalMat = targetRenderer.material;

            // 3. สร้าง Material ใหม่ โดยบังคับใช้ Shader ที่โปร่งแสงได้แน่นอน
            // "Sprites/Default" รองรับการเปลี่ยนสีและความโปร่งใสได้ดีที่สุดสำหรับกรณีนี้
            highlightMat = new Material(Shader.Find("Sprites/Default"));
            
            // พยายามก๊อปปี้รูปเดิมมาแปะด้วย (ถ้ามี) จะได้เห็นลายพื้นเดิม
            if (originalMat.HasProperty("_MainTex"))
            {
                highlightMat.mainTexture = originalMat.mainTexture;
            }
            else if (originalMat.HasProperty("_BaseMap")) // สำหรับ URP
            {
                highlightMat.mainTexture = originalMat.GetTexture("_BaseMap");
            }
        }
        else
        {
            Debug.LogError($"❌ [TileClickable] หา Renderer ไม่เจอใน {name}");
        }
    }

    public void SetSelectable(bool active)
    {
        isSelectable = active;
        
        if (targetRenderer != null && highlightMat != null)
        {
            if (active)
            {
                // คำนวณสีพร้อมค่า Alpha (ความโปร่ง)
                Color finalColor = highlightColor;
                finalColor.a = transparency; // ใส่ค่าความโปร่งที่ตั้งไว้
                
                highlightMat.color = finalColor; // ตั้งค่าสี
                targetRenderer.material = highlightMat; // สลับเป็นอันใหม่
            }
            else
            {
                // คืนค่าอันเดิม
                targetRenderer.material = originalMat;
            }
        }
    }

    void OnMouseDown()
    {
        if (isSelectable)
        {
            RouteManager.Instance.OnTileClicked(this.transform);
        }
    }
}