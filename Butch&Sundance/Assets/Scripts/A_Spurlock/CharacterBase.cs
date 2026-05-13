using System.Collections;
using UnityEngine;

public class CharacterBase : MonoBehaviour, I_Damage
{
    [SerializeField] public Renderer Body;

    [SerializeField] public Color OriginalColor;
    [SerializeField] public Color FlashColor;

    public float CurrHealth;
    [SerializeField] public float MaxHealth;

    [SerializeField] public GameObject WeaponArm_R;
    [SerializeField] public GameObject WeaponSlot_R;
    [SerializeField] public GameObject ActiveWeapon_R;
    public Gun Weapon_R; 
    [SerializeField] public GameObject WeaponArm_L;
    [SerializeField] public GameObject WeaponSlot_L;
    [SerializeField] public GameObject ActiveWeapon_L;
    public Gun Weapon_L;



    [SerializeField] float CritMulti;
    [SerializeField] public float DamageReducBase;
    public float DamageReduc;


    public bool IsAiming;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public virtual void Start()
    {
        DamageReduc = DamageReducBase;

        // Set the Material Color as the Original Color, Modular Version
        Body.material.color = OriginalColor;

        Renderer[] Renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer R in Renderers)
        { R.material.color = OriginalColor; }

        if (ActiveWeapon_R != null && WeaponSlot_R != null)
        {
            GameObject ActiveGun_R = Instantiate(ActiveWeapon_R).gameObject;
            ActiveGun_R.transform.SetParent(WeaponSlot_R.transform);
            ActiveGun_R.transform.localPosition = Vector3.zero;
            ActiveGun_R.transform.localRotation = Quaternion.identity;
            ActiveGun_R.transform.localScale = Vector3.one;

            Weapon_R = ActiveGun_R.GetComponent<Gun>();

            Weapon_R.GunPivot = WeaponSlot_R.transform;
        }

        if (ActiveWeapon_L != null && WeaponSlot_L != null)
        {
            GameObject ActiveGun_L = Instantiate(ActiveWeapon_L).gameObject;
            ActiveGun_L.transform.SetParent(WeaponSlot_L.transform);
            ActiveGun_L.transform.localPosition = Vector3.zero;
            ActiveGun_L.transform.localRotation = Quaternion.identity;
            ActiveGun_L.transform.localScale = Vector3.one;

            Weapon_L = ActiveGun_L.GetComponent<Gun>();

            Weapon_L.GunPivot = WeaponSlot_L.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void TakeDamage(int Amount, string BodyPart, bool Single) // Take Damage, Damage Interface Override
    {
        if (!Single)
        {
            if (BodyPart == "Head") { CritMulti += Random.Range(1, 1.5f); }
            if (BodyPart == "Arm_R" || BodyPart == "Arm_L") { CritMulti += Random.Range(.25f, .6f); }
            if (BodyPart == "Leg_R" || BodyPart == "Leg_L") { CritMulti += Random.Range(.5f, .8f); }
        }

        CurrHealth -= (Amount * CritMulti) * DamageReduc; // Subtract Health by Amount

        CritMulti = 1; //Resets crit multi after damage

        if (CurrHealth <= 0) // If Health is Less Than or Equal To 0...
        { Death(); } // Destroy the Object
        else { StartCoroutine(Flash(BodyPart)); } // Call the Flash Function, Modular Version

    }

    IEnumerator Flash(string BodyPart)
    {
        // Find the exact Body Part
        Transform tPart = transform.Find(BodyPart);

        if (tPart == null) { yield break; }

        Renderer rPart = tPart.GetComponent<Renderer>();

        if (rPart == null) { yield break; }

        // Flash Body Part
        rPart.material.color = FlashColor;

        yield return new WaitForSeconds(0.1f);

        // Reset Body Part
        rPart.material.color = OriginalColor;
    }

    public void Aim()
    {
        Vector3 OriginalPos = new Vector3(.6f, .1f, 0);
        Vector3 AimPos = new Vector3(.47f, .6f, .47f);

        Vector3 OriginalRot = new Vector3(-14, 90, 0);
        Vector3 AimRot = new Vector3(-14, 90, -90);


        if (!IsAiming) 
        {
            IsAiming = true;

            WeaponArm_R.transform.localPosition = AimPos;

            Quaternion Aim = Quaternion.Euler(AimRot);
            WeaponArm_R.transform.localRotation = Aim;

            Debug.Log("Should be aiming");
        } 
        else
        {
            IsAiming = false;

            WeaponArm_R.transform.localPosition = OriginalPos;

            Quaternion Aim = Quaternion.Euler(OriginalRot);
            WeaponArm_R.transform.localRotation = Aim;

            Debug.Log("Should NOT be aiming");
        }
    }

    public virtual void Death() { Debug.Log("Blegh! I'm dead! :)"); Destroy(gameObject); }
}
