using UnityEngine;

public class DynamiteCrate : MonoBehaviour, I_Damage
{
    [SerializeField] int crateMaxHP;
    int crateCurHP;

    [SerializeField] int explodeRadius;
    [SerializeField] int explodeDamage;

    bool exploded;

    void Awake()
    {
        crateCurHP = crateMaxHP;
    }

    public void TakeDamage(int Amount, string BodyPart)
    {
        if (exploded)
        {
            return;
        }

        crateCurHP -= Amount;

        if(crateCurHP <=0)
        {
            Explode();
        }
    }

    void Explode()
    {
        exploded = true;

        Collider[] Hits = Physics.OverlapSphere(transform.position, explodeRadius);

        foreach (Collider c in Hits)
        {
            if (c.gameObject == gameObject) continue;

            I_Damage damage = c.GetComponent<I_Damage>();

            if (damage != null)
            {
                damage.TakeDamage(explodeDamage, "Body");
            }
            Debug.Log(c.name);
        }

        Destroy(gameObject);
    }
}
