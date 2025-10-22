using UnityEngine;
using System.Collections.Generic;

public abstract class HeroAbility : ScriptableObject
{
    public string abilityName;
    public float manaCost = 20f;
    public float cooldown = 5f;

    private Dictionary<HeroCombat, float> cooldownTimers = new();

    public bool TryUse(HeroCombat hero, HeroStats stats)
    {
        if (!cooldownTimers.ContainsKey(hero))
            cooldownTimers[hero] = 0;

        if (cooldownTimers[hero] > 0)
        {
            Debug.Log($"{abilityName} is still on cooldown: {cooldownTimers[hero]:F1}s");
            return false;
        }

        if (stats.currentMana < manaCost)
        {
            Debug.Log("Not enough mana");
            return false;
        }

        if (!stats.SpendMana(manaCost))
        {
            return false;
        }

        Activate(hero, stats);
        cooldownTimers[hero] = cooldown;
        hero.StartAbilityCooldownUI(this, cooldown);
        hero.StartCoroutine(CooldownRoutine(hero));
        return true;
    }

    private System.Collections.IEnumerator CooldownRoutine(HeroCombat hero)
    {
        while (cooldownTimers[hero] > 0)
        {
            cooldownTimers[hero] -= Time.deltaTime;
            yield return null;
        }

        cooldownTimers[hero] = 0;
    }

    public abstract void Activate(HeroCombat hero, HeroStats stats);
}
