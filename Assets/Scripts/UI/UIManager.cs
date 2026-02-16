using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.AI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI soldierCountText;
    [SerializeField] private TextMeshProUGUI groundGainedText;
    [SerializeField] private TextMeshProUGUI requisitionPointsText;
    [SerializeField] private TextMeshProUGUI enemyHPText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Button clickButton;
    [SerializeField] private Button startAssaultButton;

    [Header("Weather Display")]
    [SerializeField] private TextMeshProUGUI weatherText;
    [SerializeField] private TextMeshProUGUI weatherNotificationText;

    [Header("Weather Forecast")]
    [SerializeField] private TextMeshProUGUI forecastText;

    [Header("Warning Settings")]
    [SerializeField] private Color normalTimerColor = Color.white;
    [SerializeField] private Color warningTimerColor = Color.yellow;
    [SerializeField] private Color criticalTimerColor = Color.red;
    [SerializeField] private float warningThreshold = 30f;
    [SerializeField] private float criticalThreshold = 10f;

    [Header("Elite Troops UI")]
    [SerializeField] private Button deployElitesButton;
    [SerializeField] private Image eliteButtonFill;
    [SerializeField] private TextMeshProUGUI eliteButtonText;
    [SerializeField] private TextMeshProUGUI eliteReserveText;

    private bool isShowingWarning = false;
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
        if (clickButton != null)
        {
            clickButton.onClick.AddListener(OnClickButtonPressed);
        }
        if (startAssaultButton != null)
        {
            startAssaultButton.onClick.AddListener(OnStartAssaultButtonPressed);
        }
        if (deployElitesButton != null)
        {
            deployElitesButton.onClick.AddListener(OnDeployElitesButtonPressed);
        }
        if (eliteButtonFill != null)
        {
            eliteButtonFill.fillAmount = 0f;
        }
        if (weatherNotificationText != null)
        {
            weatherNotificationText.alpha = 0f;
        }
        UpdateUI();

    }

    private IEnumerator InitializeUINextFrame()
    {
        yield return null;
        UpdateUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance != null)
        {
            bool assaultActive = GameManager.Instance.IsAssaultActive();

            // Start assault button only available when NOT in assault
            if (startAssaultButton != null)
            {
                startAssaultButton.interactable = !assaultActive;
            }
            //Click button only available DURING assault
            if (clickButton != null)
            {
                clickButton.interactable = assaultActive;
            }
            if (deployElitesButton != null)
            {
                deployElitesButton.interactable = assaultActive &&
                !EliteTroopManager.Instance.IsElitesActive()
                && EliteTroopManager.Instance.GetEliteTroopReserve() > 0;
            }

        }


    }
    private void OnClickButtonPressed()
    {
        GameManager.Instance?.OnSoldierClick();
    }
    public void UpdateUI()
    {
        if (GameManager.Instance == null) return;

        if (soldierCountText != null)
        {
            soldierCountText.text = $"Soldiers sent: {GameManager.Instance.GetTotalSoldiersSent()}";
        }
        if (groundGainedText != null)
        {
            float inches = GameManager.Instance.GetgroundGained();
            if (inches >= 12f)
            {
                float feet = inches / 12f;
                groundGainedText.text = $"Ground gained {feet:F1} feet";
            }
            else
            {
                groundGainedText.text = $"Ground gained: {inches:F1} inches";
            }
        }
        if (requisitionPointsText != null)
        {
            requisitionPointsText.text = $"Requisition Points: {GameManager.Instance.GetRequisitionPoints()}";
        }
        if (enemyHPText != null)
        {
            float currentHP = GameManager.Instance.GetCurrentEnemyHP();
            int maxHP = GameManager.Instance.GetMaxEnemyHP();
            if (GameManager.Instance.IsReinforcing())
            {
                float maxReinforcedHP = GameManager.Instance.GetMaxReinforcedHP();
                // Show current vs original max, with reinforced cap in parentheses
                enemyHPText.text = $"Enemy Trench: {currentHP:F1}/{maxHP} (Max: {maxReinforcedHP:F1})";
                enemyHPText.color = Color.red;
            }
            else
            {
                enemyHPText.text = $"Enemy Trench: {currentHP:F1}/{maxHP}";
                enemyHPText.color = Color.white;
            }
        }
        UpdateTimerDisplay();
        UpdateWeatherDisplay();
        UpdateForecastDisplay();
        UpdateEliteDisplay();
    }
    private void UpdateTimerDisplay()
    {
        if (timerText == null) return;
        if (!GameManager.Instance.IsAssaultActive())
        {
            timerText.text = "Ready to Assault";
            timerText.color = normalTimerColor;
            return;
        }
        float timeRemaining = GameManager.Instance.GetAssaultTimeRemaining();
        if (GameManager.Instance.IsReinforcing())
        {
            timerText.text = "REINFORCEMENTS!";
            timerText.color = criticalTimerColor;

            // pulse effect
            float pulse = Mathf.PingPong(Time.time * 2f, 1f);
            timerText.fontSize = 24 + (pulse * 4);
        }
        else
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(timeRemaining % 60f);
            timerText.text = $"Time: {minutes:00}:{seconds:00}";

            //change colour based on time remaining
            if (timeRemaining <= criticalThreshold)
            {
                timerText.color = criticalTimerColor;
                //pulse effect when critical
                float pulse = Mathf.PingPong(Time.time * 3f, 1f);
                timerText.fontSize = 24 + (pulse * 6);
            }
            else if (timeRemaining <= warningThreshold)
            {
                timerText.color = warningTimerColor;
                timerText.fontSize = 24;
            }
            else
            {
                timerText.color = normalTimerColor;
                timerText.fontSize = 24;
            }
        }
    }
    public void ShowReinforcementWarning()
    {
        if (isShowingWarning) return;
        isShowingWarning = true;
        Debug.Log("UI: Showing reinforcement warning!");
    }
    private void OnStartAssaultButtonPressed()
    {
        GameManager.Instance?.StartAssaultButton();
    }
    private void OnDeployElitesButtonPressed()
    {
        EliteTroopManager.Instance?.DeployEliteTroops();
    }
    public void TriggerClickButtonAnimation()
    {
        if (clickButton != null)
        {
            ClickButton buttonAnim = clickButton.GetComponent<ClickButton>();
            if (buttonAnim != null)
            {
                buttonAnim.TriggerPunchAnimation();
            }
        }
    }

    // --- Weather Display ---
    private void UpdateWeatherDisplay()
    {
        if (weatherText == null) return;

        if (!GameManager.Instance.IsAssaultActive())
        {
            weatherText.text = "";
            return;
        }

        float eff = WeatherManager.Instance.GetEffectiveness();
        string name = WeatherConfig.GetDisplayName(WeatherManager.Instance.GetCurrentWeather());
        weatherText.text = $"{Mathf.RoundToInt(eff * 100)}% - {name}";

        // Color based on effectiveness
        if (eff >= 0.9f)
            weatherText.color = Color.green;
        else if (eff >= 0.7f)
            weatherText.color = Color.yellow;
        else
            weatherText.color = Color.red;
    }
    private void UpdateForecastDisplay()
    {
        if (forecastText == null) return;
        if (WeatherStationManager.Instance == null || WeatherStationManager.Instance.GetLevel() == 0)
        {
            forecastText.text = "";
            return;
        }
        string message = WeatherStationManager.Instance.GetForecast(out ForecastRisk risk);
        forecastText.text = message;
        switch (risk)
        {
            case ForecastRisk.Low:
                forecastText.color = Color.green;
                break;
            case ForecastRisk.Medium:
                forecastText.color = Color.yellow;
                break;
            case ForecastRisk.High:
                forecastText.color = Color.red;
                break;

        }
    }
    private void UpdateEliteDisplay()
    {
        if (EliteTroopManager.Instance == null) return;
        //Reserve counter
        if (eliteReserveText != null)
        {
            int reserve = EliteTroopManager.Instance.GetEliteTroopReserve();
            eliteReserveText.text = $"Elite Reserve: {reserve}";
        }
        //Button fill and text
        if (eliteButtonText != null)
        {
            if (EliteTroopManager.Instance.IsElitesActive())
            {
                float timer = EliteTroopManager.Instance.GetEliteDeploymentTimer();
                eliteButtonText.text = $"{timer:F1}s";
                if (eliteButtonFill != null)
                {
                    float duration = EliteTroopManager.Instance.GetEliteDeploymentDuration();
                    eliteButtonFill.fillAmount = duration > 0f ? timer / duration : 0f;
                }
            }
            else
            {
                eliteButtonText.text = EliteTroopManager.Instance.GetEliteTroopReserve() > 0
                ? "Deploy Stormtroopers" : "No reserves";
                if (eliteButtonFill != null)
                {
                    eliteButtonFill.fillAmount = 0f;
                }
            }
        }
    }
    public void ShowEliteSurvivalNotification(int survivors, int deployed)
    {
        ShowWeatherNotification($"Storm Troopers: {survivors}/{deployed} survived");
    }
    public void ShowWeatherNotification(string message)
    {
        if (weatherNotificationText == null) return;
        StopCoroutine(nameof(FadeNotification));
        weatherNotificationText.text = message;
        weatherNotificationText.alpha = 1f;
        StartCoroutine(FadeNotification());
    }

    private IEnumerator FadeNotification()
    {
        yield return new WaitForSeconds(1f);

        float elapsed = 0f;
        float fadeDuration = 1f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            weatherNotificationText.alpha = 1f - (elapsed / fadeDuration);
            yield return null;
        }
        weatherNotificationText.alpha = 0f;
    }
}
