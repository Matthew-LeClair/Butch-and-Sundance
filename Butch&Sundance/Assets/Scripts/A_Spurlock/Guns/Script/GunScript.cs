using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] GameObject Bullet;

    [SerializeField] float ShootRate;
    float ShootTimer;

    [SerializeField] public Transform GunPivot;
    [SerializeField] Transform ShootPos;
    [SerializeField] int ShootDistance;

    int CurrAmmo;
    [SerializeField] int MaxAmmo;
    [SerializeField] float ReloadSpeed;

    bool IsOut;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CurrAmmo = MaxAmmo;
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
            if (CurrAmmo-- > 0) { CurrAmmo--; } // Decrement Ammo
            else { IsOut = true; }

            // Spawn Bullet at the Shoot Pos at the Gun Pivot Rotation
            Instantiate(Bullet, ShootPos.position, GunPivot.rotation);
        }
    }
}
