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
        // Find the matching GunData asset for this weapon's type
        GunData data = GunLibrary.Find(g => g.GunType == typeMod);

        if (data == null)
        {
            Debug.LogWarning("AlienTech: No GunData found for type " + typeMod);
            return;
        }

        if (eGun != null)
        {
            eGun.ShootDistance = (int)data.ShootDistance;
            eGun.Spread = data.HasSpread;
            eGun.ShootRate = data.FireRate;
            eGun.ReloadSpeed = data.ReloadSpeed;
            eGun.MaxAmmo = Random.Range(data.AmmoMin, data.AmmoMax + 1);
            eGun.DamageMin = Random.Range(data.DamageMin, data.DamageMax);
            eGun.DamageMax = (int)(eGun.DamageMin * Random.Range(1.5f, 2.5f));

            if (data.HasSpread)
            {
                eGun.PelletCount = Random.Range(data.PelletCountMin, data.PelletCountMax + 1);
                eGun.SpreadAngle = Random.Range(data.SpreadAngleMin, data.SpreadAngleMax + 1);
            }
        }

        if (pGun != null)
        {
            pGun.ShootDistance = data.ShootDistance;
            pGun.Spread = data.HasSpread;
            pGun.FireRate = data.FireRate;
            pGun.ReloadSpeed = data.ReloadSpeed;
            pGun.MaxAmmo[pGun.Active_aTech] = Random.Range(data.AmmoMin, data.AmmoMax + 1);
            pGun.CurrAmmo[pGun.Active_aTech] = pGun.MaxAmmo[pGun.Active_aTech];
            pGun.BaseMinDamage = Random.Range(data.DamageMin, data.DamageMax);
            pGun.BaseMaxDamage = (int)(pGun.BaseMinDamage * Random.Range(1.5f, 2.5f));

            if (data.HasSpread)
            {
                pGun.PelletCount = Random.Range(data.PelletCountMin, data.PelletCountMax + 1);
                pGun.SpreadAngle = Random.Range(data.SpreadAngleMin, data.SpreadAngleMax + 1);
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