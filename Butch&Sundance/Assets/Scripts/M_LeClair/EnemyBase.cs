using System.Collections;
using UnityEngine;

public class EnemyBase : MonoBehaviour, I_Damage
{
    [SerializeField] public Color OriginalColor;
    [SerializeField] public Color FlashColor;

    public float CurrHealth;
    [SerializeField] public float MaxHealth;

    [SerializeField] GameObject[] PowerupDrops;
    [SerializeField] float PowerupDropChance = 0.5f;
    [SerializeField] GameObject[] WeaponDrops;

    public GameObject ActiveWeapon_R;
    public GameObject ActiveWeapon_L;
    public Gun Weapon_R;
    public Gun Weapon_L;

    [SerializeField] float CritMulti;
    [SerializeField] public float DamageReducBase;
    public float DamageReduc;

    public bool IsAiming;
    public Animator anim;
    public bool isDead;

    Material[] CachedMaterials;

    public virtual void Start()
    {
        CurrHealth = MaxHealth;
        DamageReduc = DamageReducBase;

        Renderer[] Renderers = GetComponentsInChildren<Renderer>();

        if (ActiveWeapon_L != null)
        {
            Weapon_L = ActiveWeapon_L.GetComponent<Gun>();
        }
        if (ActiveWeapon_R != null)
        {
            Weapon_R = ActiveWeapon_R.GetComponent<Gun>();
        }

        CachedMaterials = new Material[Renderers.Length];
        for (int i = 0; i < Renderers.Length; i++)
        {
            CachedMaterials[i] = Renderers[i].material;
            CachedMaterials[i].color = OriginalColor;
        }

        anim = GetComponentInChildren<Animator>();
    }

    void Update() { }

    public virtual void TakeDamage(int Amount, bool AlienTech)
    {
        Debug.Log("TakeDamage called on " + gameObject.name + " | Amount: " + Amount + " | DamageReduc: " + DamageReduc + " | CritMulti: " + CritMulti);
        CurrHealth -= (Amount * CritMulti) * DamageReduc;
        Debug.Log("Health after: " + CurrHealth);

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

        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if(agent != null)
        {
            agent.enabled = false;
        }
        var col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        isDead = true;
        float fallDuration = 0.5f;
        float fadeDuration = 3f;
        float fadeDelay = 2f;
        Quaternion startRot = transform.rotation;
        Quaternion deadRot = startRot * Quaternion.Euler(90f, 0f, 0f);
        float t = 0f;

        while (t < fallDuration)
        {
            t += Time.deltaTime;
            transform.rotation = Quaternion.Lerp(startRot, deadRot, t / fallDuration);
            yield return null;
        }

        yield return new WaitForSeconds(fadeDelay);

        Vector3 originalScale = transform.localScale;
        Vector3 sunkenScale = new Vector3(originalScale.x, 0f, originalScale.z);
        t = 0f;

        while(t < fadeDuration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, sunkenScale, t / fadeDuration);
            yield return null;
        }
        Destroy(gameObject);
    }
}