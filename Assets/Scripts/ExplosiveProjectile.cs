using UnityEngine;

public class ExplosiveProjectile : Projectile
{
    [SerializeField] private float explosionRadius = 4f;
    [SerializeField] private float explosionDamage = 40f;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private LayerMask groundMask; 
    [SerializeField] private GameObject explosionEffect;

    private bool exploded = false;

    protected override void HitTarget()
    {
        Explode();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (exploded) return;

        if (((1 << other.gameObject.layer) & groundMask) != 0)
        {
            Explode();
        }
        else if (other.CompareTag("Enemy"))
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (exploded) return;
        exploded = true;

        Vector3 pos = transform.position;

        if (explosionEffect != null)
        {
            GameObject fx = Instantiate(explosionEffect, pos, Quaternion.identity);
            Destroy(fx, 0.3f); 
        }

        Collider[] hits = Physics.OverlapSphere(pos, explosionRadius, enemyMask);

        foreach (var hit in hits)
        {
            Enemy e = hit.GetComponent<Enemy>();
            if (e != null && !e.IsDead)
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.DamageEnemy(e, explosionDamage);
                else
                    e.TakeDamage(explosionDamage);
            }
        }

        Destroy(gameObject);
    }

}