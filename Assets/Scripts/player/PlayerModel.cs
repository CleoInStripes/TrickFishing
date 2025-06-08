using DG.Tweening;
using System;
using UnityEngine;

public class PlayerModel : SingletonMonoBehaviour<PlayerModel>
{
    [Serializable]
    public class CameraShakeSettings
    {
        public float duration = 0.5f;
        public float strength = 0.5f;
        public int vibrato = 10;
        public float randomness = 90f;
    }

    [HideInInspector] public Health health;
    [HideInInspector] public gun gun;
    [HideInInspector] public Rigidbody rb;

    public CameraShakeSettings damageCameraShakeSettings;

    [ReadOnly]
    public int score = 0;

    public bool allowInput
    {
        get
        {
            if (!health.IsAlive)
            {
                return false;
            }

            if (PlayerHud.Instance.isPaused)
            {
                return false;
            }

            return true;
        }
    }

    public new void Awake()
    {
        base.Awake();
        health = GetComponent<Health>();
        gun = GetComponent<gun>();
        rb = GetComponent<Rigidbody>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        health.OnDamageTaken.AddListener(() => OnDamageTaken());
        health.OnHealthDepleted.AddListener(() => OnDeath());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (PlayerHud.Instance.isPaused)
            {
                PlayerHud.Instance.Resume();
            }
            else
            {
                LevelManager.Instance.PauseGame();
            }
        }
    }

    public void AddScore(int _score)
    {
        score += _score;
    }

    public void OnDamageTaken()
    {
        SoundEffectsManager.Instance.Play("TF_Playerhurt", 0.3f);
        
        var cam = PlayerCam.Instance.cam;
        var oldCamLocalPosition = cam.transform.localPosition;
        PlayerCam.Instance.cam.transform.DOShakePosition(
            damageCameraShakeSettings.duration,
            damageCameraShakeSettings.strength,
            damageCameraShakeSettings.vibrato,
            damageCameraShakeSettings.randomness,
            false,
            true
        ).OnComplete(() =>
        {
            cam.transform.localPosition = oldCamLocalPosition;
        });
    }

    public void OnDeath()
    {
        SoundEffectsManager.Instance.Play("TF_Playerdie");
        LevelManager.Instance.TriggerGameOver();
    }
}
