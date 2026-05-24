using UnityEngine;
using System.Collections.Generic;   

public class ExplosionScript : MonoBehaviour
{

    // Explosion stats
    [SerializeField] float radius;
    [SerializeField] int damage;

    [SerializeField] ParticleSystem explosionFX;

    [SerializeField] bool IsAlienTech;
    [SerializeField] bool destroyAfterExplode;

    bool exploded;

    public void Explode()
    {
        if (exploded) return;

        exploded = true;

        if (explosionFX != null)
        {
            Instantiate(explosionFX, transform.position, Quaternion.identity);
        }

        Collider[] Hits = Physics.OverlapSphere(transform.position, radius);

        List<I_Damage> DamagedObjects = new List<I_Damage>();

        if (destroyAfterExplode)
        {
            Destroy(gameObject);
        }

        foreach (Collider c in Hits)
        {
            I_Damage damageTarget = c.GetComponentInParent<I_Damage>();

            if (damageTarget ==  null || damageTarget == (I_Damage)GetComponent<I_Damage>())
            {
                continue;
            }

            if(!DamagedObjects.Contains(damageTarget))
            {
                damageTarget.TakeDamage(damage, IsAlienTech);
                {
                    DamagedObjects.Add(damageTarget);
                }
            }
        }
    }
}
