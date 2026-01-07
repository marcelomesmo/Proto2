using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public GameObject weaponPrefab; // The visual representation of the weapon
    [Tooltip("Weapon UI Icon")]
    public GameObject weaponUIPrefab;
    [Tooltip("Prefab to shoot")]
    public GameObject projectilePrefab;
    [Tooltip("Projectile UI Icon")]
    public GameObject projectileUIPrefab;
    [Tooltip("Define if weapon can aim up")]
    public bool canAimUp;
    public float damagePerBullet;
    [Tooltip("Time between shots / smaller = higher rate of fire")]
    public float fireRate;
    [Tooltip("Time required to reload")]
    public float reloadTime;
    [Tooltip("Distance (in world units) traversed by the projectile")]
    public float range;
    [Tooltip("Max Ammo capacity")]
    public int ammoCapacity;
    public AudioClip shootAudio;
    public AudioClip reloadAudio;
    // Add other weapon-specific properties as needed
}
