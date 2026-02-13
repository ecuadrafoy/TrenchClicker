


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

    private float currentEnemyHP;
    private float maxReinforcedHP;
    private float currentAssaultTime = 0f;
    private bool isAssaultActive = false;
    private bool isReinforcing = false;
    private float currentReinforcementRate;




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
        float previousHP = currentEnemyHP;
        currentEnemyHP = Mathf.Min(currentEnemyHP + currentReinforcementRate * Time.deltaTime, maxReinforcedHP);

        // Debug.Log($"Enemy reinforced: +{(currentEnemyHP - previousHP):F2} HP. Current: {currentEnemyHP:F1}/{maxReinforcedHP:F1}");
    }
    private void ProcessCombat(int soldiersSent)
    {
        float damageDealt = 0f;
        for (int i = 0; i < soldiersSent; i++)
        {
            damageDealt += GetRandomSoldierDamage();
        }
        currentEnemyHP -= damageDealt;

        Debug.Log($"Sent {soldiersSent} soldiers, dealt {damageDealt:F1} damage. Enemy HP: {currentEnemyHP:F1}/{enemyTrenchHP}");

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

        ResetAssault();
        // Increase difficulty for next trench
        enemyTrenchHP = Mathf.RoundToInt(enemyTrenchHP * 1.2f);
        currentEnemyHP = (float)enemyTrenchHP;
        maxReinforcedHP = enemyTrenchHP * maxReinforcementMultiplier;

        // Scale reinforcement rate with difficulty
        currentReinforcementRate = reinforcementRate * (enemyTrenchHP / 100f);
        Debug.Log($"Next trench HP: {enemyTrenchHP}. Reinforcement rate: {currentReinforcementRate:F1}/sec");
    }

    private void ResetAssault()
    {
        isAssaultActive = false;
        isReinforcing = false;
        currentAssaultTime = 0f;
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
    public void ModifySoldierDamageMax(float amount) => SoldierDamageMax = Mathf.Clamp(SoldierDamageMax + amount, soldierDamageMin, 2f);

}
