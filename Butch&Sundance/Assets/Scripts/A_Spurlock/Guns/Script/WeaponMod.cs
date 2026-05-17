using UnityEngine;

public class WeaponMod : MonoBehaviour
{
    GameObject Player;
    PlayerController PlayerScript;


    private void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        PlayerScript = Player.GetComponent<PlayerController>();
    }
    public enum Type { Health, Shield, AlienEnergy, Speed, MomentumGain }

    Type ModType;
    float ModAmount;

    public void InitMod(Type mType, float Amount) 
    {
        ModType = mType;
        ModAmount = Amount;
    }

    public void ApplyBonus() 
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
        }
    }
}
