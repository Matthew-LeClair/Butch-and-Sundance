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
    public enum Type { Health, MomentumGain, DamageReduction, Speed }

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
                PlayerScript.MaxHealth *= ModAmount;
                PlayerScript.CurrHealth = PlayerScript.MaxHealth;
                break;


            case Type.MomentumGain:
                PlayerScript.MomentumBuildRate = PlayerScript.BaseMomentumBuildRate * ModAmount;
                break;


            case Type.DamageReduction:
                PlayerScript.DamageReduc = PlayerScript.DamageReducBase * ModAmount;
                break;


            case Type.Speed:
                PlayerScript.Speed = PlayerScript.SpeedBase * ModAmount;
                break;
        }
    }
}
