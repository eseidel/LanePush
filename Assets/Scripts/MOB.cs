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
    Wandering,
    Chasing,
    Attacking,
}

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

    float attackRange = 1.0f;
    float attackInterval = 1.0f;
    float aggroRadius = 4.0f;

    bool isAlive = true;
    LayerMask mobsMask;

    // Start is called before the first frame update
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        mobsMask = LayerMask.GetMask("MOB");
        AdjustHealth(maxHealth);

        var aggroCircle = new GameObject { name = "AggroRadius" };
        aggroCircle.DrawCircle(aggroRadius, .02f);
        aggroCircle.transform.parent = transform;
        var attackCircle = new GameObject { name = "AttackRange" };
        attackCircle.DrawCircle(attackRange, .02f);
        attackCircle.transform.parent = transform;
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
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, aggroRadius);
            foreach (var hitCollider in hitColliders)
            {
                var mob = hitCollider.gameObject.GetComponent<MOB>();
                // FIXME: This is wrong.
                if (mob == null) continue; 
                if (mob.team != team)
                {
                    Chase(mob);
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
            if (currentTarget.IsTargetable())
            {
                ClearTarget();
            }
            else if (DistanceTo(currentTarget) > aggroRadius)
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

    void Chase(MOB target)
    {
        currentTarget = target;
        status = Status.Chasing;
        navMeshAgent.SetDestination(target.transform.position);
    }

    void ClearNavigationPath()
    {
        navMeshAgent.SetDestination(transform.position);
    }

    void StartAutoAttack()
    {
        ClearNavigationPath(); // Stop chasing.
        status = Status.Attacking;
        Debug.Log("StartAttack");
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

        if (currentTarget != null)
        {
            if (DistanceTo(currentTarget) < attackRange)
            {
                StartAutoAttack();
            }
            else
            {
                status = Status.Chasing;
            }
        }
        else
        {
            // Path towards the nearest waypoint.
        }
    }
}