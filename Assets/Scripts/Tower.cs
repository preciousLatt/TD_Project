using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    public enum TargetingPriority
    {
        Closest,
        NotSlowed
    }

    [Header("Stats")]
    [SerializeField] public float attackRange = 5f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float damage = 10f;

    [Header("Setup")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private TargetingPriority targetingPriority = TargetingPriority.Closest;

    public List<UpgradePath> upgradePaths = new List<UpgradePath>();

    private int[] currentUpgradeIndexes;
    private Transform shootPoint;
    private float fireTimer;

    public float AttackRange => attackRange;

    private void Awake()
    {
        shootPoint = transform.Find("ShootFrom") ?? transform;
        currentUpgradeIndexes = new int[upgradePaths.Count];
    }

    private void Update()
    {
        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            Enemy target = GetTargetBasedOnPriority();

            if (target != null)
            {
                Projectile.Spawn(projectilePrefab, shootPoint.position, target, damage);
                fireTimer = 1f / fireRate;
            }
        }
    }

    private Enemy GetTargetBasedOnPriority()
    {
        if (GameManager.Instance == null) return null;

        // Access the list of enemies directly from GameManager
        // Assuming GameManager exposes: public IReadOnlyList<Enemy> Enemies => enemies;
        var enemies = GameManager.Instance.Enemies;
        if (enemies == null || enemies.Count == 0) return null;

        Enemy bestEnemy = null;
        float closestDistSqr = attackRange * attackRange;

        // --- STRATEGY: NOT SLOWED ---
        if (targetingPriority == TargetingPriority.NotSlowed)
        {
            // First pass: Look for Closest NON-SLOWED enemy in range
            foreach (var e in enemies)
            {
                if (e == null || e.IsDead) continue;

                // Check Range
                float distSqr = (e.transform.position - transform.position).sqrMagnitude;
                if (distSqr > closestDistSqr) continue;

                // Priority Check: Is he already slowed?
                if (!e.IsSlowed)
                {
                    closestDistSqr = distSqr;
                    bestEnemy = e;
                }
            }

            // If we found a non-slowed enemy, return it
            if (bestEnemy != null) return bestEnemy;

            // FALLBACK: If everyone is slowed (or no one unslowed is in range), 
            // just shoot the closest one (fall through to Closest logic below)
            // reset distance check
            closestDistSqr = attackRange * attackRange;
        }

        // --- STRATEGY: CLOSEST (Default / Fallback) ---
        foreach (var e in enemies)
        {
            if (e == null || e.IsDead) continue;

            float distSqr = (e.transform.position - transform.position).sqrMagnitude;
            if (distSqr <= closestDistSqr)
            {
                closestDistSqr = distSqr;
                bestEnemy = e;
            }
        }

        return bestEnemy;
    }

    // --- UPGRADE SYSTEM (Unchanged) ---
    public UpgradeStep GetNextUpgrade(int pathIndex)
    {
        if (pathIndex < 0 || pathIndex >= upgradePaths.Count) return null;
        var path = upgradePaths[pathIndex];
        int stepIndex = currentUpgradeIndexes[pathIndex];
        if (stepIndex >= path.steps.Count) return null;
        return path.steps[stepIndex];
    }

    public void ApplyUpgrade(int pathIndex)
    {
        UpgradeStep next = GetNextUpgrade(pathIndex);
        if (next == null) return;
        if (!GameManager.Instance.SpendMoney((int)next.cost)) return;

        float newRange = attackRange + next.rangeBonus;
        float newFireRate = fireRate + next.fireRateBonus;
        float newDamage = damage + next.damageBonus;
        int[] newIndexes = (int[])currentUpgradeIndexes.Clone();

        newIndexes[pathIndex]++;

        if (next.upgradedTowerPrefab != null)
        {
            GameObject newGO = Instantiate(next.upgradedTowerPrefab, transform.position, transform.rotation, transform.parent);
            Tower newTower = newGO.GetComponent<Tower>();
            if (newTower != null)
            {
                newTower.attackRange = newRange;
                newTower.SetFireRate(newFireRate);
                newTower.SetUpgradeIndexes(newIndexes);
                newTower.damage = newDamage;
                // Preserve targeting priority if needed, or let prefab dictate it
                newTower.targetingPriority = this.targetingPriority;
            }

            UIManager.Instance.SetActiveTower(newTower);
            newGO.GetComponent<TowerSelector>()?.SelectTower();

            Destroy(gameObject);
            return;
        }

        attackRange = newRange;
        fireRate = newFireRate;
        damage = newDamage;
        currentUpgradeIndexes = newIndexes;

        UIManager.Instance.SetActiveTower(this);
    }

    public void SetUpgradeIndexes(int[] indexes) { currentUpgradeIndexes = indexes; }
    public void SetFireRate(float newRate) { fireRate = newRate; }
}