using System.Collections.Generic;
using UnityEngine;

public class PlayerGun : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] LayerMask IgnoreLayer;


    [Header("Stats")]
    int MinDamage;
    int MaxDamage;

    float FireRate;
    float ShootTimer;

    float ShootDistance;

    int MaxAmmo;
    int CurrAmmo;
    int AmmoReserve;

    [Header("Aim")]
    public bool IsAiming;
    Camera PlayerCamera;
    CameraController CC;


    [Header("Mods")]
    List<WeaponMod> Mods;

    [Header("Alien Tech")]
    AlienTech aTech;

    private void Start()
    {
        PlayerCamera = GameManager.Instance.Player.GetComponent<Camera>();
        CC = PlayerCamera.GetComponent<CameraController>();
    }

    private void Update()
    { ShootTimer = Time.deltaTime; }

    public void Shoot() 
    {
        if (ShootTimer >= FireRate) 
        {
            ShootTimer = 0; // Reset Shoot Timer

            RaycastHit hit; // Init the Raycast Hit

            // If the Ray Hits then it will apply Damage after Debug Printing
            if (Physics.Raycast(Camera.main.transform.position, // Ray Position is Camera Position 
                Camera.main.transform.forward, // Ray Direction is Camera Forward
                out hit, // Hit is now what the Raycast Hit
                ShootDistance, // Ray is long as Shoot Distance
                ~IgnoreLayer)) // It will now Ignore the Player in Collision
            {
                Debug.Log(hit.collider.name); // Debug Print the name of what the Ray Collides with

                I_Damage dmg = hit.collider.GetComponent<I_Damage>(); // Init and Set the Interface as the Hit GameObjects Version

                if (dmg != null) // If the GameObjects Damage Interface is NOT Null...
                { dmg.TakeDamage(Random.Range(MinDamage, MaxDamage), aTech != null); } // Hit GameObject takes Damage
            }
        }
        
    }

    public void Reload()
    {
        if (MaxAmmo > CurrAmmo && AmmoReserve > 0)
        {
            AmmoReserve = MaxAmmo - CurrAmmo;
            CurrAmmo = MaxAmmo;
        }
        Debug.Log(CurrAmmo);
    }

    // Mayne use this Function for the UI when you buy Mods
    public void AddMod(WeaponMod.Type ModType, float ModAmount)
    {
        WeaponMod NewMod = new WeaponMod();
        NewMod.InitMod(ModType, ModAmount);
        Mods.Add(NewMod);
    }

    public void Aim()
    {
        if (!IsAiming) { }
        else { }
    }
}
