using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;
using System.Linq;


// Enemy champions attacking an allied champion.
// Enemy minions attacking an allied champion.
// Enemy minions attacking an allied minion.
// Enemy turrets attacking an allied minion.
// Enemy champions attacking an allied minion.
// The closest enemy minion.
// The closest enemy champion.

enum MinionPriority
{
    Highest = 10,
    ChampAttacksChamp = 9,
    MinionAttacksChamp = 8,
    MinionAttacksMinion = 7,
    TurretAttacksMinion = 6,
    ChampAttacksMinion = 5,
    NearbyMinion = 4,
    NearbyChampion = 3,
    NearbyTurret = 2, // Unclear what order this is in.
    NearbyInhib = 1, // Unclear what order this is in.
    NearbyNexus = 0, // Unclear what order this is in.
    Lowest = -1,
}

public enum MOBType
{
    Champ,
    Minion,
    Turret,
    Inhibitor,
    Nexus,
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

class AttackTarget
{
    public MOB target;
    public MinionPriority priority;

    public AttackTarget(MOB target, MinionPriority priority)
    {
        this.target = target;
        this.priority = priority;
    }
}

[RequireComponent(typeof(NavMeshAgent))]
public class MOB : MonoBehaviour
{
    NavMeshAgent navMeshAgent;
    public Team team;
    public HealthBar healthBar;
    public StatusBar statusBar;
    public MOBStats stats;

    AttackTarget target;
    Status status = Status.Idle;

    // FIXME: Should health be on a separate object?
    float currentHealth;
    bool isAlive = true;

    // All mobs should update at the same time based on server tick.
    float targetUpdateInterval = 0.25f;
    float timeUntilTargetUpdate = 0;

    float acquisitionRange = 7.0f;
    float callForHelpRange = 7.0f; // No clue what this should be?
    float timeUntilNextAttack = 0;

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

    List<MOB> NearbyMobs(float range)
    {
        List<MOB> mobs = new List<MOB>();
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, range, mobsMask);
        foreach (var hitCollider in hitColliders)
        {
            var mob = hitCollider.gameObject.GetComponent<MOB>();
            if (mob == null)
            {
                Debug.LogFormat("{0} in MOB layer does not have parent with MOB component", hitCollider);
                continue;
            }
            mobs.Add(mob);
        }
        return mobs;
    }

    MinionPriority AggroPriorityByMobType(MOBType type)
    {
        switch (type)
        {
            case MOBType.Minion:
                return MinionPriority.NearbyMinion;
            case MOBType.Champ:
                return MinionPriority.NearbyChampion;
            case MOBType.Turret:
                return MinionPriority.NearbyTurret;
            case MOBType.Inhibitor:
                return MinionPriority.NearbyInhib;
            case MOBType.Nexus:
                return MinionPriority.NearbyNexus;
        }
        // Assert not reached?
        return MinionPriority.Lowest;
    }

    // Minion States
    // Fighting -- has a target, in range.
    // Chasing -- has a target, not in range.
    // Walking -- has no target.
    MOB FindBestTargetInAcquisitionRange()
    {
        var mobs = NearbyMobs(acquisitionRange).Where(mob => mob.team != team).ToList();
        if (mobs.Count > 0)
        {
            mobs.Sort(delegate (MOB a, MOB b)
            {
                var aPrio = AggroPriorityByMobType(a.mobType);
                var bPrio = AggroPriorityByMobType(b.mobType);
                if (aPrio != bPrio)
                {
                    return -aPrio.CompareTo(bPrio);
                }
                // Prefer closer targets.
                return DistanceTo(a).CompareTo(DistanceTo(b));
            });
            return mobs[0];
        }
        return null;
    }

    bool HaveTarget()
    {
        return GetTarget() != null;
    }

