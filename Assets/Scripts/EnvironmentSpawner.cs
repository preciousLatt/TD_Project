using UnityEngine;

[System.Serializable]
public class EnvironmentItem
{
    public GameObject prefab;
    public int count;
}

public class EnvironmentSpawner : MonoBehaviour
{
    public Vector3 areaCenter = Vector3.zero;
    public Vector3 areaSize = new Vector3(20f, 0f, 20f);

    public EnvironmentItem[] items;

    public Transform parent;

    private void Start()
    {
        SpawnEnvironment();
    }

    private void SpawnEnvironment()
    {
        foreach (var item in items)
        {
            for (int i = 0; i < item.count; i++)
            {
                Vector3 randomPos = GetRandomPosition();
                Quaternion randomRot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                GameObject obj = Instantiate(item.prefab, randomPos, randomRot, parent);
            }
        }
    }

    private Vector3 GetRandomPosition()
    {
        float x = Random.Range(-areaSize.x / 2f, areaSize.x / 2f) + areaCenter.x;
        float z = Random.Range(-areaSize.z / 2f, areaSize.z / 2f) + areaCenter.z;
        float y = areaCenter.y; 

        return new Vector3(x, y, z);
    }
}
