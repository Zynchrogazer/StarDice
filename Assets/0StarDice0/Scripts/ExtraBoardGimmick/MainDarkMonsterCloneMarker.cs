using UnityEngine;

/// <summary>
/// Marker สำหรับร่าง clone เฉพาะ MainDark
/// เก็บ reference ไปยังตัว original เพื่อให้ระบบ battle ลบเฉพาะ clone ที่ชนได้แบบ KISS
/// </summary>
public class MainDarkMonsterCloneMarker : MonoBehaviour
{
    [SerializeField] private PlayerState originalMonster;

    public PlayerState OriginalMonster => originalMonster;

    public void Initialize(PlayerState sourceMonster)
    {
        originalMonster = sourceMonster;
    }

    public static bool TryGet(GameObject target, out MainDarkMonsterCloneMarker marker)
    {
        marker = target != null ? target.GetComponent<MainDarkMonsterCloneMarker>() : null;
        return marker != null;
    }
}
