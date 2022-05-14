using System.Collections;
using System.Collections.Generic;
using UnityEngine;




[RequireComponent(typeof(MOB))]
public class MinionAI : MonoBehaviour
{
    MOB me;
    GameObject[] waypoints;

    void Start()
    {
        me = GetComponent<MOB>();
    }
}
