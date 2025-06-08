using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

    public class TrickShotInfo
    {
        public string name = "";
        public int extraScore = 0;
    }


    [HideInInspector] public Health health;
    [HideInInspector] public gun gun;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public playerMovement playerMovement;

    public CameraShakeSettings damageCameraShakeSettings;

    [ReadOnly]
    public int score = 0;
    public float bulletTimeMaxCharge = 1000f;
    public float bulletTimeDischargeRate = 300f;
    public int maxTrickShotInfos = 10;
    public float trickShotCleanupInterval = 3;

    [HideInInspector] public bool inBulletTime = false;
    [HideInInspector] public float bulletTimeCharge = 0f;
    [HideInInspector] public bool bulletTimeAvailable = false;
    public float bulletTimeChargeNormalized => HelperUtilities.Remap(bulletTimeCharge, 0, bulletTimeMaxCharge, 0, 1);

    [HideInInspector] public List<TrickShotInfo> recentTrickShotInfos = new List<TrickShotInfo>();

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
        playerMovement = GetComponent<playerMovement>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        health.OnDamageTaken.AddListener(() => OnDamageTaken());
        health.OnHealthDepleted.AddListener(() => OnDeath());
        WatchAndCleanupTrickShotInfo();
    }

    // Update is called once per frame
    void Update()
    {
        if (LevelManager.Instance.isGameOver)
        {
            return;
        }

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

        if (allowInput)
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                bulletTimeCharge = bulletTimeMaxCharge;
                bulletTimeAvailable = true;
            }


            inBulletTime = false;
            if (bulletTimeAvailable && Input.GetButton("Fire2"))
            {
                inBulletTime = true;
                bulletTimeCharge -= bulletTimeDischargeRate * Time.unscaledDeltaTime;
                if (bulletTimeCharge <= 0)
                {
                    bulletTimeCharge = 0;
                    bulletTimeAvailable = false;
                }
            }

            if (Input.GetKeyDown(KeyCode.Home))
            {
                rb.position = LevelManager.Instance.playerRespawnPoint.position;
                rb.rotation = LevelManager.Instance.playerRespawnPoint.rotation;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    public void AddScore(int _score)
    {
        score += _score;

        if (!bulletTimeAvailable)
        {         
            bulletTimeCharge += _score;
            if (bulletTimeCharge >= bulletTimeMaxCharge)
            {
                bulletTimeCharge = bulletTimeMaxCharge;
                bulletTimeAvailable = true;
            }
        }
    }

    public void AddTrickShotInfo(TrickShotInfo trickShotInfo)
    {
        if (recentTrickShotInfos.Count >= maxTrickShotInfos)
        {
            recentTrickShotInfos.RemoveAt(0);
        }
        recentTrickShotInfos.Add(trickShotInfo);
        AddScore(trickShotInfo.extraScore);
    }

    public async void WatchAndCleanupTrickShotInfo()
    {
        while (health.IsAlive)
        {
            if (recentTrickShotInfos.Count > 0)
            {
                recentTrickShotInfos.RemoveAt(0);
            }
            await Task.Delay((int)(trickShotCleanupInterval * 1000));
        }
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
