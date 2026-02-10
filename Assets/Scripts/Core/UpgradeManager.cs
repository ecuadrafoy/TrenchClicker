
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("Available Upgrades")]
    [SerializeField] private UpgradeData soldiersPerClickUpgrade;
    [SerializeField] private UpgradeData soldiersBulkUpgrade;

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
    public bool TryPurchaseUpgrade(UpgradeData upgrade)
    {
        if (GameManager.Instance == null) return false;
        float currentCurrency = GameManager.Instance.GetgroundGained();

        // check if can afford
        if (!upgrade.CanPurchase(currentCurrency))
        {
            Debug.Log($"Cannot purchase {upgrade.upgradeName} Need {upgrade.GetCurrentCost():F1} inches, have {currentCurrency: F1}");
            return false;
        }
        float cost = upgrade.GetCurrentCost();
        GameManager.Instance.SpendGroundGained(cost);

        ApplyUpgrade(upgrade);

        upgrade.timesPurchased++;
        Debug.Log($"Purchased {upgrade.upgradeName}! (Level {upgrade.timesPurchased})");

        // Update UI
        UIManager.Instance?.UpdateUI();

        return true;

    }
    private void ApplyUpgrade(UpgradeData upgrade)
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.AddSoldiersPerClick(upgrade.soldiersPerClickIncrease);
        Debug.Log($"Applied upgrade: +{upgrade.soldiersPerClickIncrease} soldiers per click");
    }
    public UpgradeData GetSoldiersPerClickUpgrade() => soldiersPerClickUpgrade;
    public UpgradeData GetSoldiersBulkUpgrade() => soldiersBulkUpgrade;
}
