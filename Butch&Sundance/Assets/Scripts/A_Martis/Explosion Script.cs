using UnityEngine;
using System.Collections.Generic;   

// Handle radial explosion logic including damage application, visual effects, and optional self-destruction
public class ExplosionScript : MonoBehaviour
{

    [Header("Explosion Settings")]
    [SerializeField] float radius;
    [SerializeField] int damage;

    [Header("VFX")]
    [SerializeField] ParticleSystem explosionFX;

    [Header("Behaviour")]
    [SerializeField] bool IsAlienTech;
    [SerializeField] bool destroyAfterExplode;

    bool exploded; // Prevents multiple explosions

    // Triggers explosion effect and applies damage
    public void Explode()
    {
        Debug.Log("Explode called - already exploded: " + exploded);
        // Prevent multiple explosions
        if (exploded) return;

        exploded = true;

        // Spawn explosion effect
        if (explosionFX != null)
        {
            ParticleSystem fx = Instantiate(explosionFX, transform.position, Quaternion.identity);
            Destroy(fx.gameObject, fx.main.duration + fx.main.startLifetime.constantMax);
        }

        // Detect all colliders within radius
        Collider[] Hits = Physics.OverlapSphere(transform.position, radius);

        // Track already damaged targets
        List<I_Damage> DamagedObjects = new List<I_Damage>();

        // Optional destroy after explode
        if (destroyAfterExplode)
        {
            Destroy(gameObject);
        }

        // Apply damage to valid targets
        foreach (Collider c in Hits)
        {
            I_Damage damageTarget = c.GetComponentInParent<I_Damage>();

            // Ignore invalid targets and self
            if (damageTarget ==  null || damageTarget == (I_Damage)GetComponent<I_Damage>())
            {
                continue;
            }

            // Ensure each target is only damaged once per explosion
            if (!DamagedObjects.Contains(damageTarget))
            {
                damageTarget.TakeDamage(damage, IsAlienTech);
                {
                    DamagedObjects.Add(damageTarget);
                }
            }
        }
    }
}
