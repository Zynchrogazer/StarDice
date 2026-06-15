using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class RouteNodeSynchronizer
{
    public static bool ShouldSync(Transform root, List<NodeConnection> nodeConnections)
    {
        if (root == null || nodeConnections == null || nodeConnections.Count != root.childCount)
        {
            return true;
        }

        HashSet<Transform> childSet = new HashSet<Transform>();
        foreach (Transform child in root)
        {
            childSet.Add(child);
        }

        for (int i = 0; i < nodeConnections.Count; i++)
        {
            NodeConnection nc = nodeConnections[i];
            if (nc == null || nc.node == null || !childSet.Contains(nc.node))
            {
                return true;
            }
        }

        return false;
    }

    public static void Sync(
        Transform root,
        List<NodeConnection> nodeConnections,
        bool autoFillEventName,
        bool autoFillVisual,
        System.Action<NodeConnection> visualApplier)
    {
        Dictionary<Transform, NodeConnection> oldData = new Dictionary<Transform, NodeConnection>();
        if (nodeConnections != null)
        {
            foreach (NodeConnection nc in nodeConnections)
            {
                if (nc != null && nc.node != null)
                {
                    oldData[nc.node] = nc;
                }
            }
        }

        nodeConnections.Clear();

        List<Transform> children = new List<Transform>();
        foreach (Transform child in root)
        {
            children.Add(child);
        }

        children.Sort((a, b) => ExtractNumberFromName(a.name).CompareTo(ExtractNumberFromName(b.name)));

        foreach (Transform child in children)
        {
            NodeConnection nc = BuildNodeConnection(child, oldData, autoFillEventName);
            if (autoFillVisual)
            {
                visualApplier?.Invoke(nc);
            }

            nodeConnections.Add(nc);
        }
    }

    public static void AutoFillEventNames(List<NodeConnection> nodeConnections)
    {
        if (nodeConnections == null)
        {
            return;
        }

        foreach (NodeConnection nc in nodeConnections)
        {
            if (nc != null && RouteTileMetadata.ShouldAutoAssignEventName(nc.type, nc.eventName))
            {
                nc.eventName = RouteTileMetadata.GetDefaultEventName(nc.type);
            }
        }
    }

    public static int ExtractNumberFromName(string name)
    {
        Match match = Regex.Match(name, @"\d+");
        return match.Success && int.TryParse(match.Value, out int result) ? result : int.MaxValue;
    }

    private static NodeConnection BuildNodeConnection(Transform child, Dictionary<Transform, NodeConnection> oldData, bool autoFillEventName)
    {
        NodeConnection nc = new NodeConnection
        {
            node = child,
            tileID = ExtractNumberFromName(child.name)
        };

        if (oldData.TryGetValue(child, out NodeConnection saved))
        {
            nc.connectedNodes = saved.connectedNodes;
            nc.type = saved.type;
            nc.eventName = saved.eventName;
            nc.lockRandomType = saved.lockRandomType;
        }

        if (autoFillEventName && RouteTileMetadata.ShouldAutoAssignEventName(nc.type, nc.eventName))
        {
            nc.eventName = RouteTileMetadata.GetDefaultEventName(nc.type);
        }

        return nc;
    }
}
