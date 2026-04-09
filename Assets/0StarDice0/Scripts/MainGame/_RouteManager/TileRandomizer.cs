using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public struct TileRandomizerSettings
{
    public TileRandomMode mode;
    public List<TileRandomLimit> tileRandomLimits;
    public TileType fallbackTileType;
    public bool useLimitAllowList;
    public List<TileType> limitAllowedTypes;
    public bool excludeLockedTilesFromLimitCounts;
    public bool validateInvariantsAfterRandom;
    public int maxRandomizeAttempts;
    public List<TileInvariantRule> tileInvariantRules;
}

public sealed class TileRandomizer
{
    public bool Randomize(
        List<NodeConnection> allNodes,
        List<NodeConnection> unlockedNodes,
        List<TileType> originalUnlockedTypes,
        TileRandomizerSettings settings,
        System.Random rng,
        Func<TileType, string> eventNameResolver)
    {
        int attempts = Mathf.Max(1, settings.maxRandomizeAttempts);
        for (int attempt = 1; attempt <= attempts; attempt++)
        {
            RestoreUnlockedTypes(unlockedNodes, originalUnlockedTypes, eventNameResolver);

            if (settings.mode == TileRandomMode.FullShuffle)
            {
                ApplyFullShuffle(unlockedNodes, rng, eventNameResolver);
            }
            else
            {
                ApplyLimitShuffle(allNodes, unlockedNodes, settings, rng, eventNameResolver);
            }

            if (!settings.validateInvariantsAfterRandom || ValidateTileInvariants(allNodes, settings.tileInvariantRules))
            {
                return true;
            }
        }

        RestoreUnlockedTypes(unlockedNodes, originalUnlockedTypes, eventNameResolver);
        return false;
    }

    private void RestoreUnlockedTypes(List<NodeConnection> unlockedNodes, List<TileType> originalUnlockedTypes, Func<TileType, string> eventNameResolver)
    {
        for (int i = 0; i < unlockedNodes.Count && i < originalUnlockedTypes.Count; i++)
        {
            unlockedNodes[i].type = originalUnlockedTypes[i];
            unlockedNodes[i].eventName = eventNameResolver(originalUnlockedTypes[i]);
        }
    }

    private void ApplyFullShuffle(List<NodeConnection> unlockedNodes, System.Random rng, Func<TileType, string> eventNameResolver)
    {
        List<TileType> types = unlockedNodes.Select(nc => nc.type).ToList();

        for (int i = types.Count - 1; i > 0; i--)
        {
            int randomIndex = rng.Next(0, i + 1);
            TileType temp = types[i];
            types[i] = types[randomIndex];
            types[randomIndex] = temp;
        }

        for (int i = 0; i < unlockedNodes.Count; i++)
        {
            unlockedNodes[i].type = types[i];
            unlockedNodes[i].eventName = eventNameResolver(types[i]);
        }
    }

    private void ApplyLimitShuffle(
        List<NodeConnection> allNodes,
        List<NodeConnection> unlockedNodes,
        TileRandomizerSettings settings,
        System.Random rng,
        Func<TileType, string> eventNameResolver)
    {
        Dictionary<TileType, int> maxByType = new Dictionary<TileType, int>();
        if (settings.tileRandomLimits != null)
        {
            for (int i = 0; i < settings.tileRandomLimits.Count; i++)
            {
                TileRandomLimit limit = settings.tileRandomLimits[i];
                maxByType[limit.type] = limit.maxCount;
            }
        }

        IEnumerable<NodeConnection> countedNodes = settings.excludeLockedTilesFromLimitCounts
            ? unlockedNodes
            : allNodes.Where(nc => nc != null && nc.node != null);

        Dictionary<TileType, int> currentCounts = countedNodes
            .GroupBy(nc => nc.type)
            .ToDictionary(g => g.Key, g => g.Count());

        List<NodeConnection> randomOrderNodes = unlockedNodes.OrderBy(_ => rng.Next()).ToList();
        System.Array allTypes = Enum.GetValues(typeof(TileType));

        HashSet<TileType> allowSet = new HashSet<TileType>();
        if (settings.useLimitAllowList && settings.limitAllowedTypes != null)
        {
            for (int i = 0; i < settings.limitAllowedTypes.Count; i++)
            {
                allowSet.Add(settings.limitAllowedTypes[i]);
            }
        }

        foreach (NodeConnection node in randomOrderNodes)
        {
            List<TileType> allowedTypes = new List<TileType>();
            foreach (TileType tileType in allTypes)
            {
                if (settings.useLimitAllowList && !allowSet.Contains(tileType))
                {
                    continue;
                }

                int currentCount = currentCounts.ContainsKey(tileType) ? currentCounts[tileType] : 0;
                if (!maxByType.TryGetValue(tileType, out int maxCount) || currentCount < maxCount)
                {
                    allowedTypes.Add(tileType);
                }
            }

            TileType selectedType = allowedTypes.Count > 0
                ? allowedTypes[rng.Next(0, allowedTypes.Count)]
                : settings.fallbackTileType;

            if (currentCounts.ContainsKey(node.type))
            {
                currentCounts[node.type] = Mathf.Max(0, currentCounts[node.type] - 1);
            }

            node.type = selectedType;
            node.eventName = eventNameResolver(selectedType);
            currentCounts[selectedType] = (currentCounts.ContainsKey(selectedType) ? currentCounts[selectedType] : 0) + 1;
        }
    }

    private bool ValidateTileInvariants(List<NodeConnection> allNodes, List<TileInvariantRule> rules)
    {
        if (rules == null || rules.Count == 0)
        {
            return true;
        }

        Dictionary<TileType, int> counts = allNodes
            .Where(nc => nc != null && nc.node != null)
            .GroupBy(nc => nc.type)
            .ToDictionary(g => g.Key, g => g.Count());

        for (int i = 0; i < rules.Count; i++)
        {
            TileInvariantRule rule = rules[i];
            int current = counts.ContainsKey(rule.type) ? counts[rule.type] : 0;
            int max = rule.maxCount < 0 ? int.MaxValue : rule.maxCount;
            if (current < rule.minCount || current > max)
            {
                return false;
            }
        }

        return true;
    }
}
