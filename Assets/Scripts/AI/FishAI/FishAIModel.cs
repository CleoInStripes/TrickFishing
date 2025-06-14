using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UI;

public class FishAIModel : MonoBehaviour
{
    public Transform avatarRoot;
    public Transform avatarYControl;
    public RangeFloat avatarYOffsetRange;

    [Header("Swimming")]
    public RangeFloat swimYRange;
    public RangeFloat swimYDurationRange;
    public RangeFloat swimYLongTermRange;
    public RangeFloat swimYLongTermDurationRange;

    [Header("Scoring")]
    public int hitScore;
    public int killScore;

    [Header("UI")]
    public Slider healthSlider;
    public float healthSliderVisibilityTimeout = 2f;
    public Image chasingIcon;

    [Header("Misc")]
    public Transform projectileSpawnPoint;
    public GameObject deathParticleEffectPrefab;
    public Animator animator;
    public GameObject fishFoodPrefab;

    [HideInInspector]
    public bool destroyOnDeath = true;

    [HideInInspector]
    public Health health;
    [HideInInspector]
    public FishAIBrain aiBrain;
    [HideInInspector]
    public NavMeshAgent agent;

    public UnityEvent OnDeath;

    private Tween swimYTween;
    private Tween swimYLongTermTween;

    private void Awake()
    {
        health = GetComponent<Health>();
        aiBrain = GetComponent<FishAIBrain>();
        agent = GetComponent<NavMeshAgent>();

        FishAIManager.Instance.spawnedFishes.Add(this);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RandomizeYOffset();
        PlaySwimmingAnimationAlongYAxis();
        PlayLongTermSwimmingAnimationAlongYAxis();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        healthSlider.value = health.normalizedHealth;
        healthSlider.gameObject.SetActive(health.normalizedHealth < 1f && health.timeSinceLastDamage < healthSliderVisibilityTimeout);
        chasingIcon.gameObject.SetActive(aiBrain.CurrentState == FishAIBrain.State.Chasing);
    }

    void RandomizeYOffset()
    {
        avatarRoot.localPosition += Vector3.up * avatarYOffsetRange.GetRandom();
    }

    void PlaySwimmingAnimationAlongYAxis()
    {
        swimYTween = avatarYControl.DOLocalMoveY(avatarYControl.localPosition.y + swimYRange.GetRandom(), swimYDurationRange.GetRandom())
                 .SetLoops(-1, LoopType.Yoyo)
                 .SetEase(Ease.InOutSine)
                 .SetTarget(this);
    }

    void PlayLongTermSwimmingAnimationAlongYAxis()
    {
        swimYLongTermTween = avatarRoot.DOLocalMoveY(avatarRoot.localPosition.y + swimYLongTermRange.GetRandom(), swimYLongTermDurationRange.GetRandom())
                 .SetLoops(-1, LoopType.Yoyo)
                 .SetEase(Ease.InOutSine)
                 .SetTarget(this);
    }

    public void OnTakenDamage()
    {
        // TODO: Play animation, particle fx, etc..

        PlayerModel.Instance.AddScore(hitScore);
    }

    public void OnDead()
    {
        SoundEffectsManager.Instance.PlayAt("TF_Enemyhurt", avatarYControl.transform.position, 0.4f);
        
        // TODO: Play animation, particle fx, etc..
        var pfx = Instantiate(deathParticleEffectPrefab, avatarYControl.transform.position + (Vector3.up * 1), avatarYControl.transform.rotation);
        Destroy(pfx, 10f);

        Instantiate(fishFoodPrefab, avatarYControl.transform.position + (Vector3.up * 1), avatarYControl.transform.rotation);

        PlayerModel.Instance.AddScore(killScore);
        OnDeath.Invoke();

        if (destroyOnDeath)
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        FishAIManager.Instance.spawnedFishes.Remove(this);
        DOTween.Kill(this);
    }
}
