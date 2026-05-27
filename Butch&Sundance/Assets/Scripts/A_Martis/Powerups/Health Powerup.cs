using UnityEngine;

public class HealthPowerUp : PowerupBase
{
    [SerializeField] int healAmount;
    [SerializeField] bool allowOverheal;
    [SerializeField] int maxOverheal;

    protected override void ApplyEffect(PlayerController player)
    {
        float maxHealth = player.HealthMax;

        if(allowOverheal)
        {
            float maxAllowed = maxHealth + maxOverheal;
            player.Health = Mathf.Min(player.Health + healAmount, maxAllowed);
        }
        else
        {
            player.Health = Mathf.Min(player.Health + healAmount, maxHealth);
        }
        player.UpdatePlayerUI();
    }

    protected override void RemoveEffect(PlayerController player)
    { }
}
