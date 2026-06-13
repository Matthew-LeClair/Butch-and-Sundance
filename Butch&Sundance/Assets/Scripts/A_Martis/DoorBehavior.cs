using UnityEngine;
using System.Collections;

// Handles an interactable sliding door system.
// The player can open the door while inside the trigger area, and the door automatically closes when the player exits.

public class DoorBehavior : MonoBehaviour
{
    [SerializeField] GameObject doorModel; // Physical door object that moves
    [SerializeField] GameObject doorButtons; // Interaction UI/buttons

    [SerializeField] float slideDistance; // Distance the door moves when opening
    [SerializeField] float slideSpeed; // Speed of the sliding movement

    [SerializeField] Vector3 slideDirection; // Direction the door slides toward

    bool playerInTrigger; // Tracks if the player is within interaction range
    bool isOpen; // Tracks whether the door is currently open
    bool isMoving; // Prevents overlapping movement coroutines

    Vector3 closedPos;
    Vector3 openPos;

    Transform playerOnDoor;

    [SerializeField] bool IsBossDoor;

    [SerializeField] bool requiresButton;
    [SerializeField] PressurePlate linkedPlate;

    private void Start()
    {
        // Store the starting position as the closed position
        closedPos = doorModel.transform.position;

        // Calculate the open position using direction and distanc
        openPos = closedPos + slideDirection.normalized * slideDistance;

        // Hide interaction buttons at start
        if (doorButtons != null ) doorButtons.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (requiresButton && linkedPlate != null && !linkedPlate.IsActive) return;
        // Allow interaction only if player is nearby and door is not moving
        if (playerInTrigger && !isMoving)
        {
            // Check for interaction input
            if (Input.GetButtonDown("Interact"))
            {
                if (IsBossDoor)
                {
                    if (GameManager.Instance.HasAllObjectiveItems())
                    {
                        if (!isOpen)
                        {
                            StartCoroutine(MoveDoor(openPos, true));
                        }
                    }
                    else
                    {
                        Debug.Log("Need all objective items.");
                    }
                }
                else
                {
                    // Open the door if currently closed
                    if (!isOpen)
                    {
                        StartCoroutine(MoveDoor(openPos, true));
                    }
                }
                    // Hide interaction UI after interacting
                    if (doorButtons != null)
                    {
                        doorButtons.SetActive(false);
                    }                
            }
        }
    }

    public void SetExternalOpen(bool open)
    {
        if (isMoving)
        {
            StopAllCoroutines();
            isMoving = false;
        }

        if (open && !isOpen)
        {
            StartCoroutine (MoveDoor(openPos, true));
        }
        else if (!open && isOpen)
        {
            StartCoroutine(MoveDoor(closedPos, false));
        }
    }

    IEnumerator MoveDoor(Vector3 targetPos, bool opening)
    {
        isMoving = true;

        // Continue moving until the door reaches the target position
        while (Vector3.Distance(doorModel.transform.position, targetPos) > 0.01f)
        {
            // Store previous position
            Vector3 previousPos = doorModel.transform.position;

            // Move door
            doorModel.transform.position = Vector3.MoveTowards(
                doorModel.transform.position, targetPos, slideSpeed * Time.deltaTime);

            // Move player WITH door
            Vector3 moveDelta = doorModel.transform.position - previousPos;
            if (playerOnDoor != null)
            {
                playerOnDoor.position += moveDelta;
            }


            yield return null;
        }
        // Snap exactly to the target position
        doorModel.transform.position = targetPos;

        isOpen = opening;
        isMoving = false;
    }


    // Detects the player for interactions
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
            if (!isOpen && doorButtons != null)
            {
                doorButtons.SetActive(true);
            }
        }
    }

    // Detects when player has left for interactions
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;

            if (doorButtons != null)
            {
                doorButtons.SetActive(false);
            }

            if(isOpen && !isMoving)
            {
                StartCoroutine (MoveDoor(closedPos, false));
            }
        }
    }

    // Detect player TOUCHING door

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            playerOnDoor = collision.transform;
        }
    }

    // Detect when player STOPS TOUCHING door

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            if (playerOnDoor == collision.transform)
            {
                playerOnDoor = null;
            }
        }
    }
}
