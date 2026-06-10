using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PullableObject : MonoBehaviour
{
    Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void PullTowards(Vector3 targetPosition, float force)
    {
        Vector3 dir = (targetPosition - transform.position).normalized;
        rb.AddForce(dir * force, ForceMode.Acceleration);
    }
}
