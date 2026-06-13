using UnityEngine;

public class DamageUp : PowerupBase
{
    [SerializeField] float damageMult;

    [SerializeField] AudioClip pickupSound;
    [SerializeField, Range(0f, 1f)] float volume;

    float originalDamage;

    [SerializeField] GameObject powerupVFX;
    GameObject vfxInstance;

    protected override void ApplyEffect(PlayerController player)
    {
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, volume);
        }

        originalDamage = player.pGun.damageMuliplier;

        player.pGun.damageMuliplier *= damageMult;

        if (powerupVFX != null && vfxInstance == null)
        {
            vfxInstance = Instantiate(powerupVFX, player.CamTransform);
        }

        if (vfxInstance != null)
        {
            vfxInstance.SetActive(true);
            var ps = vfxInstance.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
        }


    }

    protected override void RemoveEffect(PlayerController player)
    {
        player.pGun.damageMuliplier = originalDamage;

        if (powerupVFX != null)
        {
            var ps = vfxInstance.GetComponent<ParticleSystem>();
            if (ps != null) ps.Stop();
            Destroy(vfxInstance);
            vfxInstance = null;
        }
    }
}
