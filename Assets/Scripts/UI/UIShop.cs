using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIShop : MonoBehaviour
{
    [Header("Shop Panel")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Button shopToggleButton;
    [SerializeField] private TextMeshProUGUI shopToggleButtonText;

    [Header("Upgrade: Soldiers Per Click")]
    [SerializeField] private GameObject soldiersUpgradeItem;
    [SerializeField] private TextMeshProUGUI soldiersUpgradeNameText;
    [SerializeField] private TextMeshProUGUI soldiersUpgradeDescText;
    [SerializeField] private TextMeshProUGUI soldiersUpgradeCostText;
    [SerializeField] private Button soldiersUpgradeBuyButton;
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
        UpgradeData soldiersUpgrade = UpgradeManager.Instance.GetSoldiersPerClickUpgrade();
        if (soldiersUpgrade != null)
        {
            // Update text
            soldiersUpgradeNameText.text = soldiersUpgrade.upgradeName;
            soldiersUpgradeDescText.text = $"{soldiersUpgrade.description}\n+{soldiersUpgrade.soldiersPerClickIncrease} soldiers per click";
            float cost = soldiersUpgrade.GetCurrentCost();
            soldiersUpgradeCostText.text = cost >= 12f
                    ? $"Cost: {(cost / 12f):F1} feet"
                    : $"Cost: {cost:F1} inches";
            // Update button state
            float currentCurrency = GameManager.Instance.GetgroundGained();
            bool canAfford = soldiersUpgrade.CanPurchase(currentCurrency);
            soldiersUpgradeBuyButton.interactable = canAfford;
            //Show Purchase level
            if (soldiersUpgrade.timesPurchased > 0)
            {
                soldiersUpgradeNameText.text += $" (Level {soldiersUpgrade.timesPurchased})";
            }

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
}
