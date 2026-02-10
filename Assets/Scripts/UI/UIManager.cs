using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEditor.Rendering;
using System.Collections;
using NUnit.Framework;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI soldierCountText;
    [SerializeField] private TextMeshProUGUI groundGainedText;
    [SerializeField] private TextMeshProUGUI enemyHPText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Button clickButton;
    [SerializeField] private Button startAssaultButton;

    [Header("Warning Settings")]
    [SerializeField] private Color normalTimerColor = Color.white;
    [SerializeField] private Color warningTimerColor = Color.yellow;
    [SerializeField] private Color criticalTimerColor = Color.red;
    [SerializeField] private float warningThreshold = 30f;
    [SerializeField] private float criticalThreshold = 10f;

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
            soldierCountText.text = $"Soldiers sent: {GameManager.Instance.GetTotalSoldiers()}";
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
        if (enemyHPText != null)
        {
            int currentHP = GameManager.Instance.GetCurrentEnemyHP();
            int maxHP = GameManager.Instance.GetMaxEnemyHP();
            if (GameManager.Instance.IsReinforcing())
            {
                int maxReinforcedHP = GameManager.Instance.GetMaxReinforcedHP();
                // Show current vs original max, with reinforced cap in parentheses
                enemyHPText.text = $"Enemy Trench: {currentHP}/{maxHP} (Max: {maxReinforcedHP})";
                enemyHPText.color = Color.red;
            }
            else
            {
                enemyHPText.text = $"Enemy Trench: {currentHP}/{maxHP}";
                enemyHPText.color = Color.white;
            }
        }
        UpdateTimerDisplay();
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
}
