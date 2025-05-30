using System.Collections;
using System.Collections.Generic;
using Fusion;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

[System.Serializable]

public enum AmmoTypes{
    SmallAmmo,
    MidAmmo,
    BigAmmo,
    ShotgunShells
}

public enum Modes{
    Automatic,
    SemiAutomatic
}

public class RangeWeapon : BaseWeapon{
    private PlayerCam playerCam;
    [SerializeField] protected TransformData adsTransform;
    [SerializeField] public Modes mode;
    [SerializeField] public AmmoTypes ammoType;
    [SerializeField] protected float adsTime;
    [SerializeField] protected float reloadTime;
    [SerializeField] protected GameObject bulletPrefab;
    [SerializeField] protected Transform bulletSpawn;
    [SerializeField] protected float bulletVelocity;
    [SerializeField] protected float bulletLifeTime = 5;
    [SerializeField] protected int amountBullets;
    [SerializeField] protected GameObject magazineGameobject;
    
    [Header("HeatCoeficients")]
    public float addHeatCoeficient = 1.2f;
    
    public float subsHeatCoeficient = -0.03f; 
    private float weaponHeat = 0;

    public float WeaponHeat{
        get{return weaponHeat;}
        set{
            weaponHeat += value;
            weaponHeat = value > 0 ? math.min(weaponHeat, 100): math.max(0, weaponHeat);
        }
    }

    public int AmountBullets {
        get{return amountBullets;}
        private set{amountBullets = value;}
    }
    public int currentBullets { get; protected set;}
    // public ParticleSystem muzzleFlash {get; private set;}

    [Header("Fire Rate in RPM")]
    [SerializeField] protected float fireRate;
    private float nextFire;

    [SerializeField] private RecoilController recoil;

    [Header("States")]
    public bool isAiming;
    public bool isReloading;
    private GameObject cameraObject;

    public Dictionary<string, VisualEffect> visualEffects = new();

    new void Awake(){
        base.Awake();
        fireRate = 60/fireRate;
        currentBullets = amountBullets;
        var VFXs= GetComponentsInChildren<VisualEffect>();
        
        foreach(VisualEffect vfx in VFXs){
            visualEffects.Add(vfx.transform.name, vfx);
        }

        cameraObject = GameObject.FindWithTag("MainCamera");
        playerCam = GetComponentInParent<PlayerCam>();

        foreach(Transform bullet in magazineGameobject.transform){
            bullet.gameObject.SetActive(false);
        }
    }
    
    public override void ActionateWeapon(){
        if(Time.time > nextFire && currentBullets > 0 && !isReloading){
            var muzzleFlash = visualEffects["Muzzle Flash"];
            muzzleFlash.Play();
            muzzleFlash.transform.GetChild(0).gameObject.SetActive(true);
            visualEffects["Shell Ejection"].Play();
            nextFire = Time.time + fireRate;
            WeaponHeat = addHeatCoeficient;
            
            recoil.RecoilFire();
            
            NetworkObject bullet = Runner.Spawn(bulletPrefab, bulletSpawn.position, Quaternion.Euler(-90, 0, 0));
            bullet.GetComponent<BulletController>().SetDamage(damage);
            bullet.GetComponent<Rigidbody>().AddForce(bulletSpawn.forward.normalized * bulletVelocity, ForceMode.Impulse);

            var audioSource = GetComponent<AudioSource>();
            audioSource.PlayOneShot(actionateWeaponSound, 0.2f);

            currentBullets--;
            StartCoroutine(DespawnBulletByTime(bullet));
        }
    }
    
    IEnumerator DespawnBulletByTime(NetworkObject bullet){
        yield return new WaitForSeconds(bulletLifeTime);
        Runner.Despawn(bullet);
    }

    public IEnumerator ReloadWeapon(int bulletsToLoad)
    {
        isReloading = true;
        if (currentBullets >= 2)
        {
            foreach (Transform bullet in magazineGameobject.transform)
            {
                bullet.gameObject.SetActive(true);
            }
        }
        else if (currentBullets == 1)
        {
            magazineGameobject.transform.GetChild(0).gameObject.SetActive(true);
        }

        animator.SetBool("isReloading", true);
        yield return new WaitForSeconds(reloadTime);

        currentBullets = bulletsToLoad + (currentBullets == 0 ? 0 : 1);

        Debug.Log("Reload ended");
        isReloading = false;
        animator.SetBool("isReloading", false);

        foreach (Transform bullet in magazineGameobject.transform)
        {
            bullet.gameObject.SetActive(false);
        }
    }

    public override void FixedUpdateNetwork(){
        if(animator.GetBool("isAiming") != isAiming){
            animator.SetBool("isAiming", isAiming);
        }
        
        recoil.Update();
        cameraObject.transform.localRotation = Quaternion.Euler(recoil.GetCurrentRotation() +  new Vector3(playerCam.GetMouseXRotation(), 0, 0));
        
        // if(isAiming && transform.position != adsTransform.position && transform.rotation != adsTransform.rotation){
        //     LerpTransform(adsTransform, adsTime);
        //     Debug.Log("Changing to ads position");
        // }else if(!isAiming && transform.position != hipTransform.position && transform.rotation != hipTransform.rotation){
        //     LerpTransform(hipTransform, adsTime);   
        //     Debug.Log("Changing to hip position");
        // }

        LerpTransform(isAiming ? adsTransform : hipTransform, adsTime);
    }

    private void LerpTransform(TransformData newTransform, float time){
        var newPosition = Vector3.Lerp(transform.localPosition, newTransform.position, Runner.DeltaTime * time);
        var newRotation = Quaternion.Lerp(transform.localRotation, newTransform.rotation, Runner.DeltaTime * time);
        transform.SetLocalPositionAndRotation(newPosition, newRotation);
    }
}