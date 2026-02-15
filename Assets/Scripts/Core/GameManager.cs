
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [Header("Game State")]
    [SerializeField] private int totalSoldiers = 0;
    [SerializeField] private float groundGained = 0f;
    [SerializeField] private int soldiersPerClick = 1;

    [Header("Combat Settings")]
    [SerializeField] private int enemyTrenchHP = 100;
    [SerializeField] private float soldierDamageMin = 0f;
    [SerializeField] private float soldierDamageMax = 1f;

    [Header("Assault Timer Settings")]
    [SerializeField] private float assaultDuration = 90f; // 90 seconds
    [SerializeField] private float reinforcementRate = 5f; //HP per seconds
    [SerializeField] private float maxReinforcementMultiplier = 1.5f;

    [Header("Weather Settings")]
    [SerializeField] private int minWeatherChanges = 4;
    [SerializeField] private int maxWeatherChanges = 6;

    private float currentEnemyHP;
    private float maxReinforcedHP;
    private float currentAssaultTime = 0f;
    private bool isAssaultActive = false;
    private bool isReinforcing = false;
    private float currentReinforcementRate;
    private int trenchesCaptured = 0;

    // Weather system
    private WeatherState currentWeather = WeatherState.Clear;
    private List<WeatherTransition> weatherTable;
    private int weatherTableIndex = 0;
    private float lastRawDamage = 0f;
    private float lastWeatherDamage = 0f;


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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeGame();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isAssaultActive) return;

        // Increment assault timer
        currentAssaultTime += Time.deltaTime;

        // Advance weather table
        UpdateWeather();

        // Check if timer expired and reinforcements should start
        if (currentAssaultTime >= assaultDuration && !isReinforcing)
        {
            TriggerReinforcements();
        }

        // Process reinforcements if active
        if (isReinforcing)
        {
            ProcessReinforcements();
        }

        // Update UI every frame during assault
        UIManager.Instance?.UpdateUI();
    }
    private void InitializeGame()
    {
        SoldierDamageMin = soldierDamageMin;
        SoldierDamageMax = soldierDamageMax;
        currentEnemyHP = (float)enemyTrenchHP;
        maxReinforcedHP = enemyTrenchHP * maxReinforcementMultiplier;
        currentReinforcementRate = reinforcementRate;
        GenerateWeatherTable();
        Debug.Log("Game Initialized. Enemy Trench HP: " + currentEnemyHP);
    }
    public void OnSoldierClick()
    {
        if (!isAssaultActive)
        {
            Debug.Log("Assault not started yet! Click 'Start Assault' first.");
            return;
        }
        totalSoldiers += soldiersPerClick;
        Debug.Log($"Soldiers sent: {soldiersPerClick}. Total: {totalSoldiers}");

        // Process combat immediately (for clicker prototype)
        ProcessCombat(soldiersPerClick);

        // Update UI
        UIManager.Instance?.UpdateUI();
    }
    private void StartAssault()
    {
        isAssaultActive = true;
        currentAssaultTime = 0f;
        Debug.Log("ASSAULT STARTED! Timer begins...");
    }
    private void TriggerReinforcements()
    {
        isReinforcing = true;
        Debug.Log("Reinforcements Arriving! Trench is being reinforced...");

        UIManager.Instance?.ShowReinforcementWarning();
    }
    private void ProcessReinforcements()
    {
        // Weather slows reinforcements at 50% of the attacker penalty
        float weatherPenalty = 1f - GetWeatherEffectiveness();
        float reinforcementModifier = 1f - (weatherPenalty * 0.5f);

        float previousHP = currentEnemyHP;
        currentEnemyHP = Mathf.Min(currentEnemyHP + currentReinforcementRate * reinforcementModifier * Time.deltaTime, maxReinforcedHP);

        // Debug.Log($"Enemy reinforced: +{(currentEnemyHP - previousHP):F2} HP. Current: {currentEnemyHP:F1}/{maxReinforcedHP:F1}");
    }
    private void ProcessCombat(int soldiersSent)
    {
        float damageDealt = 0f;
        for (int i = 0; i < soldiersSent; i++)
        {
            damageDealt += GetRandomSoldierDamage();
        }
        lastRawDamage = damageDealt;
        damageDealt *= GetWeatherEffectiveness();
        lastWeatherDamage = damageDealt;
        currentEnemyHP -= damageDealt;

        Debug.Log($"Sent {soldiersSent} soldiers, dealt {damageDealt:F1} damage ({WeatherConfig.GetDisplayName(currentWeather)}). Enemy HP: {currentEnemyHP:F1}/{enemyTrenchHP}");

        if (currentEnemyHP <= 0)
        {
            CaptureTrench();
        }
        else
        {
            float inchesGained = soldiersSent * 0.1f;
            groundGained += inchesGained;
            Debug.Log($"Ground gained: {inchesGained:F2} inches. Total: {groundGained:F2} inches");
        }

    }
    private void CaptureTrench()
    {
        Debug.Log("Trench Captured!");
        groundGained += 120f;
        trenchesCaptured++;

        ResetAssault();
        // Increase difficulty for next trench
        enemyTrenchHP = Mathf.RoundToInt(enemyTrenchHP * 1.2f);
        currentEnemyHP = (float)enemyTrenchHP;
        maxReinforcedHP = enemyTrenchHP * maxReinforcementMultiplier;

        // Scale reinforcement rate with difficulty
        currentReinforcementRate = reinforcementRate * (enemyTrenchHP / 100f);
        GenerateWeatherTable();
        Debug.Log($"Next trench HP: {enemyTrenchHP}. Reinforcement rate: {currentReinforcementRate:F1}/sec");
    }

    private void ResetAssault()
    {
        isAssaultActive = false;
        isReinforcing = false;
        currentAssaultTime = 0f;
        currentWeather = WeatherState.Clear;
        weatherTableIndex = 0;
    }

    private float GetRandomSoldierDamage()
    {
        // Random damage between 0.0 and 1.0 with one decimal place
        // (0.0, 0.1, 0.2, ... 0.9, 1.0)
        float raw = Random.Range(SoldierDamageMin, SoldierDamageMax);
        return Mathf.Round(raw * 10f) / 10f;
    }
    public void SpendGroundGained(float amount)
    {
        groundGained -= amount;
        if (groundGained < 0) groundGained = 0;
        Debug.Log($"Spent {amount:F1} inches. Remaining: {groundGained:F1}");
    }
    public void AddSoldiersPerClick(int amount)
    {
        soldiersPerClick += amount;
        Debug.Log($"Soldiers per click increased by {amount}. Now: {soldiersPerClick}");
    }
    public void StartAssaultButton()
    {
        if (isAssaultActive)
        {
            Debug.Log("Assault already active!");
            return;
        }
        StartAssault();
        Debug.Log("Assault started by player!");
        // Update UI
        UIManager.Instance?.UpdateUI();
    }

    //Getters for UI
    public int GetTotalSoldiers() => totalSoldiers;
    public float GetgroundGained() => groundGained;
    public float GetCurrentEnemyHP() => currentEnemyHP;
    public int GetMaxEnemyHP() => enemyTrenchHP;
    public float GetAssaultTimeRemaining() => Mathf.Max(0, assaultDuration - currentAssaultTime);
    public bool IsAssaultActive() => isAssaultActive;
    public bool IsReinforcing() => isReinforcing;
    public float GetMaxReinforcedHP() => maxReinforcedHP;

    public int GetSoldierPerClick() => soldiersPerClick;

    public float SoldierDamageMin { get; private set; }
    public float SoldierDamageMax { get; private set; }

    public void ModifySoldierDamageMin(float amount) => SoldierDamageMin = Mathf.Clamp(SoldierDamageMin + amount, 0f, soldierDamageMax);
    public void ModifySoldierDamageMax(float amount) => SoldierDamageMax = Mathf.Clamp(SoldierDamageMax + amount, SoldierDamageMin, float.MaxValue);

    // Debug helper methods
    public void SetCurrentEnemyHP(float hp)
    {
        currentEnemyHP = Mathf.Clamp(hp, 0f, maxReinforcedHP);
        if (currentEnemyHP <= 0f)
        {
            CaptureTrench();
        }
    }
    public void AddGroundGained(float amount) => groundGained += amount;
    public void ResetAssaultTimer()
    {
        currentAssaultTime = 0f;
        isReinforcing = false;
    }
    public int GetTrenchesCaptured() => trenchesCaptured;
    public float GetReinforcementRate() => currentReinforcementRate;
    public float GetAssaultDuration() => assaultDuration;
    public void SetSoldierDamageMin(float value) => SoldierDamageMin = Mathf.Clamp(value, 0f, SoldierDamageMax);
    public void SetSoldierDamageMax(float value) => SoldierDamageMax = Mathf.Clamp(value, SoldierDamageMin, 100f);
    public void SetSoldiersPerClick(int value) => soldiersPerClick = Mathf.Max(1, value);
    public void SetIsReinforcing(bool value) => isReinforcing = value;

    // --- Weather System ---
    public WeatherState GetCurrentWeather() => currentWeather;
    public float GetWeatherEffectiveness() => WeatherConfig.GetEffectiveness(currentWeather);
    public int GetWeatherTableCount() => weatherTable != null ? weatherTable.Count : 0;
    public int GetWeatherTableIndex() => weatherTableIndex;
    public void SetWeather(WeatherState weather) => currentWeather = weather;
    public float GetLastRawDamage() => lastRawDamage;
    public float GetLastWeatherDamage() => lastWeatherDamage;
    public float GetCurrentAssaultTime() => currentAssaultTime;
    public List<WeatherTransition> GetWeatherTable() => weatherTable;


    private void GenerateWeatherTable()
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

    private void UpdateWeather()
    {
        if (weatherTable == null || weatherTable.Count == 0) return;

        // Check if we should advance to the next weather entry
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

}
