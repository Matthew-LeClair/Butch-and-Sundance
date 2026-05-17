using UnityEngine;

public class WeaponMod
{
    GameObject Player;
    PlayerController PlayerScript;

    public enum Type { Health, Shield, AlienEnergy, Speed, MomentumGain }

    Type ModType;
    float ModAmount;
    bool Applied = false;

    public void InitMod(Type mType, float Amount) 
    {
        ModType = mType;
        ModAmount = Amount;
        Player = GameManager.Instance.Player;
        PlayerScript = Player.GetComponent<PlayerController>();
    }

    public void ApplyBonus() 
    {
        if (!Applied)
        {
            switch (ModType)
            {
                case Type.Health:
                    PlayerScript.HealthMax *= ModAmount;
                    PlayerScript.Health = PlayerScript.HealthMax;
                    break;


                case Type.Shield:
                    PlayerScript.ShieldMax = PlayerScript.ShieldMax * ModAmount;
                    PlayerScript.Shield = PlayerScript.ShieldMax;
                    break;


                case Type.AlienEnergy:
                    PlayerScript.AlienEnergyMax = PlayerScript.AlienEnergyMax * ModAmount;
                    PlayerScript.AlienEnergy = PlayerScript.AlienEnergyMax;
                    break;


                case Type.Speed:
                    PlayerScript.Speed = PlayerScript.SpeedBase * ModAmount;
                    break;


                case Type.MomentumGain:
                    PlayerScript.MomentumBuildRate = PlayerScript.BaseMomentumBuildRate * ModAmount;
                    break;
            } Applied = true;
        }
    }
}
