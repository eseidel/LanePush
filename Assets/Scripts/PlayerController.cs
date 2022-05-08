using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(MOB)), RequireComponent(typeof(NavMeshAgent))]
public class PlayerController : MonoBehaviour
{
    new public Camera camera;
    MOB me;
    public NavMeshAgent navMeshAgent;

    private void Start()
    {
        me = GetComponent<MOB>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void BlinkTo(Vector3 point)
    {
        me.ClearNavigationPath();
        transform.position = point;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                me.WalkTo(hit.point);
            }
        }

        if (Input.GetKey(KeyCode.F))
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                BlinkTo(hit.point);
            }
        }
    }

    private void LateUpdate()
    {
        if (navMeshAgent.velocity.sqrMagnitude > Mathf.Epsilon)
        {
            transform.rotation = Quaternion.LookRotation(navMeshAgent.velocity.normalized);
        }
    }
}
