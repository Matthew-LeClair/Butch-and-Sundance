using UnityEngine;
using System.Collections;


// Updates the player's respawn position and displays a checkpoint popup
public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Ensure the collider belongs to the player and prevent reactivating the same checkpoint repeatedly
        if (other.CompareTag("Player") && GameManager.Instance.RespawnPosition != transform.position)
        {
            GameManager.Instance.RespawnPosition = transform.position;
            StartCoroutine(displayPopup());
        }
    }

    // Display brief checkpoint popup
    IEnumerator displayPopup()
    {
        GameManager.Instance.CheckpointPopup.SetActive(true);
        yield return new WaitForSeconds(3);
        GameManager.Instance.CheckpointPopup.SetActive(false);
    }

}
