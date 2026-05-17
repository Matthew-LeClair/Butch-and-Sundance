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

        foreach (Collider c in Hits)
        {
            if (c.gameObject == gameObject) continue;

            I_Damage damageTarget = c.GetComponentInParent<I_Damage>();
            
            if (damageTarget != null && !DamagedObjects.Contains(damageTarget))
            {
                damageTarget.TakeDamage(damage, IsAlienTech);

                DamagedObjects.Add(damageTarget);
            }
        }

        if (destroyAfterExplode)
        {
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
