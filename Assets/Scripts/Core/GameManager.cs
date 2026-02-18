
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [Header("Game State")]
    [SerializeField] private int totalSoldiersSent = 0;
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

    [Header("Economy")]
    [SerializeField] private int requisitionPoints = 0;
    [SerializeField] private int baseRequisitionReward = 50;
    [SerializeField] private int speedBonusReward = 10;
    [SerializeField] private int efficiencyBonusReward = 5;
    [SerializeField] private float warningThreshold = 30f;

    private float currentEnemyHP;
    private float maxReinforcedHP;
    private float currentAssaultTime = 0f;
    private bool isAssaultActive = false;
    private bool isReinforcing = false;
    private float currentReinforcementRate;
    private int totalTrenchesCaptured = 0;

    // Combat diagnostics
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
        WeatherManager.Instance.UpdateWeather(currentAssaultTime);

        // Elite troops: update timer then apply damage
        EliteTroopManager.Instance.UpdateDeployment();
        float eliteDamage = EliteTroopManager.Instance.GetFrameDamage();
        if (eliteDamage > 0f)
        {
            currentEnemyHP -= eliteDamage;
            if (currentEnemyHP <= 0f)
            {
                CaptureTrench();
                return;
            }
        }

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
        WeatherManager.Instance.GenerateWeatherTable(assaultDuration);
        EliteTroopManager.Instance.Initialize();
        Debug.Log("Game Initialized. Enemy Trench HP: " + currentEnemyHP);
    }
    public void OnSoldierClick()
    {
        if (!isAssaultActive)
        {
            Debug.Log("Assault not started yet! Click 'Start Assault' first.");
            return;
        }
        totalSoldiersSent += soldiersPerClick;
        Debug.Log($"Soldiers sent: {soldiersPerClick}. Total: {totalSoldiersSent}");

        // Process combat immediately (for clicker prototype)
        ProcessCombat(soldiersPerClick);

        // Update UI
        UIManager.Instance?.UpdateUI();
        SoldierVisualManager.Instance?.SpawnSoldierBatch();
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
        float weatherPenalty = 1f - WeatherManager.Instance.GetEffectiveness();
        float reinforcementModifier = 1f - (weatherPenalty * 0.5f);

        float previousHP = currentEnemyHP;
        currentEnemyHP = Mathf.Min(currentEnemyHP + currentReinforcementRate * reinforcementModifier * Time.deltaTime, maxReinforcedHP);
    }
    private void ProcessCombat(int soldiersSent)
    {
        float damageDealt = 0f;
        for (int i = 0; i < soldiersSent; i++)
        {
            damageDealt += GetRandomSoldierDamage();
        }
        lastRawDamage = damageDealt;
        damageDealt *= WeatherManager.Instance.GetEffectiveness();
        lastWeatherDamage = damageDealt;
        currentEnemyHP -= damageDealt;

        string weatherName = WeatherConfig.GetDisplayName(WeatherManager.Instance.GetCurrentWeather());
        Debug.Log($"Sent {soldiersSent} soldiers, dealt {damageDealt:F1} damage ({weatherName}). Enemy HP: {currentEnemyHP:F1}/{enemyTrenchHP}");

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
        totalTrenchesCaptured++;
        // End elite deployment with victory bonus (before ResetAssault clears weather)
        EliteTroopManager.Instance.OnTrenchCaptured();
        // Award new elite troops
        EliteTroopManager.Instance.AwardNewElites();
        AwardRequisitionPoints();
        ResetAssault();
        // Increase difficulty for next trench
        enemyTrenchHP = Mathf.RoundToInt(enemyTrenchHP * 1.2f);
        currentEnemyHP = (float)enemyTrenchHP;
        maxReinforcedHP = enemyTrenchHP * maxReinforcementMultiplier;

        // Scale reinforcement rate with difficulty
        currentReinforcementRate = reinforcementRate * (enemyTrenchHP / 100f);
        WeatherManager.Instance.GenerateWeatherTable(assaultDuration);
        Debug.Log($"Next trench HP: {enemyTrenchHP}. Reinforcement rate: {currentReinforcementRate:F1}/sec");

        UIManager.Instance?.UpdateUI();
    }
    private void AwardRequisitionPoints()
    {
        int reward = baseRequisitionReward;
        float timeRemaining = assaultDuration - currentAssaultTime;
        //Speed bonus: captured before warning threshold
        if (timeRemaining > warningThreshold)
        {
            reward += speedBonusReward;
        }
        //Efficiency bonus: scales with time remaining as a % of assault duration
        float efficiencyPercent = timeRemaining / assaultDuration;
        reward += Mathf.RoundToInt(efficiencyBonusReward * efficiencyPercent);
        requisitionPoints += reward;
        Debug.Log($"Awarded {reward} RP (base:{baseRequisitionReward} speed:{(timeRemaining > warningThreshold ? speedBonusReward : 0)} efficiency:{Mathf.RoundToInt(efficiencyBonusReward * efficiencyPercent)}).Total: {requisitionPoints}");
    }

    private void ResetAssault()
    {
        isAssaultActive = false;
        isReinforcing = false;
        currentAssaultTime = 0f;
        WeatherManager.Instance.ResetWeather();
    }

    private float GetRandomSoldierDamage()
    {
        // Random damage between 0.0 and 1.0 with one decimal place
        // (0.0, 0.1, 0.2, ... 0.9, 1.0)
        float raw = Random.Range(SoldierDamageMin, SoldierDamageMax);
        return Mathf.Round(raw * 10f) / 10f;
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
    public int GetTotalSoldiersSent() => totalSoldiersSent;
    public float GetgroundGained() => groundGained;
    public float GetCurrentEnemyHP() => currentEnemyHP;
    public int GetMaxEnemyHP() => enemyTrenchHP;
    public float GetAssaultTimeRemaining() => Mathf.Max(0, assaultDuration - currentAssaultTime);
    public bool IsAssaultActive() => isAssaultActive;
    public bool IsReinforcing() => isReinforcing;
    public float GetMaxReinforcedHP() => maxReinforcedHP;
    public int GetSoldierPerClick() => soldiersPerClick;
    public int GetRequisitionPoints() => requisitionPoints;
    public void SpendRequisitionPoints(int amount)
    {
        requisitionPoints -= amount;
        if (requisitionPoints < 0) requisitionPoints = 0;
        Debug.Log($"Spent {amount} RP. Remaining: {requisitionPoints}");
    }

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
    public void AddRequisitionPoints(int amount) => requisitionPoints += amount;
    public void ResetAssaultTimer()
    {
        currentAssaultTime = 0f;
        isReinforcing = false;
    }
    public int GettotalTrenchesCaptured() => totalTrenchesCaptured;
    public float GetReinforcementRate() => currentReinforcementRate;
    public float GetAssaultDuration() => assaultDuration;
    public void SetSoldierDamageMin(float value) => SoldierDamageMin = Mathf.Clamp(value, 0f, SoldierDamageMax);
    public void SetSoldierDamageMax(float value) => SoldierDamageMax = Mathf.Clamp(value, SoldierDamageMin, 100f);
    public void SetSoldiersPerClick(int value) => soldiersPerClick = Mathf.Max(1, value);
    public void SetIsReinforcing(bool value) => isReinforcing = value;

    // Combat diagnostics (stays here — these are combat data, not weather state)
    public float GetLastRawDamage() => lastRawDamage;
    public float GetLastWeatherDamage() => lastWeatherDamage;
    public float GetCurrentAssaultTime() => currentAssaultTime;
}
