using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // GameManager Instance

    [Header("Menu Elements")]
    [SerializeField] GameObject MenuActive; // Active Menu
    [SerializeField] GameObject MenuPause; // Pause Menu
    [SerializeField] GameObject MenuWin; // Win Screen
    [SerializeField] GameObject MenuLose; // Lose Screen

    

    [Header("Player Elements")]
    public Image PlayerHP_Bar;
    public Image AlienEnergy_Bar;
    public Image PlayerShieldHP_Bar;
    public TMP_Text WeaponNameText;
    public TMP_Text AmmoCount;
    public GameObject PlayerDamage_Screen;
    public GameObject PlayerShield_Screen;
    public GameObject LowHealth_Screen;
    public GameObject PlayerMomentum;
    

    [Header("Checkpoint Elements")]
    public GameObject CheckpointPopup;
    public GameObject PlayerStartPos;

    public bool IsPaused; // IsPaused Bool
    public GameObject Player; // Player GameObject
    public PlayerController PlayerScript; // Player Controller Script

    float TimeScale_Original; // Chached Original Time Scale for better setting

    [Header("Objective Items")]
    public GameObject ItemsLeftPopUp;
    public int CollectedItems;
    public int RequiredItems = 3;
    public TMP_Text ObjectiveText;

    // Old stuff that needs to be reworked
    public int KillCount;
    public bool GoalCompleted;
    public Vector3 RespawnPosition;
    public bool killGoal;

    // Awake is called once before the first execution of Start after the MonoBehaviour is created
    void Awake()
    {
        Instance = this; // Set the Instance
        Player = GameObject.FindWithTag("Player"); // Find Player GameObject by Tag 
        PlayerScript = Player.GetComponent<PlayerController>(); // Get the Player Controller Script Component from Player
        TimeScale_Original = Time.timeScale; // Set Time Scale Original
        PlayerStartPos = GameObject.FindWithTag("Player Start Pos"); // Find Player Start Position GameObject by Tag
        RespawnPosition = PlayerStartPos.transform.position;

        MenuPause.SetActive(false); // Ensure the pause menu is initially inactive

        MenuWin.SetActive(false); // Ensure the win menu is initially inactive

        MenuLose.SetActive(false); // Ensure the lose menu is initially inactive
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        { 
            if (MenuActive == null) // Check if there is no active menu
            {
                StatePause(); // Call the method to pause the game and show the pause menu

                MenuActive = MenuPause; // Set the active menu to the pause menu

                MenuActive.SetActive(true); // Activate the pause menu GameObject to show the menu
            }
            else if (MenuActive == MenuPause) // Check if the active menu is the pause menu
            {
                StateUnpause(); // Call the method to unpause the game and hide the pause menu
            }
        }

        if (killGoal)
        {
            if(KillCount <= 0)
            {
                GoalCompleted = true;
            }
        }
    }

    public void StatePause()
    {
        IsPaused = true; // Set the menu paused flag to true

        Time.timeScale = 0; // Pause the game by setting the time scale to 0

        Cursor.visible = true; // Make the cursor visible

        Cursor.lockState = CursorLockMode.None; // Unlock the cursor so it can be moved freely
    }

    public void StateUnpause()
    {
        IsPaused = false; // Set the menu paused flag to false

        Time.timeScale = TimeScale_Original; // Restore the original time scale to resume the game

        Cursor.visible = false; // Hide the cursor

        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center of the screen

        MenuActive.SetActive(false); // Deactivate the menu GameObject to hide the menu

        MenuActive = null; // Clear the reference to the pause menu GameObject
    }


    public void CollectedObjectiveItem() 
    {
        CollectedItems++;

        CollectedItems = Mathf.Clamp(CollectedItems, 0, RequiredItems);

        if (ObjectiveText != null)
        {
            ObjectiveText.text = CollectedItems + "/" + RequiredItems;
        }
    }

    public bool HasAllObjectiveItems()
    {
        return CollectedItems >= RequiredItems;
    }

    public void OnWeaponChanged(AlienTech weapon, int slotIndex)
    {
         if (weapon != null)
        {
            if (WeaponNameText != null)
                WeaponNameText.text = weapon.typeMod.ToString(); // Update UI with AlienTech type name
        }
        else
        {
            if (WeaponNameText != null)
                WeaponNameText.text = "Revolver";               // Update UI with base weapon name
        }
        UpdateAmmoUI(slotIndex); // Sync ammo on weapon switch
    }

    public void UpdateAmmoUI(int slot)
    {
        PlayerGun pGun = PlayerScript.pGun;

        if (AmmoCount != null)

            AmmoCount.text = $"{pGun.CurrAmmo[slot]} / {pGun.MaxAmmo[slot]}";
    }

    public void YouWin()
    {
        StatePause();
        MenuActive = MenuWin;
        MenuActive.SetActive(true);
    }

    public void YouLose() 
    {
        if (PlayerDamage_Screen != null) PlayerDamage_Screen.SetActive(false);
        if (LowHealth_Screen != null) LowHealth_Screen.SetActive(false);
        if (PlayerShield_Screen != null) PlayerShield_Screen.SetActive(false);

        StatePause(); // Pause Game
        MenuActive = MenuLose; // Set MenuActive as MenuLose
        MenuActive.SetActive(true); // Set MenuActive as Active
    }

    Coroutine ActiveCheckpointPopup;
    public Checkpoint ActiveCheckpoint;

    public void ShowCheckpointPopup(float dur = 3f)
    {
        if(ActiveCheckpointPopup != null)
        {
            StopCoroutine(ActiveCheckpointPopup);
        }
        ActiveCheckpointPopup = StartCoroutine(CheckpointPopupRoutine(dur));
    }

    IEnumerator CheckpointPopupRoutine(float duration)
    {
        CheckpointPopup.SetActive(true);
        yield return new WaitForSeconds(duration);
        CheckpointPopup.SetActive(false);
        ActiveCheckpointPopup = null;
    }
    public void CollectedItemsRemain(int collection)
    {
        if (ItemsLeftPopUp != null)
        {
            ItemsLeftPopUp.SetActive(true);

            ItemsLeftPopUp.GetComponent<TMP_Text>().text = collection + " of " + RequiredItems + " found";

        }
    }
}