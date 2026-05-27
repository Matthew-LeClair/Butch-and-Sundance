using UnityEngine;


// Powerup that restores players health
// Optionally allow overhealing beyond player's maximum
public class HealthPowerUp : PowerupBase
{
    [SerializeField] int healAmount;
    [SerializeField] bool allowOverheal;
    [SerializeField] int maxOverheal;

    protected override void ApplyEffect(PlayerController player)
    {
        //Store player's normal maximum
        float maxHealth = player.HealthMax;

        if(allowOverheal)
        {
            // Calculate heal amount without going over the limit
            float maxAllowed = maxHealth + maxOverheal;
            player.Health = Mathf.Min(player.Health + healAmount, maxAllowed);
        }
        else
        {
            // Heal without exceeding normal limit
            player.Health = Mathf.Min(player.Health + healAmount, maxHealth);
        }

        // Refresh UI
        player.UpdatePlayerUI();
    }

    protected override void RemoveEffect(PlayerController player)
    { }
}
