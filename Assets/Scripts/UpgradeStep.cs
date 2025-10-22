using UnityEngine;

[System.Serializable]
public class UpgradeStep
{
    public string displayName;
    public Sprite icon;
    public int cost;
    public float rangeBonus;
    public float fireRateBonus;
    public float damageBonus;
    public GameObject upgradedTowerPrefab;
}
