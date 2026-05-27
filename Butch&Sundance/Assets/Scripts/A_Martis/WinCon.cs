using UnityEngine;

public class WinCon : MonoBehaviour
{
    [SerializeField] bool destroyOnWin;

    bool hasWon;

    private void OnTriggerEnter(Collider other)
    {
        if (hasWon) return;

        if (!GameManager.Instance.GoalCompleted)
        {
            Debug.Log("Objective not completed yet");
            return;
        }

        PlayerController player = other.GetComponent<PlayerController>();

        if (player == null)
        {
            player = other.GetComponentInParent<PlayerController>();
        }

        if(player == null)
        {
            player = other.transform.root.GetComponent<PlayerController>();
        }

        if (player != null)
        { 
            hasWon = true;

            GameManager.Instance.YouWin();

            if (destroyOnWin)
            {
                Destroy(gameObject);
            }
        }
    }
}
