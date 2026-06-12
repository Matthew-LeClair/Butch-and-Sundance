using UnityEngine;
using System.Collections;


// Updates the player's respawn position and displays a checkpoint popup
public class Checkpoint : MonoBehaviour
{
    [SerializeField] Renderer checkpointRenderer;
    [SerializeField] Color inactive;
    [SerializeField] Color active;
    bool IsActivated;

    void Start()
    {
        SetGlow(inactive);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || IsActivated) return;

        if(GameManager.Instance.ActiveCheckpoint != null && GameManager.Instance.ActiveCheckpoint != this)
        {
            GameManager.Instance.ActiveCheckpoint.IsActivated = false;
            GameManager.Instance.ActiveCheckpoint.SetGlow(inactive);
        }

        GameManager.Instance.ActiveCheckpoint = this;

        IsActivated = true;

        GameManager.Instance.RespawnPosition = transform.position;

        SetGlow(active);

        GameManager.Instance.ShowCheckpointPopup();
    }

    public void SetGlow(Color glowColor)
    {
        if (checkpointRenderer == null) return;

        Material mat = checkpointRenderer.material;

        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", glowColor);
    }
}
