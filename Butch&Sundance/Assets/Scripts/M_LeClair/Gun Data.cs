using UnityEngine;

[CreateAssetMenu(fileName = "GunData", menuName = "Gun Data")]
public class GunData : ScriptableObject
{
    [Header("Identity")]
    public string GunName;
    public AlienTech_Pickup.GunTypeMod GunType;

    [Header("Stats")]
    public float ShootDistance;
    public float FireRate;
    public float ReloadSpeed;

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
}
