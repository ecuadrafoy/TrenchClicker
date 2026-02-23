using UnityEngine;
using System.Collections.Generic;


public class WeatherStationManager : MonoBehaviour
{
    public static WeatherStationManager Instance { get; private set; }
    private int currentLevel = 0;
    private readonly int[] levelCosts = { 500, 2000, 5000 };
    [SerializeField] private int requiredLevel = 5;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public int GetLevel() => currentLevel;
    public int GetNextLevelCost()
    {
        if (currentLevel >= levelCosts.Length) return 0;
        return levelCosts[currentLevel];
    }
    public bool CanUpgrade()
    {
        if (currentLevel >= levelCosts.Length) return false;
        if (ProgressionManager.Instance != null && ProgressionManager.Instance.GetLevel() < requiredLevel) return false;
        int cost = levelCosts[currentLevel];
        return GameManager.Instance.GetRequisitionPoints() >= cost;
    }
    public bool TryUpgrade()
    {
        if (!CanUpgrade()) return false;
        if (ProgressionManager.Instance != null && ProgressionManager.Instance.GetLevel() < requiredLevel) return false;
        int cost = levelCosts[currentLevel];
        GameManager.Instance.SpendRequisitionPoints(cost);
        currentLevel++;
        Debug.Log($"Weather Station upgraded to level {currentLevel}");
        return true;
    }
    public string GetForecast(out ForecastRisk risk)
    {
        risk = ForecastRisk.Low;
        if (currentLevel == 0) return "";
        List<WeatherTransition> table = WeatherManager.Instance.GetWeatherTable();
        if (table == null || table.Count == 0) return "";
        float currentTime = GameManager.Instance.GetCurrentAssaultTime();
        float timeUntilChange = WeatherManager.Instance.GetTimeUntilNextChange(currentTime);
        string countdownPrefix = timeUntilChange > 0f ? $"Next change in {Mathf.CeilToInt(timeUntilChange)}s: " : "No further changes";
        float windowEnd = currentTime + GameManager.Instance.GetAssaultDuration() / 3f;
        // Calculate fraction of upcoming 30s window spent in rain states
        float rainTime = 0f;
        float totalTime = 0f;
        for (int i = 0; i < table.Count; i++)
        {
            float entryStart = table[i].timestamp;
            float entryEnd = (i + 1 < table.Count) ? table[i + 1].timestamp : windowEnd;

            //Clamp to our look-ahead window
            float segStart = Mathf.Max(entryStart, currentTime);
            float segEnd = Mathf.Min(entryEnd, windowEnd);
            if (segStart >= segEnd) continue;
            if (segStart >= windowEnd) break;
            float duration = segEnd - segStart;
            totalTime += duration;
            WeatherState state = table[i].weather;
            if (state == WeatherState.LightRain || state == WeatherState.HeavyRain)
            {
                rainTime += duration;
            }
        }
        //Classify risk
        float rainFraction = totalTime > 0 ? rainTime / totalTime : 0f;
        if (rainFraction > 0.5f) risk = ForecastRisk.High;
        else if (rainFraction > 0.2f) risk = ForecastRisk.Medium;
        else risk = ForecastRisk.Low;
        //Build message based on level
        switch (currentLevel)
        {
            case 1:
                if (risk == ForecastRisk.High) return countdownPrefix + "Sky looks threatening";
                else if (risk == ForecastRisk.Medium) return countdownPrefix + "Clouds gathering..";
                else return countdownPrefix + "Weather looks stable.";
            case 2:
                return countdownPrefix + $"Weather Risk: {risk}";
            case 3:
                float avgEffectiveness = CalculateAverageEffectiveness(table, currentTime, windowEnd);
                int rainPercent = Mathf.RoundToInt(rainFraction * 100f);
                return countdownPrefix + $"{rainPercent}% chance of rain | Avg Effectiveness: {Mathf.RoundToInt(avgEffectiveness * 100f)}%";
            default:
                return "";
        }
    }
    private float CalculateAverageEffectiveness(List<WeatherTransition> table, float from, float to)
    {
        float totalWeightedEff = 0f;
        float totalTime = 0f;
        for (int i = 0; i < table.Count; i++)
        {
            float entryStart = table[i].timestamp;
            float entryEnd = (i + 1 < table.Count) ? table[i + 1].timestamp : to;
            float segStart = Mathf.Max(entryStart, from);
            float segEnd = Mathf.Min(entryEnd, to);
            if (segStart >= segEnd) continue;
            if (segStart >= to) break;
            float duration = segEnd - segStart;
            totalTime += duration;
            totalWeightedEff += duration * WeatherConfig.GetEffectiveness(table[i].weather);
        }
        return totalTime > 0f ? totalWeightedEff / totalTime : 1f;
    }

    public int GetRequiredLevel() => requiredLevel;
}
