using System;
using System.Collections;
using Entity;
using UnityEngine;
using UnityEngine.Events;
using Enum;
using Gameplay.Projectile;
using Services.Manager;
using UnityEngine.Rendering.Universal;

// TODO: Convert this into a PlayerAttackSubsystem.
public class WeaponManager : MonoBehaviour
{
    [Header("Weapon Data")]
    [Tooltip("Weapon Data")]
    public WeaponData weaponData;

    [Tooltip("End point of gun where shots appear")]
    [SerializeField] private Transform muzzlePositionLeft;
    [SerializeField] private Transform muzzlePositionRight;
    [SerializeField] private Transform muzzlePositionTopLeft;
    [SerializeField] private Transform muzzlePositionTopRight;

    [Header("Events")]
    [SerializeField] private UnityEvent m_WeaponFired;
    [SerializeField] private UnityEvent m_WeaponReloaded;
    private AimState AimDirection { get; set; } = AimState.Right;
    private float _timeToShoot;
    private int _currAmmo = 0;
    public bool IsAmmoEmpty => _currAmmo <= 0;
    public bool IsAmmoFull => _currAmmo == weaponData.ammoCapacity;
    private float _timeToReload;
    
    [Header("Broadcast")]
    [SerializeField] protected IntIntEventChannelSO OnAmmoChanged;
    [SerializeField] protected IntFloatEventChannelSO OnReloadChanged;
    [SerializeField] protected WeaponDataEventChannelSO OnWeaponChanged;

    [Header("Effects")]
    public AudioSource audioSource;
    [SerializeField] private Light2D muzzleLight;
    private const float _maxIntensity = 4f;
    private const float _flashDuration = 0.04f;
    private Coroutine _flashRoutine;
    
    private SpriteRenderer _weaponRenderer;

    // TODO: Deprecated when changing to Subsystem.
    private EntityController _controller;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        
        if(!_weaponRenderer)
            _weaponRenderer = weaponData.weaponPrefab.GetComponentInChildren<SpriteRenderer>();

        if (!weaponData)
            throw new NullReferenceException("Invalid or missing Weapon Data in WeaponManager.");

        // TODO: Deprecated when changing to Subsystem.
        _controller = GetComponentInParent<EntityController>();
        //if(!_controller)
        //    Debug.LogWarning("[WeaponManager] Controller is null.");
        
        _currAmmo = weaponData.ammoCapacity;
        
