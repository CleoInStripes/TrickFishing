
using UnityEngine;
using System.Collections;
using System;

public class gun : MonoBehaviour
{
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 15f;
    public float impactForce = 30f;

    public int maxAmmo = 10;
    private int curAmmo;
    public float reloadTime = 1f;
    private bool isReloading = false;

    public bool Automatic;

    private Camera fpsCam;

    public GameObject muzzlePoint;

    public GameObject bulletTrailPrefab;
    public GameObject impactEffectPrefab;

    private float nextTimeToFire = 0f;

    public int CurrentAmmo => curAmmo;

    void Start()
    {
        curAmmo = maxAmmo;
        fpsCam = PlayerCam.Instance.GetComponentInChildren<Camera>();
    }

    void Update()
    {
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

        if (curAmmo <= 0)
        {
            PerformReload();
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
        Debug.Log("Reloading...");

        yield return new WaitForSeconds(reloadTime);

        curAmmo = maxAmmo;
        isReloading = false;
    }

    void Shoot()
    {
        curAmmo--;

        if (bulletTrailPrefab)
        {
            var bulletTrail = Instantiate(bulletTrailPrefab, muzzlePoint.transform.position, muzzlePoint.transform.rotation);
            Destroy(bulletTrail, 3f);
        }

        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            //Debug.Log(hit.transform.name);

            target target = hit.transform.GetComponentInParent<target>();
            if (target != null)
            {
                //Debug.Log($"Found Target: {target.gameObject.name}");
                target.Hit(damage);
            }

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
    }
}
