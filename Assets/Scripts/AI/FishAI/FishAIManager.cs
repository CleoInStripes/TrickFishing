using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.FilePathAttribute;
using Random = UnityEngine.Random;

public class FishAIManager : SingletonMonoBehaviour<FishAIManager>
{
    public GameObject fishAIPrefab;

    [Header("Initial Spawning")]
    public RangeInt initialSpawnCountRangePerSpot = new(2, 5);
    public RangeFloat initialSpawnRadiusRange = new(0f, 15f);

    [Header("Targeted Spawning")]
    public RangeFloat targetedSpawnRadiusRange = new(5f, 15f);

    [Header("Alerting")]
    public bool enableAlerting = true;
    public RangeFloat alertRadiusRange = new(10f, 12f);

    [Header("Misc")]
    public float spawnSampleRadius = 2f;
    public int maxNavMeshSampleAttempts = 10;


    private List<Transform> initialFishSpawnSpots = new();
    private List<FishAIModel> spawnedFishes = new ();

    private new void Awake()
    {
        base.Awake();
        initialFishSpawnSpots = LevelManager.Instance.initialFishSpawnSpotsHolder.GetComponentsInChildren<Transform>().ToList();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpawnFishesInInitialSpots();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    Transform GetTargetTransform()
    {
        return PlayerModel.Instance.transform;
    }

    void SpawnFishesInInitialSpots()
    {
        foreach (var spawnSpot in initialFishSpawnSpots)
        {
            var spawnCount = initialSpawnCountRangePerSpot.GetRandom();
            for (int i = 0; i < spawnCount; i++)
            {
                var targetTransform = GetTargetTransform();
                Vector3 spawnPosition;
                if (TryGetRandomNavMeshLocation(spawnSpot.position, initialSpawnRadiusRange, out spawnPosition))
                {
                    var spawnRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                    SpawnFish(spawnPosition, spawnRotation);
                }
                else
                {
                    Debug.LogError("Could not find a random position on the Nav Mesh");
                }
            }
        }
    }

    void SpawnNewFishAwayFromTarget()
    {
        var targetTransform = GetTargetTransform();
        Vector3 spawnPosition;
        if (TryGetRandomNavMeshLocation(targetTransform.position, targetedSpawnRadiusRange, out spawnPosition))
        {
            var dirToTarget = targetTransform.position - spawnPosition;
            var spawnRotation = Quaternion.LookRotation(dirToTarget, Vector3.up);
            SpawnFish(spawnPosition, spawnRotation);
        } 
        else
        {
            Debug.LogError("Could not find a random position on the Nav Mesh");
        }
    }

    void SpawnFish(Vector3 spawnPosition, Quaternion spawnRotation)
    {
        var fish = Instantiate(fishAIPrefab, spawnPosition, spawnRotation);
        var fishModel = fish.GetComponent<FishAIModel>();
        fishModel.health.OnDamageTaken.AddListener(() =>
        {
            if (enableAlerting)
            {
                AlertFishesAround(fish.transform.position, alertRadiusRange.GetRandom());
            }
        });
        fishModel.health.OnHealthDepleted.AddListener(() => spawnedFishes.Remove(fishModel));
        spawnedFishes.Add(fishModel);
    }

    public bool TryGetRandomNavMeshLocation(Vector3 center, RangeFloat range, out Vector3 result)
    {
        for (int i = 0; i < maxNavMeshSampleAttempts; i++)
        {
            // Get random direction on the XZ plane
            Vector2 randomCircle = Random.insideUnitCircle.normalized;
            float distance = Random.Range(range.min, range.max);
            Vector3 offset = new Vector3(randomCircle.x, 0f, randomCircle.y) * distance;

            Vector3 potentialPosition = center + offset;

            if (NavMesh.SamplePosition(potentialPosition, out NavMeshHit hit, spawnSampleRadius, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }

        result = Vector3.zero;
        return false;
    }

    void AlertFishesAround(Vector3 center, float radius)
    {
        foreach (var fish in spawnedFishes)
        {
            if (Vector3.Distance(fish.transform.position, center) <= radius)
            {
                fish.aiBrain.SwitchState(FishAIBrain.State.Chasing);
            }
        }
    }
}
