using UnityEngine;

public class AmmoPickup : PowerupBase
{
    [SerializeField] int ammoAmount;

    protected override void ApplyEffect(PlayerController player)
    {
        player.pGun.ActiveCurrAmmo += ammoAmount;

        // Refresh UI
        player.UpdatePlayerUI();
    }

    protected override void RemoveEffect(PlayerController player)
    { }
}

