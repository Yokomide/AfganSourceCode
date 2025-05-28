using System;
using UnityEngine;

public class CoverPoint : MonoBehaviour
{
    public bool IsOccupied { get; private set; } = false;

    public void Occupy()
    {
        IsOccupied = true;
    }

    public void Free()
    {
        IsOccupied = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.3f);

    }
}