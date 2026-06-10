using UnityEngine;

public class AmmoPickup : PowerupBase
{
    [SerializeField] int ammoAmount;

    protected override void ApplyEffect(PlayerController player)
    {
        PlayerGun gun = player.pGun;
        int slot = gun.Active_aTech;

        gun.CurrAmmo[slot] = Mathf.Min(gun.CurrAmmo[slot] + ammoAmount, gun.MaxAmmo[slot]);

        // Refresh UI
        player.UpdatePlayerUI();
    }

    protected override void RemoveEffect(PlayerController player)
    { }
}

