using UnityEngine;

[CreateAssetMenu(menuName = "Hero/Ability/BoulderToss")]
public class BoulderTossAbility : HeroAbility
{
    public GameObject BoulderTossPrefab;
    public float range = 10f;
    public float damage = 50f;

    public override void Activate(HeroCombat hero, HeroStats stats)
    {
        Debug.Log($"{abilityName} cast!");
    }
}
