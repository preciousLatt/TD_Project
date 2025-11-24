using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Earth Boulder")]
public class BoulderTossability : HeroAbility
{
    public BoulderTossProjectile projectilePrefab;
    public float damage = 50f;
    public float explosionRadius = 4f;

    [Header("Arc Settings")]
    public float arcHeight = 5f; // How high the rock flies
    public float travelTime = 1.2f; // How long it takes to reach the target

    [Header("Range Settings")]
    public float maxCastRange = 15f; // Maximum distance the boulder can be thrown

    public override void Activate(HeroCombat hero, HeroStats stats)
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        // Raycast to find where the player clicked on the ground
        if (Physics.Raycast(ray, out RaycastHit hit, 300f, LayerMask.GetMask("Ground")))
        {
            Vector3 spawnPos = hero.transform.position + Vector3.up * 1.5f; // Spawn slightly above head
            Vector3 targetPos = hit.point;

            // --- Range Clamping Logic ---
            Vector3 heroPos = hero.transform.position;

            // Calculate horizontal vector from hero to target (ignore height difference)
            Vector3 directionVector = targetPos - heroPos;
            directionVector.y = 0;

            float currentDistance = directionVector.magnitude;

            // If the click is further than our max range, clamp it
            if (currentDistance > maxCastRange)
            {
                // Calculate the X/Z position at the max range
                Vector3 clampedOffset = directionVector.normalized * maxCastRange;
                Vector3 clampedPosXZ = heroPos + clampedOffset;

                // Important: We need to find the GROUND height at this new clamped location.
                // We cast a ray down from high up at the clamped X/Z coordinate.
                if (Physics.Raycast(clampedPosXZ + Vector3.up * 50f, Vector3.down, out RaycastHit groundHit, 100f, LayerMask.GetMask("Ground")))
                {
                    targetPos = groundHit.point;
                }
                else
                {
                    // Fallback if the ray misses (e.g. off the map), just use the hero's height
                    targetPos = clampedPosXZ;
                    targetPos.y = heroPos.y;
                }
            }
            // ---------------------------

            BoulderTossProjectile boulder = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

            // Initialize the lob
            boulder.Init(spawnPos, targetPos, arcHeight, travelTime, damage, explosionRadius);
        }
    }
}