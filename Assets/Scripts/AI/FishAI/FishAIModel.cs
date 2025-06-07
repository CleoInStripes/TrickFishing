using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

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

    [Header("Misc")]
    public Transform projectileSpawnPoint;
    public GameObject deathParticleEffectPrefab;

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
        // TODO: Play animation, particle fx, etc..
        var pfx = Instantiate(deathParticleEffectPrefab, transform.position, transform.rotation);
        Destroy(pfx, 10f);

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
