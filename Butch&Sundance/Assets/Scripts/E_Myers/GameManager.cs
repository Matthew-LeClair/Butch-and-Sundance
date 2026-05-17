using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // GameManager Instance

    [SerializeField] GameObject MenuActive; // Active Menu
    [SerializeField] GameObject MenuPause; // Pause Menu
    [SerializeField] GameObject MenuWin; // Win Screen
    [SerializeField] GameObject MenuLose; // Lose Screen
    public Image PlayerHP_Bar;
    public GameObject PlayerDamage_Screen;
    public GameObject PlayerShield_Screen;

    public bool IsPaused; // IsPaused Bool
    public GameObject Player; // Player GameObject
    public PlayerController PlayerScript; // Player Controller Script

    float TimeScale_Original; // Chached Original Time Scale for better setting

    public int KillCount;


    // Awake is called once before the first execution of Start after the MonoBehaviour is created
    void Awake()
    {
        Instance = this; // Set the Instance
        Player = GameObject.FindWithTag("Player"); // Find Player GameObject by Tag 
        PlayerScript = Player.GetComponent<PlayerController>(); // Get the Player Controller Script Component from Player
        TimeScale_Original = Time.timeScale; // Set Time Scale Original
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel")) // If [Escape] is pressed
        {
            if (MenuActive == null) // If MenuActive is Null...
            {
                StatePause(true); // Pause Game
                MenuActive = MenuPause; // Set MenuActive as MenuPause
                MenuActive.SetActive(true); // Set MenuActive as Active
            }
            else if (MenuActive == MenuPause) // If Menu Active is NOT Null and instead EQUAL TO MenuPause
            { StatePause(false); } // Resume Game
        }
    }

    public void StatePause(bool ShouldPause) // Made Modular for Pause and Unpause || Easier for me to use and understand
    {
        if (ShouldPause) // If Should Pause
        { // Pause by...

            IsPaused = true; // Setting IsPaused to True
            Time.timeScale = 0.0f; // Setting Time Scale to 0
            Cursor.visible = true; // Making Cursor Visble
            Cursor.lockState = CursorLockMode.None; // Unlocking the Cursor
        }
        else // If Should NOT Pause...
        { // Resume by...

            IsPaused = false; // Setting IsPaused to False
            Time.timeScale = TimeScale_Original; // Resetting Time Scale to Original Value
            Cursor.visible = false; // Making the Cursor Invisible
            Cursor.lockState = CursorLockMode.Locked; // Locking the Cursor

            MenuActive.SetActive(false); // Set Pause Menu Inactive
            MenuActive = null; // Set Pause Menu Null
        }
    }


    public void UpdateGameGoal(int Amount) 
    {
        KillCount += Amount; // Dynamically Increment or Decrement GameGoalCount
        if (KillCount <= 0) // If Goal is MET
        { // WIN!!
            StatePause(true); // Pause Game
            MenuActive = MenuWin; // Set MenuActive as MenuWin
            MenuActive.SetActive(true); // Set MenuActive as Active
        }
    }

    public void YouLose() 
    {
        StatePause(true); // Pause Game
        MenuActive = MenuLose; // Set MenuActive as MenuLose
        MenuActive.SetActive(true); // Set MenuActive as Active
    }
}