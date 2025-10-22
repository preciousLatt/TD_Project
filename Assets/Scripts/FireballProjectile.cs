using UnityEngine;

public class FireballProjectile : Projectile
{
    [Header("Explosion Settings")]
    [SerializeField] private float explosionRadius = 4f;
    [SerializeField] private float explosionDamage = 40f;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private GameObject explosionEffect;

    private bool exploded = false;

    protected override void OnReachedTarget()
    {
        Explode();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (exploded) return;

        if (other.CompareTag("Enemy") || other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (exploded) return;
        exploded = true;

        Vector3 pos = transform.position;

        Collider[] hits = Physics.OverlapSphere(pos, explosionRadius, enemyMask);

        foreach (var hit in hits)
        {
            Enemy e = hit.GetComponent<Enemy>();
            if (e != null)
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.DamageEnemy(e, explosionDamage);
                else
                    e.TakeDamage(explosionDamage);
            }
        }

        if (explosionEffect)
        {
            GameObject fx = Instantiate(explosionEffect, pos, Quaternion.identity);
            Destroy(fx, 0.5f);
        }
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
#endif
}
