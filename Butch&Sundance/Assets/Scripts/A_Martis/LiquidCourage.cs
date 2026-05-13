using System.Collections;
using UnityEngine;

public class LiquidCourage : MonoBehaviour
{
    [SerializeField] int healAmount;
    [SerializeField] float speedMult;
    [SerializeField] float buffDuration;

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;

        PlayerController player = other.GetComponent<PlayerController>();

        if (player != null)
        {
            StartCoroutine(ApplyLQ(player));

            Destroy(gameObject);
        }
    }

    IEnumerator ApplyLQ(PlayerController player)
    {
        player.CurrHealth += healAmount;

        float originalSpeed = player.SpeedBase;

        player.SpeedBase *= speedMult;

        yield return new WaitForSeconds(buffDuration);

        player.SpeedBase = originalSpeed;
    }    
}
