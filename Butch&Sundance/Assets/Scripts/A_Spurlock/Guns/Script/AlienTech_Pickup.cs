using System.Collections.Generic;
using UnityEngine;
public class AlienTech_Pickup : PickUp_Interact
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
    
    [System.Serializable]
    public struct Mod
    {
        public WeaponMod.Type ModType;
        public float ModAmount;
    }
    [Header("Config")]
    [SerializeField] public GunTypeMod puTypeMod;
    [SerializeField] public int ModCount;

    public override void EventPickUp() 
    {
        if (GameManager.Instance.Player.GetComponentInParent<PlayerController>().pGun.aTech != null)
        {
            GameManager.Instance.Player.GetComponentInParent<PlayerController>().pGun.aTech.typeMod = puTypeMod;
            Debug.Log("Gun Type: " + puTypeMod);
            GameManager.Instance.Player.GetComponentInParent<PlayerController>().pGun.aTech.SwitchGun(); // Switch the Gun
            for (global::System.Int32 i = 0; i < ModCount; i++)
            {
                GameManager.Instance.Player.GetComponentInParent<PlayerController>().pGun.aTech.AddMod();
            } // Apply each Mod
        }
    }
}
