using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class Portals : MonoBehaviour
{
    public Portals LinkedPortal;
    [Header("Settings")]
    [SerializeField] float teleportCooldown = 0.5f;

    bool isOnCooldown = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!PortalManager.Instance.BothPortalsActive() || LinkedPortal == null || isOnCooldown)
        {
            return;
        }

        if (other.CompareTag("Player") || other.transform.root.CompareTag("Player"))
        {
            TeleportPlayer(other.transform.root.gameObject);
            return;
        }

        EnemyBase enemy = other.GetComponentInParent<EnemyBase>();
        if (enemy != null)
        {
            TeleportObject(enemy.gameObject);
            return;
        }

        Damage projectile = other.GetComponent<Damage>();
        if (projectile != null)
        {
            TeleportProjectile(projectile);
            return;
        }
    }

    void TeleportPlayer(GameObject player)
    {
        PlayerController pc = player.GetComponent<PlayerController>();
        if(pc == null)
        {
            return;
        }

        Vector3 exitPos = LinkedPortal.transform.position + LinkedPortal.transform.forward * 1.5f;

        Vector3 inVel = pc.GetComponent<CharacterController>().velocity;
        Vector3 relativeVel = transform.InverseTransformDirection(inVel);
        Vector3 exitVel = LinkedPortal.transform.TransformDirection(relativeVel);

        Vector3 relativeDir = transform.InverseTransformDirection(player.transform.forward);
        Vector3 exitDir = LinkedPortal.transform.TransformDirection(relativeDir);
        player.transform.rotation = Quaternion.LookRotation(exitDir);

        pc.Controller.enabled = false;
        player.transform.position = exitPos;
        pc.Controller.enabled = true;
        Physics.SyncTransforms();

        LinkedPortal.StartCooldown();
    }

    void TeleportObject(GameObject obj)
    {
        Vector3 exitPos = LinkedPortal.transform.position + LinkedPortal.transform.forward * 1.5f;

        Vector3 relativeDir = transform.InverseTransformDirection(obj.transform.forward);
        Vector3 exitDir = LinkedPortal.transform.TransformDirection(relativeDir);

        obj.transform.position = exitPos;
        obj.transform.rotation = Quaternion.LookRotation(exitDir);

        LinkedPortal.StartCooldown();
    }

    void TeleportProjectile(Damage projectile)
    {
        Vector3 exitPos = LinkedPortal.transform.position + LinkedPortal.transform.forward * 1.5f;

        Vector3 relativeDir = transform.InverseTransformDirection(projectile.transform.forward);
        Vector3 exitDir = LinkedPortal.transform.TransformDirection(relativeDir);

        projectile.transform.position = exitPos;
        projectile.transform.rotation = Quaternion.LookRotation(exitDir);

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if(rb != null)
        {
            float speed = rb.linearVelocity.magnitude;
            rb.linearVelocity = exitDir * speed;
        }

        LinkedPortal.StartCooldown();
    }

    public void StartCooldown()
    {
        StartCoroutine(Cooldown());
    }

    IEnumerator Cooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(teleportCooldown);
        isOnCooldown = false;
    }
}
