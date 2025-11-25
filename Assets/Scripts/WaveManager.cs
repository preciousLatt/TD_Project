using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Singleton;
using UnityEngine.Pool; // Added for Object Pooling

public class WaveManager : Singleton<WaveManager>
{
    [Header("Configuration")]
    [SerializeField] private TextAsset jsonFile; // Drag your .json or .txt file here
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform nexusTarget;

    [Header("Enemy Registry")]
    // Map string IDs from JSON to actual Prefabs
    [SerializeField] private List<EnemyIDPair> enemyRegistryList;

    // Replaced simple Dictionary with Dictionary of Pools
    private Dictionary<string, IObjectPool<Enemy>> enemyPools;
    // We keep the prefab reference for the pool's create function
    private Dictionary<string, GameObject> enemyPrefabs;

    private WaveContainer waveData;
    private int currentWaveIndex = 0;

    // "isSpawning" tracks if the coroutine is currently outputting enemies.
    // "isWaveInProgress" logic effectively becomes: isSpawning || GameManager.Enemies.Count > 0
    private bool isSpawning = false;

    [System.Serializable]
    public struct EnemyIDPair
    {
        public string id; // e.g., "Blue", "Red"
        public GameObject prefab;
    }

    public override void Awake()
    {
        base.Awake();
        InitializeRegistryAndPools();
        LoadWaveData();
    }

    private void Start()
    {
        // Subscribe to GameManager to track enemy deaths
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEnemyDied += OnEnemyDied;
        }

        // Initial UI Update
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateWaveUI(currentWaveIndex + 1, waveData.waves.Count);

            // Only allow start if no enemies exist (should be true at start)
            bool safeToStart = GameManager.Instance.Enemies.Count == 0;
            UIManager.Instance.SetWaveButtonState(safeToStart);
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEnemyDied -= OnEnemyDied;
        }
    }

    private void OnEnemyDied(Enemy enemy)
    {
        // Logic: If we are NOT currently spitting out new enemies,
        // AND the enemy count just dropped to zero, allow the next wave.
        if (!isSpawning && GameManager.Instance.Enemies.Count == 0)
        {
            if (currentWaveIndex < waveData.waves.Count)
            {
                UIManager.Instance.SetWaveButtonState(true);
            }
            else
            {
                Debug.Log("All Waves Cleared!");
                // Optionally trigger Victory UI here
            }
        }
    }

    private void InitializeRegistryAndPools()
    {
        enemyPools = new Dictionary<string, IObjectPool<Enemy>>();
        enemyPrefabs = new Dictionary<string, GameObject>();

        foreach (var pair in enemyRegistryList)
        {
            // 1. Store the prefab for reference
            if (!enemyPrefabs.ContainsKey(pair.id))
            {
                enemyPrefabs.Add(pair.id, pair.prefab);
            }

            // 2. Create a pool for this specific Enemy ID
            if (!enemyPools.ContainsKey(pair.id))
            {
                // We capture 'pair.id' and 'pair.prefab' in the closure
                string enemyId = pair.id;
                GameObject prefab = pair.prefab;

                // Create the pool
                IObjectPool<Enemy> pool = new ObjectPool<Enemy>(
                    createFunc: () => CreateEnemy(prefab, enemyId),
                    actionOnGet: (enemy) => enemy.gameObject.SetActive(true),
                    actionOnRelease: (enemy) => enemy.gameObject.SetActive(false),
                    actionOnDestroy: (enemy) => Destroy(enemy.gameObject),
                    collectionCheck: true,
                    defaultCapacity: 10,
                    maxSize: 50
                );

                enemyPools.Add(enemyId, pool);
            }
        }
    }

    // Helper function used by the Pool's createFunc
    private Enemy CreateEnemy(GameObject prefab, string id)
    {
        GameObject go = Instantiate(prefab);
        Enemy e = go.GetComponent<Enemy>();

        // IMPORTANT: We must tell the enemy which pool it belongs to
        // This requires adding 'public void SetPool(IObjectPool<Enemy> pool)' to your Enemy.cs
        e.SetPool(enemyPools[id]);

        return e;
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
            waveData = new WaveContainer();
        }
    }

    // Called by the UI Button
    public void StartNextWave()
    {
        // 1. Check if we have more waves
        if (currentWaveIndex >= waveData.waves.Count) return;

        // 2. Strict Check: Prevent start if spawning active OR enemies still alive
        if (isSpawning || GameManager.Instance.Enemies.Count > 0)
        {
            Debug.Log("Cannot start wave: Enemies still alive or spawning in progress.");
            return;
        }

        StartCoroutine(SpawnWaveRoutine(waveData.waves[currentWaveIndex]));
    }

    private IEnumerator SpawnWaveRoutine(WaveDefinition wave)
    {
        isSpawning = true;
        UIManager.Instance.SetWaveButtonState(false); // Disable button immediately

        // Iterate through all groups in this wave
        foreach (var group in wave.groups)
        {
            if (group.initialDelay > 0)
                yield return new WaitForSeconds(group.initialDelay);

            yield return StartCoroutine(SpawnGroupRoutine(group));
        }

        // Spawning Phase Complete
        isSpawning = false;
        currentWaveIndex++;

        // Update UI Text for the *next* wave number
        UIManager.Instance.UpdateWaveUI(Mathf.Min(currentWaveIndex + 1, waveData.waves.Count), waveData.waves.Count);

        // NOTE: We do NOT re-enable the button here. 
        // We wait for OnEnemyDied to detect 0 enemies left.

        // Edge Case: If the wave was empty or enemies died instantly, check now
        if (GameManager.Instance.Enemies.Count == 0 && currentWaveIndex < waveData.waves.Count)
        {
            UIManager.Instance.SetWaveButtonState(true);
        }
    }

    private IEnumerator SpawnGroupRoutine(EnemySpawnGroup group)
    {
        if (!enemyPools.ContainsKey(group.enemyId))
        {
            Debug.LogError($"Enemy ID '{group.enemyId}' not found in Registry/Pool!");
            yield break;
        }

        // We don't need the prefab here anymore, we need the POOL
        IObjectPool<Enemy> pool = enemyPools[group.enemyId];

        for (int i = 0; i < group.count; i++)
        {
            SpawnSingleEnemy(pool);
            yield return new WaitForSeconds(group.rate);
        }
    }

    private void SpawnSingleEnemy(IObjectPool<Enemy> pool)
    {
        // Get from pool instead of Instantiate
        Enemy e = pool.Get();

        Transform point = spawnPoints.Length > 0
            ? spawnPoints[Random.Range(0, spawnPoints.Length)]
            : transform;

        // Reset position and rotation
        e.transform.position = point.position;
        e.transform.rotation = Quaternion.identity;

        if (e != null)
        {
            e.Initialize(2.0f, 100f, nexusTarget);
        }
    }
}