using BasicTools.ButtonInspector;
using UnityEngine;
using UnityEngine.AI;

public class FishAIBrain : MonoBehaviour
{
    public enum State {
        Idle,
        Roaming,
        Chasing,
        Attacking,
        Fleeing,
    }

    [Header("Roaming")]
    public RangeFloat roamRange;
    public RangeFloat waitTimeRangeAtRoamTarget;
    [SerializeField]
    private bool isWaitingAtRoamTarget = false;

    [Header("Fleeing")]
    public RangeFloat fleeDistanceRange;
    public float fleeAngleMaxDegrees = 30f;
    public float fleeSpeedMultiplier = 2f;

    [Header("Alerting")]
    public State alertSwitchState = State.Fleeing;

    [Header("DEBUG")]

    [SerializeField]
    private State currentState = State.Idle;

    [SerializeField]
    private State switchStateTarget = State.Idle;

    [Button("SwitchState", "BtnExecute_SwitchState")]
    [SerializeField]
    private bool btnSwitchState;


    private NavMeshAgent agent;
    private Vector3 roamTargetLocation;

    private float originalSpeed;
    private float originalAngularSpeed;
    private float originalAcceleration;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        originalSpeed = agent.speed;
        originalAngularSpeed = agent.angularSpeed;
        originalAcceleration = agent.acceleration;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SwitchState(State.Roaming);
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
        SwitchState(alertSwitchState);
    }

    public void BtnExecute_SwitchState()
    {
        currentState = switchStateTarget;
    }

    void OnStateExit(State state, State newState)
    {
        switch (state)
        {
            case State.Roaming:
                isWaitingAtRoamTarget = false;
                break;
            case State.Fleeing:
                agent.speed = originalSpeed;
                agent.angularSpeed = originalAngularSpeed;
                agent.acceleration = originalAcceleration;
                break;
        }
    }

    void OnStateEnter(State state, State prevState)
    {
        switch (state)
        {
            case State.Roaming:
                PickNewRoamDestination();
                break;
            case State.Fleeing:
                agent.speed = originalSpeed * fleeSpeedMultiplier;
                agent.angularSpeed = originalAngularSpeed * fleeSpeedMultiplier;
                agent.acceleration = originalAcceleration * fleeSpeedMultiplier;
                PickNewFleeDestination();
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
            case State.Attacking:
                break;
            case State.Fleeing:
                HandleFleeingUpdate();
                break;
        }
    }

    // Roaming

    void PerformRoam()
    {
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
        if (PlayerModel.Instance)
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
            SwitchState(State.Roaming);
        }
    }

    // Helpers

}
