using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("UI Reference")]
    public TextMeshProUGUI waveText;

    [Header("Wave Settings")]
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;
    public int baseEnemyCount = 3;
    public float timeBetweenWaves = 10f;

    private int currentWave = 0;
    private int maxWaves = 5;
    private bool allWavesSpawned = false;

    private readonly Queue<int> waveSpawnPlan = new Queue<int>();

    public int CurrentWave => currentWave;
    public int MaxWaves => maxWaves;

    void Awake()
    {
        Instance = this;
        BuildWavePlan();
    }

    void Start()
    {
        UpdateWaveUI();
        StartCoroutine(WaveRoutine());
    }

    private void BuildWavePlan()
    {
        waveSpawnPlan.Clear();
        for (int w = 1; w <= maxWaves; w++)
        {
            waveSpawnPlan.Enqueue(baseEnemyCount + (w * 2));
        }
    }

    IEnumerator WaveRoutine()
    {
        while (waveSpawnPlan.Count > 0)
        {
            currentWave++;
            UpdateWaveUI();

            int enemiesToSpawn = waveSpawnPlan.Dequeue();

            Queue<Transform> perWaveSpawnQueue = new Queue<Transform>();
            for (int i = 0; i < enemiesToSpawn; i++)
            {
                perWaveSpawnQueue.Enqueue(PickSpawnPoint());
            }

            while (perWaveSpawnQueue.Count > 0)
            {
                SpawnEnemy(perWaveSpawnQueue.Dequeue());
                yield return new WaitForSeconds(0.5f);
            }

            yield return new WaitForSeconds(timeBetweenWaves);
        }

        allWavesSpawned = true;
        if (waveText != null)
        {
            waveText.text = "Final Wave: Clear remaining enemies!";
        }
    }

    void UpdateWaveUI()
    {
        if (waveText != null)
        {
            waveText.text = $"Wave: {currentWave} / {maxWaves}";
        }
    }

    Transform PickSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return transform;
        int idx = Random.Range(0, spawnPoints.Length);
        return spawnPoints[idx];
    }

    void SpawnEnemy(Transform point)
    {
        if (enemyPrefab == null || point == null) return;
        Instantiate(enemyPrefab, point.position, Quaternion.identity);
    }

    void Update()
    {
        if (allWavesSpawned && Enemy.ActiveEnemies.Count == 0)
        {
            GameManager.Instance.TriggerWin();
            this.enabled = false;
        }
    }
}
