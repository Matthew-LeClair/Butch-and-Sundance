using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Manages all player weapon behaviour - shooting, aiming, ammo tracking, and weapon switching.
// Maintains parallel lists (aTechPool, MaxAmmo, CurrAmmo) where each index represents one weapon slot,
// so the active weapon's stats are always accessed via Active_aTech as the shared index.
public class PlayerGun : MonoBehaviour
{
    public enum ProjectileType { Basic, Lobbed, Seeking }
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


    [Header("Projectile")]
    public ProjectileType BulletType;
    public GameObject BulletPrefab;


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


    //===[Cache]===\\

    MeshFilter GunMeshFilter; // Cached MeshFilter on this GameObject - fetched once in Start() to avoid repeated GetComponent calls
    GameManager GM;


    public float damageMuliplier;

    //===[Lifecycle]===\\

    // Called once by Unity before the first frame.
    // Caches the CameraController and MeshFilter, ensures MaxAmmo and CurrAmmo are sized to match aTechPool,
    // then initializes the starting weapon slot. MaxAmmo and CurrAmmo start as empty lists in the
    // Inspector so the while loops must run before any index access or every subsequent line throws.
    private void Start()
    {
        CC = PlayerCamera.GetComponent<CameraController>();                    // Cache CameraController from the assigned camera
        GunMeshFilter = gameObject.GetComponent<MeshFilter>();                 // Cache MeshFilter once - avoids repeated GetComponent calls across Shoot, Switch, and Destroy paths
        GM = GameManager.Instance;

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
        GM.OnWeaponChanged(aTechPool[Active_aTech], Active_aTech);
    }

    // Called every frame by Unity.
    // Advances the shoot cooldown timer - this is the only weapon logic that belongs in Update.
    // All stat configuration is handled once in Start() and SwitchWeapons() - never re-run here.
    private void Update()
    {
        ShootTimer += Time.deltaTime; // Accumulate time toward the next allowed shot
    }


