using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    new public Camera camera;
    public NavMeshAgent navMeshAgent;

    private void Start()
    {
        navMeshAgent.updateRotation = false;
    }

    void WalkTo(Vector3 point)
    {
        navMeshAgent.SetDestination(point);
    }

    void BlinkTo(Vector3 point)
    {
        navMeshAgent.SetDestination(point);
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
                WalkTo(hit.point);
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
