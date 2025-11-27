using UnityEngine;

public abstract class HeroAbility : ScriptableObject
{
    public string abilityName;
    public float manaCost;
    public float cooldown;
    public Sprite icon;

    public void TryUse(HeroCombat combat, HeroStats stats, float costMultiplier = 1f)
    {
        float finalCost = manaCost * costMultiplier;

        if (stats.currentMana < finalCost)
        {
            Debug.Log($"Not enough mana! Need {finalCost}");
            return;
        }

        if (Activate(combat, stats))
        {
            stats.currentMana -= finalCost;

            if (UIManager.Instance != null)
                UIManager.Instance.UpdateHeroBars(stats);

            combat.StartAbilityCooldownUI(this, cooldown);
        }
    }

    protected abstract bool Activate(HeroCombat combat, HeroStats stats);
}