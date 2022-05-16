using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    new Transform camera;
    private void Start()
    {
        camera = Camera.main.transform;

    }
    private void LateUpdate()
    {
        Vector3 v3T = transform.position + camera.transform.rotation * Vector3.forward;
        v3T.y = transform.position.y;
        transform.LookAt(v3T, Vector3.up);
    }
}
