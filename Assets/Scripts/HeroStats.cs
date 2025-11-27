using UnityEngine;

public class HeroStats : MonoBehaviour
{
    public float maxHealth = 200f;
    public float maxMana = 100f;
    public float moveSpeed = 5f;
    public float attackDamage = 25f;
    public float attackRange = 4f;
    public float attackRate = 1f;

    public float healthRegen = 2f;
    public float manaRegen = 3f;

    [HideInInspector] public float currentHealth;
    [HideInInspector] public float currentMana;

    private void Start()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;
        InvokeRepeating(nameof(Regenerate), 1f, 1f);
        UIManager.Instance?.UpdateHeroBars(this);
    }

    private void Regenerate()
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + healthRegen);
        currentMana = Mathf.Min(maxMana, currentMana + manaRegen);
        UIManager.Instance?.UpdateHeroBars(this);
    }

    public bool SpendMana(float cost)
    {
        if (currentMana < cost) return false;
        currentMana -= cost;
        UIManager.Instance?.UpdateHeroBars(this);
        return true;
    }

    public void TakeDamage(float dmg)
    {
        currentHealth -= dmg;
        UIManager.Instance?.UpdateHeroBars(this);
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        Debug.Log("Hero died!");
    }
}
