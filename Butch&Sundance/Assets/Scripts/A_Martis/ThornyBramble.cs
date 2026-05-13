using System.Collections;
using UnityEngine;

public class ThornyBramble : MonoBehaviour
{
    [SerializeField] int thornDamage;
    [SerializeField] float damageRate;
    [SerializeField] float slowMult;

    bool isDamaging;

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;

        PlayerController player = other.GetComponent<PlayerController>();

        if (player != null)
        {
            player.Speed *= slowMult;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.isTrigger) return;

        I_Damage damage = other.GetComponent<I_Damage>();

        if (damage != null && !isDamaging)
        {
            StartCoroutine(DamageOverTime(damage));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.isTrigger) return;

        PlayerController player = other.GetComponent<PlayerController>();

        if (player != null)
        {
            player.Speed /= slowMult;
        }
    }

    IEnumerator DamageOverTime(I_Damage damage)
    {
        isDamaging = true;

        damage.TakeDamage(thornDamage, "Body");

        yield return new WaitForSeconds(damageRate);

        isDamaging = false;
    }
}