    MOB GetTarget()
    {
        if (target != null)
        {
            if (!target.target.IsTargetable())
            {
                ClearTarget();
                return null;
            }
            // If target is out of range, drop.
            if (IsMinion() && DistanceTo(target.target) > acquisitionRange)
            {
                ClearTarget();
                return null;
            }
            return target.target;
        }
        return null;
    }
    void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.SetMaxHealth(MaxHealth());
            healthBar.SetCurrentHealth(currentHealth);
        }
    }

    public void TakeDamage(float damage, MOB source)
    {
        AdjustHealth(-damage, source);
        // FIXME: Should it still call for help even if dead?
        if (isAlive)
        {
            CallForHelp(source);
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
            isAlive = false;
            Destroy(gameObject);
        }
    }

    public void ClearTarget()
    {
        target = null;
    }

    public bool IsTargetable()
    {
        return isAlive;
    }

    public float DistanceTo(MOB mob)
    {
        return (mob.transform.position - transform.position).magnitude;
    }

    void ChaseCurrentTarget()
    {
        status = Status.Chasing;
        navMeshAgent.SetDestination(GetTarget().transform.position);
    }

    void CancelAutoAttack()
    {

    }

    public void SetPlayerTarget(MOB mob)
    {
        target = new AttackTarget(mob, MinionPriority.Highest);
        if (InAttackRange(GetTarget()))
        {
            StartAutoAttack();
        }
        else
        {
            ChaseCurrentTarget();
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
        Missile.FireMissile(missilePrefab, missileSpawn.position, this, GetTarget(), AttackDamage());
    }

    void CallForHelp(MOB source)
    {
        // Find all nearby minions and call for help to them.
        var mobs = NearbyMobs(callForHelpRange).Where(mob => mob.team == team).ToList();
        foreach (var mob in mobs)
        {
            mob.OnCallForHelp(this, source);
        }
    }

    MinionPriority PriorityFromCallForHelp(MOBType attackerType, MOBType targetType)
    {
        bool matches(MOBType expectedAttacker, MOBType expectedTarget)
        {
            return expectedAttacker == attackerType && expectedTarget == targetType;
        }

        if (matches(MOBType.Champ, MOBType.Champ))
        {
            return MinionPriority.ChampAttacksChamp;
        }
        else if (matches(MOBType.Minion, MOBType.Champ))
        {
            return MinionPriority.MinionAttacksChamp;
        }
        else if (matches(MOBType.Minion, MOBType.Minion))
        {
            return MinionPriority.MinionAttacksMinion;
        }
        else if (matches(MOBType.Turret, MOBType.Minion))
        {
            return MinionPriority.TurretAttacksMinion;
        }
        else if (matches(MOBType.Champ, MOBType.Minion))
        {
            return MinionPriority.ChampAttacksMinion;
        }
        // Assert not reached?
        return MinionPriority.Lowest;
    }

    void OnCallForHelp(MOB victim, MOB attacker)
    {
        if (DistanceTo(attacker) <= acquisitionRange)
        {
            UpdateMinionAttackTarget(attacker, PriorityFromCallForHelp(attacker.mobType, victim.mobType));
        }
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

    void MinionBehaviorSweep()
    {
        //Follow any current specialized behavior rules, such as from CC(Taunts, Flees, Fears)
        //Continue attacking(or moving towards) their current target if that target is still valid.
        //If they have failed to attack their target for 4 seconds, they temporarily ignore them instead.
        //Find a new valid target in the minion’s acquisition range to attack.
        //If multiple valid targets, prioritize based on “how hard is it for me to path there ?”
        //Check if near a target waypoint, if so change the target waypoint to the next in the line.
        //Walk towards the target waypoint.If a minion can’t follow any of these behaviors, it will do nothing.Minions have a lot of checks to note whether or not a target is valid or not.There’s obvious ones like “which team is the target on” but also non - obvious ones like “where on the map is my target”. Many of these will be covered further down.

        timeUntilTargetUpdate -= Time.deltaTime;
        if (timeUntilTargetUpdate < 0)
        {
            timeUntilTargetUpdate = targetUpdateInterval;
            FindNewNearbyTarget();
            // FIXME: If in the middle of an action, don't do anything.
        }
    }

    void FindNewNearbyTarget()
    {
        var target = FindBestTargetInAcquisitionRange();
        if (target != null)
        {
            UpdateMinionAttackTarget(target, AggroPriorityByMobType(target.mobType));
        }
    }

    void UpdateMinionAttackTarget(MOB mob, MinionPriority priority)
    {
        if (target == null || target.priority < priority)
        {
            target = new AttackTarget(mob, priority);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsMinion() && statusBar != null)
        {
            statusBar.SetText(DebugStatusString());
        }

        if (IsMinion())
        {
            MinionBehaviorSweep();
        }
        if (IsChamp() && !HaveTarget())
        {
            FindNewNearbyTarget();
        }

        if (status == Status.ExplicitWalk)
        {
            if (ReachedDestination())
            {
                status = Status.Idle;
            }
        }
        else if (status == Status.Attacking)
        {
            if (HaveTarget() && InAttackRange(GetTarget()))
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
            if (HaveTarget())
            {
                if (InAttackRange(GetTarget()))
                {
                    StartAutoAttack();
                }
                else
                {
                    // Update our target location.
                    ChaseCurrentTarget();
                }
            }
            else
            {
                status = Status.Idle;
            }
        }
        else if (status == Status.Idle)
        {
            if (HaveTarget())
            {
                if (InAttackRange(GetTarget()))
                {
                    StartAutoAttack();
                }
                else
                {
                    if (IsMinion())
                    {
                        // If we have a target start chasing it.
                        ChaseCurrentTarget();
                    }
                }
            }
            else
            {
                if (waypoints != null)
                {
                    // Move towards the next waypoint.
                    if (Vector3.Distance(transform.position, currentWaypoint.position) < waypointDistanceThreshold)
                    {
                        currentWaypoint = waypoints.GetNextWaypoint(currentWaypoint, team == Team.red);
                        // Mark that we're done with waypoints (so we don't loop).
                        if (currentWaypoint == null)
                        {
                            waypoints = null;
                        }
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