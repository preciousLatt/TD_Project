using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float maxHealth = 50f;

    public float CurrentHealth { get; private set; }
    public bool IsDead { get; private set; } = false;

    private void Awake()
    {
        CurrentHealth = maxHealth;
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterEnemy(this);
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null && !IsDead)
            GameManager.Instance.RegisterEnemy(this);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.UnregisterEnemy(this);
    }


    public void TakeDamage(float amount)
    {
        if (IsDead) return;
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
    }

    private void Die()
    {
        if (IsDead) return;
        IsDead = true;

        if (GameManager.Instance != null)
            GameManager.Instance.NotifyEnemyDied(this);


        Destroy(gameObject);
    }
}
