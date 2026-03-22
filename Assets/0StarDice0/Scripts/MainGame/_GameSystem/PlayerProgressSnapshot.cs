using System;
using UnityEngine;

[Serializable]
public class PlayerProgressSnapshot
{
    [SerializeField] private int level = 1;
    [SerializeField] private int currentExp = 0;
    [SerializeField] private int maxExp = 100;

    public int Level => level;
    public int CurrentExp => currentExp;
    public int MaxExp => maxExp;

    public void Set(int newLevel, int newCurrentExp, int newMaxExp)
    {
        level = Mathf.Max(1, newLevel);
        currentExp = Mathf.Max(0, newCurrentExp);
        maxExp = Mathf.Max(1, newMaxExp);
    }
}
