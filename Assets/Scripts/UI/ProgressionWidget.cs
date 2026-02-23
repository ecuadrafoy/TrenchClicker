using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressionWidget : MonoBehaviour
{
    [Header("Fill Image")]
    [SerializeField] private Image progressFill;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI levelNumberText;
    [SerializeField] private TextMeshProUGUI levelNameText;

    [Header("Colors")]
    [SerializeField] private Color fillColor = new Color(0.83f, 0.78f, 0.47f, 1f);
    [SerializeField] private Color maxLevelColor = new Color(0.49f, 0.78f, 0.78f, 1f);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // Update is called once per frame
    void Update()
    {
        if (ProgressionManager.Instance == null) return;
        int level = ProgressionManager.Instance.GetLevel();
        string rankName = ProgressionManager.Instance.GetLevelName();
        float avg = ProgressionManager.Instance.GetRollingAverage();
        int[] thresholds = ProgressionManager.Instance.GetLevelThresholds();

        if (progressFill != null)
        {
            progressFill.fillAmount = CalculateFill(level, avg, thresholds);
            progressFill.color = (level >= 4) ? maxLevelColor : fillColor;
        }
        if (levelNumberText != null)
            levelNumberText.text = level.ToString();
        if (levelNameText != null)
            levelNameText.text = rankName;
    }
    private float CalculateFill(int level, float avg, int[] thresholds)
    {
        if (level >= 4) return 1f;
        int idx = level - 1;
        float prev = (idx > 0) ? thresholds[idx - 1] : 0f;
        float next = thresholds[idx];
        float range = next - prev;
        if (range <= 0f) return 1f;
        return Mathf.Clamp01((avg - prev) / range);
    }
}
