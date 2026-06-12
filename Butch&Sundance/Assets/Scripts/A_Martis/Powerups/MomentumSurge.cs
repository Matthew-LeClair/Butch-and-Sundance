using UnityEngine;

public class MomentumSurge : PowerupBase
{
    [SerializeField] float momentumMultiplier;
    [SerializeField] float speedBonus;

    [SerializeField] AudioClip pickupSound;
    [SerializeField, Range(0f, 1f)] float volume;

    float originalMomentum;
    float originalSpeed;

    protected override void ApplyEffect(PlayerController player)
    {
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, volume);
        }

        originalMomentum = player.MomentumBuildRate;
        originalSpeed = player.SpeedBase;

        player.MomentumBuildRate *= momentumMultiplier;
        player.SpeedBase += speedBonus;
        GameManager.Instance.PlayerMomentum.SetActive(true);
    }

    protected override void RemoveEffect(PlayerController player)
    {
        player.MomentumBuildRate = originalMomentum;
        player.SpeedBase = originalSpeed;
        GameManager.Instance.PlayerMomentum.SetActive(false);
    }
}
