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
        if (target == null)
        {
            // Should also check MOB.isDead?
            Destroy(gameObject);
            return;
        }
        Vector3 direction = target.position - rigidBody.position;
        direction.Normalize();
        var rotateAmount = Vector3.Cross(direction, transform.up);
        rigidBody.angularVelocity = -angleChangingSpeed * rotateAmount;
        rigidBody.velocity = transform.up * movementSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        var mob = other.GetComponent<MOB>();
        if (mob != null)
        {
            mob.AdjustHealth(-damage);
            Destroy(gameObject);
        }
        // Otherwise we ran into another missle or the ground, etc.
        // FIXME: Is there a way to only collide with certain layers?
    }
}