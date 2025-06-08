using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class FishSchool : MonoBehaviour
{
    public bool initializeOnStart = false;
    public bool infiniteMode = false;
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

    public UnityEvent OnAlert;
    public UnityEvent OnAllFishesDead;

    private bool stayAtFocalPoint;

    private NavMeshAgent agent;
    private Vector3 roamTargetLocation;

    private bool hasAlerted = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} is not on the NavMesh!");
        }

        if (initializeOnStart)
        {
            Initialize();
        }
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
        var totalSpawned = 0;
        var totalAttempts = 0;
        while (totalSpawned == 0 && totalAttempts < 10)
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
                        fish.aiBrain.OnAlert.AddListener(() =>
                        {
                            if (!hasAlerted)
                            {
                                hasAlerted = true;
                                roamFocalPoint = null;
                                AlertAllFishes();
                                return;
                            }

                            if (fishes.Contains(fish))
                            {
                                OnAlert.Invoke();
                            }
                        });
                        fish.OnDeath.AddListener(() =>
                        {
                            fishes.Remove(fish);
                            if (fishes.Count == 0)
                            {
                                onLastFishDied();
                            }
                        });
                        fishes.Add(fish);

                        totalSpawned++;
                    }
                    else
                    {
                        Debug.LogError("Could not find a random position on the Nav Mesh");
                    }
                }
            }
            
            totalAttempts++;
        }
    }

    void onLastFishDied()
    {
        if (infiniteMode)
        {
            SpawnFishes();
        } 
        else 
        { 
            OnAllFishesDead.Invoke();
        }
    }

    public void CircleFocalPoint()
    {
        stayAtFocalPoint = true;
        roamRange = circleRoamRange;
    }

    void AlertAllFishes()
    {
        foreach (var fish in fishes)
        {
            fish.aiBrain.Alert();
        }
    }

    void RemoveFishFromSchool(FishAIModel fish, bool removeFromList = true)
    {
        fish.aiBrain.followTarget = null;
        if (fish.aiBrain.CurrentState == FishAIBrain.State.Following)
        {
            fish.aiBrain.SwitchState(FishAIBrain.State.Roaming);
            if (removeFromList)
            {
                fishes.Remove(fish);
            }
        }
    }

    private void OnDestroy()
    {
        // Clean up fishes
        foreach (var fish in fishes)
        {
            RemoveFishFromSchool(fish, false);
        }
        fishes.Clear();
    }
}
