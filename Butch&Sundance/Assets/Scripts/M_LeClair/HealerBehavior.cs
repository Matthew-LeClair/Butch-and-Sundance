using UnityEngine;
using System.Collections;

public class HealerBehavior : EnemyBehavior
{
    [SerializeField] float healAmount = 20f;
    [SerializeField] float healInterval = 5f;
    [SerializeField] float healRange = 10f;
    [SerializeField] float retreatDistance = 8f;

    float healTimer;

    public override void Tick()
    {
        healTimer += Time.deltaTime;

        if (ai.seePlayer)
        {
            ai.FOV = 180f;

            if (ai.DistanceToPlayer < retreatDistance)
            {
                Vector3 retreatDir = (ai.transform.position - ai.player.position).normalized;
                Vector3 retreatPos = ai.transform.position + retreatDir * retreatDistance;
                ai.agent.SetDestination(retreatPos);
            }
        }
        else
        {
            ai.CheckRoam();
            ai.FOV = ai.FOVOrig;
        }

        if (healTimer >= healInterval)
        {
            healTimer = 0f;
            HealNearestAlly();
        }
    }

    void HealNearestAlly()
    {
        EnemyBase[] enemies = FindObjectsByType<EnemyBase>();
        EnemyBase target = null;
        float minDist = Mathf.Infinity;

        foreach (EnemyBase e in enemies)
        {
            if (e.gameObject == ai.gameObject) continue;

            if (e.CurrHealth >= e.MaxHealth) continue;

            float dist = Vector3.Distance(ai.transform.position, e.transform.position);
            if (dist < healRange && dist < minDist)
            {
                minDist = dist;
                target = e;
            }
        }

        if (target != null)
        {
            target.CurrHealth = Mathf.Min(target.CurrHealth + healAmount, target.MaxHealth);
            ai.StartCoroutine(HealFlash(target));
        }
    }

    IEnumerator HealFlash(EnemyBase target)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers) { r.material.color = Color.green; }
        yield return new WaitForSeconds(0.3f);
        foreach (Renderer r in renderers) { r.material.color = target.OriginalColor; }
    }
}
