using UnityEngine;
using System.Collections.Generic;

public class SoldierVisualManager : MonoBehaviour
{
    public static SoldierVisualManager Instance { get; private set; }
    [Header("Prefab & Spawn Settings")]
    [SerializeField] private GameObject soldierPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform targetPoint;
    [SerializeField] private int initialPoolSize = 20;
    [SerializeField] private int maxSoldiersPerBatch = 10;

    [Header("Movement")]
    [SerializeField] private float baseSpeed = 2f;
    [SerializeField] private float speedVariation = 0.5f;
    [SerializeField] private float casualtyChance = 0.1f;
    [SerializeField] private float runAttackChance = 0.3f;

    [Header("Spawn Spread")]
    [SerializeField] private float yMin = -1f;
    [SerializeField] private float yMax = 1f;
    [SerializeField] private float zSpread = 1f;

    [Header("Animation")]
    [SerializeField] private int directionRow = 6;
    [SerializeField] private int framesPerRow = 8;

    //Object pool
    private Queue<SoldierVisual> pool;
    private HashSet<SoldierVisual> activeSoldiers = new HashSet<SoldierVisual>();
    private Sprite[] runFrames;
    private Sprite[] attackFrames;
    private Sprite[] runAttackFrames;
    private Sprite[][] allDieFrames;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        LoadSprites();
        InitializePool();
    }
    private void Update()
    {
        if (activeSoldiers.Count > 0 && !GameManager.Instance.IsAssaultActive())
            OnTrenchCaptured();
    }
    private void LoadSprites()
    {
        runFrames = LoadSingleRow("Run");
        attackFrames = LoadSingleRow("Attack1");
        runAttackFrames = LoadSingleRow("RunAttack");

        Sprite[] allDieSprites = Resources.LoadAll<Sprite>("Die");
        System.Array.Sort(allDieSprites, (a, b) => ExtractIndex(a.name).CompareTo(ExtractIndex(b.name)));
        int dieTotalRows = allDieSprites.Length / framesPerRow;
        allDieFrames = new Sprite[dieTotalRows][];
        for (int row = 0; row < dieTotalRows; row++)
        {
            allDieFrames[row] = new Sprite[framesPerRow];
            int rowStart = row * framesPerRow;
            for (int i = 0; i < framesPerRow; i++)
                allDieFrames[row][i] = allDieSprites[rowStart + i];
        }
    }
    private Sprite[] LoadSingleRow(string resourceName)
    {
        Sprite[] all = Resources.LoadAll<Sprite>(resourceName);
        System.Array.Sort(all, (a, b) => ExtractIndex(a.name).CompareTo(ExtractIndex(b.name)));
        int totalRows = all.Length / framesPerRow;
        int unityRow = (totalRows - 1) - directionRow;
        int startIndex = unityRow * framesPerRow;
        Sprite[] row = new Sprite[framesPerRow];
        for (int i = 0; i < framesPerRow; i++)
            row[i] = all[startIndex + i];
        return row;
    }

    private int ExtractIndex(string spriteName)
    {
        // "Run_42" -> 42, "Die_7" -> 7
        int underscorePos = spriteName.LastIndexOf('_');
        if (underscorePos < 0 || !int.TryParse(spriteName.Substring(underscorePos + 1), out int index))
        {
            Debug.LogError($"Couldn't parse sprite index from name: {spriteName}");
            return 0;
        }
        return index;
    }
    public void SpawnSoldierBatch(int count)
    {
        int visualCount = Mathf.Clamp(Mathf.CeilToInt(Mathf.Log(count + 1, 2)), 1, maxSoldiersPerBatch);
        for (int i = 0; i < visualCount; i++)
        {
            SoldierVisual soldier = GetFromPool();
            float speed = baseSpeed + Random.Range(-speedVariation, speedVariation);
            float yPos = Random.Range(yMin, yMax);
            float zOffset = Random.Range(-zSpread, zSpread);
            soldier.transform.position = spawnPoint.position + new Vector3(0f, yPos, zOffset);
            Sprite[] chosenRunFrames = Random.value < runAttackChance ? runAttackFrames : runFrames;
            soldier.Initialize(chosenRunFrames, attackFrames, allDieFrames, speed, targetPoint.position + new Vector3(0f, 0f, zOffset), casualtyChance);
        }
    }

    private SoldierVisual GetFromPool()
    {
        SoldierVisual soldier;
        if (pool.Count > 0)
            soldier = pool.Dequeue();
        else
        {
            // Pool empty — grow it on demand, soldier will return via ReturnToPool
            GameObject obj = Instantiate(soldierPrefab);
            obj.SetActive(false);
            soldier = obj.GetComponent<SoldierVisual>();
        }
        activeSoldiers.Add(soldier);
        return soldier;

    }
    public void ReturnToPool(SoldierVisual soldier)
    {
        activeSoldiers.Remove(soldier);
        pool.Enqueue(soldier);
    }
    private void InitializePool()
    {
        pool = new Queue<SoldierVisual>();
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject obj = Instantiate(soldierPrefab);
            obj.SetActive(false);
            SoldierVisual soldier = obj.GetComponent<SoldierVisual>();
            pool.Enqueue(soldier);
        }
    }
    public void OnTrenchCaptured()
    {
        var toDeactivate = new List<SoldierVisual>(activeSoldiers);
        foreach (var soldier in toDeactivate)
            soldier.ForceDeactivate();
    }

}
