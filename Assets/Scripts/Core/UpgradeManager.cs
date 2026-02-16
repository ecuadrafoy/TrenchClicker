
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("Available Upgrades")]
    [SerializeField] private UpgradeData[] upgrades;

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
        int currentCurrency = GameManager.Instance.GetRequisitionPoints();

        // check if can afford
        if (!upgrade.CanPurchase(currentCurrency))
        {
            Debug.Log($"Cannot purchase {upgrade.upgradeName}. Need {upgrade.GetCurrentCost():F0} RP, have {currentCurrency}");
            return false;
        }
        // Check if DamageMin would exceed 50% of DamageMax after purchase
        if (upgrade.statTarget == StatTarget.DamageMin)
        {
            float newMin = GameManager.Instance.SoldierDamageMin + upgrade.effectValue;
            if (newMin > GameManager.Instance.SoldierDamageMax * 0.5f)
            {
                Debug.Log("Need better weapons first! Min damage cannot exceed 50% of max damage.");
                return false;
            }
        }
        float cost = upgrade.GetCurrentCost();
        GameManager.Instance.SpendRequisitionPoints((int)cost);

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
        switch (upgrade.statTarget)
        {
            case StatTarget.SoldiersPerClick:
                if (upgrade.upgradeType == UpgradeType.FlatAddition)
                    GameManager.Instance.AddSoldiersPerClick((int)upgrade.effectValue);
                //else if (upgrade.upgradeType == UpgradeType.Multiplier)
                //    GameManager.Instance.MultiplySoldiersPerClick(upgrade.effectValue);
                break;
            case StatTarget.DamageMin:
                if (upgrade.upgradeType == UpgradeType.FlatAddition)
                    GameManager.Instance.ModifySoldierDamageMin(upgrade.effectValue);
                else if (upgrade.upgradeType == UpgradeType.Multiplier)
                    GameManager.Instance.ModifySoldierDamageMin(GameManager.Instance.SoldierDamageMin * upgrade.effectValue - GameManager.Instance.SoldierDamageMin);
                break;

            case StatTarget.DamageMax:
                if (upgrade.upgradeType == UpgradeType.FlatAddition)
                    GameManager.Instance.ModifySoldierDamageMax(upgrade.effectValue);
                else if (upgrade.upgradeType == UpgradeType.Multiplier)
                    GameManager.Instance.ModifySoldierDamageMax(GameManager.Instance.SoldierDamageMax * upgrade.effectValue - GameManager.Instance.SoldierDamageMax);
                break;

            default:
                Debug.LogWarning($"ApplyUpgrade: unhandled StatTarget {upgrade.statTarget}");
                break;
        }
    }

    public UpgradeData[] GetUpgrades() => upgrades;

}
