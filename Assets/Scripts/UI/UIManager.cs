using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEditor.Rendering;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI soldierCountText;
    [SerializeField] private TextMeshProUGUI groundGainedText;
    [SerializeField] private TextMeshProUGUI enemyHPText;
    [SerializeField] private Button clickButton;
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

        //StartCoroutine(InitializeUINextFrame());
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
            enemyHPText.text = $"Enemy HP: {currentHP}/{maxHP}";
        }
    }
}
