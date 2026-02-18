using UnityEngine;
using System.Collections.Generic;

public class SoldierVisualManager : MonoBehaviour
{
    public static SoldierVisualManager Instance { get; private set; }
    [Header("Prefab & Spawn Settings")]
    [SerializeField] private GameObject soldierPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform targetPoint;
    [SerializeField] private int poolSize = 30;
    [SerializeField] private int soldiersPerBatch = 3;

    [Header("Movement")]
    [SerializeField] private float baseSpeed = 2f;
    [SerializeField] private float speedVariation = 0.5f;

    [Header("Spawn Spread")]
    [SerializeField] private float yMin = -1f;
    [SerializeField] private float yMax = 1f;
    [SerializeField] private float zSpread = 1f;

    [Header("Animation")]
    [SerializeField] private int directionRow = 6;

    //Object pool
    private Queue<SoldierVisual> pool;
    private Sprite[] runFrames;
    private Sprite[] dieFrames;

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
        int unityRow = 7 - directionRow;
        int startIndex = unityRow * 8;

        runFrames = new Sprite[8];
        dieFrames = new Sprite[8];

        for (int i = 0; i < 8; i++)
        {
            runFrames[i] = allRunSprites[startIndex + i];
            dieFrames[i] = allDieSprites[startIndex + i];
        }
    }

    private int ExtractIndex(string spriteName)
    {
        // "Run_42" -> 42, "Die_7" -> 7
        int underscorePos = spriteName.LastIndexOf('_');
        return int.Parse(spriteName.Substring(underscorePos + 1));
    }
    public void SpawnSoldierBatch()
    {
        for (int i = 0; i < soldiersPerBatch; i++)
        {
            if (pool.Count == 0) return;
            SoldierVisual soldier = pool.Dequeue();
            float speed = baseSpeed + Random.Range(-speedVariation, speedVariation);
            float yPos = Random.Range(yMin, yMax);
            float zOffset = Random.Range(-zSpread, zSpread);
            soldier.transform.position = spawnPoint.position + new Vector3(0f, yPos, zOffset);
            soldier.Initialize(runFrames, dieFrames, speed, targetPoint.position + new Vector3(0f, 0f, zOffset), 0f); ;
            Debug.Log($"Soldier spawned at ({spawnPoint.position.x}, {yPos}) targeting X={targetPoint.position.x}, speed={speed:F2}");
        }
    }
    public void ReturnToPool(SoldierVisual soldier)
    {
        pool.Enqueue(soldier);
    }
    private void InitializePool()
    {
        pool = new Queue<SoldierVisual>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(soldierPrefab);
            obj.SetActive(false);
            SoldierVisual soldier = obj.GetComponent<SoldierVisual>();
            pool.Enqueue(soldier);
        }
    }

}
