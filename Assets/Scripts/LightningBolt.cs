using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Lightning Bolt")]
public class LightningBoltAbility : HeroAbility
{
    public LightningBoltProjectile projectilePrefab;
    public float damage = 25f;
    public float travelDistance = 25f;
    public float castYOffset = 1.2f;

    public override void Activate(HeroCombat hero, HeroStats stats)
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

  
        if (!Physics.Raycast(ray, out RaycastHit hit, 300f, LayerMask.GetMask("Ground")))
            return;

        Vector3 spawnPos = hero.transform.position + Vector3.up * castYOffset;

        Vector3 dir = hit.point - spawnPos;
        dir.y = 0f; // Force direction to be horizontal
        dir.Normalize();

        Vector3 endPos = spawnPos + dir * travelDistance;

        LightningBoltProjectile bolt =
            Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(dir));

        bolt.InitDirectionOnly(dir, endPos, damage);
    }
}