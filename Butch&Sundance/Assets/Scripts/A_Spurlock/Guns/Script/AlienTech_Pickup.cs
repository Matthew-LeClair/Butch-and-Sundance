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
    
    [System.Serializable]
    public struct Mod
    {
        public WeaponMod.Type ModType;
        public float ModAmount;
    }
    [Header("Config")]
    [SerializeField] public GunTypeMod puTypeMod;
    [SerializeField] public int ModCount;
    [Header("Visual")]
    [SerializeField] public Renderer Mat;
    [SerializeField] public Material Outline;
    Material OriginalMat;

    bool InRange;

    private void Update()
    {
        if (InRange) 
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (GameManager.Instance.Player.GetComponentInParent<PlayerController>().pGun.aTech != null)
                {
                    GameManager.Instance.Player.GetComponentInParent<PlayerController>().pGun.aTech.typeMod = puTypeMod;
                    Debug.Log("Gun Type: " +  puTypeMod);
                    GameManager.Instance.Player.GetComponentInParent<PlayerController>().pGun.aTech.SwitchGun(); // Switch the Gun
                    for (global::System.Int32 i = 0; i < ModCount; i++)
                    {
                        GameManager.Instance.Player.GetComponentInParent<PlayerController>().pGun.aTech.AddMod();
                    } // Apply each Mod
                }
            }
            if (Input.GetKeyUp(KeyCode.E)) { Destroy(gameObject); } // Destroy the Pickup
        }
    }

    private void Start()
    {
        if (Mat != null && Outline != null)
        {
            OriginalMat = Mat.material; // Store Original Material
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Mat.material = Outline; // Apply Outline Material
            InRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Mat.material = OriginalMat; // Apply Outline Material
            InRange = false;
        }
    }
    }