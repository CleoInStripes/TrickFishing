using UnityEngine;

public class CapturePointSystem : SingletonMonoBehaviour<CapturePointSystem>
{
    public Transform capturePointsHolder;
    public CapturePoint capturePointPrefab;

    private Randomizer<Transform> capturePointsRandomizer;
    private CapturePoint capturePoint;

    private new void Awake()
    {
        base.Awake();
        capturePointsRandomizer = new(capturePointsHolder.GetComponentsInChildren<Transform>());
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpawnCapturePoint();

        capturePoint.PreActivate();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SpawnCapturePoint()
    {
        var spawnPoint = capturePointsRandomizer.GetRandomItem();
        capturePoint = Instantiate(capturePointPrefab, spawnPoint.position, spawnPoint.rotation);
    }

    public void MoveToNewLocation()
    {
        var spawnPoint = capturePointsRandomizer.GetRandomItem();
        capturePoint.transform.position = spawnPoint.position;
        capturePoint.transform.rotation = spawnPoint.rotation;

        capturePoint.PreActivate();
    }
}
