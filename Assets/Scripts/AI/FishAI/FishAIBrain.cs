using BasicTools.ButtonInspector;
using UnityEngine;
using UnityEngine.AI;

public class FishAIBrain : MonoBehaviour
{
    public enum State {
        Idle,
        Roaming,
        Chasing,
        Attacking
    }

    [Header("Roam")]
    public RangeFloat roamRange;
    public RangeFloat waitTimeRangeAtRoamTarget;

    [SerializeField]
    private bool isWaitingAtRoamTarget = false;


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

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
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
        }
    }

    void OnStateEnter(State state, State prevState)
    {
        switch (state)
        {
            case State.Roaming:
                PickNewRoamDestination();
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
        }
    }

    // Roam

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
        }
        isWaitingAtRoamTarget = false;
    }

    // Chase

    void PerformChase()
    {
        if (PlayerModel.Instance)
        {
            agent.SetDestination(PlayerModel.Instance.transform.position);
        }
    }

    // Helpers

}
