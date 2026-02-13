using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIShop : MonoBehaviour
{
    [Header("Shop Panel")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Button shopToggleButton;
    [SerializeField] private TextMeshProUGUI shopToggleButtonText;

    [Header("Upgrade: Basic Soldiers Upgrade")]
    [SerializeField] private GameObject soldiersUpgradeItem;
    [SerializeField] private TextMeshProUGUI soldiersUpgradeNameText;
    [SerializeField] private TextMeshProUGUI soldiersUpgradeDescText;
    [SerializeField] private TextMeshProUGUI soldiersUpgradeCostText;
    [SerializeField] private Button soldiersUpgradeBuyButton;

    [Header("Upgrade: Soldiers Battalion Upgrade")]
    [SerializeField] private GameObject soldiersBulkUpgradeItem;
    [SerializeField] private TextMeshProUGUI soldiersBulkUpgradeNameText;
    [SerializeField] private TextMeshProUGUI soldiersBulkUpgradeDescText;
    [SerializeField] private TextMeshProUGUI soldiersBulkUpgradeCostText;
    [SerializeField] private Button soldiersBulkUpgradeBuyButton;
    private bool isShopOpen = false;
    void Start()
    {
        shopPanel.SetActive(false);
        if (shopToggleButton != null)
        {
            shopToggleButton.onClick.AddListener(ToggleShop);
        }
        if (soldiersUpgradeBuyButton != null)
        {
            soldiersUpgradeBuyButton.onClick.AddListener(OnBuySoldiersUpgrade);
        }
        if (soldiersBulkUpgradeBuyButton != null)
        {
            soldiersBulkUpgradeBuyButton.onClick.AddListener(OnBuySoldiersBulkUpgrade);
        }
        UpdateShopUI();

    }

    // Update is called once per frame
    void Update()
    {
        if (shopToggleButton != null)
        {
            bool canOpenShop = !GameManager.Instance.IsAssaultActive();
            shopToggleButton.interactable = canOpenShop;
            if (!canOpenShop && isShopOpen)
            {
                CloseShop();
            }
        }
        if (isShopOpen)
        {
            UpdateShopUI();
        }

    }
    private void ToggleShop()
    {
        if (isShopOpen)
        {
            CloseShop();
        }
        else
        {
            OpenShop();
        }
    }
    private void OpenShop()
    {
        isShopOpen = true;
        shopPanel.SetActive(true);
        shopToggleButtonText.text = "Close Shop";
        UpdateShopUI();
        Debug.Log("Shop opened");
    }
    private void CloseShop()
    {
        isShopOpen = false;
        shopPanel.SetActive(false);
        shopToggleButtonText.text = "Open Shop";
        Debug.Log("Shop closed");
    }
    private void UpdateShopUI()
    {
        if (UpgradeManager.Instance == null) return;
        // Update Small Soldiers Upgrade (+5)
        UpdateUpgradeUI(
            UpgradeManager.Instance.GetSoldiersPerClickUpgrade(),
            soldiersUpgradeNameText,
            soldiersUpgradeDescText,
            soldiersUpgradeCostText,
            soldiersUpgradeBuyButton
        );

        // Update Bulk Soldiers Upgrade (+100)
        UpdateUpgradeUI(
            UpgradeManager.Instance.GetSoldiersBulkUpgrade(),
            soldiersBulkUpgradeNameText,
            soldiersBulkUpgradeDescText,
            soldiersBulkUpgradeCostText,
            soldiersBulkUpgradeBuyButton
        );
    }
    private void UpdateUpgradeUI(UpgradeData upgrade, TextMeshProUGUI nameText, TextMeshProUGUI descText, TextMeshProUGUI costText, Button buyButton)
    {
        if (upgrade == null) return;

        // Update text
        nameText.text = upgrade.upgradeName;
        descText.text = $"{upgrade.description}\n+{upgrade.effectValue} {upgrade.statTarget}";

        float cost = upgrade.GetCurrentCost();
        costText.text = cost >= 12f
            ? $"Cost: {(cost / 12f):F1} feet"
            : $"Cost: {cost:F1} inches";

        // Update button state
        float currentCurrency = GameManager.Instance.GetgroundGained();
        bool canAfford = upgrade.CanPurchase(currentCurrency);
        buyButton.interactable = canAfford;

        // Show purchase level
        if (upgrade.timesPurchased > 0)
        {
            nameText.text += $" (Level {upgrade.timesPurchased})";
        }
    }

    private void OnBuySoldiersUpgrade()
    {
        if (UpgradeManager.Instance == null) return;
        UpgradeData soldiersUpgrade = UpgradeManager.Instance.GetSoldiersPerClickUpgrade();
        if (soldiersUpgrade != null)
        {
            bool success = UpgradeManager.Instance.TryPurchaseUpgrade(soldiersUpgrade);
            if (success)
            {
                UpdateShopUI();
            }
        }
    }
    private void OnBuySoldiersBulkUpgrade()
    {
        if (UpgradeManager.Instance == null) return;
        UpgradeData soldiersBulkUpgrade = UpgradeManager.Instance.GetSoldiersBulkUpgrade();
        if (soldiersBulkUpgrade != null)
        {
            bool success = UpgradeManager.Instance.TryPurchaseUpgrade(soldiersBulkUpgrade);
            if (success)
            {
                UpdateShopUI();
            }
        }
    }
}
