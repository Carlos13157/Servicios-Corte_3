using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class WeaponManager : NetworkBehaviour{
    private PlayerInput playerInput;

    private InputAction handleWeaponAction;
    private InputAction aimWeaponAction;
    private InputAction reloadWeaponAction;

    [SerializeField] private List<GameObject> WeaponsToLoad = new(); 
    private readonly List<NetworkObject> Weapons = new();
    private Action handleWeaponMethod;
    private NetworkObject currentWeapon;
    private BaseWeapon currentWeaponComponent;
    private Type currentWeaponType;
    public Type CurrentWeaponType {
        get{return currentWeaponType;}
        set{
            currentWeaponType = value;
            if(currentWeaponType == typeof(RangeWeapon)){
                handleWeaponMethod = HandleRangeWeaponActions;
                currentWeaponComponent = currentWeapon.GetComponent<RangeWeapon>();
            }else if (currentWeaponType == typeof(MeleeWeapon)){
                handleWeaponMethod = HandleMeleeWeaponActions;
                currentWeaponComponent = currentWeapon.GetComponent<MeleeWeapon>();
            }
        }
    }
    public Dictionary<string, int> differentAmmoAmounts = new();

    private int _currentBulletsWeapon;
    private int _currentBulletsReserve;
    private string _currentAmmoType;

    public int currentBulletsWeapon
    {
        get => _currentBulletsWeapon;
        set{
            if(value != _currentBulletsWeapon){
                OnCurrentWeaponBulletsChanged?.Invoke(value);
                _currentBulletsWeapon = value;
            }
        }
    }
    public int currentBulletsReserve
    {
        get => _currentBulletsReserve;
        set{
            if(value != _currentBulletsReserve){
                OnCurrentWeaponReserveChanged?.Invoke(value);
                _currentBulletsReserve = value;
            }
        }
    }
    public string currentAmmoType{
        get => _currentAmmoType;
        set{
            if(value != _currentAmmoType){
                OnCurrentWeaponAmmoTypeChanged?.Invoke(value);
                _currentAmmoType = value;
            }
        }
    }

    public event Action<int> OnCurrentWeaponBulletsChanged;
    public event Action<int> OnCurrentWeaponReserveChanged;
    public event Action<string> OnCurrentWeaponAmmoTypeChanged;

    [SerializeField] protected float CycleRate;
    private float nextCycle;

    void Start(){
        playerInput = GetComponent<PlayerInput>();
        handleWeaponAction = playerInput.actions.FindAction("HandleWeapon");
        aimWeaponAction = playerInput.actions.FindAction("AimWeapon");
        reloadWeaponAction = playerInput.actions.FindAction("reloadWeapon");

        // Weapons = GameObject.FindGameObjectsWithTag("Weapon").ToList();

        differentAmmoAmounts.Add("SmallAmmo", 60);
        differentAmmoAmounts.Add("MidAmmo" ,120);
        differentAmmoAmounts.Add("BigAmmo", 30);
        differentAmmoAmounts.Add("ShotgunShells", 60);

        foreach(GameObject weapon in WeaponsToLoad){
            var weaponGO = Runner.Spawn(weapon, weapon.transform.position, Quaternion.identity);
            weaponGO.transform.SetParent(Camera.main.transform);
            var networkComp = weaponGO.GetComponent<NetworkObject>();
            weaponGO.gameObject.SetActive(false);
            Weapons.Add(weaponGO);
        }

        SetCurrentWeapon(Weapons[0]);
    }

    public override void FixedUpdateNetwork(){
        handleWeaponMethod?.Invoke();
    }

    private void HandleRangeWeaponActions() {
        var rangeWeapon = currentWeaponComponent as RangeWeapon;

        var actionateWeaponCondition = rangeWeapon.mode == Modes.Automatic ? handleWeaponAction.ReadValue<float>() > 0 : handleWeaponAction.triggered;

        if (actionateWeaponCondition && rangeWeapon.currentBullets > 0){
            rangeWeapon.ActionateWeapon();
            rangeWeapon.animator.SetBool("isIdle", false);
            rangeWeapon.animator.SetBool("isShooting", true);
        } else {
            rangeWeapon.animator.SetBool("isIdle", true);
            rangeWeapon.animator.SetBool("isShooting", false);

            rangeWeapon.WeaponHeat = 2.5f*Mathf.Pow(rangeWeapon.subsHeatCoeficient, 3);
            rangeWeapon.visualEffects["Muzzle Flash"].transform.GetChild(0).gameObject.SetActive(false);
        }

        rangeWeapon.visualEffects["Muzzle Flash"].SetFloat("WeaponHeat", rangeWeapon.WeaponHeat);

        if (reloadWeaponAction.ReadValue<float>() > 0 && rangeWeapon.currentBullets <= rangeWeapon.AmountBullets && !rangeWeapon.isReloading){
            int bulletsToLoad = Mathf.Min(differentAmmoAmounts[currentAmmoType], rangeWeapon.AmountBullets);
            differentAmmoAmounts[currentAmmoType] -= bulletsToLoad - (rangeWeapon.currentBullets - (rangeWeapon.currentBullets == 0 ? 0 : 1));
            StartCoroutine(rangeWeapon.ReloadWeapon(bulletsToLoad));
        }

        rangeWeapon.isAiming = aimWeaponAction.ReadValue<float>() > 0;
        currentBulletsWeapon = rangeWeapon.currentBullets;
        currentBulletsReserve = differentAmmoAmounts[currentAmmoType];
    }

    private void HandleMeleeWeaponActions(){
        
    }

    public void SetCurrentWeapon(NetworkObject weapon) {
        if (currentWeapon) currentWeapon.gameObject.SetActive(false);
        currentWeapon = weapon;
        currentWeapon.gameObject.SetActive(true);
        currentWeapon.transform.SetParent(Camera.main.transform);

        CurrentWeaponType = currentWeapon.GetComponent<BaseWeapon>().GetType();
        if(currentWeaponComponent is RangeWeapon rangeWeapon){
            currentAmmoType = rangeWeapon.ammoType.ToString();
            currentBulletsReserve = differentAmmoAmounts[currentAmmoType];
        }
    }

    public NetworkObject GetCurrentWeapon(){
        return currentWeapon;
    }

    public void CycleWeapons(int value){
        if(Time.time > nextCycle){
            nextCycle = Time.time + CycleRate;

            var currentWeaponIdx = Weapons.IndexOf(currentWeapon);

            var adjustedIdx = (currentWeaponIdx + value + Weapons.Count) % Weapons.Count;

            SetCurrentWeapon(Weapons[adjustedIdx]);
        }
    }
}
