using UnityEngine;

public class BlueEnemySpawner : EnemyFactorySpawner
{
    protected override void SpawnEnemy()
    {
        baseHealth = 120f;
        baseSpeed = 1.5f;
        base.SpawnEnemy();  // use the base spawn behaviour
    }
}