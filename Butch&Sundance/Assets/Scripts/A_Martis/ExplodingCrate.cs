using UnityEngine;

public class DynamiteCrate : MonoBehaviour, I_Damage
{
    // HP initializers
    [SerializeField] int maxHP;
    int currentHP;

    ExplosionScript explosion;

    void Awake()
    {
        currentHP = maxHP;

        explosion = GetComponent<ExplosionScript>();
    }

    public void TakeDamage(int amount, bool AlienTech)
    {
        currentHP -= amount;

        if (currentHP <= 0)
        {
            explosion.Explode();
        }
    }
}
