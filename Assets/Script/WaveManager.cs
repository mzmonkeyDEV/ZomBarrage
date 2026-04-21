using UnityEngine;
using TMPro; // Required for UI
using System.Collections;

public class WaveManager : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI waveText; // Drag Wave_Text here

    [Header("Wave Settings")]
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;
    public int baseEnemyCount = 3;
    public float timeBetweenWaves = 10f;

    private int currentWave = 0;
    private int maxWaves = 5;
    private bool allWavesSpawned = false;

    void Start()
    {
        UpdateWaveUI(); // Initial text
        StartCoroutine(WaveRoutine());
    }

    IEnumerator WaveRoutine()
    {
        while (currentWave < maxWaves)
        {
            currentWave++;
            UpdateWaveUI(); // Update text when wave starts

            int enemiesToSpawn = baseEnemyCount + (currentWave * 2);

            for (int i = 0; i < enemiesToSpawn; i++)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(0.5f);
            }

            // Wait 10 seconds before starting the next wave
            yield return new WaitForSeconds(timeBetweenWaves);
        }

        allWavesSpawned = true;
        waveText.text = "Final Wave: Clear remaining enemies!";
    }

    void UpdateWaveUI()
    {
        if (waveText != null)
        {
            waveText.text = $"Wave: {currentWave} / {maxWaves}";
        }
    }

    void SpawnEnemy()
    {
        int randomIndex = Random.Range(0, spawnPoints.Length);
        Instantiate(enemyPrefab, spawnPoints[randomIndex].position, Quaternion.identity);
        // Note: The Enemy script now handles adding itself to the list in Awake()
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