using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RockObstacleTrigger2D : MonoBehaviour
{
    [Tooltip("tileID ของช่องหิน ถ้าเป็น 0 จะพยายามอ่านจากชื่อ GameObject")]
    public int tileID;

    [Tooltip("ถ้าเปิด จะให้ชนได้เฉพาะวัตถุที่ Tag ตรงกับ playerTag")]
    public bool requirePlayerTag = true;

    [Tooltip("Tag ของผู้เล่น")]
    public string playerTag = "Player";

    private RouteManager routeManager;

    private void Awake()
    {
        RouteManager.TryGet(out routeManager);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        if (tileID <= 0 && routeManager != null)
        {
            tileID = routeManager.ExtractNumberFromName(gameObject.name);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (requirePlayerTag && !other.CompareTag(playerTag))
        {
            return;
        }

        PlayerPathWalker walker = other.GetComponent<PlayerPathWalker>();
        if (walker == null)
        {
            walker = other.GetComponentInParent<PlayerPathWalker>();
        }

        if (walker == null || tileID <= 0)
        {
            return;
        }

        bool brokeRock = walker.TryBreakRockAndBounceBack(tileID);
        if (brokeRock)
        {
            Debug.Log($"🪨 Trigger2D: {walker.name} ชนหิน tile {tileID} แล้วเด้งกลับ");
        }
    }
}
