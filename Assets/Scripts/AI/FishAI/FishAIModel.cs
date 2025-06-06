using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

public class FishAIModel : MonoBehaviour
{
    public Transform avatarRoot;
    public RangeFloat avatarYOffsetRange;

    [Header("Swimming")]
    public RangeFloat swimYRange;
    public RangeFloat swimYDurationRange;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RandomizeYOffset();
        PlaySwimmingAnimationAlongYAxis();
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
        avatarRoot.DOMoveY(avatarRoot.position.y + swimYRange.GetRandom(), swimYDurationRange.GetRandom())
                 .SetLoops(-1, LoopType.Yoyo)
                 .SetEase(Ease.InOutSine);
    }
}
