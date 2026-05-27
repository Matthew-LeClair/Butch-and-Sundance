using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Manages all player weapon behaviour - shooting, aiming, ammo tracking, and weapon switching.
// Maintains parallel lists (aTechPool, MaxAmmo, CurrAmmo) where each index represents one weapon slot,
// so the active weapon's stats are always accessed via Active_aTech as the shared index.
public class PlayerGun : MonoBehaviour
{

    //===[Config]===\\

    [Header("Config")]
    [SerializeField] LayerMask IgnoreLayer; // Layers the shoot raycast should pass through - typically the Player layer

    [SerializeField] public Transform GunPivot;  // Pivot point used by animations or visual rotation
    [SerializeField] Transform ShootPos;         // World position the raycast originates from (unused in current raycast - kept for future muzzle FX)


    //===[Stats]===\\

    [Header("Stats")]
    public int BaseMinDamage;     // Unmodified minimum damage - source of truth used by Aim() to scale from
    public int MinDamage;         // Active minimum damage - equals Base unless aim boost is active
    public int BaseMaxDamage;     // Unmodified maximum damage
    public int MaxDamage;         // Active maximum damage
    public float FireRate;        // Minimum seconds between shots - lower = faster
    public float ShootTimer;      // Accumulates Time.deltaTime each frame; shot fires when this >= FireRate
    public float ShootDistance;   // Maximum raycast distance in world units
    public List<int> MaxAmmo;     // Per-slot maximum ammo - indexed in parallel with aTechPool
    public int ActiveMaxAmmo;     // Convenience mirror of MaxAmmo[Active_aTech] - kept for UI reads
    public List<int> CurrAmmo;    // Per-slot current ammo - decremented on each shot
    public int ActiveCurrAmmo;    // Convenience mirror of CurrAmmo[Active_aTech] - kept for UI reads
    public float ReloadSpeed;     // Duration in seconds of the reload animation (referenced externally)


    //===[Aim]===\\

    [Header("Aim")]
    public bool IsAiming;                      // True while the player is in aimed fire mode - read by PlayerController for time scale
    [SerializeField] Camera PlayerCamera;      // The player's main camera - used to fetch CameraController
    CameraController CC;                       // Cached CameraController used for any camera-driven aim effects


    //===[Alien Tech]===\\

    [Header("Alien Tech")]
    public List<AlienTech> aTechPool;          // Active weapon arsenal - null slots are empty; non-null slots are equipped guns
    public int Active_aTech;                   // Index of the currently wielded weapon in aTechPool
    [SerializeField] public Mesh BaseMesh;     // The default revolver mesh shown when no AlienTech is equipped
    [SerializeField] public List<Mesh> GunMeshes; // Meshes indexed by GunTypeMod int value - updated by pickup


    //===[Shotgun]===\\

    [Header("Shotgun")]
    [SerializeField] public bool Spread;       // True when the active weapon fires a pellet spread instead of a single ray
    [SerializeField] public int PelletCount;   // Number of individual pellets fired per shot when Spread is true
    [SerializeField] public int SpreadAngle;   // Half-angle of the spread cone in degrees - larger = wider


    //===[Lifecycle]===\\

    // Called once by Unity before the first frame.
    // Caches the CameraController, ensures MaxAmmo and CurrAmmo are sized to match aTechPool,
    // then initializes the starting weapon slot. MaxAmmo and CurrAmmo start as empty lists in the
    // Inspector so the while loops must run before any index access or every subsequent line throws.
    private void Start()
    {
        CC = PlayerCamera.GetComponent<CameraController>(); // Cache CameraController from the assigned camera

        while (MaxAmmo.Count < aTechPool.Count) { MaxAmmo.Add(0); }   // Pad MaxAmmo to match aTechPool size - avoids index out of range on first access
        while (CurrAmmo.Count < aTechPool.Count) { CurrAmmo.Add(0); } // Pad CurrAmmo to match aTechPool size - avoids index out of range on first access

        if (aTechPool[Active_aTech] != null)  // If an AlienTech weapon occupies the starting slot...
        {
            aTechPool[Active_aTech].SwitchGun(); // Apply that weapon's archetype stats immediately
        }
        else // No AlienTech in slot 0 - initialize as the base revolver
        {
            ShootDistance = 8;                           // Medium range
            Spread = false;                              // Single projectile
            FireRate = .6f;                              // Moderate fire rate
            ReloadSpeed = .85f;                          // Fast reload
            MaxAmmo[Active_aTech] = Random.Range(3, 5); // Set this slot's ammo - do NOT Add()
            CurrAmmo[Active_aTech] = MaxAmmo[Active_aTech]; // Fill to max
            BaseMinDamage = Random.Range(4, 12);         // Low base damage
            BaseMaxDamage = (int)(BaseMinDamage * Random.Range(1.25f, 2f)); // Max is 1.25x-2x the min
        }
    }

