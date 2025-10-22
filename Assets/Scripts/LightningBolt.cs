using UnityEngine;

[CreateAssetMenu(menuName = "Hero/Ability/LightningBolt")]
public class LightningBoltAbility : HeroAbility
{
    public GameObject LightningBoltPrefab;
    public float range = 10f;
    public float damage = 50f;

    public override void Activate(HeroCombat hero, HeroStats stats)
    {
        Debug.Log($"{abilityName} cast!");
    }
}