    public void Shoot()
    {
        if (ShootTimer >= FireRate)
        {
            ShootTimer = 0;

            if (BulletPrefab == null)
            {
                Debug.LogWarning("No BulletPrefab assigned on PlayerGun");
                return;
            }

            GunData activeData = null;
            if (aTechPool[Active_aTech] != null)
            {
                activeData = aTechPool[Active_aTech].GunLibrary.Find(g => g.GunType == aTechPool[Active_aTech].typeMod);
            }

            if (!Spread)
            {
                GameObject bullet = Instantiate(
                    BulletPrefab,
                    Camera.main.transform.position + Camera.main.transform.forward,
                    Camera.main.transform.rotation);

                Damage dmg = bullet.GetComponent<Damage>();
                if (dmg != null)
                {
                    dmg.DamageAmount = Random.Range(MinDamage, MaxDamage);
                    if (IsAiming) { dmg.DamageAmount = (int)(dmg.DamageAmount * 1.5f); }
                    dmg.OwnerTag = "Player";
                    dmg.IsAlienTech = aTechPool[Active_aTech] != null;
                    dmg.MaxRange = activeData != null ? activeData.MaxRange : 20f;
                }
            }
            else
            {
                for (int i = 0; i < PelletCount; i++)
                {
                    float spreadX = Random.Range(-SpreadAngle, SpreadAngle) * 0.01f;
                    float spreadY = Random.Range(-SpreadAngle, SpreadAngle) * 0.01f;

                    Quaternion spreadRot = Camera.main.transform.rotation *
                        Quaternion.Euler(spreadX, spreadY, 0f);

                    GameObject bullet = Instantiate(
                        BulletPrefab,
                        Camera.main.transform.position + Camera.main.transform.forward,
                        spreadRot);

                    Damage dmg = bullet.GetComponent<Damage>();
                    if (dmg != null)
                    {
                        dmg.DamageAmount = Random.Range(MinDamage, MaxDamage);
                        if (IsAiming) { dmg.DamageAmount = (int)(dmg.DamageAmount * 1.5f); }
                        dmg.OwnerTag = "Player";
                        dmg.IsAlienTech = aTechPool[Active_aTech] != null;
                    }
                }
            }

            CurrAmmo[Active_aTech]--;

            if (CurrAmmo[Active_aTech] <= 0)
            {
                if (Active_aTech == 0)
                {
                    CurrAmmo[Active_aTech] = MaxAmmo[Active_aTech];
                }
                else
                {
                    DestroyActiveGun();
                }
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
    // Walks forward from the current slot (wrapping at the end) and stops at the first non-null entry.
    // Null slots - empty or not yet filled - are skipped entirely so the player only ever lands on real weapons.
    // If no non-null slot exists anywhere in the pool, falls back to the base revolver mesh.
    public void SwitchWeapons()
    {
        for (int i = 1; i <= aTechPool.Count; i++) // Start at 1 so the search always moves forward at least one slot
        {
            int candidate = (Active_aTech + i) % aTechPool.Count; // Wrap-around index - keeps the search within pool bounds

            if (aTechPool[candidate] != null) // Found the next occupied slot - switch to it
            {
                Active_aTech = candidate;
                aTechPool[Active_aTech].SwitchGun();                                             // Apply the new weapon's archetype stats
                GM.OnWeaponChanged(aTechPool[Active_aTech], Active_aTech);
                GunMeshFilter.sharedMesh = GunMeshes[(int)aTechPool[Active_aTech].typeMod];      // sharedMesh assigns the asset directly - typeMod is the runtime value set by EventPickUp(), not puTypeMod which is Inspector-only
                gameObject.transform.localScale = new Vector3(18.75f, 11.71875f, 11.71875f);     // Restore correct display scale after mesh swap
                return;                                                                           // Stop searching - first non-null hit is enough
            }
        }

        // Every slot in the pool is null - fall back to the base revolver
        GunMeshFilter.sharedMesh = BaseMesh;                                                     // sharedMesh assigns the asset directly - show the default revolver mesh
        GM.OnWeaponChanged(null, 0);                                             // Notify GM - reverted to base revolver
        gameObject.transform.localScale = new Vector3(18.75f, 11.71875f, 11.71875f);             // Restore correct display scale after mesh swap
    }

    // Called from Shoot() when the active weapon's CurrAmmo drops to zero or below.
    // Reverts all mods the weapon applied to the player before removing it, enforcing the risk/reward economy.
    // Destroys the AlienTech component from the player's gun before removing the pool entry - without this
    // the component leaks onto the GameObject indefinitely.
    // Clamps Active_aTech after removal and applies the next weapon's stats directly - SwitchWeapons() is
    // intentionally NOT called here because it would increment the index again on the now-shorter list.
    public void DestroyActiveGun()
    {
        if (Active_aTech == 0) { return; } // Slot 0 is permanent - only weapons beyond the first slot are destroyed on empty

        if (aTechPool[Active_aTech] == null) // Guard against a null active slot - cleans up orphaned ammo entries and returns early
        {
            Debug.LogWarning("DestroyActiveGun: slot " + Active_aTech + " is already null - removing orphaned ammo entries.");
            MaxAmmo.RemoveAt(Active_aTech);
            CurrAmmo.RemoveAt(Active_aTech);
            aTechPool.RemoveAt(Active_aTech);
            Active_aTech = Mathf.Clamp(Active_aTech, 0, Mathf.Max(0, aTechPool.Count - 1));
            return;
        }

        aTechPool[Active_aTech].RevertMods();           // Undo every stat buff this weapon's mods applied to the player
        Destroy(aTechPool[Active_aTech]);               // Destroy the AlienTech component from the player's gun - RemoveAt only drops the reference, leaving the component leaked on the GameObject

        MaxAmmo.RemoveAt(Active_aTech);                 // Remove this slot's max ammo entry to keep lists in sync
        CurrAmmo.RemoveAt(Active_aTech);                // Remove this slot's current ammo entry
        aTechPool.RemoveAt(Active_aTech);               // Remove the now-destroyed component reference from the pool - all three lists are now one shorter

        if (aTechPool.Count > 0) // If any weapons remain in the arsenal...
        {
            // Clamp BEFORE any list access - if we just removed the last index, Active_aTech is now out of range
            Active_aTech = Mathf.Clamp(Active_aTech, 0, aTechPool.Count - 1);

            if (aTechPool[Active_aTech] != null) // Slot exists but may still be null - guard before any member access
            {
                // Apply the now-active weapon's stats and mesh directly rather than calling SwitchWeapons(),
                // which would increment Active_aTech again and go out of range on the freshly shortened list
                aTechPool[Active_aTech].SwitchGun();
                // Next weapon branch:
                aTechPool[Active_aTech].SwitchGun();
                GM.OnWeaponChanged(aTechPool[Active_aTech], Active_aTech); // Notify GM - auto-switched after destroy

                // Base revolver fallback:
                GunMeshFilter.sharedMesh = BaseMesh;
                GM.OnWeaponChanged(null, 0); // Notify GM - arsenal empty, back to revolver

                GunMeshFilter.sharedMesh = GunMeshes[(int)aTechPool[Active_aTech].typeMod]; // sharedMesh assigns the asset directly - typeMod is the runtime value set by EventPickUp(), not puTypeMod which is Inspector-only
                gameObject.transform.localScale = new Vector3(18.75f, 11.71875f, 11.71875f);                        // Restore correct display scale after mesh swap
            }
        }
        else // Arsenal is completely empty - fall back to the base revolver
        {
            Active_aTech = 0;                                                                     // Reset index ready for the next pickup
            GunMeshFilter.sharedMesh = BaseMesh;                                                  // sharedMesh assigns the asset directly - show the default revolver mesh
            GM.OnWeaponChanged(null, 0);
            gameObject.transform.localScale = new Vector3(18.75f, 11.71875f, 11.71875f);          // Restore correct display scale after mesh swap
        }
    }
}