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

    [Header("Spawn Spread")]
    [SerializeField] private float yMin = -1f;
    [SerializeField] private float yMax = 1f;
    [SerializeField] private float zSpread = 1f;

    [Header("Animation")]
    [SerializeField] private int directionRow = 6;
    [SerializeField] private int framesPerRow = 8;

    //Object pool
    private Queue<SoldierVisual> pool;
    private Sprite[] runFrames;
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
    private void LoadSprites()
    {
        Sprite[] allRunSprites = Resources.LoadAll<Sprite>("Run");
        Sprite[] allDieSprites = Resources.LoadAll<Sprite>("Die");

        // Sort by name numerically (Resources.LoadAll order is not guaranteed)
        System.Array.Sort(allRunSprites, (a, b) => ExtractIndex(a.name).CompareTo(ExtractIndex(b.name)));
        System.Array.Sort(allDieSprites, (a, b) => ExtractIndex(a.name).CompareTo(ExtractIndex(b.name)));

        // Rows are ordered bottom-to-top in the spritesheet but top-to-bottom in Unity's slice order
        // So we flip: Unity row = (totalRows - 1) - directionRow
        int totalRows = allRunSprites.Length / framesPerRow;
        int unityRow = (totalRows - 1) - directionRow;
        int startIndex = unityRow * framesPerRow;

        runFrames = new Sprite[framesPerRow];
        for (int i = 0; i < framesPerRow; i++)
            runFrames[i] = allRunSprites[startIndex + i];

        // Load all rows of the die sheet so soldiers can pick a random death animation
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
            soldier.Initialize(runFrames, allDieFrames, speed, targetPoint.position + new Vector3(0f, 0f, zOffset), casualtyChance);
        }
    }

    private SoldierVisual GetFromPool()
    {
        if (pool.Count > 0)
            return pool.Dequeue();

        // Pool empty — grow it on demand, soldier will return via ReturnToPool
        GameObject obj = Instantiate(soldierPrefab);
        obj.SetActive(false);
        return obj.GetComponent<SoldierVisual>();
    }
    public void ReturnToPool(SoldierVisual soldier)
    {
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

}
