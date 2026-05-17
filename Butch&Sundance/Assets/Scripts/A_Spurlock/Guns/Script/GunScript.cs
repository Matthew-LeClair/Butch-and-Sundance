using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class Gun : MonoBehaviour
{
    [SerializeField] GameObject Bullet;
    [SerializeField] public bool Spread;
    [SerializeField] int PelletCount;
    [SerializeField] int SpreadAngle;

    [SerializeField] public float ShootRate;
    float ShootTimer;

    [SerializeField] public Transform GunPivot;
    [SerializeField] Transform ShootPos;
    [SerializeField] public int ShootDistance;

    public int CurrAmmo;
    [SerializeField] public int MaxAmmo;
    [SerializeField] public float ReloadSpeed;
    int AmmoReserve;

    [SerializeField] public int DamageMin;
    [SerializeField] public int DamageMax;

    List<WeaponMod> Mods;

    public bool IsOut;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CurrAmmo = MaxAmmo;
        AmmoReserve = MaxAmmo;

        if (!Mods.IsUnityNull())
        {
            foreach (var Mod in Mods)
            { Mod.ApplyBonus(); }
        }
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

    public void Shoot(string cOwnerTag)
    {
        if (ShootTimer > ShootRate) 
        {
            if (CurrAmmo <= 0) { IsOut = true; }

            if (CurrAmmo > 0)
            {
                ShootTimer = 0; // Reset Shoot Timer
                CurrAmmo -= 1; // Decrement Ammo

                Bullet.GetComponent<Damage>().DamageAmount = Random.Range(DamageMin, DamageMax);

                if (!Spread)
                {
                    // Spawn Bullet at the Shoot Pos at the Gun Pivot Rotation
                    Instantiate(Bullet, ShootPos.position, ShootPos.rotation);
                }
                else
                {
                    Debug.Log(Bullet.GetComponent<Damage>().DamageAmount);
                    Bullet.GetComponent<Damage>().DamageAmount = 
                        (Bullet.GetComponent<Damage>().DamageAmount / 
                        (PelletCount)) * Random.Range(2, 3);

                    Bullet.GetComponent<Damage>().DamageAmount = 
                        Mathf.Clamp(Bullet.GetComponent<Damage>().DamageAmount, 1, (30));


                    for (int i = 0; i < PelletCount; i++)
                    {
                        Debug.Log(Bullet.GetComponent<Damage>().DamageAmount);
                        float SpreadX = Random.Range(-SpreadAngle, SpreadAngle);
                        float SpreadY = Random.Range(-SpreadAngle, SpreadAngle);

                        Quaternion SpreadRot =
                            ShootPos.rotation *
                            Quaternion.Euler(SpreadX, SpreadY, 0);

                        Instantiate(Bullet, ShootPos.position, SpreadRot);
                        Damage cDamage = Bullet.GetComponent<Damage>();
                        cDamage.OwnerTag = cOwnerTag;
                    }
                }

            }
        }
    }
    public void Reload() 
    {
        Debug.Log("Trying Reload");
        if (MaxAmmo > CurrAmmo && AmmoReserve > 0) 
        {
            AmmoReserve = MaxAmmo - CurrAmmo;
            CurrAmmo = MaxAmmo;
        }
        Debug.Log("Reload Results Below!");
        Debug.Log(CurrAmmo);
    }
}
