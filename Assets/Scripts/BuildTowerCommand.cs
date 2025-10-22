using UnityEngine;

public class BuildTowerCommand : ICommand
{
    private GameObject towerPrefab;
    private Vector3 position;
    private Quaternion rotation;
    private GameObject placedTower;
    private int towerCost;

    public BuildTowerCommand(GameObject prefab, Vector3 pos, Quaternion rot, int cost)
    {
        towerPrefab = prefab;
        position = pos;
        rotation = rot;
        towerCost = cost;
    }

    public bool Execute()
    {
        // Check resources
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