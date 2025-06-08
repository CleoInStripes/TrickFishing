
using UnityEngine;
using System.Collections;
using DG.Tweening;
using static UnityEngine.UI.Image;
using NUnit.Framework;
using System.Collections.Generic;

public class gun : MonoBehaviour
{
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 15f;
    public float impactForce = 30f;
    public float bulletSpawnOffsetFactor = 1f;

    public int maxAmmo = 10;
    private int curAmmo;
    public float reloadTime = 1f;
    private bool isReloading = false;
    public float bulletTimeScale = 0.5f;
    public float aerialShotDetectionDistance = 5f;

    public bool Automatic;

    private Camera fpsCam;

    public GameObject muzzlePoint;

    public GameObject bulletTrailPrefab;
    public GameObject impactEffectPrefab;

    [Header("Model")]
    public GameObject gunAvatarRoot;
    public GameObject reel;
    public Animator tempAnimator;

    private float nextTimeToFire = 0f;

    public int CurrentAmmo => curAmmo;

    void Start()
    {
        curAmmo = maxAmmo;
        fpsCam = PlayerCam.Instance.GetComponentInChildren<Camera>();
    }

    void Update()
    {
        if (!PlayerModel.Instance.allowInput)
        {
            return;
        }

        tempAnimator.SetBool("ADS", PlayerModel.Instance.bulletTimeAvailable && Input.GetButton("Fire2"));
        Time.timeScale = 1f;

        if (isReloading)
        {
            return;
        }

        if (curAmmo <= 0)
        {
            PerformReload();
            return;
        }

        if (curAmmo < maxAmmo && Input.GetKeyDown(KeyCode.R))
        {
            PerformReload();
            return;
        }

        Time.timeScale = PlayerModel.Instance.inBulletTime ? bulletTimeScale : 1f;

        if (Automatic)
        {
            if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
            {
                nextTimeToFire = Time.time + 1f / fireRate;
                Shoot();
            }
        }
        else
        {
            if (Input.GetButtonDown("Fire1") && Time.time >= nextTimeToFire)
            {
                nextTimeToFire = Time.time + 1f / fireRate;
                Shoot();
            }
        }
    }

    public void PerformReload()
    {
        if (isReloading)
        {
            return;
        }
        StartCoroutine(Reload());
    }

    IEnumerator Reload()
    {
        isReloading = true;
        tempAnimator.SetTrigger("Reload");
        SoundEffectsManager.Instance.Play("TF_Reelshot", 0.1f);

        yield return new WaitForSeconds(reloadTime);

        curAmmo = maxAmmo;
        isReloading = false;
    }

    List<PlayerModel.TrickShotInfo> GetCurrentTricks(int targetsHit)
    {
        List<PlayerModel.TrickShotInfo> trickshots = new();

        // Number of Hits
        if (targetsHit > 1)
        {
            PlayerModel.TrickShotInfo trickShotInfo = new();
            switch (targetsHit)
            {
                case 2:
                    trickShotInfo.name = "Double Shot";
                    trickShotInfo.extraScore = 500;
                    break;
                case 3:
                    trickShotInfo.name = "Triple Shot";
                    trickShotInfo.extraScore = 1000;
                    break;
                default:
                    trickShotInfo.name = $"Kilimanjaro - {targetsHit}x Shot";
                    trickShotInfo.extraScore = 500 * (targetsHit - 1);
                    break;
            }
            trickshots.Add(trickShotInfo);
        }

        if (PlayerModel.Instance.inBulletTime)
        {
            PlayerModel.TrickShotInfo trickShotInfo = new()
            {
                name = "Slow-Mo Shot",
                extraScore = 200,
            };
            trickshots.Add(trickShotInfo);
        }

        bool isCloseToGround = Physics.Raycast(transform.position, Vector3.down, aerialShotDetectionDistance, PlayerModel.Instance.playerMovement.groundCheck);
        if (!isCloseToGround)
        {
            PlayerModel.TrickShotInfo trickShotInfo = new()
            {
                name = "Aerial Shot",
                extraScore = 200,
            };
            trickshots.Add(trickShotInfo);
        }

        return trickshots;
    }

    void Shoot()
    {
        curAmmo--;

        SoundEffectsManager.Instance.Play("TF_Bubbleshot", 0.5f);

        if (bulletTrailPrefab)
        {
            var playerVelocity = PlayerModel.Instance.rb.linearVelocity;
            var bulletSpawnOffset = playerVelocity * bulletSpawnOffsetFactor;

            var bulletTrail = Instantiate(bulletTrailPrefab, muzzlePoint.transform.position + bulletSpawnOffset, muzzlePoint.transform.rotation);
            Destroy(bulletTrail, 3f);
        }
        PlayRecoil();

        RaycastHit[] hits = Physics.RaycastAll(fpsCam.transform.position, fpsCam.transform.forward, range);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        var targetsHit = 0;
        foreach (RaycastHit hit in hits)
        {
            target target = hit.transform.GetComponentInParent<target>();
            if (!target)
            {
                break;
            }

            //Debug.Log($"Found Target: {target.gameObject.name}");
            target.Hit(damage);
            targetsHit++;

            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForce(-hit.normal * impactForce);
            }

            if (impactEffectPrefab)
            {
                GameObject impactGO = Instantiate(impactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactGO, 2f);
            }
        }

        if (targetsHit > 0)
        {
            var trickshots = GetCurrentTricks(targetsHit);
            foreach (var ts in trickshots)
            {
                PlayerModel.Instance.AddTrickShotInfo(ts);
            }
        }

        //RaycastHit hit;
        //if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        //{
        //    //Debug.Log(hit.transform.name);

        //    target target = hit.transform.GetComponentInParent<target>();
        //    if (target != null)
        //    {
        //        //Debug.Log($"Found Target: {target.gameObject.name}");
        //        target.Hit(damage);
        //    }

        //    if (hit.rigidbody != null)
        //    {
        //        hit.rigidbody.AddForce(-hit.normal * impactForce);
        //    }

        //    if (impactEffectPrefab)
        //    {
        //        GameObject impactGO = Instantiate(impactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        //        Destroy(impactGO, 2f);
        //    }
        //}
    }

    void PlayRecoil()
    {
        var gun = gunAvatarRoot.transform;
        Vector3 originalPos = gun.localPosition;
        Vector3 originalRot = gun.localEulerAngles;

        DOTween.Sequence()
            .Append(gun.DOLocalMove(originalPos + new Vector3(0, 0, -0.2f), 0.05f))
            .Join(gun.DOLocalRotate(originalRot + new Vector3(-5, 0, 0), 0.05f))
            .Append(gun.DOLocalMove(originalPos, 0.1f))
            .Join(gun.DOLocalRotate(originalRot, 0.1f));
    }
}
