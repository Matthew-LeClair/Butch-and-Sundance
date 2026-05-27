using UnityEngine;

// Represents a single randomized stat bonus attached to an AlienTech weapon.
// Each mod targets one PlayerController stat and scales it by a random multiplier within a type-appropriate range.
// Mods are applied when the weapon is picked up and reverted when the weapon is destroyed or switched away from.
public class WeaponMod
{
    //===[Enum]===\\

    public enum Type { Health, Shield, AlienEnergy, Speed, MomentumGain } // The five stat categories a mod can affect


    //===[Fields]===\\

    public Type ModType;    // Which stat this mod boosts - set randomly by InitMod()
    public float ModAmount; // The multiplier applied to the stat - randomized per type in InitMod()


    //===[Init]===\\

    // Called once from ApplyBonus() before the bonus is applied.
    // Randomly selects a mod type from the Type enum and assigns a multiplier within that type's balance range.
    // Speed and MomentumGain use a tighter range (1.1-1.3) because they compound more aggressively with the momentum system.
    public void InitMod()
    {
        ModType = (Type)Random.Range(0, System.Enum.GetValues(typeof(Type)).Length); // Pick a random Type from the full enum range

        switch (ModType)
        {
            case Type.Health:
                ModAmount = Random.Range(1.1f, 1.5f); // 10% to 50% HealthMax multiplier
                break;

            case Type.Shield:
                ModAmount = Random.Range(1.1f, 1.5f); // 10% to 50% ShieldMax multiplier
                break;

            case Type.AlienEnergy:
                ModAmount = Random.Range(1.1f, 1.5f); // 10% to 50% AlienEnergyMax multiplier
                break;

            case Type.Speed:
                ModAmount = Random.Range(1.1f, 1.3f); // 10% to 30% SpeedBase multiplier - tighter range to avoid runaway momentum
                break;

            case Type.MomentumGain:
                ModAmount = Random.Range(1.1f, 1.3f); // 10% to 30% MomentumBuildRate multiplier - tighter range for the same reason
                break;
        }
    }


    //===[Apply / Revert]===\\

    // Called from AlienTech.AddMod() when a weapon with mods is picked up or equipped.
    // Initializes the mod type and amount via InitMod(), then scales the relevant PlayerController stat by ModAmount.
    // Max values are multiplied and current values are set to the new max so the bonus is immediately felt.
    public void ApplyBonus()
    {
        InitMod(); // Randomize type and amount before applying
        Debug.Log("Mod Applied - Type: " + ModType + " | Amount: " + ModAmount); // Debug log for inspection

        PlayerController PC = GameManager.Instance.Player.GetComponent<PlayerController>(); // Cache PlayerController to avoid repeated GetComponent calls

        switch (ModType)
        {
            case Type.Health:
                PC.HealthMax *= ModAmount;           // Scale the max up by the multiplier
                PC.Health = PC.HealthMax;            // Heal to new max so the player feels the benefit immediately
                PC.UpdatePlayerUI();                 // Refresh HP bar to reflect new max
                break;

            case Type.Shield:
                PC.ShieldMax *= ModAmount;           // Scale shield max by multiplier
                PC.Shield = PC.ShieldMax;            // Restore shield to new max
                PC.UpdatePlayerUI();                 // Refresh Shield bar
                break;

            case Type.AlienEnergy:
                PC.AlienEnergyMax *= ModAmount;      // Scale energy max by multiplier
                PC.AlienEnergy = PC.AlienEnergyMax;  // Fill energy to new max
                PC.UpdatePlayerUI();                 // Refresh any energy UI
                break;

            case Type.Speed:
                PC.SpeedBase *= ModAmount;           // Scale SpeedBase so HandleStats() lerp ceiling rises correctly
                break;

            case Type.MomentumGain:
                PC.BaseMomentumBuildRate *= ModAmount; // Scale base rate so HandleStats() recalculates correctly
                PC.MomentumBuildRate = PC.BaseMomentumBuildRate; // Apply immediately to the active rate
                break;
        }
    }

    // Called from AlienTech.RevertMods() when a weapon is destroyed or the player switches away from it.
    // Undoes the exact multiplication ApplyBonus() performed by dividing by the same ModAmount.
    // Current values are clamped to the reverted max so the player cannot retain stat overflow after losing a gun.
    public void RevertBonus()
    {
        Debug.Log("Mod Reverted - Type: " + ModType + " | Amount: " + ModAmount); // Debug log for inspection

        PlayerController PC = GameManager.Instance.Player.GetComponent<PlayerController>(); // Cache PlayerController to avoid repeated GetComponent calls

        switch (ModType)
        {
            case Type.Health:
                PC.HealthMax /= ModAmount;                              // Undo the multiplier on max
                PC.Health = Mathf.Min(PC.Health, PC.HealthMax);         // Clamp current health to new lower max
                PC.UpdatePlayerUI();                                    // Refresh HP bar
                break;

            case Type.Shield:
                PC.ShieldMax /= ModAmount;                              // Undo the multiplier on max
                PC.Shield = Mathf.Min(PC.Shield, PC.ShieldMax);         // Clamp current shield to new lower max
                PC.UpdatePlayerUI();                                    // Refresh Shield bar
                break;

            case Type.AlienEnergy:
                PC.AlienEnergyMax /= ModAmount;                                     // Undo the multiplier on max
                PC.AlienEnergy = Mathf.Min(PC.AlienEnergy, PC.AlienEnergyMax);      // Clamp current energy to new lower max
                PC.UpdatePlayerUI();                                                // Refresh energy UI
                break;

            case Type.Speed:
                PC.SpeedBase /= ModAmount; // Undo the multiplier on SpeedBase so HandleStats() lerp returns to original ceiling
                break;

            case Type.MomentumGain:
                PC.BaseMomentumBuildRate /= ModAmount;               // Undo the multiplier on the base rate
                PC.MomentumBuildRate = PC.BaseMomentumBuildRate;     // Apply reverted rate immediately
                break;
        }
    }
}