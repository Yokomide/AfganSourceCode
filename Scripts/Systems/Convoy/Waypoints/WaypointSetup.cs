using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class WaypointSetup : MonoBehaviour
{
    [SerializeField] private Transform[] _waypointTransforms;

    [Inject] private ConvoySystem _convoySystem;

    private void Awake()
    {
        List<Vector3> waypoints = _waypointTransforms.Select(t => t.position).ToList();
        _convoySystem.SetWaypoints(waypoints);
    }
    private void OnDrawGizmos()
    {
        foreach (Transform t in transform)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(t.position, 1f);
        }

        Gizmos.color = Color.red;
        for (int i = 0; i < transform.childCount - 1; i++)
        {
            Gizmos.DrawLine(transform.GetChild(i).position, transform.GetChild(i + 1).position);
        }
        
    }
}