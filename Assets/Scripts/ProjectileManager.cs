using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Singleton;

public class ProjectileManager : Singleton<ProjectileManager>
{
    // Dictionary to hold a pool for each Projectile Prefab Name
    private Dictionary<string, IObjectPool<Projectile>> pools = new Dictionary<string, IObjectPool<Projectile>>();

    // Call this instead of Instantiate in your Tower.cs or Projectile.cs
    public Projectile SpawnProjectile(Projectile prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;

        string key = prefab.name; // Use prefab name as key

        if (!pools.ContainsKey(key))
        {
            CreatePool(key, prefab);
        }

        Projectile p = pools[key].Get();
        p.transform.position = position;
        p.transform.rotation = rotation;
        return p;
    }

    private void CreatePool(string key, Projectile prefab)
    {
        IObjectPool<Projectile> pool = new ObjectPool<Projectile>(
            createFunc: () => {
                Projectile p = Instantiate(prefab);
                p.name = prefab.name; // Keep name consistent for dictionary key
                // Important: Pass the pool to the projectile so it can release itself
                p.SetPool(pools[key]);
                return p;
            },
            actionOnGet: (p) => p.gameObject.SetActive(true),
            actionOnRelease: (p) => p.gameObject.SetActive(false),
            actionOnDestroy: (p) => Destroy(p.gameObject),
            defaultCapacity: 20,
            maxSize: 100
        );

        pools.Add(key, pool);
    }
}