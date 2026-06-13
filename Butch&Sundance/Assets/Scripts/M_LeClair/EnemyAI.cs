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

    [SerializeField] public int RoamDistance;
    [SerializeField] public int RoamPauseTime;
    Vector3 startingPos;
    float roamTimer;

    public override void Start()
    {
        base.Start();
        startingPos = transform.position;
        FOVOrig = FOV;
        agent.speed = MoveSpeed;
        agent.angularSpeed = MoveSpeed;
        if (Weapon_R != null)
        {
            agent.stoppingDistance = Weapon_R.ShootDistance;
        }

        //GameManager.Instance.UpdateGameGoal(1);
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
            Vector3 rayOrigin = transform.position + Vector3.up * 1f;
            Vector3 rayTarget = player.position + Vector3.up * 1f;
            Vector3 rayDir = (rayTarget - rayOrigin).normalized;
            float dist = Vector3.Distance(rayOrigin, rayTarget);

            playerDir = (player.position + Vector3.up * 1f) - (transform.position + Vector3.up * 1f);
            DistanceToPlayer = dist;

            float angleToPlayer = Vector3.Angle(transform.forward, playerDir);
            if (angleToPlayer <= FOV)
            {
                RaycastHit hit;
                if (Physics.Raycast(rayOrigin, rayDir, out hit, dist, masks))
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

    public void CheckRoam()
    {
        if (!agent.isOnNavMesh) { 
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            roamTimer += Time.deltaTime;

            if (roamTimer >= RoamPauseTime)
            {
                roamTimer = 0;

                Vector3 randPos = Random.insideUnitSphere * RoamDistance;
                randPos += startingPos;

                NavMeshHit hit;
                if (NavMesh.SamplePosition(randPos, out hit, RoamDistance, 1))
                {
                    agent.SetDestination(hit.position);
                }
            }
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