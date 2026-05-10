using System.Collections;
using UnityEngine;

public class CharacterBase : MonoBehaviour, I_Damage
{
    [SerializeField] public Renderer Body;

    [SerializeField] public Color OriginalColor;
    [SerializeField] public Color FlashColor;

    float CurrHealth;
    [SerializeField] public float MaxHealth;

    [SerializeField] public GameObject WeaponArm;
    [SerializeField] public GameObject WeaponSlot;
    [SerializeField] public GameObject ActiveWeapon;
    public Gun Weapon;

    [SerializeField] public float AimSpeed;


    [SerializeField] float CritMulti;

    public bool IsAiming;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public virtual void Start()
    {
        // Set the Material Color as the Original Color, Modular Version
        Body.material.color = OriginalColor;

        Renderer[] Renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer R in Renderers)
        { R.material.color = OriginalColor; }

        GameObject ActiveGun = Instantiate(ActiveWeapon).gameObject;
        ActiveGun.transform.SetParent(WeaponSlot.transform);
        ActiveGun.transform.localPosition = Vector3.zero;
        ActiveGun.transform.localRotation = Quaternion.identity;
        ActiveGun.transform.localScale = Vector3.one;

        Weapon = ActiveGun.GetComponent<Gun>();

        Weapon.GunPivot = WeaponSlot.transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(int amount, string BodyPart) // Take Damage, Damage Interface Override
    {
        if (BodyPart == "Head") { CritMulti += Random.Range(1, 1.5f); }
        if (BodyPart == "Arm_R" || BodyPart == "Arm_L") { CritMulti += Random.Range(.25f, .6f); }
        if (BodyPart == "Leg_R" || BodyPart == "Leg_L") { CritMulti += Random.Range(.5f, .8f); }

        CurrHealth -= amount * CritMulti; // Subtract Health by Amount

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

            WeaponArm.transform.localPosition = AimPos;

            Quaternion Aim = Quaternion.Euler(AimRot);
            WeaponArm.transform.localRotation = Aim;

            Debug.Log("Should be aiming");
        } 
        else
        {
            IsAiming = false;

            WeaponArm.transform.localPosition = OriginalPos;

            Quaternion Aim = Quaternion.Euler(OriginalRot);
            WeaponArm.transform.localRotation = Aim;

            Debug.Log("Should be aiming");

            Debug.Log("Should NOT be aiming");
        }
    }

    public virtual void Death() { Debug.Log("Blegh! I'm dead! :)"); Destroy(gameObject); }
}
