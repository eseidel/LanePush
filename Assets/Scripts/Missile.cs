using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Missile : MonoBehaviour
{
    public MOB source;
    public MOB target;
    public Rigidbody rigidBody;
    float angleChangingSpeed = 100;
    public float movementSpeed;
    float damage = 0;

    public static Missile FireMissile(GameObject prefab, Vector3 spawnPoint, MOB source, MOB target, float damage)
    {
        var obj = Object.Instantiate(prefab, spawnPoint, Quaternion.identity);
        var missile = obj.GetComponent<Missile>();
        missile.target = target;
        missile.damage = damage;
        missile.source = source;
        obj.transform.LookAt(target.ModelCenter());
        return missile;
    }

    private void Start()
    {
        GetComponent<Rigidbody>().maxAngularVelocity = Mathf.Infinity;
    }

    void FixedUpdate()
    {
        if (target == null)
        {
            // Should also check MOB.isDead?
            Destroy(gameObject);
            return;
        }
        Vector3 direction = target.ModelCenter() - rigidBody.position;
        direction.Normalize();
        var rotateAmount = Vector3.Cross(direction, transform.up);
        rigidBody.angularVelocity = -angleChangingSpeed * rotateAmount;
        rigidBody.velocity = transform.up * movementSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (target != null && other.gameObject == target.gameObject)
        {
            var mob = other.GetComponent<MOB>();
            mob.AdjustHealth(-damage, source);
            Destroy(gameObject);
        }
        // Otherwise we ran into another missle or the ground, etc.
        // FIXME: Is there a way to only collide with certain layers?
    }
}