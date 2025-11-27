using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Lightning Bolt")]
public class LightningBoltAbility : HeroAbility
{
    public LightningBoltProjectile projectilePrefab;
    public float damage = 25f;
    public float travelDistance = 25f;
    public float castYOffset = 1.2f;

    protected override bool Activate(HeroCombat hero, HeroStats stats)
    {
        Camera cam = Camera.main;
        if (cam == null) return false;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, 300f, LayerMask.GetMask("Ground")))
            return false;

        Vector3 spawnPos = hero.transform.position + Vector3.up * castYOffset;

        Vector3 dir = hit.point - spawnPos;
        dir.y = 0f; 
        dir.Normalize();

        Vector3 endPos = spawnPos + dir * travelDistance;

        Quaternion lookRot = Quaternion.LookRotation(dir);

        Quaternion spawnRot = lookRot * Quaternion.Euler(90f, 0f, 0f);

        LightningBoltProjectile bolt =
            Instantiate(projectilePrefab, spawnPos, spawnRot);

        bolt.InitDirectionOnly(dir, endPos, damage);
        bolt.transform.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(90f, 0f, 0f);
        return true; 
    }
}