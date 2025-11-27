using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Singleton;
using UnityEngine.Pool;

public class WaveManager : Singleton<WaveManager>
{
    [Header("Configuration")]
    [SerializeField] private TextAsset jsonFile;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform nexusTarget;

    [Header("Enemy Registry")]
    [SerializeField] private List<EnemyIDPair> enemyRegistryList;

    private Dictionary<string, IObjectPool<Enemy>> enemyPools;
    private Dictionary<string, GameObject> enemyPrefabs;

    private WaveContainer waveData;
    private int currentWaveIndex = 0;

    private bool isSpawning = false;

    [System.Serializable]
    public struct EnemyIDPair
    {
        public string id;
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
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEnemyDied += OnEnemyDied;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateWaveUI(currentWaveIndex + 1, waveData.waves.Count);

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
        if (!isSpawning && GameManager.Instance.Enemies.Count == 0)
        {
            if (currentWaveIndex < waveData.waves.Count)
            {
                UIManager.Instance.SetWaveButtonState(true);
                GameManager.Instance.ChangeState(new BuildState());
            }
            else
            {
                Debug.Log("All Waves Cleared! Triggering Victory.");
                GameManager.Instance.TriggerVictory();
            }
        }
    }

    private void InitializeRegistryAndPools()
    {
        enemyPools = new Dictionary<string, IObjectPool<Enemy>>();
        enemyPrefabs = new Dictionary<string, GameObject>();

        foreach (var pair in enemyRegistryList)
        {
            if (!enemyPrefabs.ContainsKey(pair.id))
                enemyPrefabs.Add(pair.id, pair.prefab);

            if (!enemyPools.ContainsKey(pair.id))
            {
                string enemyId = pair.id;
                GameObject prefab = pair.prefab;

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

    private Enemy CreateEnemy(GameObject prefab, string id)
    {
        GameObject go = Instantiate(prefab);
        Enemy e = go.GetComponent<Enemy>();
        e.SetPool(enemyPools[id]);
        return e;
    }

    public void LoadWaveData()
    {
        if (jsonFile != null)
            waveData = JsonUtility.FromJson<WaveContainer>(jsonFile.text);
        else
            waveData = new WaveContainer();
    }

    public void StartNextWave()
    {
        if (currentWaveIndex >= waveData.waves.Count) return;

        if (isSpawning || GameManager.Instance.Enemies.Count > 0)
            return;

        GameManager.Instance.ChangeState(new CombatState());

        StartCoroutine(SpawnWaveRoutine(waveData.waves[currentWaveIndex]));
    }

    private IEnumerator SpawnWaveRoutine(WaveDefinition wave)
    {
        isSpawning = true;
        UIManager.Instance.SetWaveButtonState(false);

        foreach (var group in wave.groups)
        {
            if (group.initialDelay > 0)
                yield return new WaitForSeconds(group.initialDelay);

            yield return StartCoroutine(SpawnGroupRoutine(group));
        }

        isSpawning = false;
        currentWaveIndex++;

        UIManager.Instance.UpdateWaveUI(Mathf.Min(currentWaveIndex + 1, waveData.waves.Count), waveData.waves.Count);

        if (GameManager.Instance.Enemies.Count == 0 && currentWaveIndex < waveData.waves.Count)
        {
            UIManager.Instance.SetWaveButtonState(true);
            GameManager.Instance.ChangeState(new BuildState());
        }
    }

    private IEnumerator SpawnGroupRoutine(EnemySpawnGroup group)
    {
        if (!enemyPools.ContainsKey(group.enemyId)) yield break;

        IObjectPool<Enemy> pool = enemyPools[group.enemyId];

        for (int i = 0; i < group.count; i++)
        {
            SpawnSingleEnemy(pool);
            yield return new WaitForSeconds(group.rate);
        }
    }

    private void SpawnSingleEnemy(IObjectPool<Enemy> pool)
    {
        Enemy e = pool.Get();
        Transform point = spawnPoints.Length > 0
            ? spawnPoints[Random.Range(0, spawnPoints.Length)]
            : transform;

        e.transform.position = point.position;
        e.transform.rotation = Quaternion.identity;

        if (e != null)
            e.Initialize(2.0f, 100f, nexusTarget);
    }
}