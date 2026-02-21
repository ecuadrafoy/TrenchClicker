using UnityEngine;

public class EliteTroopManager : MonoBehaviour
{
    public static EliteTroopManager Instance { get; private set; }

    [Header("Elite Troops Settings")]
    [SerializeField] private int startingEliteReserve = 50;
    [SerializeField] private int minElitesEarnedPerTrench = 10;
    [SerializeField] private int maxElitesEarnedPerTrench = 20;
    [SerializeField] private float eliteDeploymentDurationPer100 = 3f;

    private int eliteTroopReserve;
    private bool elitesActive = false;
    private float eliteDeploymentTimer = 0f;
    private int eliteDeployedCount = 0;
    private float eliteDeploymentDuration = 0f;

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

    public void Initialize()
    {
        eliteTroopReserve = startingEliteReserve;
    }

    // --- Public API ---

    public int GetEliteTroopReserve() => eliteTroopReserve;
    public bool IsElitesActive() => elitesActive;
    public float GetEliteDeploymentTimer() => eliteDeploymentTimer;
    public float GetEliteDeploymentDuration() => eliteDeploymentDuration;
    public int GetEliteDeployedCount() => eliteDeployedCount;
    public void AddEliteReserve(int amount) => eliteTroopReserve += amount;

    public void DeployEliteTroops()
    {
        if (!GameManager.Instance.IsAssaultActive() || elitesActive || eliteTroopReserve <= 0) return;
        eliteDeployedCount = eliteTroopReserve;
        eliteTroopReserve = 0;
        eliteDeploymentDuration = (eliteDeployedCount / 100f) * eliteDeploymentDurationPer100;
        eliteDeploymentTimer = eliteDeploymentDuration;
        elitesActive = true;
        SoldierVisualManager.Instance?.SpawnEliteBatch(eliteDeployedCount);
        Debug.Log($"Deployed {eliteDeployedCount} elite troops for {eliteDeploymentDuration:F1}s");
    }

    /// <summary>
    /// Called every frame by GameManager. Updates deployment timer and ends deployment when expired.
    /// </summary>
    public void UpdateDeployment()
    {
        if (!elitesActive) return;
        eliteDeploymentTimer -= Time.deltaTime;
        if (eliteDeploymentTimer <= 0f)
        {
            EndDeployment(false);
        }
    }

    /// <summary>
    /// Returns the elite damage for this frame. GameManager applies it to enemy HP.
    /// </summary>
    public float GetFrameDamage()
    {
        if (!elitesActive) return 0f;
        float dps = eliteDeployedCount / eliteDeploymentDuration;
        return dps * Time.deltaTime * WeatherManager.Instance.GetEliteEffectiveness();
    }

    /// <summary>
    /// Called by GameManager when a trench is captured. Ends deployment with victory bonus.
    /// </summary>
    public void OnTrenchCaptured()
    {
        if (elitesActive)
        {
            EndDeployment(true);
        }
    }

    /// <summary>
    /// Awards new elite troops on trench capture. Returns the number earned.
    /// </summary>
    public int AwardNewElites()
    {
        int earned = Random.Range(minElitesEarnedPerTrench, maxElitesEarnedPerTrench + 1);
        eliteTroopReserve += earned;
        Debug.Log($"Earned {earned} elite troops. Reserve: {eliteTroopReserve}");
        return earned;
    }

    private int CalculateSurvivors(bool victory)
    {
        float survivalRate = 0.7f;
        if (victory) survivalRate += 0.1f;
        // Weather modifier based on current weather at end of deployment
        WeatherState weather = WeatherManager.Instance.GetCurrentWeather();
        switch (weather)
        {
            case WeatherState.HeavyRain:
                survivalRate -= 0.2f;
                break;
            case WeatherState.LightRain:
                survivalRate -= 0.1f;
                break;
            case WeatherState.Overcast:
            case WeatherState.PartlyCloudy:
            case WeatherState.Clear:
                survivalRate += 0.1f;
                break;
        }
        survivalRate = Mathf.Clamp01(survivalRate);
        return Mathf.RoundToInt(eliteDeployedCount * survivalRate);
    }

    private void EndDeployment(bool victory)
    {
        if (!elitesActive) return;
        int survivors = CalculateSurvivors(victory);
        eliteTroopReserve += survivors;
        Debug.Log($"Elite deployment ended. {survivors}/{eliteDeployedCount} survived (victory: {victory})");
        UIManager.Instance?.ShowEliteSurvivalNotification(survivors, eliteDeployedCount);
        elitesActive = false;
        eliteDeploymentTimer = 0f;
        eliteDeployedCount = 0;
    }
}
