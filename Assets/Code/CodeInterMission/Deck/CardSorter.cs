using System.Collections.Generic;
using UnityEngine;

public class CardSorter
{
    public static List<CardData> MergeSort(List<CardData> cards)
    {
        if (cards.Count <= 1) return cards;

        int mid = cards.Count / 2;
        var left = MergeSort(cards.GetRange(0, mid));
        var right = MergeSort(cards.GetRange(mid, cards.Count - mid));

        return Merge(left, right);
    }

    private static List<CardData> Merge(List<CardData> left, List<CardData> right)
    {
        List<CardData> result = new List<CardData>();
        int i = 0, j = 0;

        while (i < left.Count && j < right.Count)
        {
            if (CardComparer.Compare(left[i], right[j]) <= 0)
            {
                result.Add(left[i]);
                i++;
            }
            else
            {
                result.Add(right[j]);
                j++;
            }
        }

        while (i < left.Count) result.Add(left[i++]);
        while (j < right.Count) result.Add(right[j++]);

        return result;
    }
}
