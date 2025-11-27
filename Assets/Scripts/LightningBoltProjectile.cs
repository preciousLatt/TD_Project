using System.Collections.Generic;
using UnityEngine;

public class LightningBoltProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 30f;
    [SerializeField] private float chainRange = 6f;
    [SerializeField] private int maxChains = 4;
    [SerializeField] private LineRenderer linePrefab;
    [SerializeField] private float lineDuration = 0.15f;

    private Vector3 _direction;
    private Vector3 _endPosition;
    private Vector3 _startPosition; 
    private float _hitDamage = 0f;
    private bool _isFlying = true;

    private HashSet<Enemy> _hitSet = new HashSet<Enemy>();
    private int _chainsDone = 0;


    public void InitDirectionOnly(Vector3 direction, Vector3 endPosition, float damage)
    {
        _direction = direction;
        _endPosition = endPosition;
        _startPosition = transform.position; 
        _hitDamage = damage;
        _isFlying = true;

        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void Update()
    {
        if (_isFlying)
        {
            transform.position += _direction * speed * Time.deltaTime;

   
            if (Vector3.Dot(transform.position - _startPosition, _direction) >= Vector3.Dot(_endPosition - _startPosition, _direction))
            {
                Destroy(gameObject);
                _isFlying = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isFlying) return;

        Enemy hitEnemy = other.GetComponent<Enemy>();

        if (hitEnemy != null && !_hitSet.Contains(hitEnemy))
        {
            _isFlying = false;

            DamageEnemy(hitEnemy);

            _hitSet.Add(hitEnemy);
            _chainsDone = 1;

            StartInstantChain(hitEnemy);

            Destroy(gameObject);
        }
    }

    private void StartInstantChain(Enemy start)
    {
        Enemy last = start;

        while (_chainsDone < maxChains)
        {
            Enemy next = FindNextChainTarget(last.transform.position);
            if (next == null) break; // No more targets found

            DamageEnemy(next);

            if (last != null && next != null)
            {
                // Assume enemies have a center point slightly above their feet for visuals
                Vector3 startPoint = last.transform.position + Vector3.up * 1f;
                Vector3 endPoint = next.transform.position + Vector3.up * 1f;
                SpawnLightningLine(startPoint, endPoint);
            }

            // Update state
            _hitSet.Add(next);
            last = next;
            _chainsDone++;
        }
    }

    private Enemy FindNextChainTarget(Vector3 from)
    {
        Enemy closest = null;
        float bestSqr = chainRange * chainRange;

        Collider[] hits = Physics.OverlapSphere(from, chainRange, LayerMask.GetMask("Enemy"));
        float minDistanceSqr = chainRange * chainRange;

        foreach (Collider hit in hits)
        {
            Enemy e = hit.GetComponent<Enemy>();
            if (e != null && !_hitSet.Contains(e) && !e.IsDead)
            {
                float sqr = (e.transform.position - from).sqrMagnitude;
                if (sqr < minDistanceSqr)
                {
                    minDistanceSqr = sqr;
                    closest = e;
                }
            }
        }

        return closest;
    }

    private void DamageEnemy(Enemy e)
    {
        if (e == null || e.IsDead) return;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.DamageEnemy(e, _hitDamage);
        }
    }

    private void SpawnLightningLine(Vector3 a, Vector3 b)
    {
        if (linePrefab == null) return;

        LineRenderer lr = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
        lr.positionCount = 2;
        lr.SetPosition(0, a);
        lr.SetPosition(1, b);

        Destroy(lr.gameObject, lineDuration);
    }
}