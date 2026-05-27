using System.Collections;
using UnityEngine;

// Destructible crate that can take damage and explode when destroyed.
public class ExplodingCrate : MonoBehaviour, I_Damage
{
    // HP initializers
    [SerializeField] int maxHP;
    int currentHP;

    //Flash
    [SerializeField] Renderer rend; // Renderer used for visual feedback
    [SerializeField] Color flashColor = Color.red; // Color when damaged
    [SerializeField] float flashDuration = 0.1f; // Duration of flash effect

    Color originalColor; // Stores original material color

    ExplosionScript explosion; // Handles explosion

    bool isFlashing;
    bool destroyed;

    // Initializes health, renderer, and explosion component.
    void Awake()
    {
        currentHP = maxHP;

        explosion = GetComponent<ExplosionScript>();

        if(rend == null)
        {
            rend = GetComponentInChildren<Renderer>();
        }    
        if (rend != null)
        {
            originalColor = rend.material.color;
        }
    }

    public void TakeDamage(int amount, bool AlienTech)
    {
        // Prevent further if already destroyed
        if (destroyed) return;

        currentHP -= amount;

        // Trigger flash effect if not already running
        if (!isFlashing)
        {
            StartCoroutine(FlashRed());
        }

        // Check for destruction and trigger explosion
        if (currentHP <= 0)
        {
            destroyed = true;
            if (explosion != null)
            {
                explosion.Explode();
            }
            else
            {
                Debug.Log(transform.root.name);
                Destroy(transform.root.gameObject);
            }
        }
    }

    // Temporary flash
    IEnumerator FlashRed()
    {
        isFlashing = true;

        if (rend != null)
        {
            rend.material.color = flashColor;
        }

        yield return new WaitForSeconds(flashDuration);

        if (rend != null)
        {
            rend.material.color = originalColor;
        }
        isFlashing = false;
    }
}
