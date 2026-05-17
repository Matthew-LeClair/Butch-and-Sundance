using System.Collections.Generic;
using UnityEngine;

public class AlienTech : AlienTech_Pickup
{

    [Header("Config")]
    [SerializeField] public GunTypeMod typeMod;

    [Header("Mods")]
    List<WeaponMod> Mods = new List<WeaponMod>();

    [Header("Do NOT Touch!")]
    public PlayerGun pGun;
    public Gun eGun;


    public void SwitchGun() 
    {
        if (eGun != null) 
        {
            switch (typeMod)
            {
                case GunTypeMod.Pistol:
                    eGun.ShootDistance = 8;
                    eGun.Spread = false;
                    eGun.ShootRate = .5f;
                    eGun.ReloadSpeed = .75f;
                    eGun.MaxAmmo = Random.Range(4, 6);
                    eGun.DamageMin = Random.Range(5, 15);
                    eGun.DamageMax = (int)(eGun.DamageMin * Random.Range(1.5f, 2.5f));
                    break;


                case GunTypeMod.Shotgun:
                    eGun.ShootDistance = 5;
                    eGun.Spread = true;
                    eGun.PelletCount = Random.Range(4, 8);
                    eGun.SpreadAngle = Random.Range(25, 45);
                    eGun.ShootRate = .8f;
                    eGun.ReloadSpeed = .5f;
                    eGun.MaxAmmo = Random.Range(1, 2);
                    eGun.DamageMin = Random.Range(10, 20);
                    eGun.DamageMax = (int)(eGun.DamageMin * Random.Range(1.5f, 2.5f));
                    break;


                case GunTypeMod.SMG:
                    eGun.ShootDistance = 8;
                    eGun.Spread = false;
                    eGun.ShootRate = .2f;
                    eGun.ReloadSpeed = 1f;
                    eGun.MaxAmmo = Random.Range(28, 32);
                    eGun.DamageMin = Random.Range(10, 17);
                    eGun.DamageMax = (int)(eGun.DamageMin * Random.Range(1.5f, 2.5f));
                    break;


                case GunTypeMod.AssualtRifle:
                    eGun.ShootDistance = 10;
                    eGun.Spread = false;
                    eGun.ShootRate = .1f;
                    eGun.ReloadSpeed = 1.1f;
                    eGun.MaxAmmo = Random.Range(32, 52);
                    eGun.DamageMin = Random.Range(10, 20);
                    eGun.DamageMax = (int)(eGun.DamageMin * Random.Range(1.5f, 2.5f));
                    break;


                case GunTypeMod.Sniper:
                    eGun.ShootDistance = 15;
                    eGun.Spread = false;
                    eGun.ShootRate = .9f;
                    eGun.ReloadSpeed = 1.5f;
                    eGun.MaxAmmo = Random.Range(1, 5);
                    eGun.DamageMin = Random.Range(15, 30);
                    eGun.DamageMax = (int)(eGun.DamageMin * Random.Range(1.5f, 2.5f));
                    break;
            }
        }
        
        if (pGun != null) 
        {
            switch (typeMod)
            {
                case GunTypeMod.Pistol:
                    pGun.ShootDistance = 8;
                    pGun.Spread = false;
                    pGun.FireRate = .5f;
                    pGun.ReloadSpeed = .75f;
                    pGun.MaxAmmo = Random.Range(4, 6);
                    pGun.MinDamage = Random.Range(5, 15);
                    pGun.MaxDamage = (int)(pGun.MinDamage * Random.Range(1.5f, 2.5f));
                    break;

                case GunTypeMod.Shotgun:
                    pGun.ShootDistance = 5;
                    pGun.Spread = true;
                    pGun.PelletCount = Random.Range(4, 8);
                    pGun.SpreadAngle = Random.Range(25, 45);
                    pGun.FireRate = .8f;
                    pGun.ReloadSpeed = .5f;
                    pGun.MaxAmmo = Random.Range(1, 2);
                    pGun.MinDamage = Random.Range(10, 20);
                    pGun.MaxDamage = (int)(pGun.MinDamage * Random.Range(1.5f, 2.5f));
                    break;

                case GunTypeMod.SMG:
                    pGun.ShootDistance = 8;
                    pGun.Spread = false;
                    pGun.FireRate = .2f;
                    pGun.ReloadSpeed = 1f;
                    pGun.MaxAmmo = Random.Range(28, 32);
                    pGun.MinDamage = Random.Range(10, 17);
                    pGun.MaxDamage = (int)(pGun.MinDamage * Random.Range(1.5f, 2.5f));
                    break;

                case GunTypeMod.AssualtRifle:
                    pGun.ShootDistance = 10;
                    pGun.Spread = false;
                    pGun.FireRate = .1f;
                    pGun.ReloadSpeed = 1.1f;
                    pGun.MaxAmmo = Random.Range(32, 52);
                    pGun.MinDamage = Random.Range(10, 20);
                    pGun.MaxDamage = (int)(pGun.MinDamage * Random.Range(1.5f, 2.5f));
                    break;

                case GunTypeMod.Sniper:
                    pGun.ShootDistance = 15;
                    pGun.Spread = false;
                    pGun.FireRate = .9f;
                    pGun.ReloadSpeed = 1.5f;
                    pGun.MaxAmmo = Random.Range(1, 5);
                    pGun.MinDamage = Random.Range(15, 30);
                    pGun.MaxDamage = (int)(pGun.MinDamage * Random.Range(1.5f, 2.5f));
                    break;
            }
        }
    }

    // Mayne use this Function for the UI when you buy Mods
    public void AddMod(WeaponMod.Type ModType, float ModAmount)
    {
        WeaponMod NewMod = new WeaponMod();
        NewMod.InitMod(ModType, ModAmount);
        Mods.Add(NewMod);

        foreach (var Mod in Mods)
        { Mod.ApplyBonus(); }
    }
}
