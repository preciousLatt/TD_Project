using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Ice Spear")]
public class IceSpearAbility : HeroAbility
{
    public IceSpearProjectile projectilePrefab;
    public float damage = 30f;
    public float maxDistance = 15f;

    protected override bool Activate(HeroCombat hero, HeroStats stats)
    {
        Camera cam = Camera.main;
        if (cam == null) return false;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 300f, LayerMask.GetMask("Ground")))
        {
            Vector3 spawnPos = hero.transform.position + Vector3.up * 1.2f;

            Vector3 dir = hit.point - spawnPos;
            dir.y = 0f;
            dir.Normalize();

            Vector3 targetPos = spawnPos + dir * maxDistance;

            Projectile.Spawn(projectilePrefab, spawnPos, targetPos, damage);
            return true; 
        }

        return false; 
    }
}