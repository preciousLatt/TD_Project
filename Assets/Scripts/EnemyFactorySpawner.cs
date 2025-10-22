using System.Collections;
using UnityEngine;

public class EnemyFactorySpawner : MonoBehaviour
{
    [Header("Enemy Prefab")]
    [SerializeField] protected GameObject enemyPrefab;

    [Header("Base Stats")]
    [SerializeField] protected float baseSpeed = 2f;
    [SerializeField] protected float baseHealth = 100f;
    [SerializeField] protected float damageToNexus = 10f; // (optional, if you want per-type damage)

    [Header("Spawner Settings")]
    [SerializeField] protected Transform nexusTarget;
    [SerializeField] protected Transform[] spawnPoints;
    [SerializeField] protected float spawnInterval = 2f;
    [SerializeField] protected int enemiesPerWave = 10;

    protected virtual void Start()
    {
        if (nexusTarget == null)
        {
            GameObject nexusObj = GameObject.FindGameObjectWithTag("Nexus");
            if (nexusObj != null)
                nexusTarget = nexusObj.transform;
        }

        StartCoroutine(SpawnRoutine());
    }

    protected virtual IEnumerator SpawnRoutine()
    {
        for (int i = 0; i < enemiesPerWave; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    protected virtual void SpawnEnemy()
    {
        if (enemyPrefab == null || nexusTarget == null)
        {
            Debug.LogWarning("Spawner missing prefab or nexus target");
            return;
        }

        Transform point = spawnPoints.Length > 0
            ? spawnPoints[Random.Range(0, spawnPoints.Length)]
            : transform;

        GameObject go = Instantiate(enemyPrefab, point.position, Quaternion.identity);
        Enemy e = go.GetComponent<Enemy>();
        if (e != null)
        {
            // Initialize stats/target (registration is handled by the Enemy itself)
            e.Initialize(baseSpeed, baseHealth, nexusTarget);
        }
        else
        {
            Debug.LogWarning("Spawned enemy prefab does not contain an Enemy component.");
        }
    }
}
