using UnityEngine;

public class AlienTech : MonoBehaviour
{
    //===[Enums]===\\
    public enum GunTypeMod
    {
        Pistol,
        Shotgun,
        SMG,
        AssualtRifle,
        Sniper
    };

    [Header("Config")]
    [SerializeField] public GunTypeMod typeMod;

    [Header("Do NOT Touch!")]
    public PlayerGun pGun;
    public Gun eGun;

    public void SwitchGun() 
    {
        if (eGun != null) 
        {
            switch (typeMod)
            {
                case GunTypeMod.Pistol:
                    eGun.ShootDistance = 8;
                    eGun.Spread = false;
                    eGun.ShootRate = .5f;
                    eGun.ReloadSpeed = .75f;
                    eGun.MaxAmmo = Random.Range(4, 6);
                    eGun.DamageMin = Random.Range(5, 15);
                    eGun.DamageMax = (int)(eGun.DamageMin * Random.Range(1.5f, 2.5f));
                    break;


                case GunTypeMod.Shotgun:

                    break;


                case GunTypeMod.SMG:
                    eGun.ShootDistance = 8;
                    eGun.Spread = false;
                    eGun.ShootRate = .2f;
                    eGun.ReloadSpeed = 1f;
                    eGun.MaxAmmo = Random.Range(28, 32);
                    eGun.DamageMin = Random.Range(5, 15);
                    eGun.DamageMax = (int)(eGun.DamageMin * Random.Range(1.5f, 2.5f));
                    break;


                case GunTypeMod.AssualtRifle:

                    break;


                case GunTypeMod.Sniper:

                    break;
            }
        }
    }
}
