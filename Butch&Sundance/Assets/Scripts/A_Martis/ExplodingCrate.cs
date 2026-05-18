using System.Collections;
using UnityEngine;

public class ExplodingCrate : MonoBehaviour, I_Damage
{
    // HP initializers
    [SerializeField] int maxHP;
    int currentHP;

    //Flash
    [SerializeField] Renderer rend;
    [SerializeField] Color flashColor = Color.red;
    [SerializeField] float flashDuration = 0.1f;

    Color originalColor;

    ExplosionScript explosion;

    bool isFlashing;

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
        currentHP -= amount;

        if (!isFlashing)
        {
            StartCoroutine(FlashRed());
        }

        if (currentHP <= 0)
        {
            explosion.Explode();
        }
    }

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
