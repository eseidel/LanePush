#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

class AggroList
{

}
public enum Team
{
    blue,
    red,
    neutral,
}

enum Status
{
    Stopped,
    ExplicitWalk,
    Wandering,
    Chasing,
    Attacking,
}

[RequireComponent(typeof(NavMeshAgent))]
public class MOB : MonoBehaviour
{
    NavMeshAgent navMeshAgent;
    public Team team;
    public HealthBar? healthBar;

    MOB? currentTarget;
    public bool isMinion = false;
    Status status = Status.Stopped;

    float currentHealth;
    public float maxHealth;

    // All mobs should update at the same time?
    float targetUpdateInterval = 0.25f;
    float timeUntilTargetUpdate = 0;

    float attackRange = 1.5f; // Melee
    float attackInterval = 1.0f;
    float acquisitionRange = 7.0f;

    bool isAlive = true;
    LayerMask mobsMask;

    // Start is called before the first frame update
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        mobsMask = LayerMask.GetMask("MOB");
        AdjustHealth(maxHealth);

        AddCircle("AcquisitionRange", acquisitionRange);
        AddCircle("AttackRange", attackRange);
    }

    void AddCircle(string name, float radius)
    {
        var attackCircle = new GameObject { name = name };
        attackCircle.DrawCircle(radius, 0.02f);
        attackCircle.transform.parent = transform;
        attackCircle.transform.localPosition = Vector3.zero;
    }

    // Minion States
    // Fighting -- has a target, in range.
    // Chasing -- has a target, not in range.
    // Walking -- has no target.
    void UpdateAggroList()
    {
        ValidateCurrentTarget();

        if (currentTarget == null)
        {
            // Hack to get a nearby mob.
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, acquisitionRange, mobsMask);
            foreach (var hitCollider in hitColliders)
            {
                var mob = hitCollider.gameObject.GetComponent<MOB>();
                if (mob == null)
                {
                    Debug.LogFormat("{0} in MOB layer does not have parent with MOB component", hitCollider);
                    continue;
                }
                if (mob.team != team)
                {
                    currentTarget = mob;
                    break;
                }
            }
        }

        // Enemy champions attacking an allied champion.
        // Enemy minions attacking an allied champion.
        // Enemy minions attacking an allied minion.
        // Enemy turrets attacking an allied minion.
        // Enemy champions attacking an allied minion.
        // The closest enemy minion.
        // The closest enemy champion.
    }

    void ValidateCurrentTarget()
    {
        if (currentTarget != null)
        {
            // If target is untargable, drop.
            if (!currentTarget.IsTargetable())
            {
                ClearTarget();
            }
            else if (isMinion && DistanceTo(currentTarget) > acquisitionRange)
            {
                // If target is out of range, drop.
                ClearTarget();
            }
        }
    }

    void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetCurrentHealth(currentHealth);
        }
    }

    void AdjustHealth(float health)
    {
        currentHealth = Mathf.Max(Mathf.Min(currentHealth + health, maxHealth), 0);
        UpdateHealthBar();
    }

    public void ClearTarget()
    {
        currentTarget = null;
    }

    public bool IsTargetable()
    {
        return isAlive;
    }

    public float DistanceTo(MOB mob)
    {
        return (mob.transform.position - transform.position).magnitude;
    }

    void Chase(MOB mob)
    {
        currentTarget = mob;
        status = Status.Chasing;
        navMeshAgent.SetDestination(currentTarget.transform.position);
    }

    void CancelAutoAttack()
    {

    }

    public void WalkTo(Vector3 point)
    {
        CancelAutoAttack();
        status = Status.ExplicitWalk;
        navMeshAgent.SetDestination(point);
    }

    public void ClearNavigationPath()
    {
        navMeshAgent.ResetPath();
    }

    void StartAutoAttack()
    {
        ClearNavigationPath(); // Stop chasing.
        status = Status.Attacking;
        // start attack animation
        // launch missile?
    }

    void CallForHelp()
    {
        // Find all nearby minions and call for help to them.
    }

    void OnCallForHelp(MOB caller, MOB attacker)
    {
        // Add attacker to aggro list.
    }

    bool ReachedDestination()
    {
        if (!navMeshAgent.pathPending)
        {
            if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                if (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        timeUntilTargetUpdate -= Time.deltaTime;
        if (timeUntilTargetUpdate < 0)
        {
            timeUntilTargetUpdate = targetUpdateInterval;
            UpdateAggroList();

            // If in the middle of an action, don't do anything.
        }

        if (status == Status.ExplicitWalk)
        {
            if (ReachedDestination())
            {
                status = Status.Stopped;
            }
        }
        else if (currentTarget != null)
        {
            if (DistanceTo(currentTarget) < attackRange)
            {
                StartAutoAttack();
            }
            else
            {
                if (isMinion)
                {
                    Chase(currentTarget);
                }
            }
        }
        else if (isMinion)
        {
            // Path towards the nearest waypoint.
        }
    }
}