using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Bolt;
using Photon.Bolt.Utils;

public class Weapon : EntityBehaviour<IPlayerState>
{
    protected PlayerMotor playerMotor;
    protected PlayerWeapons playerWeapons;
    protected PlayerCallbacks playerCallback;
    protected Dictionary<PlayerMotor, int> _dmgCounter;
    public WeaponID id;
    public WeaponType weaponType;

    [Header("Gun Camera")]
    [SerializeField] private Transform _fpsCam;
    private Camera camera;
    private float standardCameraFOV = 75F;
    private float aimFOV = 50f;
    private float sniperFOV = 35F;


    [Header("Weapon Settings")]
    public float weaponDamage = 10f;
    public float range = 200f;
    public int ammoPerShot = 1;
    public float fireRate = 6f;
    public float spread, spreadAim;
    private float _currentSpreadX, _currentSpreadY;
    private float _nextTimeToFire = 0f;
    public bool buttonHold = true;


    [Header("Weapon Ammo Settings")]

    public int totalAmmo = 90;
    public int ammoMagazine = 30;
    public float reloadTime = 0.3f;
    [SerializeField] private int currentAmmo;
    [SerializeField] private bool isReloading = false;

    [Header("Gun Audio")]
    public AudioSource gunFire;
    public AudioSource gunReload;
    [Header("Gun UI")]
    public string gunName = "Assault Rifle";

    [Header("Gun Animations")]
    public bool isAiming = false;
    public MeshRenderer renderer;
    public Animator animator;

    [Header("Gun Particle Systems")]
    public ParticleSystem muzzleFlash;
    public ParticleSystem bulletEject;
    public GameObject terrainImpactEffect;
    public GameObject bodyImpactEffect;
    public GameObject terrainBulletHole;
    public GameObject bodyBulletHole;
    public GameObject weaponTrail;

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
    private Vector3 CurrentRecoil1;
    private Vector3 CurrentRecoil2;
    private Vector3 CurrentRecoil3;
    private Vector3 CurrentRecoil4;
    [Space(10)]
    private Vector3 RotationOutput;

    private void Awake()
    {
        renderer = GetComponentInChildren<MeshRenderer>();

    }
    public virtual void SetAmmo(int current, int total)
    {
        currentAmmo = current;
        totalAmmo = total;
        if (playerCallback.entity.HasControl)
        {
            if (weaponType == WeaponType.Shotgun)
            {
                GUI_Controller.Current.UpdateAmmo(current / ammoPerShot, total / ammoPerShot);
            }
            else
            {
                GUI_Controller.Current.UpdateAmmo(current, total);
            }
        }
    }

    public int CurrentAmmo
    {
        get => currentAmmo;
        set
        {
            if (playerMotor.entity.IsOwner)
                playerMotor.state.WeaponArray[playerWeapons.WeaponIndex].CurrentAmmo = value;
            currentAmmo = value;
        }
    }

    public int TotalAmmo
    {
        get => totalAmmo;
        set
        {
            if (playerMotor.entity.IsOwner)
                playerMotor.state.WeaponArray[playerWeapons.WeaponIndex].TotalAmmo = value;
            totalAmmo = value;
        }
    }

    public string WeaponName { get => gunName; }

    public virtual void Init(PlayerWeapons pw, int index)
    {
        playerWeapons = pw;
        playerMotor = pw.GetComponent<PlayerMotor>();
        playerCallback = pw.GetComponent<PlayerCallbacks>();
        _fpsCam = playerWeapons.Cam.transform;
        camera = _fpsCam.GetComponent<Camera>();

        if (playerMotor.entity.IsOwner)
        {
            playerMotor.state.WeaponArray[index].CurrentAmmo = currentAmmo;
            playerMotor.state.WeaponArray[index].TotalAmmo = totalAmmo;
        }
        CurrentAmmo = ammoMagazine;
        TotalAmmo = totalAmmo;
    }

    private void OnEnable()
    {
        if (playerWeapons)
        {
            if (playerWeapons.entity.IsControllerOrOwner)
            {
                if (CurrentAmmo == 0)
                    StartCoroutine(Reload());
            }
            if (playerWeapons.entity.HasControl)
            {
                if(weaponType==WeaponType.Shotgun)
                {
                    GUI_Controller.Current.UpdateAmmo(currentAmmo/4, totalAmmo/4);

                }
                GUI_Controller.Current.UpdateAmmo(currentAmmo, totalAmmo);
                GUI_Controller.Current.UpdateGunName(gunName);
            }
        }
        animator.SetBool("isReloading", false);
        animator.SetBool("isAiming", false);
    }
    private void OnDisable()
    {
        if (isReloading)
        {
            isReloading = false;
            StopCoroutine(Reload());
        }
    }

