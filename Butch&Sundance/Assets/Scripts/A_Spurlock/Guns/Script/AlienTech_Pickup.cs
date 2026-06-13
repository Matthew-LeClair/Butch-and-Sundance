using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// Base class for all AlienTech weapon pickups in the world.
// Handles the enum and struct definitions shared across the weapon system, sets the correct mesh on spawn,
// and manages the pickup logic that slots this weapon into the player's arsenal.
public class AlienTech_Pickup : PickUp_Interact
{

    //===[Enums & Structs]===\\

    // Defines every gun archetype available in the weapon system.
    // The int value doubles as an index into PlayerGun.GunMeshes so mesh and type stay in sync.
    public enum GunTypeMod
    {
        Pistol,        // 0 - balanced short-range sidearm
        Shotgun,       // 1 - close-range spread weapon with very limited ammo
        SMG,           // 2 - fast-firing medium-range weapon with large clip
        AssualtRifle,  // 3 - fastest fire rate with the largest clip
        Sniper         // 4 - maximum range and damage but slowest fire rate and tiny clip
    };

    // Serializable representation of a mod slot on this pickup.
    // Used only for designer-facing configuration in the Inspector - AlienTech.AddMod() handles the runtime mod objects.
    [System.Serializable]
    public struct Mod
    {
        public WeaponMod.Type ModType;  // Which stat category this slot targets
        public float ModAmount;         // The multiplier this slot applies - overridden by random roll at runtime
    }


    //===[Config]===\\

    [Header("Config")]
    [SerializeField] public GunTypeMod puTypeMod; // The archetype assigned to this pickup in the Inspector
    [SerializeField] public int ModCount;          // How many random mods this weapon grants when picked up


    //===[Visual]===\\

    [Header("Visual")]
    [SerializeField] public List<Mesh> GunMeshes; // All five gun meshes in GunTypeMod enum order - Pistol, Shotgun, SMG, AssaultRifle, Sniper

    [Header("GunLibrary")]
    [SerializeField] public List<GunData> GunLibrary;

    //===[Lifecycle]===\\

    // Called once by Unity before the first frame.
    // Chains to the base PickUp_Interact.Start() to initialize material tracking, then sets the mesh
    // to match this pickup's puTypeMod so the world object looks like the correct gun type.
    // GunMeshes is indexed by the enum int value - Inspector order must match enum order exactly.
    public override void Start()
    {
        base.Start(); // Initialize outline material tracking in PickUp_Interact
    }


    //===[Pickup]===\\

    // Called from PickUp_Interact.Update() when the player presses E while in range.
    // Finds the first null (free) slot in the player's aTechPool, adds a fresh AlienTech component
    // to the player's gun for that slot, then assigns pGun, applies gun stats, and applies mods.
    // The pickup carries only configuration data - the AlienTech component itself lives on the player's gun.
    // If no free slot exists the pickup silently does nothing - the player's arsenal is full.
    public override void EventPickUp()
    {
        PlayerGun pGun = GameManager.Instance.Player
            .GetComponent<PlayerController>().pGun;         // Fetch PlayerGun directly - pickup is a world object with no parent relationship to the player

        for(int i = 0; i < pGun.aTechPool.Count; i++)
        {
            if (pGun.aTechPool[i] != null && pGun.aTechPool[i].typeMod == puTypeMod)
            {
                pGun.CurrAmmo[i] = pGun.MaxAmmo[i];
                GameManager.Instance.PlayerScript.UpdatePlayerUI();
                Destroy(gameObject);
                return;
            }
        }

        int FreeSlot = -1;                                  // Default to -1 meaning no free slot found

        for (int index = 0; index < pGun.aTechPool.Count; index++) // Walk the pool looking for a free slot
        {
            if (pGun.aTechPool[index] == null)              // Null entry means this slot is available
            { FreeSlot = index; break; }                    // Record the index and stop - first free slot is enough
        }

        if (FreeSlot >= 0) // >= 0 catches slot 0 - the original > 0 silently skipped the first slot every time
        {
            pGun.aTechPool[FreeSlot] = pGun.gameObject.AddComponent<AlienTech>(); // Add a fresh AlienTech to the player's gun - the pickup carries config data only, not the component itself
            pGun.aTechPool[FreeSlot].pGun = pGun;                                 // Assign the PlayerGun reference so SwitchGun() and mod logic can access player stats
            pGun.aTechPool[FreeSlot].typeMod = puTypeMod;                         // Set the archetype on the newly created component
            pGun.aTechPool[FreeSlot].GunLibrary = GunLibrary;
            Debug.Log("Gun Type: " + puTypeMod);                                  // Debug log for pickup confirmation
            pGun.aTechPool[FreeSlot].SwitchGun();                                 // Apply all stat values for this archetype immediately

            for (int index = 0; index < ModCount; index++)                        // Apply each mod slot this pickup carries
            { pGun.aTechPool[FreeSlot].AddMod(); }                                // Create, register, and apply one random mod

            Destroy(gameObject);                                                  // Remove the pickup from the world on successful pickup
        }
    }
}