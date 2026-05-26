using UnityEngine;

public abstract class PowerupBase : MonoBehaviour
{
    [SerializeField] protected float duration;
    [SerializeField] protected bool destroyOnPickup;

    public void Activate(PlayerController player)
    {
        ApplyEffect(player);

        if (duration > 0)
        {
            StartCoroutine(RemoveAfterTime(player));
        }
        if (destroyOnPickup)
        {
            GetComponent<Collider>().enabled = false;
            GetComponent<Renderer>().enabled = false;
        }
    }

    protected abstract void ApplyEffect(PlayerController player);

    protected abstract void RemoveEffect(PlayerController player);

    System.Collections.IEnumerator RemoveAfterTime(PlayerController player)
    {
        yield return new WaitForSeconds(duration);

        RemoveEffect(player);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger: " +other.name);

        PlayerController player = other.GetComponent<PlayerController>();

        if(player == null)
        {
            player = other.GetComponentInParent<PlayerController>();
        }
        if(player == null)
        {
            player = other.transform.root.GetComponent<PlayerController>();
        }
        if (player != null)
        {
            Debug.Log("Found");
            Activate(player);
        }
    }
}
