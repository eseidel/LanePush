using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Missile : MonoBehaviour
{
    public Transform target;
    public Rigidbody rigidBody;
    public float angleChangingSpeed;
    public float movementSpeed;

    public float damage = 0;

    void FixedUpdate()
    {
        Vector3 direction = target.position - rigidBody.position;
        direction.Normalize();
        var rotateAmount = Vector3.Cross(direction, transform.up);
        rigidBody.angularVelocity = -angleChangingSpeed * rotateAmount;
        rigidBody.velocity = transform.up * movementSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other);
        var mob = other.GetComponent<MOB>();
        mob.AdjustHealth(-damage);
        Destroy(gameObject);
    }
}