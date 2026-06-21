using System.Collections;
using UnityEngine;

public class Damage : MonoBehaviour
{
    enum eDamageType { Bullet, Stationary, DOT, Lobbed, Seeking }

    [SerializeField] eDamageType DamageType;
    [SerializeField] Rigidbody Body;

    [SerializeField] public int DamageAmount;
    [SerializeField] float DamageRate;
    [SerializeField] int BulletSpeed;
    [SerializeField] int BulletDeathClock;
    [SerializeField] public float MaxRange;
    Vector3 spawnPosition;
    [SerializeField] ParticleSystem ImpactFX;
    public string OwnerTag;
    bool IsDamaging;
    [SerializeField] public bool IsAlienTech;
    [SerializeField] float TurnRate;
    Transform SeekTarget;
    public Transform shooter;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (DamageType == eDamageType.Lobbed)
        {
            Body.useGravity = true;
        }
        if (DamageType == eDamageType.Seeking)
        {
            SeekTarget = FindTarget();
        }
        if (DamageType == eDamageType.Bullet || DamageType == eDamageType.Lobbed || DamageType == eDamageType.Seeking) {
            spawnPosition = transform.position;
            Body.linearVelocity = transform.forward * BulletSpeed;
            Destroy(gameObject, BulletDeathClock);
        }
    }

    void Update()
    {
        if (DamageType == eDamageType.Bullet || DamageType == eDamageType.Lobbed)
        {
            if (Vector3.Distance(spawnPosition, transform.position) >= MaxRange)
            {
                Destroy(gameObject);
            }
        }

        if (DamageType == eDamageType.Seeking && SeekTarget != null)
        {
            Vector3 dir = (SeekTarget.position - transform.position).normalized;
            Vector3 newDir = Vector3.RotateTowards(transform.forward, dir, TurnRate * Time.deltaTime, 0f);
            transform.rotation = Quaternion.LookRotation(newDir);
            Body.linearVelocity = transform.forward * BulletSpeed;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) { return; }
        if (shooter != null && other.transform.root == shooter) { return; }


        if (other.gameObject.layer == gameObject.layer) { return; }
        if (other.tag != OwnerTag)
        {
            I_Damage Damage = other.GetComponentInParent<I_Damage>();

            if (Damage != null && DamageType != eDamageType.DOT)
            {
                if (ImpactFX != null)
                {
                    ImpactFX = Instantiate(ImpactFX, transform.position, Quaternion.identity);
                }
                Damage.TakeDamage(DamageAmount, IsAlienTech);
            }

            if (DamageType == eDamageType.Bullet || DamageType == eDamageType.Lobbed || DamageType == eDamageType.Seeking)
            {
                Destroy(gameObject);
            }
        }
    }
    

    private void OnTriggerStay(Collider other)
    {
        if (other.isTrigger) { return; }
        if (other.tag != OwnerTag)
        {
            I_Damage Damage = other.GetComponent<I_Damage>();

            if (Damage != null && DamageType == eDamageType.DOT && !IsDamaging)
            { StartCoroutine(DamageOther(Damage, other)); }
        }

    }

    IEnumerator DamageOther(I_Damage Damage, Collider Object)
    {
        IsDamaging = true;
        Damage.TakeDamage(DamageAmount, IsAlienTech);
        yield return new WaitForSeconds(DamageRate);
        IsDamaging = false;
    }

    Transform FindTarget()
    {
        if (OwnerTag == "Enemy")
        {
            return GameManager.Instance.Player.transform;
        }
        else
        {
            EnemyBase[] enemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
            Transform nearest = null;
            float minDist = Mathf.Infinity;
            foreach (EnemyBase e in enemies)
            {
                float dist = Vector3.Distance(transform.position, e.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = e.transform;
                }
            }
            return nearest;
        }
    }
}