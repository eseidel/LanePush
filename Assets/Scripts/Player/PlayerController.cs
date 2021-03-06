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
        me.isLocalPlayer = true;
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (!me.isAlive)
        {
            return;
        }
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                var mob = hit.collider.GetComponentInParent<MOB>();
                if (mob != null && mob.team != me.team)
                {
                    me.SetPlayerTarget(mob);
                }
                else
                {
                    me.WalkTo(hit.point);
                }
            }
        }

        if (Input.GetKey(KeyCode.F))
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                me.BlinkTo(hit.point);
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
