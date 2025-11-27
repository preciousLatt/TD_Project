using System;
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

    [SerializeField] private DamageText damageTextPrefab;
    [SerializeField] private float textSpawnOffsetY = 1.5f;

    private float nexusHealth;
    public int currentMoney;
    private readonly List<Enemy> enemies = new List<Enemy>();
    public IReadOnlyList<Enemy> Enemies => enemies;

    // --- OBSERVER PATTERN ---
    public event Action<Enemy> OnEnemySpawned;
    public event Action<Enemy> OnEnemyDied;
    public event Action<float> OnNexusDamaged;
    public event Action<float> OnNexusHealthChanged;

    // --- STATE PATTERN ---
    private IGameState currentState;
    public IGameState CurrentState => currentState;

    public bool IsPaused { get; private set; }

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
        // START IN BUILD STATE (Free Mana)
        ChangeState(new BuildState());

        UIManager.Instance?.UpdateCurrencyUI(currentMoney);
    }

    private void Update()
    {
        currentState?.Update(this);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void ChangeState(IGameState newState)
    {
        if (currentState != null)
            currentState.Exit(this);

        currentState = newState;
        currentState.Enter(this);
    }

    // --- NEW HELPER FOR MANA COST ---
    public float GetManaMultiplier()
    {
        if (currentState == null) return 1f;
        return currentState.GetManaCostMultiplier();
    }
    // --------------------------------

    public void TriggerVictory()
    {
        if (currentState is not GameOverState)
        {
            ChangeState(new VictoryState());
        }
    }

    public void PauseGame()
    {
        if (IsPaused) return;
        Time.timeScale = 0f;
        IsPaused = true;
        UIManager.Instance.ShowPauseMenu(true);
    }

    public void ResumeGame()
    {
        if (!IsPaused) return;
        Time.timeScale = 1f;
        IsPaused = false;
        UIManager.Instance.ShowPauseMenu(false);
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

        if (nexusHealth <= 0f)
        {
            Debug.Log("Nexus destroyed");
        }
    }

    public float GetNexusHealth() => nexusHealth;

    public void NotifyEnemyDied(Enemy e)
    {
        AddMoney(100);
        if (e == null) return;
        UnregisterEnemy(e);
        OnEnemyDied?.Invoke(e);
    }

    public bool CanAfford(int cost) => currentMoney >= cost;

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UIManager.Instance?.UpdateCurrencyUI(currentMoney);
    }

    public bool SpendMoney(int amount)
    {
        if (!CanAfford(amount)) return false;
        currentMoney -= amount;
        UIManager.Instance?.UpdateCurrencyUI(currentMoney);
        return true;
    }

    public void ShowDamageText(Vector3 worldPos, float amount, Color color)
    {
        if (damageTextPrefab == null) return;
        Vector3 spawnPos = worldPos + Vector3.up * textSpawnOffsetY;
        DamageText dt = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);
        dt.Initialize(amount, color);
    }
}