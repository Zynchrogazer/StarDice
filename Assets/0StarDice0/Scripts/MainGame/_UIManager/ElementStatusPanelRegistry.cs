using System;
using UnityEngine;

[DisallowMultipleComponent]
public class ElementStatusPanelRegistry : MonoBehaviour
{
    [Serializable]
    public struct Entry
    {
        public ElementType element;
        public PlayerStatusPanelRefs panelRefs;
    }

    [SerializeField] private Entry[] entries;

    public bool TryGetPanelRefs(ElementType element, out PlayerStatusPanelRefs panelRefs)
    {
        panelRefs = null;
        if (entries == null)
            return false;

        for (int i = 0; i < entries.Length; i++)
        {
            Entry entry = entries[i];
            if (entry.element != element || entry.panelRefs == null)
                continue;

            panelRefs = entry.panelRefs;
            return true;
        }

        return false;
    }

    public bool TryGetStatusRoot(ElementType element, out Transform statusRoot)
    {
        statusRoot = null;
        if (!TryGetPanelRefs(element, out var panelRefs) || panelRefs == null)
            return false;

        statusRoot = panelRefs.transform;
        return statusRoot != null;
    }
}
