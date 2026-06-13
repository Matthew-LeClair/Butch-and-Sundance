using UnityEngine;

public class MomentumSurge : PowerupBase
{
    [SerializeField] float momentumMultiplier;
    [SerializeField] float speedBonus;
    [SerializeField] float jumpBonus;

    [SerializeField] AudioClip pickupSound;
    [SerializeField, Range(0f, 1f)] float volume;

    [SerializeField] GameObject powerupVFX;
    GameObject vfxInstance;

    float originalMomentum;
    float originalSpeed;
    float originalJump;

    protected override void ApplyEffect(PlayerController player)
    {
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, volume);
        }

        originalMomentum = player.MomentumBuildRate;
        originalSpeed = player.SpeedBase;
        originalJump = player.JumpSpeedBase;

        player.MomentumBuildRate *= momentumMultiplier;
        player.SpeedBase += speedBonus;
        player.JumpSpeedBase += jumpBonus;

        if(powerupVFX != null && vfxInstance == null)
        {
            vfxInstance = Instantiate(powerupVFX, player.CamTransform);
        }

        if (vfxInstance != null)
        {
            vfxInstance.SetActive(true);
            var ps = vfxInstance.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
        }

        GameManager.Instance.PlayerMomentum.SetActive(true);
    }

    protected override void RemoveEffect(PlayerController player)
    {
        player.MomentumBuildRate = originalMomentum;
        player.SpeedBase = originalSpeed;
        player.JumpSpeedBase = originalJump;

        if(powerupVFX != null)
        {
            var ps = vfxInstance.GetComponent<ParticleSystem>();
            if (ps != null) ps.Stop();
            Destroy(vfxInstance);
            vfxInstance= null;
        }

        GameManager.Instance.PlayerMomentum.SetActive(false);
    }
}
