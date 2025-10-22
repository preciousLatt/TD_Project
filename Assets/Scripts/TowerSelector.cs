using UnityEngine;

public class TowerSelector : MonoBehaviour
{
    private Tower tower;
    private DrawRange rangeViz;

    public static TowerSelector ActiveTower { get; private set; }

    private void Awake()
    {
        tower = GetComponent<Tower>();
        rangeViz = GetComponent<DrawRange>();
    }

    public void SelectTower()
    {
        DeselectAll();

        ActiveTower = this;
        rangeViz?.SetRangeRadius(tower.attackRange);
        rangeViz?.ShowRange();

        UIManager.Instance.SetActiveTower(tower);
    }

    public static void DeselectAll()
    {
        if (ActiveTower != null)
        {
            ActiveTower.rangeViz?.HideRange();
            ActiveTower = null;
        }

        UIManager.Instance.SetActiveTower(null);
    }
}
