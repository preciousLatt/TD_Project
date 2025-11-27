using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [Tooltip("Enemies will go to Level 0, then 1, then 2, etc.")]
    public int level = 0;

    private void OnEnable()
    {
        WaypointManager.Instance.RegisterWaypoint(this);
    }

    private void OnDisable()
    {
        if (WaypointManager.Instance != null)
        {
            WaypointManager.Instance.UnregisterWaypoint(this);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.HSVToRGB((level * 0.1f) % 1f, 1f, 1f);
        Gizmos.DrawSphere(transform.position, 0.5f);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up, $"WP Lvl {level}");
#endif
    }
}