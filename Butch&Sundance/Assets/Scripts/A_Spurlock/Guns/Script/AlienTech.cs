using System.Collections.Generic;
using UnityEngine;

// Defines a specific alien weapon's type and manages the list of WeaponMods attached to it.
// Extends AlienTech_Pickup so it shares the GunTypeMod enum and pickup flow while adding stat-configuration and mod logic.
// This is the component placed on the weapon GameObjects that live in PlayerGun.aTechPool.
public class AlienTech : AlienTech_Pickup
{

    //===[Config]===\\

    [Header("Config")]
    [SerializeField] public GunTypeMod typeMod; // Which gun archetype this weapon uses - drives the SwitchGun() stat block


    //===[Mods]===\\

    [Header("Mods")]
    List<WeaponMod> Mods = new List<WeaponMod>(); // All mods currently active on this weapon - applied on equip, reverted on destroy/switch


    //===[References]===\\

    [Header("Do NOT Touch!")]
    public PlayerGun pGun; // Reference to the player's gun component - set by AlienTech_Pickup.EventPickUp()
    public Gun eGun;        // Reference to an enemy gun component - set when this script is attached to an enemy weapon


    //===[Lifecycle]===\\

    // Called once by Unity before the first frame.
    // AlienTech lives as a runtime component on the player's gun, not as a world pickup object.
    // Intentionally suppresses AlienTech_Pickup.Start() - mesh assignment and scale setup do not apply here
    // because this component is added via AddComponent at runtime and has no Inspector-assigned GunMeshes.
    public override void Start() { }


    //===[Gun Config]===\\

