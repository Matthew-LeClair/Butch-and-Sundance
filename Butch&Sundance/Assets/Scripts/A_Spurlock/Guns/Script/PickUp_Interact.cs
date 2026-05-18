using UnityEngine;

public class PickUp_Interact : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] public Renderer Mat;
    [SerializeField] public Material Outline;
    Material OriginalMat;

    bool InRange;

    private void Update()
    {
        if (InRange)
        {
            if (Input.GetKeyDown(KeyCode.E))
            { EventPickUp(); }
            if (Input.GetKeyUp(KeyCode.E)) { Destroy(gameObject); } // Destroy the Pickup
        }
    }

    private void Start()
    {
        if (Mat != null && Outline != null)
        {
            OriginalMat = Mat.material; // Store Original Material
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Mat.material = Outline; // Apply Outline Material
            InRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Mat.material = OriginalMat; // Apply Outline Material
            InRange = false;
        }
    }

    public virtual void EventPickUp() { }
}
