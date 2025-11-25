using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Singleton;
/*
public class WaveManager : Singleton<WaveManager>
{
    [Header("Configuration")]
    [SerializeField] private TextAsset jsonFile; // Drag your .json or .txt file here
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform nexusTarget;

    [Header("Enemy Registry")]
    // Map string IDs from JSON to actual Prefabs
    [SerializeField] private List<EnemyIDPair> enemyRegistryList;
    private Dictionary<string, GameObject> enemyRegistry;

    private WaveContainer waveData;
    private int currentWaveIndex = 0;
    private bool isWaveActive = false;

    [System.Serializable]
    public struct EnemyIDPair
    {
        public string id; 
        public GameObject prefab;
    }

    public override void Awake()
    {
        base.Awake();
        InitializeRegistry();
        LoadWaveData();
    }

    private void Start()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateWaveUI(currentWaveIndex + 1, waveData.waves.Count);
            UIManager.Instance.SetWaveButtonState(true);
        }
    }

    private void InitializeRegistry()
    {
        enemyRegistry = new Dictionary<string, GameObject>();
        foreach (var pair in enemyRegistryList)
        {
            if (!enemyRegistry.ContainsKey(pair.id))
                enemyRegistry.Add(pair.id, pair.prefab);
        }
    }

    public void LoadWaveData()
    {
        if (jsonFile != null)
        {
            waveData = JsonUtility.FromJson<WaveContainer>(jsonFile.text);
            Debug.Log($"Loaded {waveData.waves.Count} waves.");
        }
        else
        {
            Debug.LogError("No JSON file assigned to WaveManager!");
            waveData = new WaveContainer(); // Prevent null ref
        }
    }

    public void StartNextWave()
    {
        if (isWaveActive || currentWaveIndex >= waveData.waves.Count) return;

        StartCoroutine(SpawnWaveRoutine(waveData.waves[currentWaveIndex]));
    }

    private IEnumerator SpawnWaveRoutine(WaveDefinition wave)
    {
        isWaveActive = true;
        UIManager.Instance.SetWaveButtonState(false);

        foreach (var group in wave.groups)
        {
            if (group.initialDelay > 0)
                yield return new WaitForSeconds(group.initialDelay);

            yield return StartCoroutine(SpawnGroupRoutine(group));
        }

        currentWaveIndex++;
        isWaveActive = false;

        bool allWavesComplete = currentWaveIndex >= waveData.waves.Count;
        UIManager.Instance.UpdateWaveUI(Mathf.Min(currentWaveIndex + 1, waveData.waves.Count), waveData.waves.Count);

        if (!allWavesComplete)
        {
            UIManager.Instance.SetWaveButtonState(true);
        }
        else
        {
            Debug.Log("All waves complete!");
        }
    }

    private IEnumerator SpawnGroupRoutine(EnemySpawnGroup group)
    {
        if (!enemyRegistry.ContainsKey(group.enemyId))
        {
            Debug.LogError($"Enemy ID '{group.enemyId}' not found in Registry!");
            yield break;
        }

        GameObject prefab = enemyRegistry[group.enemyId];

        for (int i = 0; i < group.count; i++)
        {
            SpawnSingleEnemy(prefab);
            yield return new WaitForSeconds(group.rate);
        }
    }

    private void SpawnSingleEnemy(GameObject prefab)
    {
        Transform point = spawnPoints.Length > 0
            ? spawnPoints[Random.Range(0, spawnPoints.Length)]
            : transform;

        GameObject go = Instantiate(prefab, point.position, Quaternion.identity);
        Enemy e = go.GetComponent<Enemy>();

        if (e != null)
        {
    
            e.Initialize(2.0f, 100f, nexusTarget);
        }
    }
}
*/