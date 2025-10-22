using UnityEngine;

[CreateAssetMenu(menuName = "Hero/Ability/Fireball")]
public class FireballAbility : HeroAbility
{
    public GameObject fireballPrefab;
    public float range = 10f;
    public float damage = 50f;

    public override void Activate(HeroCombat hero, HeroStats stats)
    {
        Debug.Log($"{abilityName} cast!");
    }
}
