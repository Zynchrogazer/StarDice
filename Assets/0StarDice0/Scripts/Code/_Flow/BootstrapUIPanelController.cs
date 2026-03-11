using UnityEngine;

[DisallowMultipleComponent]
public class BootstrapUIPanelController : MonoBehaviour
{
    [SerializeField] private GameObject firstMonsterSelectPanel;

    private static BootstrapUIPanelController cached;

    public static bool TryGet(out BootstrapUIPanelController controller)
    {
        controller = cached;
        if (controller != null)
        {
            return true;
        }

        controller = FindFirstObjectByType<BootstrapUIPanelController>(FindObjectsInactive.Include);
        cached = controller;
        return controller != null;
    }

    private void Awake()
    {
        if (cached != null && cached != this)
        {
            Destroy(gameObject);
            return;
        }

        cached = this;
    }

    private void OnDestroy()
    {
        if (cached == this)
        {
            cached = null;
        }
    }

    public bool OpenFirstMonsterSelectPanel()
    {
        if (firstMonsterSelectPanel == null)
        {
            return false;
        }

        firstMonsterSelectPanel.SetActive(true);
        return true;
    }

    public void CloseFirstMonsterSelectPanel()
    {
        if (firstMonsterSelectPanel == null)
        {
            return;
        }

        firstMonsterSelectPanel.SetActive(false);
    }
}
