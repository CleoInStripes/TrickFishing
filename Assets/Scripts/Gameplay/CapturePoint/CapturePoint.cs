using BasicTools.ButtonInspector;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class CapturePoint : SingletonMonoBehaviour<CapturePoint>
{
    public GameObject beam;
    public FishSchool fishSchoolPrefab;
    public float activationProximity = 50f;
    public int captureScore = 500;
    public GameObject TreasureChestPrefab;
    public float treasureChestSpawnHeight = 50f;
    public float treasureChestSpawnOffset = 3f;
    TreasureChest treasureChest;


    [Button("Pre-Activate", "BtnExecute_PreActivate")]
    [SerializeField]
    private bool btnPreActivate;
    [Button("Activate", "BtnExecute_Activate")]
    [SerializeField]
    private bool btnActivate;

    [HideInInspector] public bool isActive = false;

    private FishSchool currentFishSchool;
    public int activeFishCount {
        get
        {
            if (!currentFishSchool)
            {
                return 0;
            }

            return currentFishSchool.fishes.Count;
        }
    }

    private new void Awake()
    {
        base.Awake();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //
    }

    // Update is called once per frame
    void Update()
    {
        if (currentFishSchool && !isActive)
        {
            CheckPlayerAndActivate();
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

    public void DropTreasure()
    {
        treasureChest = Instantiate(TreasureChestPrefab, transform.position + (Random.insideUnitSphere * treasureChestSpawnOffset) + (Vector3.up * treasureChestSpawnHeight), transform.rotation).GetComponent<TreasureChest>(); ;
        treasureChest.OnLanded.AddListener(() =>
        {
            // TODO: Open Chest, and maybe have the fishes come out of there?
            SpawnFishes();
            Activate();
        });
    }

    void SpawnFishes() {
        if (NavMesh.SamplePosition(treasureChest.transform.position, out NavMeshHit hit, fishSchoolPrefab.roamRange.max, NavMesh.AllAreas))
        {
            var spawnRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            currentFishSchool = Instantiate(fishSchoolPrefab, hit.position, spawnRotation);
            currentFishSchool.roamFocalPoint = treasureChest.transform;
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

    public void Activate()
    {
        if (isActive)
        {
            return;
        }

        isActive = true;
        beam.SetActive(true);
        currentFishSchool.CircleFocalPoint();
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
        treasureChest = null;
        isActive = false;

        CapturePointSystem.Instance.OnCapturePointDeactivated();
    }

    public void BtnExecute_PreActivate()
    {
        DropTreasure();
    }
    public void BtnExecute_Activate()
    {
        if (currentFishSchool)
        {
            Activate();
        }
    }
}
