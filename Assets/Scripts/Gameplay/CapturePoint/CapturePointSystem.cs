using UnityEngine;

public class CapturePointSystem : SingletonMonoBehaviour<CapturePointSystem>
{
    public bool capturePointsEnabled = true;
    public Transform capturePointLocation;
    public CapturePoint capturePointPrefab;

    [Header("Wave")]
    [HideInInspector] public int currentWaveIndex = 0;
    public float initialWaveCountdownTime = 15;
    public float newWaveCountdownTime = 5;
    [HideInInspector] public float timeToStartNextWave = 0;
    public bool isCountingDown => timeToStartNextWave > 0;
    public bool isWaveActive => capturePoint && capturePoint.isActive;

    [HideInInspector] public CapturePoint capturePoint;

    private new void Awake()
    {
        base.Awake();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!capturePointsEnabled)
        {
            return;
        }

        SpawnCapturePoint();
        StartInitialWave();
    }

    // Update is called once per frame
    void Update()
    {
        if (timeToStartNextWave > 0)
        {
            timeToStartNextWave -= Time.deltaTime;
            if (timeToStartNextWave <= 0)
            {
                timeToStartNextWave = 0;
                capturePoint.DropTreasure();
            }
        }
    }

    void StartInitialWave()
    {
        currentWaveIndex = 0;
        timeToStartNextWave = initialWaveCountdownTime;
    }

    public void StartNewWave()
    {
        currentWaveIndex++;
        timeToStartNextWave = newWaveCountdownTime;
    }

    void SpawnCapturePoint()
    {
        capturePoint = Instantiate(capturePointPrefab, capturePointLocation.position, capturePointLocation.rotation);
    }

    public void OnCapturePointDeactivated()
    {
        StartNewWave();
    }
}
