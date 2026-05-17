using UnityEngine;

public class WeaponMod
{
    public enum Type { Health, Shield, AlienEnergy, Speed, MomentumGain }

    Type ModType;
    float ModAmount;

    public void InitMod()
    {
        ModType = (Type)Random.Range(0, System.Enum.GetValues(typeof(Type)).Length); // Random Mod Type
        switch (ModType)
        {
            case Type.Health:
                ModAmount = Random.Range(1.1f, 1.5f); // 10% to 50% Health boost
                break;
            case Type.Shield:
                ModAmount = Random.Range(1.1f, 1.5f); // 10% to 50% Shield boost
                break;
            case Type.AlienEnergy:
                ModAmount = Random.Range(1.1f, 1.5f); // 10% to 50% Alien Energy boost
                break;
            case Type.Speed:
                ModAmount = Random.Range(1.1f, 1.3f); // 10% to 30% Speed boost
                break;
            case Type.MomentumGain:
                ModAmount = Random.Range(1.1f, 1.3f); // 10% to 30% Momentum Gain boost
                break;
        }
    }

    public void ApplyBonus() 
    {
        InitMod();
        Debug.Log("Mod Type: " + ModType + " | Mod Amount: " + ModAmount);
        switch (ModType)
        {
            case Type.Health:
                GameManager.Instance.Player.GetComponent<PlayerController>().HealthMax *= ModAmount;
                GameManager.Instance.Player.GetComponent<PlayerController>().Health = GameManager.Instance.Player.GetComponent<PlayerController>().HealthMax;
                GameManager.Instance.Player.GetComponent<PlayerController>().UpdatePlayerUI();
                break;


            case Type.Shield:
                GameManager.Instance.Player.GetComponent<PlayerController>().ShieldMax = GameManager.Instance.Player.GetComponent<PlayerController>().ShieldMax * ModAmount;
                GameManager.Instance.Player.GetComponent<PlayerController>().Shield = GameManager.Instance.Player.GetComponent<PlayerController>().ShieldMax;
                GameManager.Instance.Player.GetComponent<PlayerController>().UpdatePlayerUI();
                break;


            case Type.AlienEnergy:
                GameManager.Instance.Player.GetComponent<PlayerController>().AlienEnergyMax = GameManager.Instance.Player.GetComponent<PlayerController>().AlienEnergyMax * ModAmount;
                GameManager.Instance.Player.GetComponent<PlayerController>().AlienEnergy = GameManager.Instance.Player.GetComponent<PlayerController>().AlienEnergyMax;
                GameManager.Instance.Player.GetComponent<PlayerController>().UpdatePlayerUI();
                break;


            case Type.Speed:
                GameManager.Instance.Player.GetComponent<PlayerController>().Speed = GameManager.Instance.Player.GetComponent<PlayerController>().SpeedBase * ModAmount;
                break;


            case Type.MomentumGain:
                GameManager.Instance.Player.GetComponent<PlayerController>().MomentumBuildRate = GameManager.Instance.Player.GetComponent<PlayerController>().BaseMomentumBuildRate * ModAmount;
                break;
        }
    }
}
