using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class FishAIManager : SingletonMonoBehaviour<FishAIManager>
{
    [Header("Fish Prefabs")]
    public List<FishSpawnGroup> initialFishSpawnGroups;

    [Header("Initial Spawning")]
    public bool enableInitialSpawning = true;
    public RangeFloat initialSpawnRadiusRange = new(0f, 15f);

    [Header("Respawning")]
    public RangeFloat respawnRadiusRange = new(40f, 70f);

    [Header("Targeted Spawning")]
    public RangeFloat targetedSpawnRadiusRange = new(5f, 15f);

    [Header("Alerting")]
    public bool enableAlerting = true;
    public RangeFloat alertRadiusRange = new(10f, 12f);

    [Header("Misc")]
    public float spawnSampleRadius = 2f;
    public int maxNavMeshSampleAttempts = 10;
    public Transform overrideInitialFishSpawnSpotsHolder;


    private List<Transform> initialFishSpawnSpots = new();
    [HideInInspector]
    public List<FishAIModel> spawnedFishes = new ();

    private new void Awake()
    {
        base.Awake();

        Transform spawnSpotsHolder = overrideInitialFishSpawnSpotsHolder ? overrideInitialFishSpawnSpotsHolder : LevelManager.Instance.initialFishSpawnSpotsHolder;
        initialFishSpawnSpots = spawnSpotsHolder.GetComponentsInChildren<Transform>().ToList();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (var spawnGroup in initialFishSpawnGroups)
        {
            spawnGroup.Prepare();
        }

        if (enableInitialSpawning)
        {
            SpawnFishesInInitialSpots();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    Transform GetPlayerTransform()
    {
        return PlayerModel.Instance.transform;
    }

    void SpawnFishesInInitialSpots()
    {
        foreach (var spawnSpot in initialFishSpawnSpots)
        {
            foreach (var spawnGroup in initialFishSpawnGroups)
            {
                var spawnCount = spawnGroup.countRange.GetRandom();
                for (int i = 0; i < spawnCount; i++)
                {
                    Vector3 spawnPosition;
                    if (TryGetRandomNavMeshLocation(spawnSpot.position, initialSpawnRadiusRange, out spawnPosition))
                    {
                        var spawnRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                        var fish = SpawnFish(spawnGroup.prefabs.GetRandomItem().gameObject, spawnPosition, spawnRotation, false);
                        fish.OnDeath.AddListener(() =>
                        {
                            RespawnFishAwayFromPlayer(fish);
                        });
                    }
                    else
                    {
                        Debug.LogError("Could not find a random position on the Nav Mesh");
                    }
                }
            }
        }
    }
    
    void RespawnFishAwayFromPlayer(FishAIModel fish)
    {
        var targetTransform = GetPlayerTransform();
        Vector3 respawnPosition;
        if (TryGetRandomNavMeshLocation(targetTransform.position, respawnRadiusRange, out respawnPosition))
        {
            var spawnRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            // Teleport the agent
            fish.agent.ResetPath();
            fish.agent.Warp(respawnPosition);

            fish.health.ResetHealth();
            fish.aiBrain.SwitchState(FishAIBrain.State.Roaming);
        }
        else
        {
            Debug.LogError("Could not find a respawn position on the Nav Mesh. Attempting wider search...");
            if (TryGetRandomNavMeshLocation(targetTransform.position, new RangeFloat(0, respawnRadiusRange.max * 2), out respawnPosition))
            {
                var spawnRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

                // Teleport the agent
                fish.agent.ResetPath();
                fish.agent.Warp(respawnPosition);

                fish.health.ResetHealth();
                fish.aiBrain.SwitchState(FishAIBrain.State.Roaming);
            }
            else
            {
                Debug.LogError("Failed to find a respawn position on the Nav Mesh. Destroying Fish...");
                Destroy(fish.gameObject);
                spawnedFishes.Remove(fish);
            }
        }
    }

    void SpawnNewFishAwayFromPlayer(GameObject prefab)
    {
        var targetTransform = GetPlayerTransform();
        Vector3 spawnPosition;
        if (TryGetRandomNavMeshLocation(targetTransform.position, targetedSpawnRadiusRange, out spawnPosition))
        {
            var spawnRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            SpawnFish(prefab, spawnPosition, spawnRotation);
        } 
        else
        {
            Debug.LogError("Could not find a random position on the Nav Mesh");
        }
    }

    public FishAIModel SpawnFish(GameObject prefab, Vector3 spawnPosition, Quaternion spawnRotation, bool destroyOnDeath = true)
    {
        var fish = Instantiate(prefab, spawnPosition, spawnRotation);
        var fishModel = fish.GetComponent<FishAIModel>();
        fishModel.destroyOnDeath = destroyOnDeath;
        fishModel.health.OnDamageTaken.AddListener(() =>
        {
            if (enableAlerting)
            {
                AlertFishesAround(fish.transform.position, alertRadiusRange.GetRandom());
            }
        });

        spawnedFishes.Add(fishModel);
        return fishModel;
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

    public bool TryGetRandomNavMeshLocationInDirection(Vector3 center, Vector3 direction, float maxAngleDegrees, RangeFloat range, out Vector3 result)
    {
        direction.y = 0;
        direction.Normalize();
        maxAngleDegrees = Mathf.Clamp(maxAngleDegrees, 0, 180);

        for (int i = 0; i < maxNavMeshSampleAttempts; i++)
        {
            // Pick a random angle within the cone
            float angleOffset = Random.Range(-maxAngleDegrees / 2f, maxAngleDegrees / 2f);
            Quaternion rotation = Quaternion.Euler(0, angleOffset, 0);
            Vector3 rotatedDirection = rotation * direction;

            float distance = Random.Range(range.min, range.max);
            Vector3 offset = rotatedDirection * distance;
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
            if (fish.IsDestroyed()) continue;

            if (Vector3.Distance(fish.transform.position, center) <= radius)
            {
                fish.aiBrain.OnAlerted();
            }
        }
    }
}
