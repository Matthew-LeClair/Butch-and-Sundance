using UnityEngine;

public class ShieldRecharger : PowerupBase
{
    [SerializeField] float shieldAmount;

    protected override void ApplyEffect(PlayerController player)
    {
        player.Shield += shieldAmount;
        player.Shield = Mathf.Min(player.Shield, player.ShieldMax);

        player.UpdatePlayerUI();
    }

    protected override void RemoveEffect(PlayerController player)
    {
        
    }
}
