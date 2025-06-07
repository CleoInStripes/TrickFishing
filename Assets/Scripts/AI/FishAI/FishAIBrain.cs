using BasicTools.ButtonInspector;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

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

    [Header("Alerting")]
    public State alertSwitchState = State.Fleeing;

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

    private NavMeshAgent agent;
    private Vector3 roamTargetLocation;

    private float originalSpeed;
    private float originalAngularSpeed;
    private float originalAcceleration;
    private float originalStopDistance;

    private bool canAttack = true;

    private void Awake()
    {
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

    public void OnAlerted()
    {
        this.OnAlert.Invoke();

        // If we are chasing/attacking, we don't care about alerts - we just wanna go for the kill
        if (!nonAlertableStates.Contains(currentState))
        {
            SwitchState(alertSwitchState);
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
        if (!CanSeePlayer())
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                SwitchState(State.Roaming);
            }
            return;
        }

        RotateTowardsPlayer();

        if (agent.pathPending)
        {
            return;
        }

        var distanceToPlayer = Vector3.Distance(PlayerModel.Instance.transform.position, transform.position);
        if (distanceToPlayer <= attackDistance)
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            if (canAttack)
            {
                PerformAttack();
            }
        } 
        else
        {
            agent.SetDestination(PlayerModel.Instance.transform.position);
        }
    }

    // Fleeing

    void PickNewFleeDestination()
    {
        if (currentState == State.Fleeing)
        {
            var fleeDir = (transform.position - PlayerModel.Instance.transform.position).normalized;
            if (FishAIManager.Instance.TryGetRandomNavMeshLocationInDirection(transform.position, fleeDir, fleeAngleMaxDegrees, fleeDistanceRange, out Vector3 target))
            {
                DebugExtension.DebugWireSphere(target);
                agent.SetDestination(target);
            }
        }
    }

    void HandleFleeingUpdate()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
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
        Vector3 direction = PlayerModel.Instance.transform.position - transform.position;
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
            //Debug.Log($"Launching projectile at player: Potential Damage {attackDamage}");
            // TODO: Launch a projectile
        }
        else
        {
            // Melee
            //Debug.Log($"Dealing damage to player: {attackDamage}");
            // TODO: Integrate with player once Player Health is ready
        }

        canAttack = false;

        Invoke(nameof(ResetCanAttack), attackIntervalRange.GetRandom());
    }

    void ResetCanAttack()
    {
        canAttack = true;
    }
}
