using UnityEngine;
using System.Collections.Generic;
public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance { get; private set; }
    [Header("Level Thresholds")]
    [SerializeField] private int[] levelThresholds = { 50, 200, 500 };
    [SerializeField] private int historyWindow = 5;

    private List<int> assaultHistory = new List<int>();
    private int soldiersSentThisAssault = 0;
    private int playerLevel = 1;
    private static readonly string[] levelNames = { "Private", "Corporal", "Sergeant", "Lieutenant" };
    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        DontDestroyOnLoad(gameObject);
    }
    public int GetLevel() => playerLevel;
    public string GetLevelName() => levelNames[Mathf.Clamp(playerLevel - 1, 0, levelNames.Length - 1)];
    public float GetRollingAverage()
    {
        if (assaultHistory.Count == 0) return 0f;
        int sum = 0;
        foreach (int count in assaultHistory) sum += count;
        return (float)sum / assaultHistory.Count;
    }
    public int GetSoldiersThisAssault() => soldiersSentThisAssault;
    public int[] GetLevelThresholds() => levelThresholds;
    public void StartNewAssault()
    {
        soldiersSentThisAssault = 0;
    }
    public void RecordSoldiers(int count)
    {
        soldiersSentThisAssault += count;
    }
    public void OnAssaultComplete()
    {
        assaultHistory.Add(soldiersSentThisAssault);
        if (assaultHistory.Count > historyWindow)
            assaultHistory.RemoveAt(0);
        soldiersSentThisAssault = 0;
        CheckLevelUp(GetRollingAverage());
    }
    private void CheckLevelUp(float average)
    {
        for (int i = 0; i < levelThresholds.Length; i++)
        {
            int requiredLevel = i + 2; // thresholds [0]  = level 2, etc
            if (average >= levelThresholds[i] && playerLevel < requiredLevel)
            {
                playerLevel = requiredLevel;
                Debug.Log($"Level up! Now {GetLevelName()} (Level {playerLevel})");
                UIManager.Instance?.ShowLevelUpNotification(playerLevel, GetLevelName());
            }
        }
    }

}
