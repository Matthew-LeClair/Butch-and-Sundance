using UnityEngine;

// Power-up that restores the player's shield.
// Shield value cannot exceed the player's maximum shield capacity.

public class ShieldRecharger : PowerupBase
{
    [SerializeField] float shieldAmount;
    [SerializeField] AudioClip pickupSound;
    [SerializeField, Range(0f, 1f)] float volume;

    protected override void ApplyEffect(PlayerController player)
    {
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, volume);
        }
        // Add shield to the player
        player.Shield += shieldAmount;
        // Clamp shield value so it does not exceed maximum shield
        player.Shield = Mathf.Min(player.Shield, player.ShieldMax);
        // Update the player's UI to reflect shield changes
        player.UpdatePlayerUI();
    }

    protected override void RemoveEffect(PlayerController player)
    {
        
    }
}
