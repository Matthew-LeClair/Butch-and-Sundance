using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class Gun : MonoBehaviour
{
    [SerializeField] public GameObject BulletPrefab;
    [SerializeField] public bool Spread;
    [SerializeField] public int PelletCount;
    [SerializeField] public int SpreadAngle;

    [SerializeField] public float ShootRate;
    float ShootTimer;

    [SerializeField] public Transform GunPivot;
    [SerializeField] public Transform ShootPos;
    [SerializeField] public int ShootDistance;

    public int CurrAmmo;
    [SerializeField] public int MaxAmmo;
    [SerializeField] public float ReloadSpeed;
    int AmmoReserve;

    [SerializeField] public int DamageMin;
    [SerializeField] public int DamageMax;

    List<WeaponMod> Mods;

    public bool IsOut;

    [SerializeField] public AudioSource AudioPlayer;

    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CurrAmmo = MaxAmmo;
        AmmoReserve = MaxAmmo;

        if (!Mods.IsUnityNull())
        {
            foreach (var Mod in Mods)
            { Mod.ApplyBonus(); }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (ShootTimer < ShootRate)
        { ShootTimer += Time.deltaTime; }

        // Draw a Ray for Debugging Shooting
        Debug.DrawRay(ShootPos.position, // Draw Ray at Shoot Position...
            ShootPos.forward * ShootDistance, // Draw it as long as Shoot Distance going in the Direction of ShootPos Forward
            Color.red); // Make the Ray red
    }

    public void Shoot(string cOwnerTag)
    {
        if (ShootTimer > ShootRate) 
        {
            if (CurrAmmo <= 0) { IsOut = true; }

            if (CurrAmmo > 0)
            {
                ShootTimer = 0; // Reset Shoot Timer
                AudioPlayer.Play(); // Play Shoot SFX
                CurrAmmo -= 1; // Decrement Ammo

                BulletPrefab.GetComponent<Damage>().DamageAmount = Random.Range(DamageMin, DamageMax);
                

                if (!Spread)
                {
                    // Spawn Bullet at the Shoot Pos at the Gun Pivot Rotation
                    Instantiate(BulletPrefab, ShootPos.position, ShootPos.rotation);
                }
                else
                {
                    Debug.Log(BulletPrefab.GetComponent<Damage>().DamageAmount);
                    BulletPrefab.GetComponent<Damage>().DamageAmount = 
                        (BulletPrefab.GetComponent<Damage>().DamageAmount / 
                        (PelletCount)) * Random.Range(2, 3);

                    BulletPrefab.GetComponent<Damage>().DamageAmount = 
                        Mathf.Clamp(BulletPrefab.GetComponent<Damage>().DamageAmount, 1, (30));


                    for (int i = 0; i < PelletCount; i++)
                    {
                        Debug.Log(BulletPrefab.GetComponent<Damage>().DamageAmount);
                        float SpreadX = Random.Range(-SpreadAngle, SpreadAngle);
                        float SpreadY = Random.Range(-SpreadAngle, SpreadAngle);

                        Quaternion SpreadRot =
                            ShootPos.rotation *
                            Quaternion.Euler(SpreadX, SpreadY, 0);

                        Instantiate(BulletPrefab, ShootPos.position, SpreadRot);
                        Damage cDamage = BulletPrefab.GetComponent<Damage>();
                        cDamage.OwnerTag = cOwnerTag;
                    }
                }

            }
        }
    }
    public void Reload() 
    {
        Debug.Log("Trying Reload");
        if (MaxAmmo > CurrAmmo && AmmoReserve > 0) 
        {
            AmmoReserve = MaxAmmo - CurrAmmo;
            CurrAmmo = MaxAmmo;
        }
        Debug.Log("Reload Results Below!");
        Debug.Log(CurrAmmo);
    }
}
