using BasicTools.ButtonInspector;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class FishAIBrain : MonoBehaviour
{
    public enum State {
        Idle,
        Roaming,
        Chasing,
        Attacking,  // Obsolete
        Fleeing,
        Following,
    }

    public static readonly List<State> nonAlertableStates = new()
    {
        State.Chasing,
        State.Attacking,
    };

    [Header("Roaming")]
    public RangeFloat roamRange;
    public RangeFloat waitTimeRangeAtRoamTarget;
    [SerializeField]
    private bool isWaitingAtRoamTarget = false;

    [Header("Fleeing")]
    public RangeFloat fleeDistanceRange;
    public float fleeAngleMaxDegrees = 30f;
    public float fleeSpeedMultiplier = 2f;
    public bool chainFleeWithAttack = false;
    public float chainFleeWithAttackProbability = 0.3f;

    [Header("Chasing")]
    public float chaseSpeedMultiplier = 1.5f;
    public float chaseTurnSpeed = 5f;

    [Header("Following")]
    public float followStopDistance = 1f;
    public float followSpeedMultiplier = 2f;
    [HideInInspector] public Transform followTarget;

    [Header("Attacking")]
    public float attackDistance = 5f;
    public float attackDamage = 5f;
    public RangeFloat attackIntervalRange = new(3, 5);
    public bool isRangedAttack = false;
    public bool canRangeAttackWhileChasing = true;
    public FishProjectile projectilePrefab;
    public RangeFloat projectileTargetOffsetRange = new(-2, 2);

    [Header("Alerting")]
    public float alertChaseProbability = 0f;
    public RangeFloat alertChaseProbabilityIncreaseRange = new(0.3f, 0.5f);

    [Header("Misc")]
    [SerializeField] float viewAngle = 120f; // in degrees
    [SerializeField] float viewDistance = 200f; // optional clamp for performance
    [SerializeField] LayerMask visibilityLayers; // exclude obstacles like walls

    [Header("DEBUG")]

    [SerializeField]
    private State currentState = State.Idle;
    public State CurrentState { get { return currentState; } }

    [SerializeField]
    private State switchStateTarget = State.Idle;

    [Button("SwitchState", "BtnExecute_SwitchState")]
    [SerializeField]
    private bool btnSwitchState;

    public UnityEvent OnAlert;

    private FishAIModel fishModel;
    private NavMeshAgent agent;
    private Vector3 roamTargetLocation;

    private float originalSpeed;
    private float originalAngularSpeed;
    private float originalAcceleration;
    private float originalStopDistance;

    private bool canAttack = true;
    private bool chaseOnSight => alertChaseProbability >= 1f;

    private void Awake()
    {
        fishModel = GetComponent<FishAIModel>();
        agent = GetComponent<NavMeshAgent>();
        originalSpeed = agent.speed;
        originalAngularSpeed = agent.angularSpeed;
        originalAcceleration = agent.acceleration;
        originalStopDistance = agent.stoppingDistance;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (currentState == State.Idle)
        {
            SwitchState(State.Roaming);
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleCurrentStateUpdate();
    }

    public void SwitchState(State newState)
    {
        var prevState = currentState;
        currentState = newState;

        OnStateExit(prevState, currentState);
        OnStateEnter(currentState, prevState);
    }

    public void Alert()
    {
        this.OnAlert.Invoke();

        // If we are chasing/attacking, we don't care about alerts - we just wanna go for the kill
        if (!nonAlertableStates.Contains(currentState))
        {
            if (PlayerModel.Instance.health.IsAlive)
            {
                var alertSwitchState = UnityEngine.Random.Range(0, 1) < alertChaseProbability ? State.Chasing : State.Fleeing;
                SwitchState(alertSwitchState);
            }
        }
    }

    public void BtnExecute_SwitchState()
    {
        currentState = switchStateTarget;
    }

    void OnStateExit(State state, State newState)
    {
        if (!agent)
        {
            return;
        }

        switch (state)
        {
            case State.Roaming:
                isWaitingAtRoamTarget = false;
                break;
            case State.Chasing:
                ApplySpeedMultiplier(1f);
                agent.updateRotation = true;
                break;
            case State.Fleeing:
                ApplySpeedMultiplier(1f);
                break;
            case State.Following:
                agent.stoppingDistance = originalStopDistance;
                ApplySpeedMultiplier(1f);
                break;
        }
    }

    void OnStateEnter(State state, State prevState)
    {
        if (!agent)
        {
            return;
        }

        switch (state)
        {
            case State.Roaming:
                PickNewRoamDestination();
                break;
            case State.Chasing:
                //alertChaseProbability = 1;
                ApplySpeedMultiplier(chaseSpeedMultiplier);
                agent.updateRotation = false;
                agent.SetDestination(PlayerModel.Instance.transform.position);
                break;
            case State.Fleeing:
                ApplySpeedMultiplier(fleeSpeedMultiplier);
                PickNewFleeDestination();
                break;
            case State.Following:
                agent.stoppingDistance = followStopDistance;
                ApplySpeedMultiplier(followSpeedMultiplier);
                break;
        }
    }

    void HandleCurrentStateUpdate()
    {
        if (currentState != State.Chasing && chaseOnSight && CanSeePlayer())
        {
            SwitchState(State.Chasing);
            return;
        }

        switch(currentState)
        {
            case State.Idle:
                break;
            case State.Roaming:
                PerformRoam();
                break;
            case State.Chasing:
                PerformChase();
                break;
            //case State.Attacking:
                //PerformAttack();
                //break;
            case State.Fleeing:
                HandleFleeingUpdate();
                break;
            case State.Following:
                PerformFollow();
                break;
        }
    }

    // Roaming

    void PerformRoam()
    {
        if (followTarget != null)
        {
            SwitchState(State.Following);
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !isWaitingAtRoamTarget)
        {
            isWaitingAtRoamTarget = true;
            Invoke(nameof(PickNewRoamDestination), waitTimeRangeAtRoamTarget.GetRandom());
        }
    }

    void PickNewRoamDestination()
    {
        if (currentState == State.Roaming)
        {
            if (FishAIManager.Instance.TryGetRandomNavMeshLocation(transform.position, roamRange, out Vector3 target))
            {
                agent.SetDestination(target);
            }
            else
            {
                Debug.LogError("Could not find a roaming position on the Nav Mesh");
            }
        }
        isWaitingAtRoamTarget = false;
    }

    // Chasing

    void PerformChase()
    {
        if (!PlayerModel.Instance.health.IsAlive)
        {
            SwitchState(State.Roaming);
            return;
        }

        if (!CanSeePlayer())
        {
            RotateByVelocity();
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                SwitchState(State.Roaming);
            }
            return;
        }

        if (agent.pathPending)
        {
            RotateByVelocity();
            return;
        }

        var distanceToPlayer = Vector3.Distance(PlayerModel.Instance.transform.position, transform.position);
        if (distanceToPlayer <= attackDistance)
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            RotateTowardsPlayer();

            if (canAttack)
            {
                PerformAttack();
            }
        } 
        else
        {
            RotateByVelocity();
            agent.SetDestination(PlayerModel.Instance.transform.position);

            if (isRangedAttack && canRangeAttackWhileChasing && canAttack)
            {
                PerformAttack();
            }
        }
    }

    // Fleeing

    void PickNewFleeDestination()
    {
        if (currentState == State.Fleeing)
        {
            Vector3 target;
            var fleeDir = (transform.position - PlayerModel.Instance.transform.position).normalized;
            if (FishAIManager.Instance.TryGetRandomNavMeshLocationInDirection(transform.position, fleeDir, fleeAngleMaxDegrees, fleeDistanceRange, out target))
            {
                agent.SetDestination(target);
            }
            else if (FishAIManager.Instance.TryGetRandomNavMeshLocation(transform.position, fleeDistanceRange, out target))
            {
                agent.SetDestination(target);
            }
            else if (FishAIManager.Instance.TryGetRandomNavMeshLocation(transform.position, new RangeFloat(fleeDistanceRange.min / 2, fleeDistanceRange.max), out target))
            {
                agent.SetDestination(target);
            }
            else
            {
                Debug.LogError("Could not find a fleeing position on the Nav Mesh");
            }
        }
    }

    void HandleFleeingUpdate()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            alertChaseProbability += alertChaseProbabilityIncreaseRange.GetRandom();
            if (chainFleeWithAttack && Random.value <= chainFleeWithAttackProbability)
            {
                SwitchState(State.Chasing);
            } 
            else
            {
                SwitchState(State.Roaming);
            }
        }
    }

    // Following

    void PerformFollow()
    {
        if (followTarget)
        {
            agent.SetDestination(followTarget.position);
        }
    }

    // Helpers
    void ApplySpeedMultiplier(float speedMultiplier)
    {
        agent.speed = originalSpeed * speedMultiplier;
        agent.angularSpeed = originalAngularSpeed * speedMultiplier;
        agent.acceleration = originalAcceleration * speedMultiplier;
    }

    void RotateTowardsPlayer()
    {
        RotateTowards(PlayerModel.Instance.transform.position);
    }

    void RotateByVelocity()
    {
        if (agent.velocity.magnitude > 0)
        {
            RotateTowards(transform.position + (agent.velocity * 10));
        }
    }

    void RotateTowards(Vector3 target)
    {
        Vector3 direction = target - transform.position;
        direction.y = 0; // Optional: prevent tilting up/down

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, chaseTurnSpeed * Time.deltaTime);
        }
    }

    bool CanSeePlayer()
    {
        Vector3 origin = transform.position + Vector3.up * (agent.height / 2); // Adjust for eye height
        Vector3 target = PlayerCam.Instance.transform.position;
        Vector3 direction = target - origin;

        // Check if player is within view angle
        //float angleToPlayer = Vector3.Angle(transform.forward, direction);
        //if (angleToPlayer > viewAngle / 2f)
        //    return false;

        // Perform a linecast to detect obstruction
        if (Physics.Linecast(origin, target, out RaycastHit hit, visibilityLayers))
        {
            var playerModel = hit.transform.GetComponentInParent<PlayerModel>();
            // Only switch if we directly hit the player
            return playerModel != null;
        }

        return false;
    }

    void PerformAttack()
    {
        if (isRangedAttack)
        {
            // Ranged

            var playerVelocity = PlayerModel.Instance.rb.linearVelocity;
            var target = PlayerCam.Instance.cam.transform.position;

            Vector3? interceptPoint = HelperUtilities.FirstOrderIntercept(
                shooterPos: fishModel.projectileSpawnPoint.position,
                targetPos: PlayerCam.Instance.cam.transform.position,
                targetVelocity: playerVelocity,
                projectileSpeed: projectilePrefab.speed);

            Vector3 targetPoint = target;
            if (interceptPoint.HasValue)
            {
                targetPoint = interceptPoint.Value;
            }
            else
            {
                Debug.Log("Target can't be intercepted at this speed.");
            }

            var targetOffset = Vector3.zero;
            if (playerVelocity.magnitude > 1f)
            {
                targetOffset += Random.onUnitSphere * projectileTargetOffsetRange.GetRandom();
            }

            var adjustedTarget = targetPoint + targetOffset;
            var dirToAdjTarget = adjustedTarget - fishModel.projectileSpawnPoint.position;
            var projectile = Instantiate(projectilePrefab, fishModel.projectileSpawnPoint.position, Quaternion.LookRotation(dirToAdjTarget));
            projectile.damage = attackDamage;
        }
        else
        {
            // Melee
            fishModel.animator.SetTrigger("MeleeAttack");
            PlayerModel.Instance.health.TakeDamage(attackDamage);
        }

        canAttack = false;

        Invoke(nameof(ResetCanAttack), attackIntervalRange.GetRandom());
    }

    void ResetCanAttack()
    {
        canAttack = true;
    }
}
