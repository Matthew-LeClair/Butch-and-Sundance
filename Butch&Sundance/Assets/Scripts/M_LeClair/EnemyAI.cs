using System.Collections;
using UnityEditor;
using UnityEngine;

public class EnemyAI : CharacterBase
{

    bool seePlayer;
    Vector3 playerDir;
    float DistanceToPlayer;
    [SerializeField] int MoveSpeed;
    [SerializeField] EnemyMovement movement;
    [SerializeField] AimControl aimControl;

    // Update is called once per frame
    void Update()
    {
        if (seePlayer)
        {
            playerDir = GameManager.Instance.Player.transform.position - transform.position;
            DistanceToPlayer = Vector3.Distance(transform.position, GameManager.Instance.Player.transform.position);
            rotateToTarget();
            aimControl.SetAiming(true);
            aimControl.AimAtTarget(GameManager.Instance.Player.transform.position);
            if (DistanceToPlayer < Weapon.ShootDistance)
            {
                Weapon.Shoot();
            }
            else
            {
                movement.Move(playerDir, MoveSpeed);
            }
        }
        else
        {
            aimControl.ResetAim();
        }
        if (Weapon.IsOut)
        {
            Weapon.Reload();
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