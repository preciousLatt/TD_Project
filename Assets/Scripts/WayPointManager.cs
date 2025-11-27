using System.Collections.Generic;
using UnityEngine;
using Singleton; 

public class WaypointManager : Singleton<WaypointManager>
{
    private Dictionary<int, List<Waypoint>> waypointsByLevel = new Dictionary<int, List<Waypoint>>();

    public void RegisterWaypoint(Waypoint wp)
    {
        if (!waypointsByLevel.ContainsKey(wp.level))
        {
            waypointsByLevel[wp.level] = new List<Waypoint>();
        }

        if (!waypointsByLevel[wp.level].Contains(wp))
        {
            waypointsByLevel[wp.level].Add(wp);
        }
    }

    public void UnregisterWaypoint(Waypoint wp)
    {
        if (waypointsByLevel.ContainsKey(wp.level))
        {
            waypointsByLevel[wp.level].Remove(wp);
        }
    }

    public Transform GetClosestWaypoint(Vector3 fromPosition, int targetLevel)
    {
        if (!waypointsByLevel.ContainsKey(targetLevel) || waypointsByLevel[targetLevel].Count == 0)
        {
            return null;
        }

        List<Waypoint> potentialPoints = waypointsByLevel[targetLevel];
        Waypoint closest = null;
        float minDistSq = float.MaxValue;

        foreach (Waypoint wp in potentialPoints)
        {
            float distSq = (wp.transform.position - fromPosition).sqrMagnitude;
            if (distSq < minDistSq)
            {
                minDistSq = distSq;
                closest = wp;
            }
        }

        return closest != null ? closest.transform : null;
    }
}