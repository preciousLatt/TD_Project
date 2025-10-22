using UnityEngine;

public class RedEnemySpawner : EnemyFactorySpawner
{
    protected override void SpawnEnemy()
    {
        baseHealth = 80f;
        baseSpeed = 3.5f;
        base.SpawnEnemy();
    }
}