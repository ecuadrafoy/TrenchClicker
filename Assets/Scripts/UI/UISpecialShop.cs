using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISpecialShop : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject specialShopPanel;

    [Header("Weather Station")]
    [SerializeField] private TextMeshProUGUI weatherStationNameText;
    [SerializeField] private TextMeshProUGUI weatherStationDescText;
    [SerializeField] private TextMeshProUGUI weatherStationCostText;
    [SerializeField] private Button weatherStationBuyButton;


    void Start()
    {
        if (weatherStationBuyButton != null)
        {
            weatherStationBuyButton.onClick.AddListener(OnBuyWeatherStation);
        }

        UpdateSpecialShopUI();
    }

    void Update()
    {
        UpdateSpecialShopUI();
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
        bool isUnlocked = ProgressionManager.Instance == null ||
                    ProgressionManager.Instance.GetLevel() >= WeatherStationManager.Instance.GetRequiredLevel();
        if (!isUnlocked)
        {
            weatherStationDescText.text = $"Requires Level {WeatherStationManager.Instance.GetRequiredLevel()}";
            weatherStationCostText.text = "—";
            weatherStationBuyButton.interactable = false;
            return;
        }

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

            int cost = WeatherStationManager.Instance.GetNextLevelCost();
            weatherStationCostText.text = $"Cost: {cost} RP";

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
