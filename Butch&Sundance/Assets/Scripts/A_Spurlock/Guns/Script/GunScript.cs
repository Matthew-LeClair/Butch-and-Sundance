using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] GameObject Bullet;
    [SerializeField] bool Spread;
    [SerializeField] int PelletCount;
    [SerializeField] int SpreadAngle;

    [SerializeField] float ShootRate;
    float ShootTimer;

    [SerializeField] public Transform GunPivot;
    [SerializeField] Transform ShootPos;
    [SerializeField] int ShootDistance;

    int CurrAmmo;
    [SerializeField] int MaxAmmo;
    [SerializeField] float ReloadSpeed;

    [SerializeField] int DamageMin;
    [SerializeField] int DamageMax;

    List<WeaponMod> Mods;

    bool IsOut;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CurrAmmo = MaxAmmo;

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

    public void Shoot()
    {
        if (CurrAmmo > 0 && ShootTimer >= ShootRate)
        {
            ShootTimer = 0; // Reset Shoot Timer
            if (CurrAmmo - 1 > 0) { CurrAmmo--; } // Decrement Ammo
            else { IsOut = true; }

            Bullet.GetComponent<Damage>().DamageAmount = Random.Range(DamageMin, DamageMax);

            if (!Spread) 
            {
                // Spawn Bullet at the Shoot Pos at the Gun Pivot Rotation
                Instantiate(Bullet, ShootPos.position, GunPivot.rotation);
            }
            else 
            {
                Bullet.GetComponent<Damage>().DamageAmount /= PelletCount;
                
                for (int i = 0; i < PelletCount; i++)
                {
                    float SpreadX = Random.Range(-SpreadAngle, SpreadAngle);
                    float SpreadY = Random.Range(-SpreadAngle, SpreadAngle);

                    Quaternion SpreadRot = 
                        GunPivot.rotation * 
                        Quaternion.Euler(SpreadX, SpreadY, 0);

                    Instantiate(Bullet, ShootPos.position, SpreadRot);
                }
            }

        }
    }

    // Mayne use this Function for the UI when you buy Mods
    public void AddMod(WeaponMod.Type ModType, float ModAmount) 
    {
        WeaponMod NewMod = new WeaponMod();
        NewMod.InitMod(ModType, ModAmount);
        Mods.Add(NewMod);
    }
}
