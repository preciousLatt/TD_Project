using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(HeroStats))]
public class HeroCombat : MonoBehaviour
{
    private HeroStats stats;
    private float attackCooldown;

    [SerializeField] private Transform attackPoint;
    [SerializeField] private List<HeroAbility> abilities = new();

    private void Awake()
    {
        stats = GetComponent<HeroStats>();
    }
    private void Start()
    {
        UIManager.Instance.SetupHeroUI(this);
        UIManager.Instance.UpdateHeroBars(stats);

    }


    private void Update()
    {
        if (attackCooldown > 0)
            attackCooldown -= Time.deltaTime;
    }

    public void Attack(Transform target)
    {
        if (target == null || attackCooldown > 0) return;

        if (Vector3.Distance(transform.position, target.position) <= stats.attackRange)
        {
            var enemy = target.GetComponent<Enemy>();
            if (enemy)
            {
                enemy.TakeDamage(stats.attackDamage);
                attackCooldown = 1f / stats.attackRate;
            }
        }
    }

    public void UseAbility(int index)
    {
        if (index < 0 || index >= abilities.Count)
        {
            return;
        }

        var ability = abilities[index];
        if (ability == null)
        {
            return;
        }

        ability.TryUse(this, stats);
    }
    public HeroAbility GetAbility(int index)
    {
        if (index < 0 || index >= abilities.Count) return null;
        return abilities[index];
    }
    public void StartAbilityCooldownUI(HeroAbility ability, float duration)
    {
        var slots = UIManager.Instance.AbilitySlots;
        for (int i = 0; i < abilities.Count; i++)
        {
            if (abilities[i] == ability && i < slots.Length)
            {
                slots[i].StartCooldown(duration);
                break;
            }
        }
    }
    public int GetAbilityIndex(HeroAbility ability)
    {
        return abilities.IndexOf(ability);
    }

}