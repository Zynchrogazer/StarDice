using UnityEngine;

public class PlayerUIController : MonoBehaviour
{
    [Header("Preferred explicit registry")]
    [SerializeField] private ElementStatusPanelRegistry elementPanelRegistry;

    [Header("Legacy/Fallback UI References")]
    [SerializeField] private PlayerStatusPanelRefs fallbackPanelRefs;
    [SerializeField] private PlayerGlobalHudRefs globalHudRefs;

    [Header("Debuff Presentation")]
    public GameObject debuffIconPrefab;
    public Vector2 debuffIconSize = new Vector2(32f, 32f);
    public Sprite burnDebuffSprite;
    public Sprite iceDebuffSprite;

    private PlayerState myPlayer;
    private ElementButtonManager elementButtonManager;
    private Transform boundStatusRoot;
    private PlayerStatusPanelRefs boundPanelRefs;
    private PlayerDebuffPresenter debuffPresenter;

    private void Awake()
    {
        debuffPresenter = new PlayerDebuffPresenter(debuffIconPrefab, debuffIconSize, burnDebuffSprite, iceDebuffSprite);
    }

    private void Update()
    {
        EnsureBindings();

        if (myPlayer == null)
        {
            FindHumanPlayer();
            return;
        }

        RefreshUI();
    }

    private void FindHumanPlayer()
    {
        if (GameTurnManager.TryGet(out var gameTurnManager) && gameTurnManager.allPlayers != null)
        {
            // วนหาในลิสต์ผู้เล่นทั้งหมด
            foreach (var p in gameTurnManager.allPlayers)
            {
                // เงื่อนไข: เอาตัวที่ไม่ใช่ null และ ไม่ใช่ AI
                if (p != null && !p.isAI)
                {
                    myPlayer = p;
                    Debug.Log($"[UI] 🔒 ล็อคการแสดงผลที่ผู้เล่น: {myPlayer.name}");
                    break; // เจอแล้วหยุดหาเลย
                }
            }
        }
    }

    private void RefreshUI()
    {
        if (boundPanelRefs == null)
        {
            PlayerGlobalHudPresenter.Present(globalHudRefs, myPlayer);
            return;
        }

        PlayerStatsPanelPresenter.Present(boundPanelRefs, myPlayer);
        PlayerGlobalHudPresenter.Present(globalHudRefs, myPlayer);
        debuffPresenter.Present(globalHudRefs, myPlayer);
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
            BindPanelRefs(activeStatusRoot);
        else if (fallbackPanelRefs != null)
            boundPanelRefs = fallbackPanelRefs;
    }

    private void RebindIfStatusRootChanged(Transform activeStatusRoot)
    {
        if (boundStatusRoot == activeStatusRoot)
            return;

        boundStatusRoot = activeStatusRoot;
        boundPanelRefs = null;
        debuffPresenter.ResetBindings();
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
        {
            panelRefs = statusRoot.gameObject.AddComponent<PlayerStatusPanelRefs>();
        }

        panelRefs.BindFromRoot(statusRoot);
        boundPanelRefs = panelRefs.HasCoreBindings() ? panelRefs : fallbackPanelRefs;
    }
}
