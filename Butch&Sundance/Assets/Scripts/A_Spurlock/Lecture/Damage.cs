using System.Collections;
using UnityEngine;

public class Damage : MonoBehaviour
{
    enum eDamageType {Bullet, Stationary, DOT  }

    [SerializeField] eDamageType DamageType;
    [SerializeField] Rigidbody Body;

    [SerializeField] int DamageAmount;
    [SerializeField] float DamageRate;
    [SerializeField] int BulletSpeed;
    [SerializeField] int BulletDeathClock;
    [SerializeField] ParticleSystem ImpactFX;

    bool IsDamaging;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (DamageType == eDamageType.Bullet)
        {
            Body.linearVelocity = transform.forward * BulletSpeed;
            Destroy(gameObject, BulletDeathClock);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) { return; }

        I_Damage Damage = other.GetComponent<I_Damage>();

        if (Damage != null && DamageType != eDamageType.DOT)
        { Damage.TakeDamage(DamageAmount); }

        if (DamageType == eDamageType.Bullet) 
        {
            if (ImpactFX != null) 
            { ImpactFX = Instantiate(ImpactFX, 
                transform.position, 
                Quaternion.identity); }

            Damage.TakeDamage(DamageAmount);

            Destroy(gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.isTrigger) { return; }

        I_Damage Damage = other.GetComponent<I_Damage>();

        if (Damage != null && DamageType == eDamageType.DOT && !IsDamaging) 
        { StartCoroutine(DamageOther(Damage)); }
    }

    IEnumerator DamageOther(I_Damage Damage) 
    {
        IsDamaging = true;
        Damage.TakeDamage(DamageAmount);
        yield return new WaitForSeconds(DamageRate);
        IsDamaging = false;
    }
}
