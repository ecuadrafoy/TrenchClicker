
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
    private int currentEnemyHP;

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

    }
    private void InitializeGame()
    {
        currentEnemyHP = enemyTrenchHP;
        Debug.Log("Game Initialized. Enemy Trench HP: " + currentEnemyHP);
    }
    public void OnSoldierClick()
    {
        totalSoldiers += soldiersPerClick;
        Debug.Log($"Soldiers sent: {soldiersPerClick}. Total: {totalSoldiers}");

        // Process combat immediately (for clicker prototype)
        ProcessCombat(soldiersPerClick);

        // Update UI
        UIManager.Instance?.UpdateUI();
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

        currentEnemyHP = enemyTrenchHP;
        enemyTrenchHP = Mathf.RoundToInt(enemyTrenchHP * 1.2f);
    }

    //Getters for UI
    public int GetTotalSoldiers() => totalSoldiers;
    public float GetgroundGained() => groundGained;
    public int GetCurrentEnemyHP() => currentEnemyHP;
    public int GetMaxEnemyHP() => enemyTrenchHP;
}
