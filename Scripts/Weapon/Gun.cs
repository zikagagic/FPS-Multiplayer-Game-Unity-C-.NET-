using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Gun Camera")]
    [SerializeField]
    private Camera _fpsCam;
    [SerializeField]
    private bool _buttonHold = true;
    [Header("Gun Damage Settings")]
    [SerializeField]
    private float _damage = 10f;
    [SerializeField]
    private float _range = 200f;
    [SerializeField]
    private float _impactForce = 200f;
    [SerializeField]
    private float _fireRate = 30f;
    [SerializeField]
    private float _nextTimeToFire = 0f;

    [Header("Weapon Sway Settings")]
    public float maxSway=0.3f;
    public float swayAmount = 0.1f;
    public float currentSway;
    public float swaySpeed=3.0f;
    private Vector3 initPos;

    [Header("Ammo Settings")]
    public int ammoMags = 3;
    public int maxAmmo = 30;
    private int currentAmmo;
    public float reloadTime = 2.3f;
    private bool isReloading = false;

    [Header("Gun Audio")]
    public AudioSource gunFire;
    public AudioSource gunReload;
    [Header("Gun UI")]
    public string gunName="Assault Rifle";
    public Sprite gunImage;

    [Header("Gun Animations")]
    public bool isFiring = false;
    public bool isAiming = false;
    public Animator gunAnimator;
    
    [Header("Gun Particle Systems")]
    public ParticleSystem muzzleFlash;
    public ParticleSystem bulletEject;
    public GameObject impactEffect;
    public GameObject bulletHole;

    [Header("Recoil Transform")]
    public Transform RecoilPositionTranform;
    public Transform RecoilRotationTranform;
    [Space(10)]
    [Header("Recoil Settings")]
    public Transform gunBarrel;
    public float PositionDampTime;
    public float RotationDampTime;
    [Space(10)]
    public float Recoil1;
    public float Recoil2;
    public float Recoil3;
    public float Recoil4;
    [Space(10)]
    public Vector3 RecoilRotation;
    public Vector3 RecoilKickBack;

    public Vector3 RecoilRotation_Aim;
    public Vector3 RecoilKickBack_Aim;
    [Space(10)]
    public Vector3 CurrentRecoil1;
    public Vector3 CurrentRecoil2;
    public Vector3 CurrentRecoil3;
    public Vector3 CurrentRecoil4;
    [Space(10)]
    public Vector3 RotationOutput;
    

    void Start()
    {
        initPos = transform.localPosition;
        currentAmmo = maxAmmo;
        gunAnimator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        isReloading = false;
        gunAnimator.SetBool("isReloading", false);
        gunAnimator.SetBool("isAiming", false);
    }

    void Update()
    {
        Sway();
        if (isReloading)
            return;       
        PlayerInput();
        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }
    }

    private void FixedUpdate()
    {
        Recoil();
    }

    void Shoot()
    {

        currentAmmo--;

        if (gunFire != null)
        {
            gunFire.Play();
        }
        if(muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
        if (bulletEject != null)
        {
           bulletEject.Play();
        }
        RaycastHit hit;

        if(isAiming)
        {
            CurrentRecoil1 += new Vector3(RecoilRotation_Aim.x, Random.Range(-RecoilRotation_Aim.y, RecoilRotation_Aim.y), RecoilKickBack_Aim.z);
            CurrentRecoil3 += new Vector3(Random.Range(-RecoilKickBack_Aim.x, RecoilKickBack_Aim.x), Random.Range(-RecoilKickBack_Aim.y, RecoilKickBack_Aim.y), RecoilKickBack_Aim.z);
        }
        else
        {
            CurrentRecoil1 += new Vector3(RecoilRotation.x, Random.Range(-RecoilRotation.y, RecoilRotation.y), Random.Range(-RecoilRotation.z, RecoilRotation.z));
            CurrentRecoil3 += new Vector3(Random.Range(-RecoilKickBack.x, RecoilKickBack.x), Random.Range(-RecoilKickBack.y, RecoilKickBack.y), RecoilKickBack.z);
        }

        if (Physics.Raycast(_fpsCam.transform.position, _fpsCam.transform.forward, out hit, _range))
        {
            Debug.Log(hit.transform.name);

            //Target target = hit.transform.GetComponent<Target>();
           // if (target != null)
            {
            //    target.TakeDamage(_damage);
            }

            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForce(-hit.normal * _impactForce);
            }
            //instanciranje efekta udarca metka u povrsinu, zajedno sa rupom
            GameObject impactObject = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));

            GameObject impactHole = Instantiate(bulletHole,hit.point,Quaternion.LookRotation(hit.normal));
            impactHole.transform.parent = hit.transform;
        }
    }

    void PlayerInput()
    {
        if (_buttonHold)
        {
            if (Input.GetMouseButton(0) && currentAmmo > 0 && Time.time >= _nextTimeToFire)
            {
                _nextTimeToFire = Time.time + 1f / _fireRate;
                isFiring = true;
                Shoot();
            }
        }
        else
        {
            if(Input.GetMouseButtonDown(0)&& currentAmmo>0)
            {
                isFiring = true;
                Shoot();
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            isFiring = false;
        }

        if (Input.GetMouseButtonDown(1))
        {
            isAiming = !isAiming;
            gunAnimator.SetBool("isAiming", isAiming);
        }
        if (Input.GetButtonDown("Reload") && currentAmmo > 0 && currentAmmo < maxAmmo)
        {
            Debug.Log("Pressed R button");
            StartCoroutine(Reload());
        }
    }

    IEnumerator Reload()
    {
        if (ammoMags > 0)
        {
            isReloading = true;
            gunReload.Play();
            Debug.Log("Reloading...");

            yield return new WaitForSeconds(reloadTime);

            currentAmmo = maxAmmo;
            ammoMags--;
            isReloading = false;
        }
        else
            Debug.Log("No ammo");
    }

    void Sway()
    {
        if (isAiming)
        {
            currentSway = 0;
        }
        else currentSway = swayAmount;
        float moveX = -Input.GetAxis("Mouse X") * currentSway;
        float moveY = -Input.GetAxis("Mouse Y") * currentSway;
        moveX = Mathf.Clamp(moveX, -maxSway, maxSway);
        moveY = Mathf.Clamp(moveY, -maxSway, maxSway);

        Vector3 finalPos = new Vector3(moveX, moveY, 0);

        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPos + initPos, Time.deltaTime * swaySpeed);
    }

    //source:https://hatebin.com/hwbhihougq
    void Recoil()
    {
        CurrentRecoil1 = Vector3.Lerp(CurrentRecoil1, Vector3.zero, Recoil1 * Time.deltaTime);
        CurrentRecoil2 = Vector3.Lerp(CurrentRecoil2, CurrentRecoil1, Recoil2 * Time.deltaTime);
        CurrentRecoil3 = Vector3.Lerp(CurrentRecoil3, Vector3.zero, Recoil3 * Time.deltaTime);
        CurrentRecoil4 = Vector3.Lerp(CurrentRecoil4, CurrentRecoil3, Recoil4 * Time.deltaTime);

        RecoilPositionTranform.localPosition = Vector3.Slerp(RecoilPositionTranform.localPosition, CurrentRecoil3, PositionDampTime * Time.fixedDeltaTime);
        RotationOutput = Vector3.Slerp(RotationOutput, CurrentRecoil1, RotationDampTime * Time.fixedDeltaTime);
        RecoilRotationTranform.localRotation = Quaternion.Euler(RotationOutput);
    }

    public string getCurrentAmmo()
    {
        return currentAmmo.ToString();
    }

    public string getMags()
    {
        return ammoMags.ToString();
    }

}