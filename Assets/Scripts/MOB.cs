using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;


class AggroEntry
{
    public MOB enemy;
    public float distanceToEnemy;

    public bool IsChamp()
    {
        return enemy.mobType == MOBType.Champ;
    }
}

public enum MOBType
{
    Champ,
    Minion,
    Structure,
}

public enum Team
{
    blue,
    red,
    neutral,
}

enum Status
{
    Idle,
    ExplicitWalk,
    Chasing,
    Attacking,
}

[RequireComponent(typeof(NavMeshAgent))]
public class MOB : MonoBehaviour
{
    NavMeshAgent navMeshAgent;
    public Team team;
    public HealthBar healthBar;
    public StatusBar statusBar;
    public MOBStats stats;

    MOB currentTarget;
    Status status = Status.Idle;

    float currentHealth;

    // Should all mobs should update at the same time?
    float targetUpdateInterval = 0.25f;
    float timeUntilTargetUpdate = 0;

    float acquisitionRange = 7.0f;
    float timeUntilNextAttack = 0;

    bool isAlive = true;
    LayerMask mobsMask;

    public Transform missileSpawn;
    public GameObject missilePrefab;

    public GameObject popupTextPrefab;

    // FIXME: This belongs on a Minion AI?
    public MOBType mobType;
    public Waypoints waypoints;
    Transform currentWaypoint;
    float waypointDistanceThreshold = 1f;


    float MaxHealth()
    {
        return stats.baseHealth;
    }

    float AttackDamage()
    {
        return stats.baseAttackDamage;
    }

    float AttackDelay()
    {
        return stats.attackSpeed / 1f;
    }

    float MoveSpeed()
    {
        return stats.baseMoveSpeed;
    }

    float AttackRange()
    {
        return stats.attackRange;
    }

    bool InAttackRange(MOB mob)
    {
        return DistanceTo(mob) <= AttackRange();
    }

    bool IsMinion()
    {
        return mobType == MOBType.Minion;
    }

    bool IsChamp()
    {
        return mobType == MOBType.Champ;
    }
    public Vector3 ModelCenter()
    {
        return transform.position + new Vector3(0, 0.5f, 0);
    }

    // Start is called before the first frame update
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        mobsMask = LayerMask.GetMask("MOB");
        AdjustHealth(MaxHealth(), this);

        // AddCircle("AcquisitionRange", acquisitionRange);
        //AddCircle("AttackRange", AttackRange());

        if (statusBar && IsChamp())
        {
            statusBar.SetText("Player");
        }

        navMeshAgent.speed = MoveSpeed();

        // 
        if (waypoints != null)
        {
            currentWaypoint = waypoints.GetFirstWaypoint(team == Team.red);
        }
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
        List<AggroEntry> aggroList = new List<AggroEntry>();
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
                var entry = new AggroEntry();
                entry.enemy = mob;
                // FIXME: This should be path-distance to.
                entry.distanceToEnemy = DistanceTo(mob);
                aggroList.Add(entry);
            }
        }
        if (aggroList.Count > 0)
        {
            aggroList.Sort(delegate (AggroEntry a, AggroEntry b)
            {
                if (a.IsChamp() != b.IsChamp())
                {
                    // Attack minions before champions.
                    return a.IsChamp() ? 1 : -1;
                }
                // Prefer closer targets.
                return a.distanceToEnemy.CompareTo(b.distanceToEnemy);
            });
            currentTarget = aggroList[0].enemy;
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
            else if (IsMinion() && DistanceTo(currentTarget) > acquisitionRange)
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
            healthBar.SetMaxHealth(MaxHealth());
            healthBar.SetCurrentHealth(currentHealth);
        }
    }

    public void AdjustHealth(float adjustment, MOB source)
    {
        // Debug.Log("Adjust Health " + health + " for " + this + " from " + source);
        currentHealth = Mathf.Max(Mathf.Min(currentHealth + adjustment, MaxHealth()), 0);
        UpdateHealthBar();

        if (popupTextPrefab != null)
        {
            PopupText.Create(popupTextPrefab, transform.position, adjustment);
        }

        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
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
        Assert.AreNotEqual(mob.team, team);
        currentTarget = mob;
        status = Status.Chasing;
        navMeshAgent.SetDestination(currentTarget.transform.position);
    }

    void CancelAutoAttack()
    {

    }

    public void AttackTarget(MOB mob)
    {
        Assert.AreNotEqual(mob.team, team);
        currentTarget = mob;
        if (InAttackRange(mob))
        {
            StartAutoAttack();
        }
        else
        {
            Chase(mob);
        }
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
        timeUntilNextAttack = AttackDelay();
        // start attack animation
        // launch missile?
    }

    void LaunchMissle()
    {
        Missile.FireMissile(missilePrefab, missileSpawn.position, this, currentTarget, AttackDamage());
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

    string DebugStatusString()
    {
        switch (status)
        {
            case Status.Attacking:
                return "Attacking";
            case Status.ExplicitWalk:
                return "Explicit Walk";
            case Status.Chasing:
                return "Chasing";
            case Status.Idle:
                return "Idle";
        }
        return null;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsMinion() && statusBar != null)
        {
            statusBar.SetText(DebugStatusString());
        }

        ValidateCurrentTarget();

        if (IsMinion())
        {
            timeUntilTargetUpdate -= Time.deltaTime;
            if (timeUntilTargetUpdate < 0)
            {
                timeUntilTargetUpdate = targetUpdateInterval;
                UpdateAggroList();

                // If in the middle of an action, don't do anything.
            }
        }

        if (currentTarget != null)
        {
            if (status == Status.ExplicitWalk)
            {
                if (ReachedDestination())
                {
                    status = Status.Idle;
                }
            }
            else if (status == Status.Attacking)
            {
                if (InAttackRange(currentTarget))
                {
                    timeUntilNextAttack -= Time.deltaTime;
                    if (timeUntilNextAttack <= 0)
                    {
                        LaunchMissle();
                        StartAutoAttack();
                    }
                }
                else
                {
                    status = Status.Idle;
                }
            }
            else if (status == Status.Chasing)
            {
                if (InAttackRange(currentTarget))
                {
                    StartAutoAttack();
                }
                else
                {
                    // Update our target location.
                    Chase(currentTarget);
                }
            }
            else if (status == Status.Idle)
            {
                if (InAttackRange(currentTarget))
                {
                    StartAutoAttack();
                }
                else
                {
                    if (IsMinion())
                    {
                        // If we have a target start chasing it.
                        Chase(currentTarget);
                    }
                }

            }
        }
        else
        {
            if (status == Status.Idle)
            {
                if (waypoints != null)
                {
                    // Move towards the next waypoint.
                    if (Vector3.Distance(transform.position, currentWaypoint.position) < waypointDistanceThreshold)
                    {
                        currentWaypoint = waypoints.GetNextWaypoint(currentWaypoint, team == Team.red);
                    }
                    if (currentWaypoint)
                    {
                        navMeshAgent.SetDestination(currentWaypoint.position);
                    }
                }
            }
        }
    }
}