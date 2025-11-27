using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Singleton;

public class ProjectileManager : Singleton<ProjectileManager>
{
    private Dictionary<string, IObjectPool<Projectile>> pools = new Dictionary<string, IObjectPool<Projectile>>();

    public Projectile SpawnProjectile(Projectile prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;

        string key = prefab.name; 

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
                p.name = prefab.name;
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