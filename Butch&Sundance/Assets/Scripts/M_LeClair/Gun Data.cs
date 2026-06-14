using UnityEngine;

[CreateAssetMenu(fileName = "GunData", menuName = "Gun Data")]
public class GunData : ScriptableObject
{
    public enum ProjectileType
    {
        Basic, 
        Lobbed, 
        Seeking
    }

    [Header("Identity")]
    public string GunName;
    public AlienTech_Pickup.GunTypeMod GunType;

    [Header("Stats")]
    public float ShootDistance;
    public float FireRate;
    public float ReloadSpeed;
    public float MaxRange;

    [Header("Ammo")]
    public int AmmoMin;
    public int AmmoMax;

    [Header("Damage")]
    public int DamageMin;
    public int DamageMax;

    [Header("Spread (Shotgun only)")]
    public bool HasSpread;
    public int PelletCountMin;
    public int PelletCountMax;
    public int SpreadAngleMin;
    public int SpreadAngleMax;

    [Header("Projectile")]
    public ProjectileType BulletType;
    public GameObject BulletPrefab;

    [Header("Audio")]
    public AudioClip[] ShootSFXVariants;
    [Range(0,1)] public float ShootSFX;

    [Header("Visual")]
    public GameObject HitEffect;
}
