using BasicTools.ButtonInspector;
using UnityEngine;

public class CapturePoint : SingletonMonoBehaviour<CapturePoint>
{
    public GameObject beam;
    public FishSchool fishSchoolPrefab;

    [Button("Pre-Activate", "BtnExecute_PreActivate")]
    [SerializeField]
    private bool btnPreActivate;
    [Button("Activate", "BtnExecute_Activate")]
    [SerializeField]
    private bool btnActivate;


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
        
    }

    public void PreActivate()
    {
        Vector3 spawnPosition;
        if (FishAIManager.Instance.TryGetRandomNavMeshLocation(transform.position, fishSchoolPrefab.roamRange, out spawnPosition))
        {
            var spawnRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            currentFishSchool = Instantiate(fishSchoolPrefab, spawnPosition, spawnRotation);
            currentFishSchool.roamFocalPoint = transform;
            currentFishSchool.Initialize();
        }
        else
        {
            Debug.LogError("Could not find a random position for Fish School on the Nav Mesh");
        }
    }

    public void Activate()
    {
        beam.SetActive(true);
        currentFishSchool.CircleFocalPoint();
    }

    public void Deactivate()
    {
        beam.SetActive(false);
        Destroy(currentFishSchool);
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
