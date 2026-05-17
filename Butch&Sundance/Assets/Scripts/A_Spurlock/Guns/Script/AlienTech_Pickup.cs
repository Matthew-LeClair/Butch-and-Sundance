using System.Collections.Generic;
using UnityEngine;

public class AlienTech_Pickup : MonoBehaviour
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

    public struct Mod
    {
        public WeaponMod.Type ModType;
        public float ModAmount;
    }

    [Header("Config")]
    [SerializeField] public GunTypeMod puTypeMod;
    [SerializeField] public List<Mod> DataMods;

    [Header("Visual")]
    [SerializeField] public Renderer Mat;
    [SerializeField] public Material Outline;

    Material OriginalMat;

    bool InRange;

    

    private void Start()
    {
        if (Mat != null && Outline != null) 
        { 
            OriginalMat = Mat.material; 
            Mat.material = Outline;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == GameManager.Instance.Player) 
        { InRange = true; }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == GameManager.Instance.Player)
        { 
            InRange = false; 

            if (Mat != null && OriginalMat != null) 
            { Mat.material = OriginalMat; } 
        }
    }

    private void Update()
    {
        if (InRange)
        {
            if (Input.GetButtonDown("Interact"))
            {
                AlienTech AT = GameManager.Instance.Player.GetComponent<PlayerController>().pGun.aTech; // Cache the Player Controller
                
                AT.typeMod = puTypeMod; // Set the Gun Type Mod
                AT.SwitchGun(); // Switch the Gun

                foreach (var Mod in DataMods)
                { AT.AddMod(Mod.ModType, Mod.ModAmount); } // Apply each Mod
                Destroy(gameObject);
            }
        }
        
    }
}
