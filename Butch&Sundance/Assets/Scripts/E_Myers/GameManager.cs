using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] GameObject MenuActive;

    [SerializeField] GameObject MenuPause;

    [SerializeField] GameObject WinScreen;

    [SerializeField] GameObject MenuGameOver;

    [SerializeField] GameObject NextWave;

    public Image HealthBar;

    public GameObject PlayerDamageScreen;

    public TMP_Text GameGoalCountText;

    public bool IsGameActive;

    public GameObject Player;

    public CharacterController PlayerScript;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
