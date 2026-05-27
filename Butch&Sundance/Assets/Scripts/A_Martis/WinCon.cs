using UnityEngine;

// Handles the player's win condition when entering the trigger
// Requires the current game objective to be completed before winning

public class WinCon : MonoBehaviour
{
    [SerializeField] bool destroyOnWin;

    bool hasWon; //Prevents win from triggering multiple times

    private void OnTriggerEnter(Collider other)
    {
        //Stop if player has already won
        if (hasWon) return;

        // Prevent win until objective is complete
        if (!GameManager.Instance.GoalCompleted)
        {
            Debug.Log("Objective not completed yet");
            return;
        }

        // Try to find the PlayerController directly
        PlayerController player = other.GetComponent<PlayerController>();

        // If not found, checks the parent object
        if (player == null)
        {
            player = other.GetComponentInParent<PlayerController>();
        }

        // Final fallback, checks the root object
        if(player == null)
        {
            player = other.transform.root.GetComponent<PlayerController>();
        }

        // If valid player was found, trigger win con
        if (player != null)
        { 
            hasWon = true;

            // Call win function
            GameManager.Instance.YouWin();

            if (destroyOnWin)
            {
                Destroy(gameObject);
            }
        }
    }
}
