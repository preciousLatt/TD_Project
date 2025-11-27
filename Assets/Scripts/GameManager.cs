using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Singleton;

public class GameManager : Singleton<GameManager>
{
    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnCenter;
    [SerializeField] private int maxConcurrentEnemies = 50;
    [SerializeField] private bool startSpawningOnAwake = false;

    [Header("Game Stats")]
    [SerializeField] private float startingNexusHealth = 100f;
    [SerializeField] public int startingMoney = 500;

    [Header("Visuals & UI")]
    [SerializeField] private DamageText damageTextPrefab;
    [SerializeField] private float textSpawnOffsetY = 1.5f;
    [SerializeField] private TextMeshPro nexusHealthText;

    private float nexusHealth;
    public int currentMoney;

    // Track what is currently shown on UI for animation purposes
    private int displayedMoney;
    private Coroutine goldAnimationCoroutine;

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
        displayedMoney = startingMoney;

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
        ChangeState(new BuildState());

        // Initial UI Updates
        UIManager.Instance?.UpdateCurrencyUI(currentMoney);
        UpdateNexusHealthUI();
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

    public float GetManaMultiplier()
    {
        if (currentState == null) return 1f;
        return currentState.GetManaCostMultiplier();
    }

    // --- Centralized Win/Loss Logic ---

    public void TriggerVictory()
    {
        if (currentState is not GameOverState)
        {
            ChangeState(new VictoryState());
            UIManager.Instance?.ShowVictoryPanel();
        }
    }

    public void TriggerGameOver()
    {
        if (currentState is not GameOverState)
        {
            ChangeState(new GameOverState());
            UIManager.Instance?.ShowGameOverPanel();
        }
    }

    public void DamageHero(HeroStats hero, float damage)
    {
        if (hero == null) return;

        hero.currentHealth -= damage;
        Debug.Log($"Player hit! Health: {hero.currentHealth}");

        UIManager.Instance?.UpdateHeroBars(hero);

        if (hero.currentHealth <= 0)
        {
            TriggerGameOver();
        }
    }

    // ----------------------------------

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

        UpdateNexusHealthUI();
        OnNexusDamaged?.Invoke(amount);
        OnNexusHealthChanged?.Invoke(nexusHealth);

        if (nexusHealth <= 0f)
        {
            Debug.Log("Nexus destroyed");
            TriggerGameOver();
        }
    }

    private void UpdateNexusHealthUI()
    {
        if (nexusHealthText != null)
        {
            nexusHealthText.text = $"{Mathf.CeilToInt(nexusHealth)}/{startingNexusHealth}";
        }
    }

    public float GetNexusHealth() => nexusHealth;

    // CHANGED: Added 'giveReward' boolean parameter (default true)
    public void NotifyEnemyDied(Enemy e, bool giveReward = true)
    {
        if (giveReward)
        {
            int goldReward = 100;
            AddMoney(goldReward);

            // Call UIManager to show the gold popup on Canvas
            if (e != null)
            {
                // This calls the method we created in UIManager.cs
                UIManager.Instance?.ShowGoldPopup(e.transform.position, goldReward);
            }
        }

        if (e == null) return;
        UnregisterEnemy(e);
        OnEnemyDied?.Invoke(e);
    }

    public bool CanAfford(int cost) => currentMoney >= cost;

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        RefreshMoneyUI();
    }

    public bool SpendMoney(int amount)
    {
        if (!CanAfford(amount)) return false;
        currentMoney -= amount;
        RefreshMoneyUI();
        return true;
    }

    private void RefreshMoneyUI()
    {
        if (goldAnimationCoroutine != null) StopCoroutine(goldAnimationCoroutine);
        goldAnimationCoroutine = StartCoroutine(AnimateMoney());
    }

    private IEnumerator AnimateMoney()
    {
        float duration = 0.5f;
        float elapsed = 0f;
        int startValue = displayedMoney;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            t = Mathf.Sin(t * Mathf.PI * 0.5f);

            displayedMoney = Mathf.RoundToInt(Mathf.Lerp(startValue, currentMoney, t));
            UIManager.Instance?.UpdateCurrencyUI(displayedMoney);
            yield return null;
        }

        displayedMoney = currentMoney;
        UIManager.Instance?.UpdateCurrencyUI(displayedMoney);
    }

    public void ShowDamageText(Vector3 worldPos, float amount, Color color)
    {
        if (damageTextPrefab == null) return;
        Vector3 spawnPos = worldPos + Vector3.up * textSpawnOffsetY;
        DamageText dt = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);
        dt.Initialize(amount, color);
    }
}