using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;   

public class DynamiteCrate : MonoBehaviour, I_Damage
{
    // HP initializers
    [SerializeField] int crateMaxHP;
    int crateCurHP;

    // Explosion initializers
    [SerializeField] int explodeRadius;
    [SerializeField] int explodeDamage;

    // Prevents crate from exploding multiple times
    bool exploded;

    // Sets crate's HP to value set in Unity
    void Awake()
    {
        crateCurHP = crateMaxHP;
    }

    // Damage function that I_Damage uses
    public void TakeDamage(int Amount, string BodyPart)
    {
        // Prevents additional damage after explosion
        if (exploded)
        {
            return;
        }

        // Crate takes damage
        crateCurHP -= Amount;

        // Run Explode() if crate takes enough damage
        if(crateCurHP <=0)
        {
            Explode();
        }
    }

    void Explode()
    {
        exploded = true;

        // Creates an invisible area that detects nearby colliders
        Collider[] Hits = Physics.OverlapSphere(transform.position, explodeRadius);

        // Tracks objects that have already been damaged to prevent multi-hit
        List<I_Damage> DamagedObjects = new List<I_Damage>();

        // Loops through every collider in "Hits" radius
        foreach (Collider c in Hits)
        {
            // Force ignore the crate itself
            if (c.gameObject == gameObject) continue;

            // Search parent hierarchy for correct object to damage to prevent multi-hit
            I_Damage damage = c.GetComponentInParent<I_Damage>();
            
            // Checks if an object can be damage and hasn't been damaged yet
            if (damage != null && !DamagedObjects.Contains(damage))
            {
                // Applies explosion damage to the object
                damage.TakeDamage(explodeDamage, "Body");

                // Adds the damaged object to the list to prevent multi-hit
                DamagedObjects.Add(damage);
            }
        }

        // Removes the crate
        Destroy(gameObject);
    }
}
