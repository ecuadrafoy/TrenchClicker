using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIShop : MonoBehaviour
{
    [Header("Shop Panel")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Button shopToggleButton;
    [SerializeField] private TextMeshProUGUI shopToggleButtonText;

    [Header("Upgrade Items")]
    [SerializeField] private Transform upgradeContainer;
    [SerializeField] private GameObject upgradeItemPrefab;

    [Header("Other shops")]
    [SerializeField] private UISpecialShop specialShop;

    private bool isShopOpen = false;
    void Start()
    {
        shopPanel.SetActive(false);
        if (shopToggleButton != null)
        {
            shopToggleButton.onClick.AddListener(ToggleShop);
        }
        PopulateShop();
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
        specialShop?.ClosePanel();
        isShopOpen = true;
        shopPanel.SetActive(true);
        shopToggleButtonText.text = "Close Shop";
        UpdateShopUI();
        Debug.Log("Shop opened");
    }
    public void CloseShop()
    {
        isShopOpen = false;
        shopPanel.SetActive(false);
        shopToggleButtonText.text = "Open Shop";
        Debug.Log("Shop closed");
    }
    private void UpdateShopUI()
    {
        if (UpgradeManager.Instance == null) return;
        foreach (UpgradeUIItem item in upgradeUIITems)
        {
            UpdateUpgradeUI(item.upgrade, item.nameText, item.descText, item.costText, item.buyButton);
        }
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
        bool canApply = true;
        if (upgrade.statTarget == StatTarget.DamageMin)
        {
            float newMin = GameManager.Instance.SoldierDamageMin + upgrade.effectValue;
            canApply = newMin <= GameManager.Instance.SoldierDamageMax * 0.5f;
        }
        buyButton.interactable = canAfford && canApply;

        // Show purchase level
        if (upgrade.timesPurchased > 0)
        {
            nameText.text += $" (Level {upgrade.timesPurchased})";
        }
    }
    private void OnBuyUpgrade(UpgradeData upgrade)
    {
        if (UpgradeManager.Instance == null || upgrade == null) return;
        bool success = UpgradeManager.Instance.TryPurchaseUpgrade(upgrade);
        if (success)
        {
            UpdateShopUI();
        }
    }
    private void PopulateShop()
    {
        if (UpgradeManager.Instance == null || upgradeItemPrefab == null) return;
        UpgradeData[] upgrades = UpgradeManager.Instance.GetUpgrades();
        foreach (UpgradeData upgrade in upgrades)
        {
            if (upgrade == null) continue;
            GameObject item = Instantiate(upgradeItemPrefab, upgradeContainer);
            TextMeshProUGUI[] texts = item.GetComponentsInChildren<TextMeshProUGUI>();
            Button buyButton = item.GetComponentInChildren<Button>();
            UpgradeUIItem uiItem = new UpgradeUIItem
            {
                upgrade = upgrade,
                nameText = texts.Length > 0 ? texts[0] : null,
                descText = texts.Length > 1 ? texts[1] : null,
                costText = texts.Length > 2 ? texts[2] : null,
                buyButton = buyButton
            };
            if (buyButton != null)
            {
                UpgradeData capturedUpgrade = upgrade;
                buyButton.onClick.AddListener(() => OnBuyUpgrade(capturedUpgrade));
            }
            upgradeUIITems.Add(uiItem);
        }
    }
    private class UpgradeUIItem
    {
        public UpgradeData upgrade;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI descText;
        public TextMeshProUGUI costText;
        public Button buyButton;
    }
    private List<UpgradeUIItem> upgradeUIITems = new List<UpgradeUIItem>();
}
