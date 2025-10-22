using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Defaults (used only if not initialized by a spawner)")]
    [SerializeField] private float defaultSpeed = 2f;
    [SerializeField] private float defaultHealth = 100f;

    private float speed;
    private float health;
    private Transform target;

    private bool notifiedDeath; // prevent double unregister
    public bool IsDead => health <= 0;

    // Call this from your spawner/factory
    public void Initialize(float moveSpeed, float hp, Transform nexus)
    {
        speed = moveSpeed;
        health = hp;
        target = nexus;
    }

    private void OnEnable()
    {
        // Fallback init in case someone instantiates this without calling Initialize(...)
        if (health <= 0f) health = defaultHealth;
        if (speed <= 0f) speed = defaultSpeed;
        if (target == null)
        {
            var nexusObj = GameObject.FindGameObjectWithTag("Nexus");
            if (nexusObj != null) target = nexusObj.transform;
        }

        // Self-register with GameManager so towers/projectiles can find and damage this enemy
        GameManager.Instance?.RegisterEnemy(this);
    }

    private void Update()
    {
        if (!target || IsDead) return;

        Vector3 direction = target.position - transform.position;
        if (direction.sqrMagnitude > 0.001f)
        {
            direction.Normalize();
            transform.position += direction * speed * Time.deltaTime;
            transform.forward = direction;
        }
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;
        health -= amount;
        if (health <= 0f) Die();

/*
        if (amount <= 0f) return;

        CurrentHealth -= amount;
        CurrentHealth = Mathf.Max(0f, CurrentHealth);

        Debug.Log($"{name} took {amount} damage. HP = {CurrentHealth}/{maxHealth}");
        if (GameManager.Instance != null)
            GameManager.Instance.ShowDamageText(transform.position, amount, Color.red);
        if (CurrentHealth <= 0f)
        {
            Die();
        }
       */
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
        // Safety: if destroyed by anything that didn’t call Die(), still notify once.
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
            GameManager.Instance?.DamageNexus(10f); // tweak as needed or make this serialized
            Die();
        }
    }
}
