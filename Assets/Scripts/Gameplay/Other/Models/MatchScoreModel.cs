using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MatchScoreModel
{
    private Dictionary<IMoneyCollector, int> scores = new();
    public event Action<IMoneyCollector, int> OnScoreChanged;

    public void RegisterCollector(IMoneyCollector collector)
    {
        if (!scores.ContainsKey(collector))
        {
            scores[collector] = 0;
        }
    }

    public void AddScore(IMoneyCollector collector, int amount)
    {
        if (!scores.ContainsKey(collector)) return;

        scores[collector] = Mathf.Max(0, scores[collector] + amount);
        OnScoreChanged?.Invoke(collector, scores[collector]);
    }

    public int GetScore(IMoneyCollector collector) => scores.TryGetValue(collector, out var score) ? score : 0;
    public IReadOnlyCollection<IMoneyCollector> GetAllCollectors() => scores.Keys;

    public bool TryGetWinners(out List<IMoneyCollector> winners)
    {
        winners = new List<IMoneyCollector>();

        if (scores.Count == 0) return false;

        int maxScore = scores.Values.Max();

        winners = scores.Where(kvp => kvp.Value == maxScore).Select(kvp => kvp.Key).ToList();

        if (scores.Count > 1 && winners.Count == scores.Count)
        {
            winners.Clear();
            return false;
        }

        return true;
    }

    public void Clear() => scores.Clear();
}