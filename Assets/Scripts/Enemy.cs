using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Defaults (used only if not initialized by a spawner)")]
    [SerializeField] private float defaultSpeed = 2f;
    [SerializeField] private float defaultHealth = 100f;
    [SerializeField] private float waypointArrivalThreshold = 1.0f; // How close to get before switching

    private float speed;
    private float health;

    private Transform currentTarget;
    private Transform finalNexusTarget; 
    private int currentPathLevel = -1;

    private bool notifiedDeath;
    public bool IsDead => health <= 0;

    public void Initialize(float moveSpeed, float hp, Transform nexus)
    {
        speed = moveSpeed;
        health = hp;
        finalNexusTarget = nexus;
        currentPathLevel = -1;

        FindNextTarget();
    }

    private void OnEnable()
    {
        if (health <= 0f) health = defaultHealth;
        if (speed <= 0f) speed = defaultSpeed;

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
            transform.position += moveDir * speed * Time.deltaTime;
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