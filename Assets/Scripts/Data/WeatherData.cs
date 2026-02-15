using UnityEngine;
using System.Collections.Generic;


public enum WeatherState { Clear, PartlyCloudy, Overcast, LightRain, HeavyRain }
public enum ForecastRisk { Low, Medium, High }

public struct WeatherTransition
{
    public float timestamp;
    public WeatherState weather;

    public WeatherTransition(float timestamp, WeatherState weather)
    {
        this.timestamp = timestamp;
        this.weather = weather;
    }
}

public static class WeatherConfig
{
    public static float GetEffectiveness(WeatherState weather)
    {
        switch (weather)
        {
            case WeatherState.Clear: return 1.0f;
            case WeatherState.PartlyCloudy: return 0.95f;
            case WeatherState.Overcast: return 0.9f;
            case WeatherState.LightRain: return 0.7f;
            case WeatherState.HeavyRain: return 0.5f;
            default: return 1.0f;
        }
    }

    public static string GetDisplayName(WeatherState weather)
    {
        switch (weather)
        {
            case WeatherState.Clear: return "Clear";
            case WeatherState.PartlyCloudy: return "Partly Cloudy";
            case WeatherState.Overcast: return "Overcast";
            case WeatherState.LightRain: return "Light Rain";
            case WeatherState.HeavyRain: return "Heavy Rain";
            default: return "Unknown";
        }
    }

    // Markov transition table: from each state, cumulative probability thresholds
    // Each entry is (target state, cumulative probability)
    private static readonly Dictionary<WeatherState, (WeatherState target, float cumProb)[]> transitionTable =
        new Dictionary<WeatherState, (WeatherState, float)[]>
    {
            { WeatherState.Clear, new (WeatherState, float)[]
                {
                    (WeatherState.Clear,        0.50f),
                    (WeatherState.PartlyCloudy, 0.80f),
                    (WeatherState.Overcast,     0.95f),
                    (WeatherState.LightRain,    1.00f)
                }
            },
            { WeatherState.PartlyCloudy, new (WeatherState, float)[]
                {
                    (WeatherState.Clear,        0.25f),
                    (WeatherState.PartlyCloudy, 0.50f),
                    (WeatherState.Overcast,     0.80f),
                    (WeatherState.LightRain,    0.95f),
                    (WeatherState.HeavyRain,    1.00f)
                }
            },
            { WeatherState.Overcast, new (WeatherState, float)[]
                {
                    (WeatherState.Clear,        0.10f),
                    (WeatherState.PartlyCloudy, 0.30f),
                    (WeatherState.Overcast,     0.50f),
                    (WeatherState.LightRain,    0.80f),
                    (WeatherState.HeavyRain,    1.00f)
                }
            },
            { WeatherState.LightRain, new (WeatherState, float)[]
                {
                    (WeatherState.PartlyCloudy, 0.15f),
                    (WeatherState.Overcast,     0.40f),
                    (WeatherState.LightRain,    0.70f),
                    (WeatherState.HeavyRain,    1.00f)
                }
            },
            { WeatherState.HeavyRain, new (WeatherState, float)[]
                {
                    (WeatherState.Overcast,     0.15f),
                    (WeatherState.LightRain,    0.55f),
                    (WeatherState.HeavyRain,    1.00f)
                }
            }
    };

    public static WeatherState SampleNextState(WeatherState current, float randomValue)
    {
        var transitions = transitionTable[current];
        for (int i = 0; i < transitions.Length; i++)
        {
            if (randomValue <= transitions[i].cumProb)
                return transitions[i].target;
        }
        // Fallback (shouldn't happen with proper cumProb ending at 1.0)
        return current;
    }
}

