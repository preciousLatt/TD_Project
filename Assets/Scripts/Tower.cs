using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Tower : MonoBehaviour
{
    [Header("")]
    [SerializeField] public float attackRange = 5f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private Projectile projectilePrefab;

    public List<UpgradePath> upgradePaths = new List<UpgradePath>();

    private int[] currentUpgradeIndexes; 
    private Transform shootPoint;
    private float fireTimer;

    public float AttackRange => attackRange;

    private void Awake()
    {
        shootPoint = transform.Find("ShootFrom") ?? transform;
        currentUpgradeIndexes = new int[upgradePaths.Count];
        Debug.Log($"{name}: Awake — upgradePaths count = {upgradePaths.Count}");
    }

    private void Update()
    {
        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            var target = GameManager.Instance?.GetClosestEnemy(transform.position);
            if (target != null && Vector3.Distance(transform.position, target.transform.position) <= attackRange)
            {
                Projectile.Spawn(projectilePrefab, shootPoint.position, target);

                fireTimer = 1f / fireRate;
            }
        }
    }


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
        if (!GameManager.Instance.SpendMoney((int)next.cost))
        {
            Debug.Log("Not enough money!");
            return;
        }
        float newRange = attackRange + next.rangeBonus;
        float newFireRate = fireRate + next.fireRateBonus;
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
            }

            UIManager.Instance.SetActiveTower(newTower);
            newGO.GetComponent<TowerSelector>()?.SelectTower();

            Destroy(gameObject);
            return;
        }

        attackRange = newRange;
        fireRate = newFireRate;
        currentUpgradeIndexes = newIndexes;

        UIManager.Instance.SetActiveTower(this);
    }

    public void SetUpgradeIndexes(int[] indexes)
    {
        currentUpgradeIndexes = indexes;
    }

    public void SetFireRate(float newRate)
    {
        fireRate = newRate;
    }
 
}
