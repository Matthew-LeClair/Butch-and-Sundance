using System.Collections;
using UnityEditor;
using UnityEngine;

public class EnemyAI : CharacterBase
{

    bool seePlayer;
    Vector3 playerDir;

    // Update is called once per frame
    void Update()
    {
        if (seePlayer)
        {
            playerDir = GameManager.Instance.Player.transform.position - transform.position;
            rotateToTarget();
            if (!IsAiming)
            {
                Aim();
            }
            else
            {
                Vector3 targetPos = GameManager.Instance.Player.transform.position + Vector3.up;
                Vector3 dir = targetPos - WeaponArm.transform.position;
                Quaternion rot = Quaternion.LookRotation(dir) * Quaternion.Euler(-28, 70, -70);
                WeaponArm.transform.rotation = Quaternion.Lerp(WeaponArm.transform.rotation, rot, Time.deltaTime * AimSpeed);
                Weapon.Shoot();
            }
        }
        else
        {
            if (IsAiming)
            {
                Aim();
                Weapon.Reload();
            }
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
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime);
    }
}