using UnityEngine;
using UnityEngine.Pool;

public class Projectile : MonoBehaviour
{
    [SerializeField] protected float speed = 12f;
    [SerializeField] protected float stopDistance = 0.1f;
    [SerializeField] protected float lifeTime = 8f;
    [SerializeField] protected Vector3 rotationOffset = new Vector3(90f, 0f, 0f);
    [SerializeField] protected float rotationSpeed = 720f;
    [SerializeField] protected float damage = 25f;
    [SerializeField] protected GameObject impactVFX;
    private IObjectPool<Projectile> pool;
    public void SetPool(IObjectPool<Projectile> pool) => this.pool = pool;
    public float Damage => damage;

    protected Enemy targetEnemy = null;
    protected Vector3 targetPosition;
    protected bool useEnemyTarget = false;

    protected bool isLaunched = false;
    protected float lifeTimer = 0f;

    public static Projectile Spawn(Projectile prefab, Vector3 spawnPos, Enemy enemyTarget, float damage = 0f)
    {
        if (prefab == null) return null;
        GameObject go = Instantiate(prefab.gameObject, spawnPos, Quaternion.identity);
        Projectile p = go.GetComponent<Projectile>();
        if (p == null) { Destroy(go); return null; }

        p.damage = damage;
        p.InitializeTarget(enemyTarget);
        return p;
    }

    public static Projectile Spawn(Projectile prefab, Vector3 spawnPos, Vector3 targetPos, float damage = 2f)
    {
        if (prefab == null) return null;
        GameObject go = Instantiate(prefab.gameObject, spawnPos, Quaternion.identity);
        Projectile p = go.GetComponent<Projectile>();
        if (p == null) { Destroy(go); return null; }

        p.damage = damage;
        p.InitializeTarget(targetPos);
        return p;
    }

    public void InitializeTarget(Enemy enemy)
    {
        if (enemy == null) return;
        targetEnemy = enemy;
        useEnemyTarget = true;
        isLaunched = true;
        lifeTimer = lifeTime;
        RotateTowardsTarget();
    }

    public void InitializeTarget(Vector3 targetPos)
    {
        targetPosition = targetPos;
        useEnemyTarget = false;
        isLaunched = true;
        lifeTimer = lifeTime;
        RotateTowardsTarget();
    }

    protected virtual void Update()
    {
        if (!isLaunched) return;

        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f) { ReleaseToPool(); return; }

        Vector3 currentTargetPos;
        if (useEnemyTarget)
        {
            if (targetEnemy == null || targetEnemy.IsDead)
            {
                ReleaseToPool();
                return;
            }
            currentTargetPos = targetEnemy.transform.position;
        }
        else
        {
            currentTargetPos = targetPosition;
        }

        Vector3 currentPos = transform.position;
        float distance = Vector3.Distance(currentPos, currentTargetPos);

        if (distance <= stopDistance)
        {
            HitTarget(); 
            return;
        }

        Vector3 newPos = Vector3.MoveTowards(currentPos, currentTargetPos, speed * Time.deltaTime);
        transform.position = newPos;

        Vector3 movement = newPos - currentPos;
        if (movement.sqrMagnitude > 0.000001f)
        {
            Quaternion desired = Quaternion.LookRotation(movement.normalized) * Quaternion.Euler(rotationOffset);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, desired, rotationSpeed * Time.deltaTime);
        }
    }

    private void RotateTowardsTarget()
    {
        Vector3 tPos = useEnemyTarget && targetEnemy != null ? targetEnemy.transform.position : targetPosition;
        Vector3 dir = tPos - transform.position;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir.normalized) * Quaternion.Euler(rotationOffset);
    }

    protected virtual void HitTarget()
    {
        if (useEnemyTarget && targetEnemy != null)
        {
            GameManager.Instance?.DamageEnemy(targetEnemy, damage);
        }

        if (impactVFX != null)
            Instantiate(impactVFX, transform.position, Quaternion.identity);

        ReleaseToPool();
    }
    private void ReleaseToPool()
    {
        if (pool != null) pool.Release(this);
        else Destroy(gameObject);
    }
}