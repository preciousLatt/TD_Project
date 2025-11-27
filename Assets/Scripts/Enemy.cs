using UnityEngine;
using System.Collections;
using UnityEngine.Pool;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float defaultSpeed = 2f;
    [SerializeField] private float defaultHealth = 100f;
    [SerializeField] private float waypointArrivalThreshold = 1.0f;
    [SerializeField] private float contactDamage = 15f; 

    private float baseSpeed;
    private float currentSpeed;
    private float health;

    private Transform currentTarget;
    private Transform finalNexusTarget;
    private int currentPathLevel = -1;

    private bool notifiedDeath;
    public bool IsDead => health <= 0;

    private IObjectPool<Enemy> pool;
    public void SetPool(IObjectPool<Enemy> pool) => this.pool = pool;

    public bool IsSlowed => slowCoroutine != null;
    private Coroutine slowCoroutine;

    public void Initialize(float moveSpeed, float hp, Transform nexus)
    {
        baseSpeed = moveSpeed;
        currentSpeed = baseSpeed;
        health = hp;
        finalNexusTarget = nexus;
        currentPathLevel = -1;
        notifiedDeath = false;
        FindNextTarget();
    }

    private void OnEnable()
    {
        if (health <= 0f) health = defaultHealth;
        if (baseSpeed <= 0f) baseSpeed = defaultSpeed;
        if (currentSpeed <= 0f) currentSpeed = baseSpeed;

        if (finalNexusTarget == null)
        {
            var nexusObj = GameObject.FindGameObjectWithTag("Nexus");
            if (nexusObj != null) finalNexusTarget = nexusObj.transform;
        }

        if (currentTarget == null)
        {
            currentPathLevel = -1;
            FindNextTarget();
        }

        GameManager.Instance?.RegisterEnemy(this);
    }

    private void OnDisable()
    {
        if (slowCoroutine != null)
        {
            StopCoroutine(slowCoroutine);
            slowCoroutine = null;
        }
        currentSpeed = baseSpeed;
    }

    public void ApplySlow(float slowPct, float duration)
    {
        if (!gameObject.activeInHierarchy || IsDead) return;
        if (slowCoroutine != null) StopCoroutine(slowCoroutine);
        slowCoroutine = StartCoroutine(SlowRoutine(slowPct, duration));
    }

    private IEnumerator SlowRoutine(float slowPct, float duration)
    {
        currentSpeed = baseSpeed * (1f - slowPct);
        yield return new WaitForSeconds(duration);
        currentSpeed = baseSpeed;
        slowCoroutine = null;
    }

    private void FindNextTarget()
    {
        int nextLevel = currentPathLevel + 1;
        Transform nextWaypoint = null;
        if (WaypointManager.Instance != null)
        {
            nextWaypoint = WaypointManager.Instance.GetClosestWaypoint(transform.position, nextLevel);
        }

        if (nextWaypoint != null)
        {
            currentTarget = nextWaypoint;
            currentPathLevel = nextLevel;
        }
        else
        {
            currentTarget = finalNexusTarget;
        }
    }

    private void Update()
    {
        if (!currentTarget || IsDead) return;

        Vector3 direction = currentTarget.position - transform.position;
        float distSq = direction.sqrMagnitude;

        if (distSq > 0.001f)
        {
            Vector3 moveDir = direction.normalized;
            transform.position += moveDir * currentSpeed * Time.deltaTime;
            transform.forward = moveDir;
        }

        if (currentTarget != finalNexusTarget)
        {
            if (distSq < waypointArrivalThreshold * waypointArrivalThreshold)
            {
                FindNextTarget();
            }
        }
    }

    public void TakeDamage(float amount)
    {
        GameManager.Instance.ShowDamageText(transform.position, amount, Color.red);
        if (IsDead) return;
        health -= amount;
        if (health <= 0f) Die();
    }

    private void Die()
    {
        if (!notifiedDeath)
        {
            notifiedDeath = true;
            GameManager.Instance?.NotifyEnemyDied(this);
        }

        if (pool != null) pool.Release(this);
        else Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (!notifiedDeath)
        {
            notifiedDeath = true;
            GameManager.Instance?.NotifyEnemyDied(this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Nexus"))
        {
            GameManager.Instance?.DamageNexus(10f);
            Die();
        }
        else if (other.CompareTag("Player"))
        {
            HeroStats playerStats = other.GetComponent<HeroStats>();
            Debug.Log("Player should have been hit");
            if (playerStats != null)
            {
                GameManager.Instance?.DamageHero(playerStats, contactDamage);
            }
        }
    }
}