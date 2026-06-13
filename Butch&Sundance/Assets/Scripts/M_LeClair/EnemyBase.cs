using System.Collections;
using UnityEngine;

public class EnemyBase : MonoBehaviour, I_Damage
{
    [SerializeField] public Renderer Body;

    [SerializeField] public Color OriginalColor;
    [SerializeField] public Color FlashColor;

    public float CurrHealth;
    [SerializeField] public float MaxHealth;

    [SerializeField] GameObject[] PowerupDrops;
    [SerializeField] float PowerupDropChance = 0.5f;
    [SerializeField] GameObject[] WeaponDrops;
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

    Material[] CachedMaterials;

    public virtual void Start()
    {
        CurrHealth = MaxHealth;
        DamageReduc = DamageReducBase;

        Renderer[] Renderers = GetComponentsInChildren<Renderer>();

        CachedMaterials = new Material[Renderers.Length];
        for (int i = 0; i < Renderers.Length; i++)
        {
            CachedMaterials[i] = Renderers[i].material;
            CachedMaterials[i].color = OriginalColor;
        }

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

    void Update() { }

    public virtual void TakeDamage(int Amount, bool AlienTech)
    {
        CurrHealth -= (Amount * CritMulti) * DamageReduc;

        CritMulti = 1;

        if (CurrHealth <= 0)
        {
            // GameManager.Instance.UpdateGameGoal(-1);
            Death();
        }
        else { StartCoroutine(Flash()); }
    }

    IEnumerator Flash()
    {
        if (CachedMaterials == null) { yield break; }

        foreach (Material mat in CachedMaterials)
        { mat.color = FlashColor; }

        yield return new WaitForSeconds(0.1f);

        foreach (Material mat in CachedMaterials)
        { mat.color = OriginalColor; }
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
            WeaponArm_R.transform.localRotation = Quaternion.Euler(AimRot);
            Debug.Log("Should be aiming");
        }
        else
        {
            IsAiming = false;
            WeaponArm_R.transform.localPosition = OriginalPos;
            WeaponArm_R.transform.localRotation = Quaternion.Euler(OriginalRot);
            Debug.Log("Should NOT be aiming");
        }
    }

    public virtual void Death()
    {
        Debug.Log("Blegh! I'm dead! :)");

        if (WeaponDrops != null && WeaponDrops.Length > 0)
        {
            int index = Random.Range(0, WeaponDrops.Length);
            Instantiate(WeaponDrops[index], transform.position, Quaternion.identity);
        }

        if(PowerupDrops != null && PowerupDrops.Length > 0)
        {
            if(Random.value <= PowerupDropChance)
            {
                int index = Random.Range(0, PowerupDrops.Length);
                Instantiate(PowerupDrops[index], transform.position, Quaternion.identity);
            }
        }

        Destroy(gameObject);
    }
}