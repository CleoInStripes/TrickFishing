using BasicTools.ButtonInspector;
using UnityEngine;
using UnityEngine.AI;

public class FishAIBrain : MonoBehaviour
{
    public enum State {
        Idle,
        Chasing,
        Attacking
    }

    [SerializeField]
    private State currentState = State.Chasing;
    
    private NavMeshAgent navMeshAgent;

    [Header("DEBUG")]

    [SerializeField]
    private State switchStateTarget = State.Idle;

    [Button("SwitchState", "BtnExecute_SwitchState")]
    [SerializeField]
    private bool btnSwitchState;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        HandleCurrentStateUpdate();
    }

    public void SwitchState(State newState)
    {
        currentState = newState;
    }
    public void BtnExecute_SwitchState()
    {
        currentState = switchStateTarget;
    }

    void HandleCurrentStateUpdate()
    {
        switch(currentState)
        {
            case State.Idle:
                break;
            case State.Chasing:
                if (FishAITarget.Instance)
                {
                    navMeshAgent.SetDestination(FishAITarget.Instance.transform.position);
                }
                break;
            case State.Attacking:
                break;
        }
    }
}