        if(muzzleLight)
            muzzleLight.intensity = 0f;
    }

    private void Start()
    {
        // Initialize Weapon ammo in HUD
        OnAmmoChanged.RaiseEvent(_currAmmo, weaponData.ammoCapacity);
        OnWeaponChanged.RaiseEvent(weaponData);
    }

    public void Shoot()
    {
        if(!weaponData)
            return;

        if (Time.time <= _timeToShoot) // TBD: refactor this? tldr: can't shoot (weapon cooldown) or pool is empty
            return;

        // TODO:   refactor this
        if (this.audioSource && weaponData.shootAudio && !this.audioSource.isPlaying)
            this.audioSource.PlayOneShot(weaponData.shootAudio);
        
        if(muzzleLight != null)
            TriggerMuzzleFlash();

        /*
            OnSpawn()   // pool
            Configure() // shot
            Launch()    // simulation
         */
        
        // Get a pooled object instead of instantiating
        BaseProjectile bulletObject = ProjectilePoolManager.Instance.Spawn(weaponData.projectilePrefab);
        if (!bulletObject)
            return;

        // Align to gun barrel/muzzle position
        switch(AimDirection)
        {
            case AimState.Right:
                bulletObject.transform.position = muzzlePositionRight.position;
                break;

            case AimState.Left:
                bulletObject.transform.position = muzzlePositionLeft.position;
                break;

            case AimState.TopRight:
                bulletObject.transform.position = muzzlePositionTopRight.position;
                break;

            case AimState.TopLeft:
                bulletObject.transform.position = muzzlePositionTopLeft.position;
                break;
        }
        
        // Configure projectile in the current context.
        
        var context = new ProjectileContext(
            faction: Faction.Player,
            range: weaponData.range,
            bonusPierce: 0,         // playerUpgrades.bonusPierce,    // TODO: Replace this with Player upgrades later on.
            damageMultiplier: 1     // playerStats.damageMultiplier
        );

        var source = new DamageSource(
            faction: Faction.Player,
            controller: _controller,
            sourcePosition: transform.position
        );
        
        bulletObject.Configure(context, source);
        
        bulletObject.Launch(AimDirection);

        // Set cooldown delay
        _timeToShoot = Time.time + weaponData.fireRate;

        _currAmmo--;
        
        m_WeaponFired.Invoke();
        OnAmmoChanged.RaiseEvent(_currAmmo, weaponData.ammoCapacity);

        //Debug.Log("#### Weapon: Fired ####");
    }

    // Cast during player movement and independent of creating the actual projectile
    public void ChangeWeaponDirection(AimState newState)
    {
        AimDirection = newState;
        
        if (!weaponData.canAimUp)
        {
            AimDirection = AimDirection switch
            {
                AimState.TopRight => AimState.Right,
                AimState.TopLeft => AimState.Left,
                _ => AimDirection
            };
        }
        
        switch(AimDirection)
        {
            case AimState.Right:
                _weaponRenderer.flipX = false;
                break;
            case AimState.Left:
                _weaponRenderer.flipX = true;
                break;
            case AimState.TopRight:
                _weaponRenderer.flipX = false;
                break;
            case AimState.TopLeft:
                _weaponRenderer.flipX = true;
                break;
        }
    }

    public void Reload()
    {
        if(!weaponData)
            return;

        if (_timeToReload > 0)
            _timeToReload -= Time.deltaTime;
        else
            FinishReload();
    }
    
    public void ConstantReload()
    {
        if(!weaponData)
            return;

        if (_timeToReload < weaponData.reloadTime)
        {
            _timeToReload += Time.deltaTime;
            OnReloadChanged.RaiseEvent(_currAmmo, GetReloadProgress());
        }
        else
        {
            _timeToReload = 0;
            
            _currAmmo++;
           
            m_WeaponReloaded.Invoke();
            OnAmmoChanged.RaiseEvent(_currAmmo, weaponData.ammoCapacity);
            
            if (audioSource && weaponData.reloadAudio && !audioSource.isPlaying)
                audioSource.PlayOneShot(weaponData.reloadAudio);

            Debug.Log("#### Weapon: Reloaded Secondary Weapon ####");
        }
    }

    public float GetReloadProgress()
    {
        return 1-_timeToReload/weaponData.reloadTime;
    }

    public void StartReload()
    {
        if(!weaponData)
            return;
 
        if (audioSource && weaponData.reloadAudio && !audioSource.isPlaying)
            audioSource.PlayOneShot(weaponData.reloadAudio);

        _timeToReload = weaponData.reloadTime;
    }

    private void FinishReload()
    {
        _currAmmo = weaponData.ammoCapacity;

        m_WeaponReloaded.Invoke();
        OnAmmoChanged.RaiseEvent(_currAmmo, weaponData.ammoCapacity);

        if (this.audioSource && weaponData.reloadAudio && !this.audioSource.isPlaying)
            this.audioSource.PlayOneShot(weaponData.reloadAudio);

        //Debug.Log("#### Weapon: Reloaded ####");
    }
    
    private void TriggerMuzzleFlash()
    {
        //Debug.Log("Flash triggered");
        // Interrupt any existing flash
        if (_flashRoutine != null)
            StopCoroutine(_flashRoutine);

        SetMuzzlePosition();
        _flashRoutine = StartCoroutine(MuzzleFlashRoutine());
    }

    private void SetMuzzlePosition()
    {
        muzzleLight.transform.position = AimDirection switch
        {
            AimState.Right => muzzlePositionRight.position,
            AimState.Left => muzzlePositionLeft.position,
            AimState.TopRight => muzzlePositionTopRight.position,
            AimState.TopLeft => muzzlePositionTopLeft.position,
            _ => muzzleLight.transform.position
        };
    }

    private IEnumerator MuzzleFlashRoutine()
    {
        muzzleLight.intensity = _maxIntensity;

        yield return new WaitForSeconds(_flashDuration);

        muzzleLight.intensity = 0f;
        _flashRoutine = null;
    }
}
