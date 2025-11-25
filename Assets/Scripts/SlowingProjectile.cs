using UnityEngine;

public class SlowingProjectile : Projectile
{
    [Header("Slow Stats")]
    [Range(0f, 1f)]
    [SerializeField] private float slowPercent = 0.4f; // 0.4 = 40% slow
    [SerializeField] private float slowDuration = 2.0f;

    protected override void HitTarget()
    {
        if (useEnemyTarget && targetEnemy != null)
        {
            // 1. Deal Damage
            GameManager.Instance?.DamageEnemy(targetEnemy, damage);

            // 2. Apply Slow (Using the method we added to Enemy.cs earlier)
            targetEnemy.ApplySlow(slowPercent, slowDuration);
        }

        // 3. Visuals
        if (impactVFX != null)
            Instantiate(impactVFX, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}