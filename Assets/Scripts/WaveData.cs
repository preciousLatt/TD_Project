using System;
using System.Collections.Generic;

[Serializable]
public class WaveContainer
{
    public List<WaveDefinition> waves = new List<WaveDefinition>();
}

[Serializable]
public class WaveDefinition
{
    public List<EnemySpawnGroup> groups = new List<EnemySpawnGroup>();
}

[Serializable]
public class EnemySpawnGroup
{
    public string enemyId;  
    public int count;        
    public float rate;       
    public float initialDelay; 
}