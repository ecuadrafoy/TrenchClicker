using System.Collections.Generic;
using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    public static WeatherManager Instance { get; private set; }

    [Header("Weather Settings")]
    [SerializeField] private int minWeatherChanges = 4;
    [SerializeField] private int maxWeatherChanges = 6;

    private WeatherState currentWeather = WeatherState.Clear;
    private List<WeatherTransition> weatherTable;
    private int weatherTableIndex = 0;

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

    // --- Public API ---

    public WeatherState GetCurrentWeather() => currentWeather;
    public float GetEffectiveness() => WeatherConfig.GetEffectiveness(currentWeather);
    public float GetEliteEffectiveness() => WeatherConfig.GetEliteEffectiveness(currentWeather);
    public int GetWeatherTableCount() => weatherTable != null ? weatherTable.Count : 0;
    public int GetWeatherTableIndex() => weatherTableIndex;
    public List<WeatherTransition> GetWeatherTable() => weatherTable;
    public void SetWeather(WeatherState weather) => currentWeather = weather;
    public float GetTimeUntilNextChange(float currentAssaultTime)
    {
        if (weatherTable == null || weatherTable.Count == 0) return -1f;
        int nextIndex = weatherTableIndex + 1;
        if (nextIndex >= weatherTable.Count) return -1f;
        return weatherTable[nextIndex].timestamp - currentAssaultTime;
    }

    public void GenerateWeatherTable(float assaultDuration)
    {
        weatherTable = new List<WeatherTransition>();
        weatherTableIndex = 0;
        currentWeather = WeatherState.Clear;

        // First entry: Clear at time 0
        weatherTable.Add(new WeatherTransition(0f, WeatherState.Clear));

        WeatherState state = WeatherState.Clear;
        float time = 0f;
        float maxTime = assaultDuration + 600f; // cover assault + 10 min reinforcement buffer

        while (time < maxTime)
        {
            float interval = assaultDuration / Random.Range((float)minWeatherChanges, (float)maxWeatherChanges);
            time += interval;
            state = WeatherConfig.SampleNextState(state, Random.value);
            weatherTable.Add(new WeatherTransition(time, state));
        }
    }

    public void UpdateWeather(float currentAssaultTime)
    {
        if (weatherTable == null || weatherTable.Count == 0) return;

        int nextIndex = weatherTableIndex + 1;
        if (nextIndex < weatherTable.Count && currentAssaultTime >= weatherTable[nextIndex].timestamp)
        {
            weatherTableIndex = nextIndex;
            WeatherState newWeather = weatherTable[nextIndex].weather;
            if (newWeather != currentWeather)
            {
                currentWeather = newWeather;
                string name = WeatherConfig.GetDisplayName(currentWeather);
                float eff = WeatherConfig.GetEffectiveness(currentWeather);
                Debug.Log($"Weather changed: {name} (effectiveness: {eff * 100f:F0}%)");
                UIManager.Instance?.ShowWeatherNotification($"Weather: {name}");
            }
        }
    }

    public void ResetWeather()
    {
        currentWeather = WeatherState.Clear;
        weatherTableIndex = 0;
    }
}
