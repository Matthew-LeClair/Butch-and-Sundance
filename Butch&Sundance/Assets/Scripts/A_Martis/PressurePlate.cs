using UnityEngine;
using System.Collections.Generic;

public class PressurePlate : MonoBehaviour
{
    [SerializeField] DoorBehavior linkedDoor;
    [SerializeField] float requiredWeight;

    float currWeight = 0f;

    HashSet<Rigidbody> objectsOnPlate = new HashSet<Rigidbody>();
    public bool IsActive => currWeight >= requiredWeight;

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;

        if(rb != null )
        {
            if(objectsOnPlate.Add(rb))
            {
                currWeight += rb.mass;
            }
        }

        CheckState();
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;

        if(rb != null )
        {
            if(objectsOnPlate.Remove(rb))
            {
                currWeight -= rb.mass;
            }
        }
        CheckState();   
    }

    void CheckState()
    {
        bool pressed = currWeight >= requiredWeight;

        if(linkedDoor != null)
        {
            linkedDoor.SetExternalOpen(pressed);
        }
    }
}
