using UnityEngine;

public class BoulderTossProjectile : MonoBehaviour
{
    [SerializeField] private GameObject impactVFX; // Assign a particle effect here

    private Vector3 _startPos;
    private Vector3 _targetPos;
    private float _arcHeight;
    private float _duration;
    private float _damage;
    private float _radius;

    private float _timeElapsed = 0f;
    private bool _isFlying = false;

    public void Init(Vector3 start, Vector3 target, float height, float duration, float dmg, float rad)
    {
        _startPos = start;
        _targetPos = target;
        _arcHeight = height;
        _duration = duration;
        _damage = dmg;
        _radius = rad;
        _isFlying = true;
    }

    private void Update()
    {
        if (!_isFlying) return;

        _timeElapsed += Time.deltaTime;
        float linearT = _timeElapsed / _duration;

        if (linearT >= 1f)
        {
            Explode();
            return;
        }

        // --- Parabolic Movement Math ---
        Vector3 linearPos = Vector3.Lerp(_startPos, _targetPos, linearT);
        float heightOffset = 4 * _arcHeight * linearT * (1 - linearT);

        transform.position = linearPos + Vector3.up * heightOffset;

        // Rotate the rock to tumble
        transform.Rotate(Vector3.right * 360f * Time.deltaTime);
    }

    private void Explode()
    {
        _isFlying = false;

        // Visual Effects Cleanup Fix
        if (impactVFX != null)
        {
            GameObject vfxInstance = Instantiate(impactVFX, transform.position, Quaternion.identity);

            // FIX: Destroy the VFX object after 3 seconds so it doesn't leak memory/clutter scene
            // If your particle system is longer than 3 seconds, increase this number.
            Destroy(vfxInstance, 3f);
        }

        // Deal Area Damage
        Collider[] hits = Physics.OverlapSphere(transform.position, _radius, LayerMask.GetMask("Enemy"));

        foreach (var hit in hits)
        {
            Enemy e = hit.GetComponent<Enemy>();
            if (e != null && !e.IsDead)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.DamageEnemy(e, _damage);
                }
            }
        }

        Destroy(gameObject);
    }

    // Draw the explosion radius in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _radius > 0 ? _radius : 4f);
    }
}