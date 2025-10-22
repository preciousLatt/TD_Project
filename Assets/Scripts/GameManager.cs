using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Singleton; 

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private Transform spawnCenter;       
    [SerializeField] private int maxConcurrentEnemies = 50;  
    [SerializeField] private bool startSpawningOnAwake = false;

    [SerializeField] private float startingNexusHealth = 100f;
    [SerializeField] public int startingMoney = 500;

    private float nexusHealth;
    private int currentMoney;
    private readonly List<Enemy> enemies = new List<Enemy>();
    public event Action<Enemy> OnEnemySpawned;
    public event Action<Enemy> OnEnemyDied;
    public event Action<float> OnNexusDamaged;
    public event Action<float> OnNexusHealthChanged;
    private Coroutine spawnCoroutine;


    public override void Awake()
    {
        base.Awake();
        nexusHealth = startingNexusHealth;
        currentMoney = startingMoney;
        if (spawnCenter == null)
        {
            GameObject go = new GameObject("SpawnCenter");
            go.transform.SetParent(transform, false);
            spawnCenter = go.transform;
        }

        GameObject root = new GameObject("Enemies");
        root.transform.SetParent(transform, false);
    }

    private void Start()
    {
        if (startSpawningOnAwake && spawnCoroutine == null)
        {
            StartSpawningEnemies(defaultEnemyPrefabForEditor, 1.5f, 10f, maxConcurrentEnemies);
        }
        UIManager.Instance?.UpdateCurrencyUI(currentMoney);

    }


    public void RegisterEnemy(Enemy e)
    {
        if (e == null) return;
        if (!enemies.Contains(e)) enemies.Add(e);
    }

    public void UnregisterEnemy(Enemy e)
    {
        if (e == null) return;
        enemies.Remove(e);
    }

    public List<Enemy> GetAllEnemies() => new List<Enemy>(enemies);


    public Enemy GetClosestEnemy(Vector3 fromPosition)
    {
        Enemy closest = null;
        float bestSqr = float.PositiveInfinity;
        for (int i = 0; i < enemies.Count; i++)
        {
            Enemy e = enemies[i];
            if (e == null || e.IsDead) continue;
            float sqr = (e.transform.position - fromPosition).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                closest = e;
            }
        }
        return closest;
    }


    public bool DamageEnemy(Enemy enemy, float damage)
    {
        if (enemy == null) return false;
        if (!enemies.Contains(enemy)) return false;
        enemy.TakeDamage(damage);
        return true;
    }

    public bool DamageEnemyByGameObject(GameObject enemyGO, float damage)
    {
        if (enemyGO == null) return false;
        Enemy e = enemyGO.GetComponentInChildren<Enemy>();
        if (e == null) return false;
        return DamageEnemy(e, damage);
    }

    public void DamageNexus(float amount)
    {
        if (amount <= 0f) return;
        nexusHealth -= amount;
        if (nexusHealth < 0f) nexusHealth = 0f;
        OnNexusDamaged?.Invoke(amount);
        OnNexusHealthChanged?.Invoke(nexusHealth);
        Debug.Log($"Nexus took {amount} damage. Health = {nexusHealth}");
        if (nexusHealth <= 0f)
        {
            Debug.Log("Nexus destroyed");
        }
    }

    public float GetNexusHealth() => nexusHealth;
    public void SetNexusHealth(float hp)
    {
        nexusHealth = hp;
        OnNexusHealthChanged?.Invoke(nexusHealth);
    }

    public void NotifyEnemyDied(Enemy e)
    {
        if (e == null) return;
        UnregisterEnemy(e);
        OnEnemyDied?.Invoke(e);
    }


    [SerializeField] private GameObject defaultEnemyPrefabForEditor;

    public void StartSpawningEnemies(GameObject enemyPrefab, float spawnInterval, float spawnRadius, int maxConcurrent = 0, Transform center = null)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("StartSpawningEnemies: enemyPrefab is null");
            return;
        }
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine(SpawnRoutine(enemyPrefab, spawnInterval, spawnRadius, maxConcurrent, center));
    }

    public void StopSpawningEnemies()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    private IEnumerator SpawnRoutine(GameObject prefab, float interval, float radius, int maxConcurrent, Transform center)
    {
        Transform spawnAt = center != null ? center : spawnCenter;
        while (true)
        {
            if (maxConcurrent <= 0 || enemies.Count < maxConcurrent)
            {
                Vector3 pos = RandomPointAround(spawnAt.position, radius);
                SpawnEnemy(prefab, pos, Quaternion.identity);
            }
            yield return new WaitForSeconds(interval);
        }
    }

    public Enemy SpawnEnemy(GameObject enemyPrefab, Vector3 position, Quaternion rotation)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("SpawnEnemy: enemyPrefab is null");
            return null;
        }

        GameObject go = Instantiate(enemyPrefab, position, rotation);
        Enemy e = go.GetComponentInChildren<Enemy>();
        if (e != null)
        {
            RegisterEnemy(e);
            OnEnemySpawned?.Invoke(e);
        }
        else
        {
            Debug.LogWarning("Spawned enemy prefab does not contain an Enemy component.");
        }
        return e;
    }

    private Vector3 RandomPointAround(Vector3 center, float radius)
    {
        float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
        float r = Mathf.Sqrt(UnityEngine.Random.Range(0f, 1f)) * radius; 
        float x = Mathf.Cos(angle) * r;
        float z = Mathf.Sin(angle) * r;
        Vector3 point = center + new Vector3(x, 0f, z);

        return point;
    }
    public int Money => currentMoney;

    public bool CanAfford(int cost)
    {
        return currentMoney >= cost;
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UIManager.Instance?.UpdateCurrencyUI(currentMoney);
    }

    public bool SpendMoney(int amount)
    {
        if (!CanAfford(amount))
            return false;

        currentMoney -= amount;
        UIManager.Instance?.UpdateCurrencyUI(currentMoney);
        return true;
    }

}
