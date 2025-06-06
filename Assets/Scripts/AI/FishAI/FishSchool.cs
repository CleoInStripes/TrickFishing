using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FishSchool : MonoBehaviour
{
    public List<FishSpawnGroup> fishSpawnGroups;
    public RangeFloat spawnRadiusRange = new(0f, 15f);

    [HideInInspector]
    public List<FishAIModel> fishes;

    [Header("Roaming")]
    [HideInInspector] public Transform roamFocalPoint;
    public RangeFloat roamRange;
    public RangeFloat circleRoamRange;
    public RangeFloat waitTimeRangeAtRoamTarget;

    [SerializeField]
    private bool isWaitingAtRoamTarget = false;

    private bool stayAtFocalPoint;

    private NavMeshAgent agent;
    private Vector3 roamTargetLocation;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void Initialize()
    {
        PickNewRoamDestination();

        foreach (var spawnGroup in fishSpawnGroups)
        {
            spawnGroup.Prepare();
        }

        SpawnFishes();
    }

    // Update is called once per frame
    void Update()
    {
        if (stayAtFocalPoint)
        {
            PickNewRoamDestination();
        }
        else
        {
            PerformRoam();
        }
    }

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
        var focalTransform = roamFocalPoint != null ? roamFocalPoint : transform;
        if (FishAIManager.Instance.TryGetRandomNavMeshLocation(focalTransform.position, roamRange, out Vector3 target))
        {
            agent.SetDestination(target);
        }
        else
        {
            Debug.LogError("Could not find a roaming position on the Nav Mesh");
        }
        isWaitingAtRoamTarget = false;
    }
    
    void SpawnFishes()
    {
        foreach (var spawnGroup in fishSpawnGroups)
        {
            var spawnCount = spawnGroup.countRange.GetRandom();
            for (int i = 0; i < spawnCount; i++)
            {
                Vector3 spawnPosition;
                if (FishAIManager.Instance.TryGetRandomNavMeshLocation(transform.position, spawnRadiusRange, out spawnPosition))
                {
                    var spawnRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                    var fish = FishAIManager.Instance.SpawnFish(spawnGroup.prefabs.GetRandomItem().gameObject, spawnPosition, spawnRotation);
                    fish.aiBrain.chainFleeWithAttack = true;
                    fish.aiBrain.followTarget = transform;
                    fish.aiBrain.SwitchState(FishAIBrain.State.Following);
                    fish.OnDeath.AddListener(() =>
                    {
                        fishes.Remove(fish);
                    });
                    fishes.Add(fish);
                }
                else
                {
                    Debug.LogError("Could not find a random position on the Nav Mesh");
                }
            }
        }
    }

    public void CircleFocalPoint()
    {
        stayAtFocalPoint = true;
        roamRange = circleRoamRange;
    }

    private void OnDestroy()
    {
        // Clean up fishes
        foreach (var fish in fishes)
        {
            fish.aiBrain.followTarget = null;
            fish.aiBrain.SwitchState(fish.aiBrain.alertSwitchState);
        }
    }
}