    IEnumerator Reload()
    {
        if (!isReloading)
        {
            if (!gunReload.isPlaying)
            {
                gunReload.Play();
            }
            if (TotalAmmo > 0)
            {
                int ammoToAdd;
                isReloading = true;
                animator.SetBool("isReloading", true);
                isReloading = false;
                yield return new WaitForSeconds(reloadTime - 0.25f);
                animator.SetBool("isReloading", false);
                yield return new WaitForSeconds(0.25f);
                TotalAmmo += CurrentAmmo;
                ammoToAdd = Mathf.Min(TotalAmmo, ammoMagazine);
                TotalAmmo -= ammoToAdd;
                CurrentAmmo = ammoToAdd;
            }
        }
    }
    private void FixedUpdate()
    {
        Recoil();
    }

    public virtual void ExecuteCommand(bool fire, bool fireDown, bool fireUp, bool aiming, bool reload)
    {
        if (isReloading)
            return;

        if (buttonHold)
        {
            if (fire && CurrentAmmo > 0 && Time.time >= _nextTimeToFire)
            {
                _nextTimeToFire = Time.time + 1f / fireRate;
                Shoot();
            }
        }
        if (aiming)
        {
            GUI_Controller.Current.ShowCrosshair(false);
            //changing camera FOV while aiming
            animator.SetBool("isAiming", true);
            //if the player has the sniper equiped lerp to the camera's FOV
            if (weaponType == WeaponType.Sniper)
            {
                camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, sniperFOV, Time.deltaTime * 4);
                StartCoroutine(OnScoped());
            }
            else
            {
                camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, aimFOV, Time.deltaTime * 4);
            }
        }
        else
        {
            GUI_Controller.Current.ShowCrosshair(true);

            //changing cameraFOV back to original FOV
            camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, standardCameraFOV, Time.deltaTime * 4);
            animator.SetBool("isAiming", false);
            if (weaponType == WeaponType.Sniper)
            {
                OnUnscoped();
            }
        }

        if (reload && CurrentAmmo != ammoMagazine && TotalAmmo > 0 && !isReloading)
        {
            StartCoroutine(Reload());
        }

        GUI_Controller.Current.UpdateAmmo(currentAmmo, totalAmmo);
    }
    void Shoot()
    {
        if (currentAmmo == 0)
            StartCoroutine(Reload());
        if (!isReloading)
        {
            //if the player has sufficient ammo
            if (CurrentAmmo >= ammoPerShot)
            {
                int dmg = 0;

                _dmgCounter = new Dictionary<PlayerMotor, int>();
                CurrentAmmo -= ammoPerShot;
                //recoil kickback based on if the player is aiming or not
                if (isAiming)
                {
                    CurrentRecoil1 += new Vector3(RecoilRotation_Aim.x, Random.Range(-RecoilRotation_Aim.y, RecoilRotation_Aim.y), RecoilKickBack_Aim.z);
                    CurrentRecoil3 += new Vector3(Random.Range(-RecoilKickBack_Aim.x, RecoilKickBack_Aim.x), Random.Range(-RecoilKickBack_Aim.y, RecoilKickBack_Aim.y), RecoilKickBack_Aim.z);
                }
                else
                {
                    CurrentRecoil1 += new Vector3(RecoilRotation.x, Random.Range(-RecoilRotation.y, RecoilRotation.y), Random.Range(-RecoilRotation.z, RecoilRotation.z));
                    CurrentRecoil3 += new Vector3(Random.Range(-RecoilKickBack.x, RecoilKickBack.x), Random.Range(-RecoilKickBack.y, RecoilKickBack.y), RecoilKickBack.z);
                }

                for (int i = 0; i < ammoPerShot; i++)
                {
                    //simulating weapon spread, if the player is aiming reduces the current spread
                    if (!isAiming)
                    {
                        _currentSpreadX = Random.Range(-spread, spread);
                        _currentSpreadY = Random.Range(-spread, spread);
                    }
                    else
                    {
                        _currentSpreadX = Random.Range(-spreadAim, spreadAim);
                        _currentSpreadY = Random.Range(-spreadAim, spreadAim);
                    }
                    // if the player sprints increase spread
                    if (playerMotor.IsSprinting)
                    {
                        _currentSpreadX *= 1.5f;
                        _currentSpreadY *= 1.25f;
                    }
                    //if the player walks reduce spread
                    else if (playerMotor.IsWalking)
                    {
                        _currentSpreadX *= 0.5f;
                        _currentSpreadY *= 0.25f;
                    }
                    //direction of the bullet
                    Vector3 direction = _fpsCam.transform.forward + new Vector3(_currentSpreadX, _currentSpreadY, 0);

                    Ray r = new Ray(_fpsCam.transform.position, direction);

                    //sending the effect to other players on the server
                    if (playerCallback.entity.IsOwner)
                        playerCallback.FireEvent(_currentSpreadX, _currentSpreadY);
                    if (playerCallback.entity.HasControl)
                        ShootEffects(_currentSpreadX, _currentSpreadY);

                    //if the raycast hits a target and if the target is a player reduce life points by the weapon's damage
                    RaycastHit hit;
                    if (Physics.Raycast(r, out hit, range))
                    {
                        PlayerMotor target = hit.transform.GetComponent<PlayerMotor>();
                        if (target != null)
                        {
                            dmg = (int)weaponDamage;
                            if (!_dmgCounter.ContainsKey(target))
                                _dmgCounter.Add(target, dmg);
                            else
                                _dmgCounter[target] += dmg;
                        }
                    }
                }
                //if multiple players were hit with one shot
                foreach (PlayerMotor playerMotor in _dmgCounter.Keys)
                    playerMotor.Life(playerMotor, -_dmgCounter[playerMotor]);
            }
            else if (TotalAmmo > 0)
            {
                StartCoroutine(Reload());
            }
        }
    }

    public virtual void ShootEffects(float x, float y)
    {
        //play the gun sound effects
        if (gunFire)
        {
            gunFire.Play();
        }
        if (muzzleFlash)
        {
            muzzleFlash.Play();
        }
        if (bulletEject)
        {
            bulletEject.Play();
        }
        Vector3 direction = _fpsCam.transform.forward + new Vector3(x, y, 0);

        //create a weapon trail going from the gun barrel to the front of the gun
        var trailStart = Instantiate(weaponTrail, gunBarrel.position, Quaternion.identity);
        var trail = trailStart.GetComponent<LineRenderer>();


        for (int i = 0; i < ammoPerShot; i++)
        {
            //send a ray, the starting location is the camera going into the direction which is given
            Ray r = new Ray(_fpsCam.position, direction);
            RaycastHit hit;

            if (Physics.Raycast(r, out hit))
            {
                //if the raycast hits a player
                if (hit.collider.tag == "Player" || hit.collider.tag == "PlayerHead")
                {
                    GameObject impactObject = Instantiate(bodyImpactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                    GameObject impactHole = Instantiate(bodyBulletHole, hit.point, Quaternion.LookRotation(hit.normal));

                    impactHole.transform.parent = hit.transform;
                }
                else
                {

                    GameObject impactObject = Instantiate(terrainImpactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                    GameObject impactHole = Instantiate(terrainBulletHole, hit.point, Quaternion.LookRotation(hit.normal));

                    impactHole.transform.parent = hit.transform;
                }
                //if the raycast hits an object, the end point of the trail is that object
                trail.SetPosition(0, gunBarrel.position);
                trail.SetPosition(1, hit.point);
            }
            else
            {
                //if the raycast doesnt hit an object, the end point is the range of the weapon
                trail.SetPosition(0, gunBarrel.position);
                trail.SetPosition(1, r.direction * range + _fpsCam.position);
            }
        }
    }

    //code taken from source:https://hatebin.com/hwbhihougq
    public void Recoil()
    {
        CurrentRecoil1 = Vector3.Lerp(CurrentRecoil1, Vector3.zero, Recoil1 * BoltNetwork.FrameDeltaTime);
        CurrentRecoil2 = Vector3.Lerp(CurrentRecoil2, CurrentRecoil1, Recoil2 * BoltNetwork.FrameDeltaTime);
        CurrentRecoil3 = Vector3.Lerp(CurrentRecoil3, Vector3.zero, Recoil3 * BoltNetwork.FrameDeltaTime);
        CurrentRecoil4 = Vector3.Lerp(CurrentRecoil4, CurrentRecoil3, Recoil4 * BoltNetwork.FrameDeltaTime);

        RecoilPositionTranform.localPosition = Vector3.Slerp(RecoilPositionTranform.localPosition, CurrentRecoil3, PositionDampTime * Time.fixedDeltaTime);
        RotationOutput = Vector3.Slerp(RotationOutput, CurrentRecoil1, RotationDampTime * Time.fixedDeltaTime);
        RecoilRotationTranform.localRotation = Quaternion.Euler(RotationOutput);
    }
    //function called for sniper scope effect
    IEnumerator OnScoped()
    {
        yield return new WaitForSeconds(0.15f);
        GUI_Controller.Current.ShowScope(true);
        if (entity.HasControl)
        {
            //deactivates the sniper mesh so that it cant be seen when the player aims
            if (renderer)
            {
                renderer.enabled = false;
            }
        }
    }
    //function called when player stops aiming with the sniper
    void OnUnscoped()
    {
        GUI_Controller.Current.ShowScope(false);
        if (entity.HasControl)
        {
            if (renderer)
            {
                renderer.enabled = true;
            }
        }
    }
}
    //all the available weapon types in the game
    public enum WeaponType
    {
        AssaultRifle=1,
        Shotgun,
        Sniper,
        Pistol
    }