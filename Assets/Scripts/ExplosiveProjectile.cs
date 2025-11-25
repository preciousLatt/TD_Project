using UnityEngine;

public class ExplosiveProjectile : Projectile
{
    [Header("Explosive Stats")]
    [SerializeField] private float explosionRadius = 4f;

    // We override the base hit logic
    protected override void HitTarget()
    {
        // 1. Visuals
        if (impactVFX != null)
            Instantiate(impactVFX, transform.position, Quaternion.identity);

        // 2. Find all enemies in range
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, LayerMask.GetMask("Enemy"));

        foreach (Collider hit in hits)
        {
            Enemy e = hit.GetComponent<Enemy>();
            if (e != null && !e.IsDead)
            {
                // Deal damage to everyone found
                GameManager.Instance?.DamageEnemy(e, damage);
            }
        }

        // 3. Destroy the bullet
        Destroy(gameObject);
    }

    // Draw the radius in the editor so you can see it
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}