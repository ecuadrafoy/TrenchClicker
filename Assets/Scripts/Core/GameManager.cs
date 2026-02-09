


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
    [SerializeField] private int soldierDamage = 1;

    [Header("Assault Timer Settings")]
    [SerializeField] private float assaultDuration = 90f; // 90 seconds
    [SerializeField] private float reinforcementRate = 5f; //HP per seconds
    [SerializeField] private float maxReinforcementMultiplier = 1.5f;

    private int currentEnemyHP;
    private int maxReinforcedHP;
    private float currentAssaultTime = 0f;
    private bool isAssaultActive = false;
    private bool isReinforcing = false;
    private float currentReinforcementRate;
    private float reinforcementAccumulator = 0f;




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
        currentEnemyHP = enemyTrenchHP;
        maxReinforcedHP = Mathf.RoundToInt(enemyTrenchHP * maxReinforcementMultiplier);
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
        reinforcementAccumulator += currentReinforcementRate * Time.deltaTime;

        if (reinforcementAccumulator >= 1f)
        {
            int hpToAdd = Mathf.FloorToInt(reinforcementAccumulator);
            reinforcementAccumulator -= hpToAdd; // Keep the remainder

            int previousHP = currentEnemyHP;
            currentEnemyHP = Mathf.Min(currentEnemyHP + hpToAdd, maxReinforcedHP);

            Debug.Log($"Enemy reinforced: +{hpToAdd} HP. Current: {currentEnemyHP}/{maxReinforcedHP}");
        }
    }
    private void ProcessCombat(int soldiersSent)
    {
        int damageDealt = soldiersSent * soldierDamage;
        currentEnemyHP -= damageDealt;

        Debug.Log($"Sent {soldiersSent} soldiers, dealt {damageDealt} damage. Enemy HP: {currentEnemyHP}/{enemyTrenchHP}");

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
        // Increase difficulty for next rench
        enemyTrenchHP = Mathf.RoundToInt(enemyTrenchHP * 1.2f);
        currentEnemyHP = enemyTrenchHP;
        maxReinforcedHP = Mathf.RoundToInt(enemyTrenchHP * maxReinforcementMultiplier);

        // Scale reinforcement rate with difficulty
        currentReinforcementRate = reinforcementRate * (enemyTrenchHP / 100f);
        Debug.Log($"Next trench HP: {enemyTrenchHP}. Reinforcement rate: {currentReinforcementRate:F1}/sec");
    }

    private void ResetAssault()
    {
        isAssaultActive = false;
        isReinforcing = false;
        currentAssaultTime = 0f;
        reinforcementAccumulator = 0f;
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
    public int GetCurrentEnemyHP() => currentEnemyHP;
    public int GetMaxEnemyHP() => enemyTrenchHP;
    public float GetAssaultTimeRemaining() => Mathf.Max(0, assaultDuration - currentAssaultTime);
    public bool IsAssaultActive() => isAssaultActive;
    public bool IsReinforcing() => isReinforcing;
    public int GetMaxReinforcedHP() => maxReinforcedHP;

    public int GetSoldierPerClick() => soldiersPerClick;

}
