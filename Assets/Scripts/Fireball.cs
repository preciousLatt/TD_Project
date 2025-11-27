using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Fireball")]
public class FireballAbility : HeroAbility
{
    public FireballProjectile projectilePrefab;
    public float damage = 40f;

    // CHANGED: Update to protected override bool OnActivate
    protected override bool Activate(HeroCombat hero, HeroStats stats)
    {
        Camera cam = Camera.main;
        if (cam == null) return false;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 200f, LayerMask.GetMask("Ground")))
        {
            Vector3 spawnPos = hero.transform.position + Vector3.up * 1.2f;
            Projectile.Spawn(projectilePrefab, spawnPos, hit.point, damage);
            return true; // Successfully cast
        }

        return false; // Cast failed
    }
}