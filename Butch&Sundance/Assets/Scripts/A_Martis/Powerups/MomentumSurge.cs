using UnityEngine;

public class MomentumSurge : PowerupBase
{
    [SerializeField] float momentumMultiplier;
    [SerializeField] float speedBonus;

    float originalMomentum;
    float originalSpeed;

    protected override void ApplyEffect(PlayerController player)
    {
        originalMomentum = player.MomentumBuildRate;
        originalSpeed = player.SpeedBase;

        player.MomentumBuildRate *= momentumMultiplier;
        player.SpeedBase += speedBonus;
    }

    protected override void RemoveEffect(PlayerController player)
    {
        player.MomentumBuildRate = originalMomentum;
        player.SpeedBase = originalSpeed;
    }
}
