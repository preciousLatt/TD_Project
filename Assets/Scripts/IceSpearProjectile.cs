using UnityEngine;

public class IceSpearProjectile : Projectile
{
    [SerializeField] private LayerMask enemyMask;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy e = other.GetComponent<Enemy>();
            if (e != null)
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.DamageEnemy(e, damage);
                else
                    e.TakeDamage(damage);
            }
        }
    }

    protected void OnReachedTarget()
    {
        Destroy(gameObject); 
    }
}
