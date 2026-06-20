using System.Collections;
using UnityEngine;

public class KamikazeBehavior : EnemyBehavior
{
    [SerializeField] float explosionRange = 2f;
    [SerializeField] float windupTime = 2f;

    bool isWindingUp = false;
    bool hasExploded = false;

    public override void Tick()
    {
        if (hasExploded) return;

        if (ai.seePlayer)
        {
            ai.FOV = 180f;

            if (!isWindingUp)
            {
                float distToPlayer = Vector3.Distance(
                    ai.transform.position, ai.player.position);

                if (distToPlayer <= explosionRange)
                {
                    ai.agent.ResetPath();
                    ai.StartCoroutine(WindupAndExplode());
                }
                else
                {
                    ai.agent.SetDestination(ai.player.position);
                    ai.agent.stoppingDistance = explosionRange;
                }
            }
        }
        else
        {
            if (!isWindingUp)
            {
                ai.CheckRoam();
                ai.FOV = ai.FOVOrig;
            }
        }

        if (ai.anim != null)
        {
            ai.anim.SetFloat("Speed", ai.agent.velocity.magnitude);
        }
    }

    IEnumerator WindupAndExplode()
    {
        isWindingUp = true;

        float elapsed = 0f;
        float flashInterval = 0.2f;
        float nextFlash = 0f;
        bool flashOn = false;

        Renderer[] renderers = ai.GetComponentsInChildren<Renderer>();

        while (elapsed < windupTime)
        {
            elapsed += Time.deltaTime;

            if (elapsed >= nextFlash)
            {
                nextFlash = elapsed + flashInterval;
                flashOn = !flashOn;

                foreach (Renderer r in renderers)
                {
                    r.material.color = flashOn ? Color.red : ai.OriginalColor;
                }
            }

            if (ai.player != null)
            {
                Vector3 dir = (ai.player.position - ai.transform.position).normalized;
                dir.y = 0f;
                if (dir != Vector3.zero)
                    ai.transform.rotation = Quaternion.LookRotation(dir);
            }

            yield return null;
        }

        foreach (Renderer r in renderers)
        {
            r.material.color = ai.OriginalColor;
        }

        hasExploded = true;
        ExplosionScript explosion = ai.GetComponent<ExplosionScript>();
        if (explosion != null)
        {
            explosion.Explode();
        }

        ai.Death();
    }
}