using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(HeroStats))]
public class HeroCombat : MonoBehaviour
{
    private HeroStats stats;
    private float attackCooldown;

    private IGameState lastKnownState;

    private Dictionary<HeroAbility, float> abilityCooldowns = new Dictionary<HeroAbility, float>();

    [SerializeField] private Transform attackPoint;
    [SerializeField] private List<HeroAbility> abilities = new();

    private void Awake()
    {
        stats = GetComponent<HeroStats>();
    }

    private void Start()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetupHeroUI(this);
            UIManager.Instance.UpdateHeroBars(stats);
        }

        if (GameManager.Instance != null)
        {
            lastKnownState = GameManager.Instance.CurrentState;
        }
    }

    private void Update()
    {
        if (attackCooldown > 0)
            attackCooldown -= Time.deltaTime;

        if (GameManager.Instance != null)
        {
            var currentState = GameManager.Instance.CurrentState;
            if (currentState != lastKnownState)
            {
                if (currentState is CombatState)
                {
                    ResetCooldowns();
                }
                lastKnownState = currentState;
            }
        }
    }

    public void ResetCooldowns()
    {
        abilityCooldowns.Clear();


        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetupHeroUI(this);
        }

        Debug.Log("Abilities Refreshed for New Wave!");
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
        if (index < 0 || index >= abilities.Count) return;

        var ability = abilities[index];
        if (ability == null) return;

        if (abilityCooldowns.ContainsKey(ability))
        {
            if (Time.time < abilityCooldowns[ability])
            {
                return;
            }
        }

        float costMultiplier = 1f;
        if (GameManager.Instance != null)
        {
            costMultiplier = GameManager.Instance.GetManaMultiplier();
        }

        ability.TryUse(this, stats, costMultiplier);
    }

    public HeroAbility GetAbility(int index)
    {
        if (index < 0 || index >= abilities.Count) return null;
        return abilities[index];
    }

    public void StartAbilityCooldownUI(HeroAbility ability, float duration)
    {
        abilityCooldowns[ability] = Time.time + duration;

        if (UIManager.Instance == null) return;

        var slots = UIManager.Instance.AbilitySlots;
        if (slots == null) return;

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