    // Called once from PlayerGun.Start(), PlayerGun.SwitchWeapons(), and AlienTech_Pickup.EventPickUp().
    // Sets all gun stats on either the enemy Gun or the player PlayerGun to match the selected GunTypeMod archetype.
    // Stats are randomized within archetype-appropriate ranges to give each weapon a unique feel within its class.
    public void SwitchGun()
    {
        if (eGun != null) // Only configure enemy gun if one is assigned
        {
            switch (typeMod)
            {
                case GunTypeMod.Pistol:
                    eGun.ShootDistance = 8;                                              // Medium range
                    eGun.Spread = false;                                                 // Single projectile
                    eGun.ShootRate = .5f;                                                // Moderate fire rate
                    eGun.ReloadSpeed = .75f;                                             // Fast reload
                    eGun.MaxAmmo = Random.Range(4, 6);                                   // Small clip
                    eGun.DamageMin = Random.Range(5, 15);                                // Low-to-mid base damage
                    eGun.DamageMax = (int)(eGun.DamageMin * Random.Range(1.5f, 2.5f));  // Max is 1.5x-2.5x the min
                    break;

                case GunTypeMod.Shotgun:
                    eGun.ShootDistance = 5;                                              // Short range
                    eGun.Spread = true;                                                  // Multi-pellet spread
                    eGun.PelletCount = Random.Range(4, 8);                               // 4 to 8 pellets per shot
                    eGun.SpreadAngle = Random.Range(25, 45);                             // Wide cone
                    eGun.ShootRate = .8f;                                                // Slow fire rate
                    eGun.ReloadSpeed = .5f;                                              // Slower reload
                    eGun.MaxAmmo = Random.Range(1, 2);                                   // Very small clip - high risk
                    eGun.DamageMin = Random.Range(10, 20);                               // Mid base damage per pellet
                    eGun.DamageMax = (int)(eGun.DamageMin * Random.Range(1.5f, 2.5f));  // Max is 1.5x-2.5x the min
                    break;

                case GunTypeMod.SMG:
                    eGun.ShootDistance = 8;                                              // Medium range
                    eGun.Spread = false;                                                 // Single projectile
                    eGun.ShootRate = .2f;                                                // Very fast fire rate
                    eGun.ReloadSpeed = 1f;                                               // Standard reload
                    eGun.MaxAmmo = Random.Range(28, 32);                                 // Large clip
                    eGun.DamageMin = Random.Range(10, 17);                               // Low-to-mid damage per shot
                    eGun.DamageMax = (int)(eGun.DamageMin * Random.Range(1.5f, 2.5f));  // Max is 1.5x-2.5x the min
                    break;

                case GunTypeMod.AssualtRifle:
                    eGun.ShootDistance = 10;                                             // Long-medium range
                    eGun.Spread = false;                                                 // Single projectile
                    eGun.ShootRate = .1f;                                                // Fastest fire rate
                    eGun.ReloadSpeed = 1.1f;                                             // Slightly slow reload
                    eGun.MaxAmmo = Random.Range(32, 52);                                 // Largest clip
                    eGun.DamageMin = Random.Range(10, 20);                               // Mid base damage
                    eGun.DamageMax = (int)(eGun.DamageMin * Random.Range(1.5f, 2.5f));  // Max is 1.5x-2.5x the min
                    break;

                case GunTypeMod.Sniper:
                    eGun.ShootDistance = 15;                                             // Maximum range
                    eGun.Spread = false;                                                 // Single projectile
                    eGun.ShootRate = .9f;                                                // Slowest fire rate
                    eGun.ReloadSpeed = 1.5f;                                             // Slowest reload
                    eGun.MaxAmmo = Random.Range(1, 5);                                   // Tiny clip - high risk
                    eGun.DamageMin = Random.Range(15, 30);                               // Highest base damage
                    eGun.DamageMax = (int)(eGun.DamageMin * Random.Range(1.5f, 2.5f));  // Max is 1.5x-2.5x the min
                    break;
            }
        }

        if (pGun != null) // Only configure player gun if one is assigned
        {
            switch (typeMod)
            {
                case GunTypeMod.Pistol:
                    pGun.ShootDistance = 8;                                                        // Medium range
                    pGun.Spread = false;                                                           // Single projectile
                    pGun.FireRate = .5f;                                                           // Moderate fire rate
                    pGun.ReloadSpeed = .75f;                                                       // Fast reload
                    pGun.MaxAmmo[pGun.Active_aTech] = Random.Range(4, 6);                          // Set this slot's ammo count - do NOT Add()
                    pGun.CurrAmmo[pGun.Active_aTech] = pGun.MaxAmmo[pGun.Active_aTech];            // Fill to max on equip
                    pGun.BaseMinDamage = Random.Range(5, 15);                                      // Low-to-mid base damage
                    pGun.BaseMaxDamage = (int)(pGun.BaseMinDamage * Random.Range(1.5f, 2.5f));    // Max is 1.5x-2.5x the min
                    break;

                case GunTypeMod.Shotgun:
                    pGun.ShootDistance = 5;                                                        // Short range
                    pGun.Spread = true;                                                            // Multi-pellet spread
                    pGun.PelletCount = Random.Range(4, 8);                                         // 4 to 8 pellets per shot
                    pGun.SpreadAngle = Random.Range(25, 45);                                       // Wide cone
                    pGun.FireRate = .8f;                                                           // Slow fire rate
                    pGun.ReloadSpeed = .5f;                                                        // Slower reload
                    pGun.MaxAmmo[pGun.Active_aTech] = Random.Range(1, 2);                          // Very small clip
                    pGun.CurrAmmo[pGun.Active_aTech] = pGun.MaxAmmo[pGun.Active_aTech];            // Fill to max on equip
                    pGun.BaseMinDamage = Random.Range(10, 20);                                     // Mid base damage per pellet
                    pGun.BaseMaxDamage = (int)(pGun.BaseMinDamage * Random.Range(1.5f, 2.5f));    // Max is 1.5x-2.5x the min
                    break;

                case GunTypeMod.SMG:
                    pGun.ShootDistance = 8;                                                        // Medium range
                    pGun.Spread = false;                                                           // Single projectile
                    pGun.FireRate = .2f;                                                           // Very fast fire rate
                    pGun.ReloadSpeed = 1f;                                                         // Standard reload
                    pGun.MaxAmmo[pGun.Active_aTech] = Random.Range(28, 32);                        // Large clip
                    pGun.CurrAmmo[pGun.Active_aTech] = pGun.MaxAmmo[pGun.Active_aTech];            // Fill to max on equip
                    pGun.BaseMinDamage = Random.Range(10, 17);                                     // Low-to-mid damage per shot
                    pGun.BaseMaxDamage = (int)(pGun.BaseMinDamage * Random.Range(1.5f, 2.5f));    // Max is 1.5x-2.5x the min
                    break;

                case GunTypeMod.AssualtRifle:
                    pGun.ShootDistance = 10;                                                       // Long-medium range
                    pGun.Spread = false;                                                           // Single projectile
                    pGun.FireRate = .1f;                                                           // Fastest fire rate
                    pGun.ReloadSpeed = 1.1f;                                                       // Slightly slow reload
                    pGun.MaxAmmo[pGun.Active_aTech] = Random.Range(32, 52);                        // Largest clip
                    pGun.CurrAmmo[pGun.Active_aTech] = pGun.MaxAmmo[pGun.Active_aTech];            // Fill to max on equip
                    pGun.BaseMinDamage = Random.Range(10, 20);                                     // Mid base damage
                    pGun.BaseMaxDamage = (int)(pGun.BaseMinDamage * Random.Range(1.5f, 2.5f));    // Max is 1.5x-2.5x the min
                    break;

                case GunTypeMod.Sniper:
                    pGun.ShootDistance = 15;                                                       // Maximum range
                    pGun.Spread = false;                                                           // Single projectile
                    pGun.FireRate = .9f;                                                           // Slowest fire rate
                    pGun.ReloadSpeed = 1.5f;                                                       // Slowest reload
                    pGun.MaxAmmo[pGun.Active_aTech] = Random.Range(1, 5);                          // Tiny clip
                    pGun.CurrAmmo[pGun.Active_aTech] = pGun.MaxAmmo[pGun.Active_aTech];            // Fill to max on equip
                    pGun.BaseMinDamage = Random.Range(15, 30);                                     // Highest base damage
                    pGun.BaseMaxDamage = (int)(pGun.BaseMinDamage * Random.Range(1.5f, 2.5f));    // Max is 1.5x-2.5x the min
                    break;
            }
        }
    }


    //===[Mod Management]===\\

    // Called from AlienTech_Pickup.EventPickUp() once per mod slot on the pickup.
    // Creates a new WeaponMod, registers it in the local list, then applies its randomized bonus to the player.
    // Intended as the hook for the mod-shop UI as well - call this when the player purchases a mod.
    public void AddMod()
    {
        WeaponMod NewMod = new WeaponMod(); // Create a fresh uninitialized mod
        Mods.Add(NewMod);                   // Track it so RevertMods() can undo it later
        NewMod.ApplyBonus();                // Randomize the mod type/amount and apply it to the player immediately
    }

    // Called from PlayerGun.DestroyActiveGun() before this weapon is removed from the pool.
    // Iterates the mod list and calls RevertBonus() on each one to undo every stat change this weapon applied.
    // Clearing the list after reverting prevents double-reversion if this method is ever called more than once.
    public void RevertMods()
    {
        foreach (WeaponMod mod in Mods) // Loop through every mod this weapon applied
        {
            mod.RevertBonus(); // Undo the stat change for this individual mod
        }
        Mods.Clear(); // Empty the list - weapon is being destroyed so the mods no longer exist
    }
}