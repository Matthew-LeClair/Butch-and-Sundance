using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] GameObject Bullet;

    [SerializeField] float ShootRate;
    float ShootTimer;

    [SerializeField] Transform GunPivot;
    [SerializeField] Transform ShootPos;
    [SerializeField] int ShootDistance;

    int CurrAmmo;
    [SerializeField] int MaxAmmo;
    [SerializeField] float ReloadSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
        ShootRate = 0; // Reset Shoot Timer

        // Spawn Bullet at the Shoot Pos at the Gun Pivot Rotation
        Instantiate(Bullet, ShootPos.position, GunPivot.rotation);
    }
}
