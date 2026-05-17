using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGun : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] LayerMask IgnoreLayer;

    [SerializeField] public Transform GunPivot;
    [SerializeField] Transform ShootPos;

    [Header("Stats")]
    public int MinDamage;
    public int MaxDamage;

    public float FireRate;
    public float ShootTimer;

    public float ShootDistance;

    public int MaxAmmo;
    public int CurrAmmo;
    int AmmoReserve;

    public float ReloadSpeed;

    [Header("Aim")]
    public bool IsAiming;
    Camera PlayerCamera;
    CameraController CC;

    [Header("Alien Tech")]
    public AlienTech aTech;

    [Header("Shotgun")]
    [SerializeField] public bool Spread;
    [SerializeField] public int PelletCount;
    [SerializeField] public int SpreadAngle;

    private void Start()
    {
        PlayerCamera = GameManager.Instance.Player.GetComponent<Camera>();
        CC = PlayerCamera.GetComponent<CameraController>();

        if (aTech != null) { aTech.SwitchGun(); }
        else
        {
            ShootDistance = 8;
            Spread = false;
            FireRate = .6f;
            ReloadSpeed = .85f;
            MaxAmmo = Random.Range(3, 5);
            MinDamage = Random.Range(4, 12);
            MaxDamage = (int)(MinDamage * Random.Range(1.25f, 2f));
        }
    }

    private void Update()
    { 
        ShootTimer += Time.deltaTime;
    }

    public void Shoot() 
    {
        if (ShootTimer >= FireRate) 
        {
            ShootTimer = 0; // Reset Shoot Timer
            RaycastHit hit; // Init the Raycast Hit
            if (!Spread)
            {
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
            else 
            {
                // Fire multiple pellets in a spread pattern to simulate a shotgun blast
                for (int i = 0; i < PelletCount; i++) // Loop through each pellet
                {
                    // Calculate a random spread direction based on the camera's forward direction
                    Vector3 spreadDirection = Camera.main.transform.forward // Base direction is Camera Forward
                        + new Vector3(
                            Random.Range(-SpreadAngle, SpreadAngle), // Random horizontal spread
                            Random.Range(-SpreadAngle, SpreadAngle), // Random vertical spread
                            0f); // No spread on Z axis

                    // If the Ray Hits then it will apply Damage after Debug Printing
                    if (Physics.Raycast(Camera.main.transform.position, // Ray Position is Camera Position
                        spreadDirection.normalized,                      // Ray Direction is Spread Direction (Normalized)
                        out hit,                                         // Hit is now what the Raycast Hit
                        ShootDistance,                                   // Ray is as long as Shoot Distance
                        ~IgnoreLayer))                                   // It will now Ignore the Player in Collision
                    {
                        Debug.Log(hit.collider.name); // Debug Print the name of what the Ray Collides with
                        I_Damage dmg = hit.collider.GetComponent<I_Damage>(); // Init and Set the Interface as the Hit GameObjects Version
                        if (dmg != null) // If the GameObjects Damage Interface is NOT Null...
                        { dmg.TakeDamage(Random.Range(MinDamage, MaxDamage), aTech != null); } // Hit GameObject takes Damage
                    }
                }
            }
        }
    }

    public void Reload()
    {
        if (CurrAmmo < MaxAmmo && AmmoReserve > 0 && !IsReloading) // If Ammo is Needed, there is Reserve, and not already Reloading...
        { StartCoroutine(ReloadCoroutine()); } // Start the Reload Coroutine
    }

    bool IsReloading = false; // Track if the Gun is currently Reloading

    IEnumerator ReloadCoroutine()
    {
        IsReloading = true; // Set Reloading to True
        yield return new WaitForSeconds(ReloadSpeed); // Wait for the Reload Speed Duration
        int AmmoNeeded = MaxAmmo - CurrAmmo;   // Calculate how much Ammo is Needed
        AmmoReserve -= AmmoNeeded; // Subtract the Needed Ammo from the Reserve
        CurrAmmo = MaxAmmo; // Refill Current Ammo to Max
        IsReloading = false; // Set Reloading to False
        Debug.Log(CurrAmmo); // Debug Print the Current Ammo
    }

    public void Aim()
    {
        if (!IsAiming) { }
        else { }
    }
}
