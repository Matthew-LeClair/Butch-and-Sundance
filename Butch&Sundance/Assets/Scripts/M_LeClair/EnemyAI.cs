using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class EnemyAI : CharacterBase
{

    bool seePlayer;
    Vector3 playerDir;
    float DistanceToPlayer;
    [SerializeField] int MoveSpeed;
    [SerializeField] EnemyMovement movement;
    [SerializeField] AimControl[] aimControllers;

    // Update is called once per frame
    void Update()
    {
        if (seePlayer)
        {
            playerDir = GameManager.Instance.Player.transform.position - transform.position;
            DistanceToPlayer = Vector3.Distance(transform.position, GameManager.Instance.Player.transform.position);
            rotateToTarget();
            bool rightInRange = Weapon_R != null && DistanceToPlayer < Weapon_R.ShootDistance;
            bool leftInRange = Weapon_L != null && DistanceToPlayer < Weapon_L.ShootDistance;
            foreach (AimControl aim in aimControllers)
            {
                aim.SetAiming(true);
                aim.AimAtTarget(GameManager.Instance.Player.transform.position);
            }    
            if (rightInRange)
                {
                    Weapon_R.Shoot();
                }
                if (leftInRange)
                {
                    Weapon_L.Shoot();
                }
            if(!rightInRange && !leftInRange)
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
            seePlayer = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            seePlayer = false;
        }
    }

    void rotateToTarget()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, 0, playerDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * MoveSpeed);
    }
}