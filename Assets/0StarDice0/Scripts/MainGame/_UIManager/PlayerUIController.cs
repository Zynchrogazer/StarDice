using UnityEngine;

public class PlayerUIController : MonoBehaviour
{
    private enum UiBootstrapPhase
    {
        WaitingBoardReady,
        ResolvePlayer,
        ResolveBindings,
        ResolveAggregator,
        RefreshStats,
        Ready
    }

    [Header("Preferred explicit registry")]
    [SerializeField] private ElementStatusPanelRegistry elementPanelRegistry;

    [Header("Legacy/Fallback UI References")]
    [SerializeField] private PlayerStatusPanelRefs fallbackPanelRefs;
    [SerializeField] private PlayerGlobalHudRefs globalHudRefs;

    private PlayerState myPlayer;
    private ElementButtonManager elementButtonManager;
    private PlayerStatAggregator playerStatAggregator;
    private Transform boundStatusRoot;
    private PlayerStatusPanelRefs boundPanelRefs;
    private UiBootstrapPhase phase = UiBootstrapPhase.WaitingBoardReady;

    private void OnEnable()
    {
        ResetBootstrap(UiBootstrapPhase.ResolvePlayer);
    }

    private void OnDisable()
    {
    }

    private void Update()
    {
        if (ShouldRestartBootstrapForBoardEntry())
            ResetBootstrap(UiBootstrapPhase.ResolvePlayer);

        AdvanceBootstrap();
        if (phase == UiBootstrapPhase.Ready)
            RefreshUI();
    }

    private bool ShouldRestartBootstrapForBoardEntry()
    {
        if (!GameTurnManager.TryGet(out var turnManager) || turnManager == null)
            return false;

        return turnManager.TryConsumeBoardEntryBootstrap();
    }

    private void ResetBootstrap(UiBootstrapPhase startPhase)
    {
        phase = startPhase;
        boundStatusRoot = null;
        boundPanelRefs = null;
    }

    private void AdvanceBootstrap()
    {
        switch (phase)
        {
            case UiBootstrapPhase.WaitingBoardReady:
                return;

            case UiBootstrapPhase.ResolvePlayer:
                if (!TryResolveHumanPlayer())
                    return;
                phase = UiBootstrapPhase.ResolveBindings;
                return;

            case UiBootstrapPhase.ResolveBindings:
                EnsureBindings();
                if (boundPanelRefs == null || !boundPanelRefs.HasCoreBindings())
                    return;
                phase = UiBootstrapPhase.ResolveAggregator;
                return;

            case UiBootstrapPhase.ResolveAggregator:
                if (!TryResolveAggregator())
                    return;
                phase = UiBootstrapPhase.RefreshStats;
                return;

            case UiBootstrapPhase.RefreshStats:
                playerStatAggregator.RefreshPlayerStats(myPlayer, myPlayer.selectedPlayerPreset);
                phase = UiBootstrapPhase.Ready;
                return;

            case UiBootstrapPhase.Ready:
                return;
        }
    }

    private bool TryResolveHumanPlayer()
    {
        if (myPlayer != null)
            return true;

        if (!GameTurnManager.TryGet(out var gameTurnManager) || gameTurnManager.allPlayers == null)
            return false;

        foreach (var p in gameTurnManager.allPlayers)
        {
            if (p == null || p.isAI) continue;
            myPlayer = p;
            Debug.Log($"[UI] 🔒 ล็อคการแสดงผลที่ผู้เล่น: {myPlayer.name}");
            return true;
        }

        return false;
    }

    private bool TryResolveAggregator()
    {
        if (playerStatAggregator != null)
            return true;

        playerStatAggregator = FindFirstObjectByType<PlayerStatAggregator>();
        return playerStatAggregator != null;
    }

    private void RefreshUI()
    {
        if (myPlayer == null)
            return;

        if (boundPanelRefs != null)
            PlayerStatsPanelPresenter.Present(boundPanelRefs, myPlayer);

        PlayerGlobalHudPresenter.Present(globalHudRefs, myPlayer);
        SyncSimpleDebuffUI();
    }

    private void SyncSimpleDebuffUI()
    {
        if (globalHudRefs == null)
            return;

        SimpleDebuffUI simpleDebuffUI = globalHudRefs.ResolveSimpleDebuffUI();
        if (simpleDebuffUI == null)
            return;

        if (simpleDebuffUI.playerState != myPlayer)
            simpleDebuffUI.playerState = myPlayer;
    }

    private void EnsureBindings()
    {
        Transform activeStatusRoot = ResolveActiveStatusRoot();
        RebindIfStatusRootChanged(activeStatusRoot);
        if (boundPanelRefs != null && boundPanelRefs.HasCoreBindings())
            return;

        ElementType selectedElement = ResolveSelectedElement();
        if (elementPanelRegistry != null && elementPanelRegistry.TryGetPanelRefs(selectedElement, out var explicitPanelRefs))
        {
            boundPanelRefs = explicitPanelRefs;
        }
        else if (activeStatusRoot != null)
        {
            BindPanelRefs(activeStatusRoot);
        }
        else if (fallbackPanelRefs != null)
        {
            boundPanelRefs = fallbackPanelRefs;
        }
    }

    private void RebindIfStatusRootChanged(Transform activeStatusRoot)
    {
        if (boundStatusRoot == activeStatusRoot)
            return;

        boundStatusRoot = activeStatusRoot;
        boundPanelRefs = null;
    }

    private Transform ResolveActiveStatusRoot()
    {
        if (elementButtonManager == null || !elementButtonManager.gameObject.scene.IsValid())
            elementButtonManager = ResolvePreferredElementButtonManagerInstance();

        if (elementButtonManager == null)
            return null;

        ElementType selectedElement = ResolveSelectedElement();
        if (elementPanelRegistry != null && elementPanelRegistry.TryGetStatusRoot(selectedElement, out var registryRoot))
            return registryRoot;

        if (elementButtonManager.TryGetStatusRoot(selectedElement, out var selectedRoot))
            return selectedRoot;

        return elementButtonManager.GetActiveStatusRoot();
    }

    private ElementButtonManager ResolvePreferredElementButtonManagerInstance()
    {
        ElementButtonManager[] managers = FindObjectsByType<ElementButtonManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (managers == null || managers.Length == 0)
            return null;

        var myScene = gameObject.scene;

        foreach (var manager in managers)
        {
            if (manager == null) continue;
            if (manager.gameObject.scene == myScene)
                return manager;
        }

        return managers[0];
    }

    private ElementType ResolveSelectedElement()
    {
        if (GameData.Instance != null && GameData.Instance.selectedPlayer != null)
            return GameData.Instance.selectedPlayer.element;

        if (myPlayer != null && myPlayer.selectedPlayerPreset != null)
            return myPlayer.selectedPlayerPreset.element;

        return ElementType.Fire;
    }

    private void BindPanelRefs(Transform statusRoot)
    {
        if (statusRoot == null)
            return;

        PlayerStatusPanelRefs panelRefs = statusRoot.GetComponent<PlayerStatusPanelRefs>();
        if (panelRefs == null)
            panelRefs = statusRoot.gameObject.AddComponent<PlayerStatusPanelRefs>();

        panelRefs.BindFromRoot(statusRoot);
        boundPanelRefs = panelRefs.HasCoreBindings() ? panelRefs : fallbackPanelRefs;
    }
}
