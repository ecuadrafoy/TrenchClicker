using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISpecialShop : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject specialShopPanel;
    [SerializeField] private Button specialShopToggleButton;
    [SerializeField] private TextMeshProUGUI specialShopToggleButtonText;

    [Header("Weather Station")]
    [SerializeField] private TextMeshProUGUI weatherStationNameText;
    [SerializeField] private TextMeshProUGUI weatherStationDescText;
    [SerializeField] private TextMeshProUGUI weatherStationCostText;
    [SerializeField] private Button weatherStationBuyButton;

    [Header("Other Shops")]
    [SerializeField] private UIShop regularShop;

    private bool isOpen = false;

    void Start()
    {
        specialShopPanel.SetActive(false);

        if (specialShopToggleButton != null)
        {
            specialShopToggleButton.onClick.AddListener(TogglePanel);
        }

        if (weatherStationBuyButton != null)
        {
            weatherStationBuyButton.onClick.AddListener(OnBuyWeatherStation);
        }

        UpdateSpecialShopUI();
    }

    void Update()
    {
        if (specialShopToggleButton != null)
        {
            bool canOpen = !GameManager.Instance.IsAssaultActive();
            specialShopToggleButton.interactable = canOpen;

            if (!canOpen && isOpen)
            {
                ClosePanel();
            }
        }

        if (isOpen)
        {
            UpdateSpecialShopUI();
        }
    }

    private void TogglePanel()
    {
        if (isOpen)
            ClosePanel();
        else
            OpenPanel();
    }

    private void OpenPanel()
    {
        regularShop?.CloseShop();
        isOpen = true;
        specialShopPanel.SetActive(true);
        specialShopToggleButtonText.text = "Close Special";
        UpdateSpecialShopUI();
    }

    public void ClosePanel()
    {
        isOpen = false;
        specialShopPanel.SetActive(false);
        specialShopToggleButtonText.text = "Special Upgrades";
    }

    private void UpdateSpecialShopUI()
    {
        UpdateWeatherStationUI();
    }

    private void UpdateWeatherStationUI()
    {
        if (WeatherStationManager.Instance == null) return;
        if (weatherStationNameText == null) return;

        int level = WeatherStationManager.Instance.GetLevel();
        weatherStationNameText.text = $"Weather Station (Level {level})";

        if (level >= 3)
        {
            weatherStationDescText.text = "Fully upgraded — precise forecasts active.";
            weatherStationCostText.text = "MAX LEVEL";
            weatherStationBuyButton.interactable = false;
        }
        else
        {
            string[] nextLevelDesc =
            {
                "Purchase to get vague weather hints.",
                "Upgrade for risk category forecasts.",
                "Upgrade for exact rain % and effectiveness."
            };
            weatherStationDescText.text = nextLevelDesc[level];

            float cost = WeatherStationManager.Instance.GetNextLevelCost();
            weatherStationCostText.text = cost >= 12f
                ? $"Cost: {(cost / 12f):F1} feet"
                : $"Cost: {cost:F1} inches";

            weatherStationBuyButton.interactable = WeatherStationManager.Instance.CanUpgrade();
        }
    }

    private void OnBuyWeatherStation()
    {
        if (WeatherStationManager.Instance == null) return;
        if (WeatherStationManager.Instance.TryUpgrade())
        {
            UpdateSpecialShopUI();
        }
    }
}
