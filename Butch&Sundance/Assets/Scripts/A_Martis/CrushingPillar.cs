using UnityEngine;

public class CrushingPillar : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] public float raisedHeight;
    [SerializeField] public float moveSpeed;
    [SerializeField] public float topWaitTime;
    [SerializeField] public float bottomWaitTime;

    [Header("Crushing")]
    public Transform crushPoint;
    [SerializeField] Vector3 crushBoxSize;
    public LayerMask crushLayers;

    private Vector3 startPosition;
    private Vector3 raisedPosition;
    private Vector3 loweredPosition;

    private bool movingDown = true;
    private float waitTimer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        loweredPosition = transform.position;
        raisedPosition = transform.position + Vector3.up * raisedHeight;
        transform.position = raisedPosition;
        waitTimer = topWaitTime;
    }

    // Update is called once per frame
    private void Update()
    {
        if (waitTimer > 0)
        {
            waitTimer -= Time.deltaTime;
            return;
        }

        if (movingDown)
        {
            transform.position = Vector3.MoveTowards(transform.position, loweredPosition, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, loweredPosition) < 0.01f)
            {
                CrushTargets();

                movingDown = false;
                waitTimer = bottomWaitTime;
            }
        }
        else
        {
            transform .position = Vector3.MoveTowards(transform.position, raisedPosition, moveSpeed * Time.deltaTime);
            
            if (Vector3.Distance(transform.position, raisedPosition) < 0.01f)
            {
                movingDown = true;
                waitTimer = topWaitTime;
            }
        }
    }

    private void CrushTargets()
    {
        Collider[] hits = Physics.OverlapBox(crushPoint.position, crushBoxSize * 0.5f, crushPoint.rotation, crushLayers);
        
        foreach (Collider hit in hits)
        {
            I_Damage damageable = hit.GetComponent<I_Damage>();

            if (damageable != null)
            {
                damageable.TakeDamage(999999, false);
                continue;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (crushPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(crushPoint.position, crushPoint.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, crushBoxSize);
    }
}
