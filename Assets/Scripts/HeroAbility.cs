using UnityEngine;

public abstract class HeroAbility : ScriptableObject
{
    public string abilityName;
    public float manaCost;
    public float cooldown;
    public Sprite icon;

    // We add 'costMultiplier' with a default of 1f to prevent breaking other calls
    public void TryUse(HeroCombat combat, HeroStats stats, float costMultiplier = 1f)
    {
        // 1. Calculate actual cost based on Game State
        float finalCost = manaCost * costMultiplier;

        // 2. Check Mana
        if (stats.currentMana < finalCost)
        {
            Debug.Log($"Not enough mana! Need {finalCost}");
            return;
        }

        // 3. Activate
        if (Activate(combat, stats))
        {
            // 4. Deduct Mana (Apply the modified cost)
            stats.currentMana -= finalCost;

            // 5. Update UI
            if (UIManager.Instance != null)
                UIManager.Instance.UpdateHeroBars(stats);

            // 6. Start Cooldown
            combat.StartAbilityCooldownUI(this, cooldown);
        }
    }

    // Abstract method for specific ability logic (Fireball, Heal, etc.)
    // Returns true if successful
    protected abstract bool Activate(HeroCombat combat, HeroStats stats);
}