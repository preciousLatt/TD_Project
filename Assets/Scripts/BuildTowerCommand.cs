using UnityEngine;

public class BuildTowerCommand : IGameCommand
{
    private GameObject towerPrefab;
    private Vector3 position;
    private Quaternion rotation;
    private GameObject placedTower;
    private int towerCost;

    public BuildTowerCommand(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        towerPrefab = prefab;
        position = pos;
        rotation = rot;

        Tower t = towerPrefab.GetComponent<Tower>();
        if (t != null)
        {
            towerCost = t.cost;
        }
        else
        {
            towerCost = 100; 
            Debug.LogWarning($"Tower prefab {prefab.name} is missing a Tower script!");
        }
    }

    public bool Execute()
    {
        if (!GameManager.Instance.CanAfford(towerCost))
        {
            Debug.Log("Cannot afford tower!");
            return false;
        }

        GameManager.Instance.SpendMoney(towerCost);
        placedTower = Object.Instantiate(towerPrefab, position, rotation);
        return true;
    }

    public void Undo()
    {
        if (placedTower != null)
        {
            Object.Destroy(placedTower);
            GameManager.Instance.AddMoney(towerCost);
        }
    }
}