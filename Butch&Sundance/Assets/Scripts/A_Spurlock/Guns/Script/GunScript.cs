using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class Gun : MonoBehaviour
{
    [SerializeField] public GameObject BulletPrefab;
    [SerializeField] public bool Spread;
    [SerializeField] public int PelletCount;
    [SerializeField] public int SpreadAngle;

    [SerializeField] public float ShootRate;
    float ShootTimer;

    [SerializeField] public Transform GunPivot;
    [SerializeField] public Transform ShootPos;
    [SerializeField] public int ShootDistance;

    public int CurrAmmo;
    [SerializeField] public int MaxAmmo;
    [SerializeField] public float ReloadSpeed;
    int AmmoReserve;

    [SerializeField] public int DamageMin;
    [SerializeField] public int DamageMax;

    List<WeaponMod> Mods;

    public bool IsOut = false;

    [SerializeField] public AudioSource AudioPlayer;

    

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

        if (BulletPrefab == null)
        {
            return;
        }

        if (ShootTimer >= ShootRate)
        {
            if (CurrAmmo <= 0) { IsOut = true; return; }

            ShootTimer = 0;
            CurrAmmo--;

            int damage = Random.Range(DamageMin, DamageMax);
            if (!Spread)
            {

                GameObject bullet = Instantiate(BulletPrefab, ShootPos.position, ShootPos.rotation);

                Damage d = bullet.GetComponent<Damage>();
                if (d != null) { 
                    d.DamageAmount = damage;
                    d.shooter = transform.root;
                    d.MaxRange = ShootDistance;
                }
            }
            else
            {
                int pelletDamage = Mathf.Clamp((damage / PelletCount) * Random.Range(2, 3), 1, 30);

                for (int i = 0; i < PelletCount; i++)

                {
                    float spreadX = Random.Range(-SpreadAngle, SpreadAngle);
                    float spreadY = Random.Range(-SpreadAngle, SpreadAngle);
                    Quaternion spreadRot = ShootPos.rotation * Quaternion.Euler(spreadX, spreadY, 0);

                    GameObject bullet = Instantiate(BulletPrefab, ShootPos.position, spreadRot);
                    Damage d = bullet.GetComponent<Damage>();
                    if (d != null)
                    {
                        d.DamageAmount = damage;
                        d.shooter = transform.root;
                        d.MaxRange = ShootDistance;
                    }
                }
            }
        }
    }
    public void Reload() 
    {
        CurrAmmo = MaxAmmo;
        IsOut = false;
    }
}
