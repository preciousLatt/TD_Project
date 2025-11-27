using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Earth Boulder")]
public class BoulderTossability : HeroAbility
{
    public BoulderTossProjectile projectilePrefab;
    public float damage = 50f;
    public float explosionRadius = 4f;

    public float arcHeight = 5f; 
    public float travelTime = 1.2f;

    public float maxCastRange = 15f; 

    protected override bool Activate(HeroCombat hero, HeroStats stats)
    {
        Camera cam = Camera.main;
        if (cam == null) return false;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 300f, LayerMask.GetMask("Ground")))
        {
            Vector3 spawnPos = hero.transform.position + Vector3.up * 1.5f;
            Vector3 targetPos = hit.point;

            Vector3 heroPos = hero.transform.position;
            Vector3 directionVector = targetPos - heroPos;
            directionVector.y = 0;

            float currentDistance = directionVector.magnitude;

            if (currentDistance > maxCastRange)
            {
                Vector3 clampedOffset = directionVector.normalized * maxCastRange;
                Vector3 clampedPosXZ = heroPos + clampedOffset;

                if (Physics.Raycast(clampedPosXZ + Vector3.up * 50f, Vector3.down, out RaycastHit groundHit, 100f, LayerMask.GetMask("Ground")))
                {
                    targetPos = groundHit.point;
                }
                else
                {
                    targetPos = clampedPosXZ;
                    targetPos.y = heroPos.y;
                }
            }
   
            BoulderTossProjectile boulder = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            boulder.Init(spawnPos, targetPos, arcHeight, travelTime, damage, explosionRadius);

            return true; 
        }

        return false; 
    }
}