    // Called every frame by Unity.
    // Advances the shoot cooldown timer - this is the only weapon logic that belongs in Update.
    // All stat configuration is handled once in Start() and SwitchWeapons() - never re-run here.
    private void Update()
    {
        ShootTimer += Time.deltaTime; // Accumulate time toward the next allowed shot
    }


    //===[Shooting]===\\

    // Called from PlayerController.HandleInput() when Fire1 is held.
    // Guards against the fire rate timer and current ammo before casting.
    // Fires either a single raycast or a spread of pellet raycasts depending on the active weapon type,
    // then decrements ammo and calls DestroyActiveGun() when the clip hits zero.
    public void Shoot()
    {
        if (ShootTimer >= FireRate) // Only shoot if enough time has passed since the last shot
        {
            ShootTimer = 0; // Reset the cooldown timer for the next shot

            RaycastHit hit; // Will store whatever the raycast collides with

            if (!Spread) // Single projectile path
            {
                if (Physics.Raycast(
                    Camera.main.transform.position,  // Origin: camera position
                    Camera.main.transform.forward,   // Direction: camera forward
                    out hit,                         // Populate hit with collision data
                    ShootDistance,                   // Maximum travel distance
                    ~IgnoreLayer))                   // Exclude the player's own layer
                {
                    Debug.Log(hit.collider.name); // Debug: log name of hit object

                    I_Damage dmg = hit.collider.GetComponent<I_Damage>(); // Try to get the damage interface on the hit object

                    if (dmg != null) // Only apply damage if the hit object implements I_Damage
                    {
                        int Damage = Random.Range(MinDamage, MaxDamage);          // Roll damage in the active range
                        if (IsAiming) { Damage = (int)(Damage * 1.5f); }          // Aim multiplier stacks on top of the rolled value
                        dmg.TakeDamage(Damage, aTechPool[Active_aTech] != null);  // Pass whether this is an AlienTech shot
                    }
                }
            }
            else // Spread / shotgun path - fire one ray per pellet
            {
                for (int i = 0; i < PelletCount; i++) // One iteration per pellet
                {
                    Vector3 spreadDirection =
                        Camera.main.transform.forward                          // Start from camera forward
                        + new Vector3(
                            Random.Range(-SpreadAngle, SpreadAngle) * 0.01f,  // Small horizontal deviation - scaled down to keep in radians range
                            Random.Range(-SpreadAngle, SpreadAngle) * 0.01f,  // Small vertical deviation
                            0f);                                               // No Z deviation needed

                    if (Physics.Raycast(
                        Camera.main.transform.position, // Origin: camera position
                        spreadDirection.normalized,     // Direction: randomized spread vector normalized
                        out hit,                        // Populate hit with collision data
                        ShootDistance,                  // Maximum travel distance
                        ~IgnoreLayer))                  // Exclude the player's own layer
                    {
                        Debug.Log(hit.collider.name); // Debug: log name of hit object

                        I_Damage dmg = hit.collider.GetComponent<I_Damage>(); // Try to get the damage interface on the hit object

                        if (dmg != null) // Only apply damage if the hit object implements I_Damage
                        {
                            int Damage = Random.Range(MinDamage, MaxDamage);          // Roll damage in the active range
                            if (IsAiming) { Damage = (int)(Damage * 1.5f); }          // Aim multiplier stacks on top
                            dmg.TakeDamage(Damage, aTechPool[Active_aTech] != null);  // Pass whether this is an AlienTech shot
                        }
                    }
                }
            }

            CurrAmmo[Active_aTech]--; // Consume one round from the active weapon's clip

            if (CurrAmmo[Active_aTech] <= 0) // Clip is empty - this weapon is spent
            {
                DestroyActiveGun(); // Revert mods, remove weapon from pool, and advance to the next
            }
        }
    }


    //===[Aim]===\\

