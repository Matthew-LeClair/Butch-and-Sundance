using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && GameManager.Instance.PlayerStartPos.transform.position != transform.position)
        {
            GameManager.Instance.PlayerStartPos.transform.position = transform.position;
            StartCoroutine(displayPopup());
        }
    }

    IEnumerator displayPopup()
    {
        GameManager.Instance.CheckpointPopup.SetActive(true);
        yield return new WaitForSeconds(3);
        GameManager.Instance.CheckpointPopup.SetActive(false);
    }

}
