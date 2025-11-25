using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Defaults (used only if not initialized by a spawner)")]
    [SerializeField] private float defaultSpeed = 2f;
    [SerializeField] private float defaultHealth = 100f;
    [SerializeField] private float waypointArrivalThreshold = 1.0f;

    private float baseSpeed; // The normal speed of the enemy
    private float currentSpeed; // The actual speed (affected by slows)
    private float health;

    private Transform currentTarget;
    private Transform finalNexusTarget;
    private int currentPathLevel = -1;

    private bool notifiedDeath;
    public bool IsDead => health <= 0;

    // --- NEW: Public property to check slow state ---
    public bool IsSlowed => slowCoroutine != null;
    // ------------------------------------------------

    // Call this from your spawner/factory
    public void Initialize(float moveSpeed, float hp, Transform nexus)
    {
        baseSpeed = moveSpeed;
        currentSpeed = baseSpeed;
        health = hp;
        finalNexusTarget = nexus;
        currentPathLevel = -1;

        FindNextTarget();
    }

    private void OnEnable()
    {
        if (health <= 0f) health = defaultHealth;
        if (baseSpeed <= 0f) baseSpeed = defaultSpeed;

        // Ensure current speed is set if not initialized via spawner
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

    // --- Slow System ---
    private Coroutine slowCoroutine;

    public void ApplySlow(float slowPct, float duration)
    {
        // slowPct of 0.3f means 30% slower
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
    // ------------------------

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
            // Use currentSpeed instead of speed
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
        Destroy(gameObject);
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
    }
}