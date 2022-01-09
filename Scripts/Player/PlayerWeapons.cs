using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Bolt;
using System;

public class PlayerWeapons : EntityBehaviour<IPlayerState>
{

    [SerializeField] private Camera _cam = null;
    [SerializeField] private Weapon[] _weapons;

    private int _weaponIndex=1;
    public WeaponID _primary = WeaponID.None;
    public WeaponID _secondary = WeaponID.None;

    public Camera Cam { get => _cam; }

    [SerializeField] private Transform _weaponsTransform = null;
    [SerializeField] private GameObject[] _weaponPrefabs = null;

    private float currentSpreadX,currentSpreadY;

    public int WeaponIndex { get => _weaponIndex; }
    public Weapon[] Weapons { get => _weapons; }

    public void Init()
    {
        if(entity.IsOwner)
        {
            for(int i = 0; i<2; i++)
            {
                state.WeaponArray[i].CurrentAmmo = -1;
            }
          
            state.WeaponIndex = 1;
        }
    }

    //Execute commands on the state based on the input received
    public void ExecuteCommand(bool fire, bool fireDown,bool fireUp, bool aiming, int wheel, bool reload, bool selectPrimaryWeapon, bool selectSecondaryWeapon)
    {

        if (selectPrimaryWeapon)
        {
            if (_weapons[0])
            {
                if (entity.IsOwner)
                {
                    state.WeaponIndex = 0;
                }
            }
        }

        if (selectSecondaryWeapon)
        {
            if (_weapons[1])
            {
                if (entity.IsOwner)
                {
                    state.WeaponIndex = 1;
                }
            }
        }

        if (_weapons[_weaponIndex])
        {
            _weapons[_weaponIndex].ExecuteCommand(fire, fireDown,fireUp, aiming, reload);
        }

       
    }
    //Send the weapon effects to the other players
    public void FireEffect(float spreadX, float spreadY)
    {
        _weapons[_weaponIndex].ShootEffects(spreadX, spreadY);
    }
    public int CalculateWeaponIndex(float valueToAdd)
    {
        int i = _weaponIndex;
        int factor = 0;

        if (valueToAdd > 0)
            factor = 1;
        else if (valueToAdd < 0)
            factor = -1;

        i += factor;

        if (i == -1)
            i = _weapons.Length - 1;

        if (i == _weapons.Length)
            i = 0;

        while (_weapons[i]==null)
        {
            i += factor;
            i = i % _weapons.Length;
        }

        return i;
    }
    //set the ammo of the weapon
    public void SetAmmo(int i, int current, int total)
    {
        if(_weapons[i] && i!=0)
             _weapons[i].SetAmmo(current,total);
    }
    //Change the active weapon on the player's state
    public void ChangeActiveWeapon(int weaponIndex)
    {
        _weaponIndex = weaponIndex;

        for (int i = 0; i < _weapons.Length; i++)
            if(_weapons[i]!=null)
                _weapons[i].gameObject.SetActive(false);
        if (_weapons[_weaponIndex]!= null)
        {
            _weapons[_weaponIndex].gameObject.SetActive(true);
        }

    }
    //function to be called in player callback to remove the weapon
    public void RemoveWapon(int i)
    {
        if (_weapons[i])
            Destroy(_weapons[i].gameObject);

        _weapons[i] = null;
    }
    //function to be called to change the weaponarray state of the play
    public void AddWeaponEvent(int i, int currentAmmo, int totalAmmo)
    {
        if (i <(int)WeaponID.PrimaryEnd)
        {
            state.WeaponArray[1].ID = i;
            state.WeaponArray[1].CurrentAmmo = currentAmmo;
            state.WeaponArray[1].TotalAmmo = totalAmmo;
        }
        else
        {
            state.WeaponArray[2].ID = i;
            state.WeaponArray[2].CurrentAmmo = currentAmmo;
            state.WeaponArray[2].TotalAmmo = totalAmmo;
        }

    }

    public void AddWeaponToArray(WeaponID id)
    {
        if (id == WeaponID.None)
            return;

        GameObject weaponPrefab = null;
        foreach(GameObject w in _weaponPrefabs)
        {
            if(w.GetComponent<Weapon>().id==id)
            {
                weaponPrefab = w;
                break;
            }
        }

        weaponPrefab = Instantiate(weaponPrefab, _weaponsTransform.position, Quaternion.LookRotation(_weaponsTransform.forward), _weaponsTransform);

        if(id<WeaponID.PrimaryEnd)
        {
            //if there is already a weapon in the primary slot, destroy the weapon
            if (_primary != WeaponID.None)
            {
                RemoveWapon(0);
            }
            _primary = id;
            _weapons[0] = weaponPrefab.GetComponent<Weapon>();
            weaponPrefab.GetComponent<Weapon>().Init(this, 0);
            ChangeActiveWeapon(0);
        }
        else
        {
            //if there is already a weapon in the secondary slot, destroy the weapon
            if (_secondary != WeaponID.None)
            {
                RemoveWapon(1);
            }
            _secondary = id;
            _weapons[1] = weaponPrefab.GetComponent<Weapon>();
            weaponPrefab.GetComponent<Weapon>().Init(this, 1);
            ChangeActiveWeapon(1);
        }
    }

    public void AddWeaponToSlot(WeaponID id)
    {
        if (id == WeaponID.None)
            return;

        int i = (id < WeaponID.PrimaryEnd) ? 0 : 1;
        state.WeaponArray[i].ID = (int)id;
    }
    //Refill the ammo of the weapon
    public void ReffilWeapon(WeaponID weaponID)
    {
        Weapon prefab = null;
        foreach(GameObject w in _weaponPrefabs)
        {
            if(w.GetComponent<Weapon>().id==weaponID)
            {
                prefab = w.GetComponent<Weapon>();
                break;
            }
        }

        int i = 0;
        if(weaponID<=WeaponID.PrimaryEnd)
        {
            i = 0;
        }
        else
        {
            i = 1;
        }
        if (entity.IsOwner)
        {
            state.WeaponArray[i].CurrentAmmo = prefab.ammoMagazine;
            state.WeaponArray[i].TotalAmmo = prefab.totalAmmo;
        }
    }
    //when a player dies remove all weapons from his inventory
    public void OnDeath(bool isDead)
    {
        if(isDead)
        {
            RemoveWapon(0);
            RemoveWapon(1);
        }
        else
        {
            if(_secondary!=WeaponID.None)
            ReffilWeapon(_secondary);
            if (_primary != WeaponID.None)
               ReffilWeapon(_primary);
        }
    }
}
