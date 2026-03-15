using UnityEngine;

public class TileClickable : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("ปล่อยว่างไว้ได้เลย! ระบบจะหา Renderer ทั้งหมดในตัวมันและลูกๆ ให้อัตโนมัติ")]
    public Renderer[] targetRenderers;

    [Header("Highlight Config")]
    public Color highlightColor = Color.green; // เลือกสี
    [Range(0f, 1f)] 
    public float transparency = 0.5f; 
    
    // ต้องเปลี่ยนมาเก็บ Material เป็น Array เพื่อให้ตรงกับจำนวน Renderer
    private Material[] originalMats;
    private Material[] highlightMats;
    private bool isSelectable = false;

    void Start()
    {
        // 1. ถ้าขี้เกียจลากใส่ ให้ระบบดึง Renderer ทั้งหมดในตัวมันและลูกๆ มาให้อัตโนมัติ!
        if (targetRenderers == null || targetRenderers.Length == 0)
        {
            targetRenderers = GetComponentsInChildren<Renderer>();
        }

        if (targetRenderers.Length > 0)
        {
            // เตรียม Array ให้มีขนาดเท่ากับจำนวน Renderer ที่หาเจอ
            originalMats = new Material[targetRenderers.Length];
            highlightMats = new Material[targetRenderers.Length];

            for (int i = 0; i < targetRenderers.Length; i++)
            {
                Renderer r = targetRenderers[i];
                
                // 2. จำ Material เดิมของแต่ละชิ้น
                originalMats[i] = r.material;

                // 3. สร้าง Material ใหม่ให้แต่ละชิ้น
                highlightMats[i] = new Material(Shader.Find("Sprites/Default"));
                
                // ก๊อปปี้ Texture เดิม
                if (originalMats[i].HasProperty("_MainTex"))
                {
                    highlightMats[i].mainTexture = originalMats[i].mainTexture;
                }
                else if (originalMats[i].HasProperty("_BaseMap"))
                {
                    highlightMats[i].mainTexture = originalMats[i].GetTexture("_BaseMap");
                }
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ [TileClickable] หา Renderer ไม่เจอเลยใน {name} (อาจจะเป็น GameObject เปล่าๆ)");
        }
    }

    public void SetSelectable(bool active)
    {
        isSelectable = active;
        
        if (targetRenderers == null || targetRenderers.Length == 0) return;

        // วนลูปเปลี่ยนสีให้ครบทุก Renderer
        for (int i = 0; i < targetRenderers.Length; i++)
        {
            Renderer r = targetRenderers[i];
            if (r == null || highlightMats[i] == null) continue;

            if (active)
            {
                Color finalColor = highlightColor;
                finalColor.a = transparency;
                
                highlightMats[i].color = finalColor; 
                r.material = highlightMats[i]; 
            }
            else
            {
                // คืนค่าอันเดิม
                r.material = originalMats[i];
            }
        }
    }

    void OnMouseDown()
    {
        if (isSelectable)
        {
            if (RouteManager.TryGet(out var routeManager)) routeManager.OnTileClicked(this.transform);
        }
    }
}