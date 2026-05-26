using UnityEngine;
using System.Collections;

public class DoorBehavior : MonoBehaviour
{
    [SerializeField] GameObject doorModel;
    [SerializeField] GameObject doorButtons;

    [SerializeField] float slideDistance;
    [SerializeField] float slideSpeed;

    bool playerInTrigger;
    bool isOpen;
    bool isMoving;

    Vector3 closedPos;
    Vector3 openPos;

    private void Start()
    {
        closedPos = doorModel.transform.position;

        openPos = closedPos + Vector3.down * slideDistance;

        if(doorButtons != null ) doorButtons.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (playerInTrigger && !isMoving)
        {
            if (Input.GetButtonDown("Interact"))
            {
                if (!isOpen)
                {
                    StartCoroutine(MoveDoor(openPos, true));
                }
            }
        }
    }

    IEnumerator MoveDoor(Vector3 targetPos, bool opening)
    {
        isMoving = true;
        while (Vector3.Distance(doorModel.transform.position, targetPos) > 0.01f)
        {
            doorModel.transform.position = Vector3.MoveTowards(
                doorModel.transform.position, targetPos, slideSpeed * Time.deltaTime);
            yield return null;
        }
        doorModel.transform.position = targetPos;

        isOpen = opening;
        isMoving = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
            if (doorButtons != null)
            {
                doorButtons.SetActive(true);
            }
        }
    }

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
}
