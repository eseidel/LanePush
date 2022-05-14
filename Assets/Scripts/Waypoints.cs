using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoints : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        foreach(Transform t in transform)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(t.position, 1f);
        }

        Gizmos.color = Color.red;
        for (int i = 0; i < transform.childCount -1; i++)
        {
            Gizmos.DrawLine(transform.GetChild(i).position, transform.GetChild(i + 1).position);
        }
    }

    public Transform GetFirstWaypoint(bool reverseOrder)
    {
        return reverseOrder ? transform.GetChild(transform.childCount - 1) : transform.GetChild(0);
    }

    public Transform GetNextWaypoint(Transform currentWaypoint, bool reverseOrder)
    {
        if (reverseOrder)
        {
            if (currentWaypoint.GetSiblingIndex() > 0)
            {
                return transform.GetChild(currentWaypoint.GetSiblingIndex() - 1);
            }
        }
        if (currentWaypoint.GetSiblingIndex() < transform.childCount - 1)
        {
            return transform.GetChild(currentWaypoint.GetSiblingIndex() + 1);
        }
            return null;

        

    }
}