    // Called from PlayerController.HandleInput() on both Fire2 down and Fire2 up.
    // Toggles IsAiming and scales MinDamage/MaxDamage up from their base values when entering aim,
    // or restores them to base values when exiting. The toggle must flip IsAiming at the end so
    // the next call reads the updated state correctly.
    public void Aim()
    {
        if (!IsAiming) // Entering aim mode
        {
            MinDamage = (int)(BaseMinDamage * Random.Range(1.5f, 3)); // Boost min damage by 1.5x-3x
            MaxDamage = (int)(BaseMaxDamage * Random.Range(1.5f, 3)); // Boost max damage by the same range
        }
        else // Exiting aim mode
        {
            MinDamage = BaseMinDamage; // Restore min damage to unmodified base
            MaxDamage = BaseMaxDamage; // Restore max damage to unmodified base
        }

        IsAiming = !IsAiming; // Flip the flag AFTER the branch so this call always transitions correctly
    }


    //===[Weapon Switching]===\\

    // Called from PlayerController.HandleInput() on R press to manually cycle the arsenal.
    // Advances Active_aTech to the next slot, wrapping back to 0 when the end is reached.
    // Updates gun stats via SwitchGun() and swaps the mesh to match the newly active weapon type.
    public void SwitchWeapons()
    {
        if (aTechPool.Count > Active_aTech + 1) // There is a slot after the current one
        {
            Active_aTech++;                                                                                    // Advance to the next weapon
            aTechPool[Active_aTech].SwitchGun();                                                               // Apply the new weapon's archetype stats
            gameObject.GetComponent<MeshFilter>().sharedMesh = GunMeshes[(int)aTechPool[Active_aTech].typeMod]; // sharedMesh assigns the asset directly - typeMod is the runtime value set by EventPickUp(), not puTypeMod which is Inspector-only
        }
        else // Already at the last slot - wrap to the beginning
        {
            Active_aTech = 0;                                                                                  // Reset to first slot

            if (aTechPool[Active_aTech] != null) // If slot 0 has a weapon...
            {
                aTechPool[Active_aTech].SwitchGun();                                                           // Apply its stats
                gameObject.GetComponent<MeshFilter>().sharedMesh = GunMeshes[(int)aTechPool[Active_aTech].typeMod]; // sharedMesh assigns the asset directly - typeMod is the runtime value set by EventPickUp(), not puTypeMod which is Inspector-only
            }
            else // Slot 0 is empty - fall back to the base revolver
            {
                gameObject.GetComponent<MeshFilter>().sharedMesh = BaseMesh; // sharedMesh assigns the asset directly - show the default revolver mesh
            }
        }
    }

    // Called from Shoot() when the active weapon's CurrAmmo drops to zero or below.
    // Reverts all mods the weapon applied to the player before removing it, enforcing the risk/reward economy.
    // Clamps Active_aTech after removal and applies the next weapon's stats directly - SwitchWeapons() is
    // intentionally NOT called here because it would increment the index again on the now-shorter list.
    public void DestroyActiveGun()
    {
        aTechPool[Active_aTech].RevertMods(); // Undo every stat buff this weapon's mods applied to the player

        MaxAmmo.RemoveAt(Active_aTech);       // Remove this slot's max ammo entry to keep lists in sync
        CurrAmmo.RemoveAt(Active_aTech);      // Remove this slot's current ammo entry
        aTechPool.RemoveAt(Active_aTech);     // Remove the weapon from the pool - all three lists are now one shorter

        if (aTechPool.Count > 0) // If any weapons remain in the arsenal...
        {
            // Clamp BEFORE any list access - if we just removed the last index, Active_aTech is now out of range
            Active_aTech = Mathf.Clamp(Active_aTech, 0, aTechPool.Count - 1);

            // Apply the now-active weapon's stats and mesh directly rather than calling SwitchWeapons(),
            // which would increment Active_aTech again and go out of range on the freshly shortened list
            aTechPool[Active_aTech].SwitchGun();
            gameObject.GetComponent<MeshFilter>().sharedMesh = GunMeshes[(int)aTechPool[Active_aTech].typeMod]; // sharedMesh assigns the asset directly - typeMod is the runtime value set by EventPickUp(), not puTypeMod which is Inspector-only
        }
        else // Arsenal is completely empty - fall back to the base revolver
        {
            Active_aTech = 0;                                                   // Reset index ready for the next pickup
            gameObject.GetComponent<MeshFilter>().sharedMesh = BaseMesh;        // sharedMesh assigns the asset directly - show the default revolver mesh
        }
    }
}