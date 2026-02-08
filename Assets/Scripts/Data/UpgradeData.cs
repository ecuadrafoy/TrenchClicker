using UnityEngine;
[CreateAssetMenu(fileName = "New Upgrade", menuName = "Upgrade")]
public class UpgradeData : ScriptableObject
{
    [Header("Upgrade Info")]
    public string upgradeName = "More Soldiers";
    [TextArea(2, 4)]
    public string description = "Increases the number of soldiers sent per click.";
    [Header("Effects")]
    public int soldiersPerClickIncrease = 5;
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
