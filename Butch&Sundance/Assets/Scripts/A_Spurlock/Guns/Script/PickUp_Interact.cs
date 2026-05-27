using UnityEngine;

// Base class for all interactable pickups in the world.
// Handles proximity detection via trigger collider, outline material swapping to signal availability,
// and the E-key interaction that fires the virtual EventPickUp() implemented by each subclass.
public class PickUp_Interact : MonoBehaviour
{

    //===[Visual]===\\

    [Header("Visual")]
    [SerializeField] public Renderer Mat;       // The renderer whose material is swapped to show the outline
    [SerializeField] public Material Outline;   // The outline material applied when the player enters range
    Material OriginalMat;                       // Cached original material restored when the player leaves range
    bool InRange;                               // True while the player collider is inside this pickup's trigger


    //===[Lifecycle]===\\

    // Called once by Unity before the first frame, or by a subclass via base.Start().
    // Caches the renderer's original material so it can be restored on trigger exit.
    // Both Mat and Outline must be assigned in the Inspector for the swap to work safely.
    public virtual void Start()
    {
        if (Mat != null && Outline != null)
        {
            OriginalMat = Mat.material; // Store the original material before any swapping occurs
        }
    }

    // Called every frame by Unity.
    // Listens for the E key only while the player is inside the trigger zone.
    // EventPickUp() is responsible for its own success logic and for destroying this GameObject when appropriate.
    private void Update()
    {
        if (InRange && Input.GetKeyDown(KeyCode.E)) // Only react when in range and E is pressed this frame
        {
            EventPickUp(); // Delegate to the subclass implementation - destruction handled there on success
        }
    }


    //===[Trigger]===\\

    // Called by Unity when a collider enters this object's trigger zone.
    // Swaps to the outline material and marks the pickup as in range so the Update loop activates.
    // Only reacts to the Player tag to avoid being triggered by enemies or projectiles.
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Mat.material = Outline; // Swap to outline material to signal the pickup is available
            InRange = true;         // Activate the E-key listener in Update
        }
    }

    // Called by Unity when a collider exits this object's trigger zone.
    // Restores the original material and deactivates the E-key listener.
    // Only reacts to the Player tag to mirror the enter behaviour.
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Mat.material = OriginalMat; // Restore original material when player leaves range
            InRange = false;            // Deactivate the E-key listener in Update
        }
    }


    //===[Events]===\\

    // Virtual pickup handler overridden by each subclass to implement type-specific pickup logic.
    // The subclass is responsible for calling Destroy(gameObject) after a successful pickup.
    // The base implementation is intentionally empty - this class only handles proximity and input.
    public virtual void EventPickUp() { }
}