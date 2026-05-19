using UnityEngine;

public class BoardgameNormaDebugButtons : MonoBehaviour
{
    [Header("Keyboard Debug Shortcuts")]
    [SerializeField] private bool enableKeyboardShortcuts = true;
    [SerializeField] private KeyCode fillNormaKey = KeyCode.F7;
    [SerializeField] private KeyCode forceBossPhaseKey = KeyCode.F8;

    private void Update()
    {
        if (!enableKeyboardShortcuts)
            return;

        if (Input.GetKeyDown(fillNormaKey))
            FillNormaRequirementNow();

        if (Input.GetKeyDown(forceBossPhaseKey))
            ForceNormaToBossPhase();
    }

    // F7: Rank up 1 ขั้น และเด้งเควสใหม่ตาม flow ปกติ
    public void FillNormaRequirementNow()
    {
        if (!TryGetNormaSystem(out var normaSystem))
            return;

        if (normaSystem.currentNormaRank >= normaSystem.maxNormaRank)
        {
            Debug.Log("[BoardgameNormaDebugButtons] Rank สูงสุดแล้ว ใช้ F8 เพื่อเรียกบอส");
            return;
        }

        normaSystem.NormaLevelUp();
        Debug.Log($"[BoardgameNormaDebugButtons] Force RankUp -> {normaSystem.currentNormaRank}/{normaSystem.maxNormaRank}");
    }

    // F8: ข้ามไป rank max + spawn boss โดยไม่สลับเทิร์น/ไม่เปลี่ยน state
    public void ForceNormaToBossPhase()
    {
        if (!TryGetNormaSystem(out var normaSystem))
            return;

        if (normaSystem.currentNormaRank >= normaSystem.maxNormaRank)
        {
            Debug.Log("[BoardgameNormaDebugButtons] อยู่ใน Boss phase แล้ว");
            return;
        }

        normaSystem.currentNormaRank = normaSystem.maxNormaRank;

        RouteManager route = FindFirstObjectByType<RouteManager>();
        if (route != null)
            route.SpawnBossTile();
        else
            Debug.LogWarning("[BoardgameNormaDebugButtons] ไม่พบ RouteManager, ยังไม่สามารถ Spawn Boss Tile ได้");

        NormaUIManager ui = FindFirstObjectByType<NormaUIManager>();
        if (ui != null)
            ui.UpdateInfoUI();

        Debug.Log($"[BoardgameNormaDebugButtons] Force boss phase complete at rank {normaSystem.currentNormaRank}/{normaSystem.maxNormaRank}");
    }

    private static bool TryGetNormaSystem(out NormaSystem normaSystem)
    {
        if (NormaSystem.TryGet(out normaSystem))
            return true;

        Debug.LogWarning("[BoardgameNormaDebugButtons] ไม่พบ NormaSystem");
        return false;
    }
}
