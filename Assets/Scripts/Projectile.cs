using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float stopDistance = 0.1f;
    [SerializeField] private float lifeTime = 8f; 
    [SerializeField] private Vector3 rotationOffset = new Vector3(90f, 0f, 0f);
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] protected float damage = 25f;
    public float Damage => damage;


    private Enemy targetEnemy = null;
    private Vector3 targetPosition;
    private bool useEnemyTarget = false;

    private bool isLaunched = false;
    private float lifeTimer = 0f;


    public static Projectile Spawn(Projectile prefab, Vector3 spawnPos, Enemy enemyTarget, float damage = 0f)
    {
        if (prefab == null)
        {
            return null;
        }

        GameObject go = Instantiate(prefab.gameObject, spawnPos, Quaternion.identity);
        Projectile p = go.GetComponent<Projectile>();
        if (p == null)
        {
            Destroy(go);
            return null;
        }

        p.damage = damage;
        p.InitializeTarget(enemyTarget);
        return p;
    }

    public static Projectile Spawn(Projectile prefab, Vector3 spawnPos, Vector3 targetPos, float damage = 2f)
    {
        if (prefab == null)
        {
            return null;
        }

        GameObject go = Instantiate(prefab.gameObject, spawnPos, Quaternion.identity);
        Projectile p = go.GetComponent<Projectile>();
        if (p == null)
        {
            Destroy(go);
            return null;
        }

        p.damage = damage;
        p.InitializeTarget(targetPos);
        return p;
    }

 
    public void InitializeTarget(Enemy enemy)
    {
        if (enemy == null)
        {
            return;
        }

        targetEnemy = enemy;
        useEnemyTarget = true;
        isLaunched = true;
        lifeTimer = lifeTime;

        Vector3 dir = (targetEnemy.transform.position - transform.position);
        if (dir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(dir.normalized) * Quaternion.Euler(rotationOffset);
    }

    public void InitializeTarget(Vector3 targetPos)
    {
        targetPosition = targetPos;
        useEnemyTarget = false;
        isLaunched = true;
        lifeTimer = lifeTime;

        Vector3 dir = (targetPosition - transform.position);
        if (dir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(dir.normalized) * Quaternion.Euler(rotationOffset);
    }

    private void Update()
    {
        if (!isLaunched) return;

        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 currentTargetPos;
        if (useEnemyTarget)
        {
            if (targetEnemy == null || targetEnemy.IsDead)
            {
                OnReachedTarget();
                return;
            }
            currentTargetPos = targetEnemy.transform.position;
        }
        else
        {
            currentTargetPos = targetPosition;
        }

        Vector3 currentPos = transform.position;
        Vector3 toTarget = currentTargetPos - currentPos;
        float distance = toTarget.magnitude;

        if (distance <= stopDistance)
        {
            if (useEnemyTarget)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.DamageEnemy(targetEnemy, damage);
                }
                else
                {
                    targetEnemy.TakeDamage(damage);
                }
            }
            else
            {
            }

            Destroy(gameObject);
            return;
        }

        Vector3 newPos = Vector3.MoveTowards(currentPos, currentTargetPos, speed * Time.deltaTime);
        Vector3 movement = newPos - currentPos;
        transform.position = newPos;

        if (movement.sqrMagnitude > 0.000001f)
        {
            Quaternion desired = Quaternion.LookRotation(movement.normalized) * Quaternion.Euler(rotationOffset);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, desired, rotationSpeed * Time.deltaTime);
        }
    }

    protected virtual void OnReachedTarget()
    {
        if (useEnemyTarget && targetEnemy != null)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.DamageEnemy(targetEnemy, damage);
        }

        Destroy(gameObject);
    }

}
