using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class EnemyAI : CharacterBase
{

    Transform player;
    bool seePlayer = false;
    Vector3 playerDir;
    float DistanceToPlayer;
    [SerializeField] int MoveSpeed;
    [SerializeField] EnemyMovement movement;
    [SerializeField] AimControl[] aimControllers;
    [SerializeField] LayerMask masks;

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {
            return;
        }
        playerDir = player.position - transform.position;
        DistanceToPlayer = Vector3.Distance(transform.position, player.position);
        Debug.DrawRay(transform.position, playerDir.normalized * DistanceToPlayer, Color.red);
        if (Physics.Raycast(transform.position, playerDir.normalized, out RaycastHit see, DistanceToPlayer, masks))
        {
            Debug.Log("HIT: " + see.transform.name);
            seePlayer = see.transform.CompareTag("Player");
        }
        else
        {
            Debug.Log("No hit");
            seePlayer = false;
        }
        if (seePlayer)
        {
            rotateToTarget();
            bool rightInRange = Weapon_R != null && DistanceToPlayer < Weapon_R.ShootDistance;
            bool leftInRange = Weapon_L != null && DistanceToPlayer < Weapon_L.ShootDistance;
            foreach (AimControl aim in aimControllers)
            {
                aim.SetAiming(true);
                aim.AimAtTarget(player.position);
            }
            if (rightInRange)
            {
                Weapon_R.Shoot();
            }
            if (leftInRange)
            {
                Weapon_L.Shoot();
            }
            if (!rightInRange && !leftInRange)
            {
                movement.Move(playerDir, MoveSpeed);
            }
        }
        else
            foreach (AimControl aim in aimControllers)
            {
                {
                    aim.ResetAim();
                }
            }
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

    void rotateToTarget()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, 0, playerDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * MoveSpeed);
    }
}