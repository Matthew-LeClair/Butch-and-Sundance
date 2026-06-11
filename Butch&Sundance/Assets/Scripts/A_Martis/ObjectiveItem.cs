using UnityEngine;

public class ObjectiveItem : MonoBehaviour
{
    bool PlayerInRange;

    void Update()
    {
        if(PlayerInRange && Input.GetButtonDown("Interact"))
        {
            GameManager.Instance.CollectedObjectiveItem();
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            PlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            PlayerInRange = false;
        }
    }
}
