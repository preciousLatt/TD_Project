using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UpgradePath
{
    public string pathName;
    public Sprite pathIcon;
    public List<UpgradeStep> steps = new List<UpgradeStep>();
}
