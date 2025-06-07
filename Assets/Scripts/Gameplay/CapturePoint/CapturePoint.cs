using BasicTools.ButtonInspector;
using UnityEngine;

public class CapturePoint : SingletonMonoBehaviour<CapturePoint>
{
    public GameObject beam;
    public FishSchool fishSchoolPrefab;
    public float activationProximity = 50f;
    public SimpleTimer timer;
    public int captureScore = 500;

    [Button("Pre-Activate", "BtnExecute_PreActivate")]
    [SerializeField]
    private bool btnPreActivate;
    [Button("Activate", "BtnExecute_Activate")]
    [SerializeField]
    private bool btnActivate;

    public bool isActive => beam.activeSelf;

    private FishSchool currentFishSchool;

    private new void Awake()
    {
        base.Awake();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (currentFishSchool && !isActive)
        {
            CheckPlayerAndActivate();
        }

        if (isActive & !timer.expired)
        {
            timer.Update();
            if (timer.expired)
            {
                OutOfTime();
            }
        }
    }

    void CheckPlayerAndActivate()
    {
        var distance = Vector3.Distance(PlayerModel.Instance.transform.position, transform.position);
        if (distance <= activationProximity)
        {
            Activate();
        }
    }

    public void PreActivate()
    {
        Vector3 spawnPosition;
        if (FishAIManager.Instance.TryGetRandomNavMeshLocation(transform.position, fishSchoolPrefab.roamRange, out spawnPosition))
        {
            var spawnRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            currentFishSchool = Instantiate(fishSchoolPrefab, spawnPosition, spawnRotation);
            currentFishSchool.roamFocalPoint = transform;
            currentFishSchool.OnAlert.AddListener(() =>
            {
                if (currentFishSchool)
                {
                    Activate();
                }
            });
            currentFishSchool.OnAllFishesDead.AddListener(() =>
            {
                OnSuccessfulCapture();
            });
            currentFishSchool.Initialize();
        }
        else
        {
            Debug.LogError("Could not find a random position for Fish School on the Nav Mesh");
        }
    }

    void OnSuccessfulCapture()
    {
        PlayerModel.Instance.AddScore(captureScore);
        Deactivate();
    }

    void OutOfTime()
    {
        Deactivate();
    }

    public void Activate()
    {
        if (isActive)
        {
            return;
        }

        beam.SetActive(true);
        currentFishSchool.CircleFocalPoint();
        timer.Reset();
    }

    public void Deactivate()
    {
        if (!isActive)
        {
            return;
        }

        beam.SetActive(false);
        Destroy(currentFishSchool);
        currentFishSchool = null;

        CapturePointSystem.Instance.MoveToNewLocation();
    }

    public void BtnExecute_PreActivate()
    {
        PreActivate();
    }
    public void BtnExecute_Activate()
    {
        if (currentFishSchool)
        {
            Activate();
        }
    }
}
