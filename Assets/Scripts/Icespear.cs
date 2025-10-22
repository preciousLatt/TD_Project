using UnityEngine;

[CreateAssetMenu(menuName = "Hero/Ability/IceSpear")]
public class IceSpearAbility : HeroAbility
{
    public GameObject IceSpearPrefab;
    public float range = 10f;
    public float damage = 50f;

    public override void Activate(HeroCombat hero, HeroStats stats)
    {
        Debug.Log($"{abilityName} cast!");
    }
}
