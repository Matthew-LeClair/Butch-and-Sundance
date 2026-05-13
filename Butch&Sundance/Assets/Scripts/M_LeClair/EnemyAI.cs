using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class EnemyAI : CharacterBase
{
    public AimControl[] aimControllers;
    public EnemyMovement movement;
    public int MoveSpeed;
    public LayerMask masks;
    public Transform player;
    public bool seePlayer = false;
    public Vector3 playerDir;
    public float DistanceToPlayer;
    [SerializeField] EnemyBehavior behavior;


    // Update is called once per frame
    void Update()
    {
        if(player == null)
        {
            foreach(AimControl aim in aimControllers)
            {
                aim.ResetAim();
            }
            return;
        }
        playerDir = player.position - transform.position;
        DistanceToPlayer = playerDir.magnitude;
        Debug.DrawRay(transform.position, playerDir.normalized * DistanceToPlayer, Color.red);
        if (Physics.Raycast(transform.position,playerDir.normalized, out RaycastHit see, DistanceToPlayer, masks))
        {
            Debug.Log("HIT: " + see.transform.name);
            seePlayer = see.transform.CompareTag("Player");
        }
        else
        {
            Debug.Log("No hit");
            seePlayer = false;
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