using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : CharacterBase
{
    public AimControl[] aimControllers;
    public int MoveSpeed;
    public LayerMask masks;
    public Transform player;
    public bool seePlayer = false;
    public Vector3 playerDir;
    public float DistanceToPlayer;
    [SerializeField] EnemyBehavior behavior;
    [SerializeField] public NavMeshAgent agent;

    void Start()
    {
        agent.speed = MoveSpeed;
        agent.angularSpeed = MoveSpeed;
        agent.stoppingDistance = Weapon_R.ShootDistance;
    }
    // Update is called once per frame
    void Update()
    {
        if(player == null)
        {
            seePlayer = false;
        }
        else
        {
            seePlayer = false;
            playerDir = player.position - transform.position;
            DistanceToPlayer = playerDir.magnitude;
            RaycastHit hit;
            Vector3 origin = transform.position;
            if (Physics.Raycast(origin, playerDir.normalized, out hit, DistanceToPlayer, masks))
            {
                seePlayer = hit.transform.root.CompareTag("Player");
            }
        }
        behavior.Tick();

        if (Weapon_R != null && Weapon_R.IsOut)
        {
            Weapon_R.Reload();
        }
        if (Weapon_L != null && Weapon_L.IsOut)
        {
            Weapon_L.Reload();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = null;
        }
    }

    public void rotateToTarget()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, 0, playerDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * MoveSpeed);
    }
}