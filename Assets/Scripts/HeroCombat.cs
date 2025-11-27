using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(HeroStats))]
public class HeroCombat : MonoBehaviour
{
    private HeroStats stats;
    private float attackCooldown;

    // NEW: Track the game state to detect wave starts
    private IGameState lastKnownState;

    // Dictionary to track when each ability will be ready again
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

        // Initialize state tracker
        if (GameManager.Instance != null)
        {
            lastKnownState = GameManager.Instance.CurrentState;
        }
    }

    private void Update()
    {
        if (attackCooldown > 0)
            attackCooldown -= Time.deltaTime;

        // --- DETECT WAVE START (State Change) ---
        if (GameManager.Instance != null)
        {
            var currentState = GameManager.Instance.CurrentState;
            if (currentState != lastKnownState)
            {
                // If we switched TO CombatState, it means a wave just started
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
        // 1. Clear logical cooldowns so abilities can be used immediately
        abilityCooldowns.Clear();

        // 2. Reset UI Visuals cleanly
        // Do NOT call StartCooldown(0.05f). 
        // Instead, tell UIManager to re-bind the slots. 
        // This calls Bind() on the slots, which triggers ResetCooldownUI(), stopping the coroutine instantly.
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

        // --- 1. COOLDOWN CHECK ---
        if (abilityCooldowns.ContainsKey(ability))
        {
            if (Time.time < abilityCooldowns[ability])
            {
                // Ability is still on cooldown
                return;
            }
        }

        // --- 2. GET MANA MULTIPLIER ---
        float costMultiplier = 1f;
        if (GameManager.Instance != null)
        {
            costMultiplier = GameManager.Instance.GetManaMultiplier();
        }

        // --- 3. ATTEMPT USE ---
        // TryUse will call StartAbilityCooldownUI if successful
        ability.TryUse(this, stats, costMultiplier);
    }

    public HeroAbility GetAbility(int index)
    {
        if (index < 0 || index >= abilities.Count) return null;
        return abilities[index];
    }

    // Called by HeroAbility when usage is successful
    public void StartAbilityCooldownUI(HeroAbility ability, float duration)
    {
        // 1. LOGIC: Set the time when this ability is allowed to be used next
        abilityCooldowns[ability] = Time.time + duration;

        // 2. VISUALS: Update the UI
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