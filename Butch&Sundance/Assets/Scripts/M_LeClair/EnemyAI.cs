using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : EnemyBase
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

    [SerializeField] public float FOV;
     public float FOVOrig;

    public override void Start()
    {
        base.Start();
        FOVOrig = FOV;
        agent.speed = MoveSpeed;
        agent.angularSpeed = MoveSpeed;
        if (Weapon_R != null)
        {
            agent.stoppingDistance = Weapon_R.ShootDistance;
        }

        GameManager.Instance.UpdateGameGoal(1);
    }

    void Update()
    {
        if (player == null)
        {
            seePlayer = false;
        }
        else
        {
            seePlayer = false;
            playerDir = player.position - transform.position;
            DistanceToPlayer = playerDir.magnitude;

            float angleToPlayer = Vector3.Angle(transform.forward, playerDir);
            if (angleToPlayer <= FOV)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, playerDir.normalized, out hit, DistanceToPlayer, masks))
                {
                    seePlayer = hit.transform.root.CompareTag("Player");
                }
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

    private void OnTriggerStay(Collider other)
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