using UnityEngine;

public class BoulderTossProjectile : MonoBehaviour
{
    [SerializeField] private GameObject impactVFX;

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

        Vector3 linearPos = Vector3.Lerp(_startPos, _targetPos, linearT);
        float heightOffset = 4 * _arcHeight * linearT * (1 - linearT);

        transform.position = linearPos + Vector3.up * heightOffset;

        transform.Rotate(Vector3.right * 360f * Time.deltaTime);
    }

    private void Explode()
    {
        _isFlying = false;

        if (impactVFX != null)
        {
            GameObject vfxInstance = Instantiate(impactVFX, transform.position, Quaternion.identity);

            Destroy(vfxInstance, 3f);
        }

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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _radius > 0 ? _radius : 4f);
    }
}