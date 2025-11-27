using UnityEngine;

public class SlowingProjectile : Projectile
{
    [Header("Slow Stats")]
    [Range(0f, 1f)]
    [SerializeField] private float slowPercent = 0.4f; 
    [SerializeField] private float slowDuration = 2.0f;

    protected override void HitTarget()
    {
        if (useEnemyTarget && targetEnemy != null)
        {
            GameManager.Instance?.DamageEnemy(targetEnemy, damage);

            targetEnemy.ApplySlow(slowPercent, slowDuration);
        }

        if (impactVFX != null)
            Instantiate(impactVFX, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}