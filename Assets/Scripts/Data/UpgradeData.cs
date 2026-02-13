using UnityEngine;
public enum StatTarget
{
    SoldiersPerClick,
    DamageMin,
    DamageMax
}
public enum UpgradeType
{
    FlatAddition,
    Multiplier
}
[CreateAssetMenu(fileName = "New Upgrade", menuName = "WW1Clicker/Upgrade")]
public class UpgradeData : ScriptableObject
{
    [Header("Display")]
    public string upgradeName = "New Upgrade.";
    [TextArea(2, 4)]
    public string description = "Upgrade Description.";
    [Header("Effect")]
    public StatTarget statTarget = StatTarget.SoldiersPerClick;
    public UpgradeType upgradeType = UpgradeType.FlatAddition;
    public float effectValue = 1f;
    [Header("Cost")]
    public float baseCost = 100f;
    public float costMutiplier = 1.5f;

    [Header("Limits")]
    public int maxPurchases = -1; // -1 for unlimited

    //Track purchases at runtime
    [System.NonSerialized]
    public int timesPurchased = 0;

    public float GetCurrentCost()
    {
        return baseCost * Mathf.Pow(costMutiplier, timesPurchased);
    }
    public bool CanPurchase(float currentCurrency)
    {
        if (maxPurchases >= 0 && timesPurchased >= maxPurchases)
            return false;
        return currentCurrency >= GetCurrentCost();
    }
}
