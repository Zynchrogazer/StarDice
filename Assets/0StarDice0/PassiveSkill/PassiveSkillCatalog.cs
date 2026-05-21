using UnityEngine;

public static class PassiveSkillCatalog
{
    private const string ResourcePath = "PassiveSkills";
    private static PassiveSkillData[] cache;

    public static PassiveSkillData[] GetAll()
    {
        if (cache != null)
            return cache;

        cache = Resources.LoadAll<PassiveSkillData>(ResourcePath);

        if (cache == null || cache.Length == 0)
        {
            Debug.LogWarning($"[PassiveSkillCatalog] No PassiveSkillData found at Resources/{ResourcePath}. Passive bonuses will not be applied.");
            cache = System.Array.Empty<PassiveSkillData>();
        }

        return cache;
    }

    public static void ClearCache()
    {
        cache = null;
    }
}
