using System.Collections;
using UnityEngine;

public class TankBehavior : EnemyBehavior
{
    bool isDropping = false;
    bool isSlamming = false;
    float dropHeight = 5;
    float dropCooldown = 2;
    bool canDrop = true;
    [SerializeField] SphereCollider slamHitBox;
    private int slamDamage;

    private void Start()
    {
        slamDamage = slamHitBox.GetComponent<Damage>().DamageAmount;
    }
    public override void Tick()
    {
        if (isDropping || isSlamming)
        {
            return;
        }
        if (ai.seePlayer)
        {
            if (ai.DistanceToPlayer > 5 && !isDropping && canDrop)
            {
                StartCoroutine(DropAttack());
            }
            else if(!isDropping)
            {
                ai.rotateToTarget();
                ai.agent.SetDestination(ai.player.transform.position);

                if(ai.DistanceToPlayer <= 3)
                {
                    StartCoroutine(GroundSlam());
                }
            }
        }
    }
    private IEnumerator DropAttack()
    {
        isDropping = true;
        canDrop = false;

        ai.agent.isStopped = true;
        ai.agent.ResetPath();
        ai.agent.enabled = false;

        Vector3 startPos = ai.transform.position;
        Vector3 targetPos = ai.player.position;

        Vector3 airPos = targetPos + Vector3.up * dropHeight;

        float t = 0f;
        float duration = 0.25f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float lerpT = t / duration;

            ai.transform.position = Vector3.Lerp(startPos, airPos, lerpT);
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        startPos = ai.transform.position;
        targetPos = ai.player.position;

        t = 0f;
        duration = 0.25f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float lerpT = t / duration;

            ai.transform.position = Vector3.Lerp(startPos, targetPos, lerpT);


            yield return null;
        }

        Slam(2);

        yield return new WaitForSeconds(1.5f);

        ai.agent.enabled = true;
        ai.agent.isStopped = false;

        isDropping = false;

        yield return new WaitForSeconds(dropCooldown);
        canDrop = true;
    }

    private IEnumerator GroundSlam()
    {
        isSlamming = true;
        yield return new WaitForSeconds(0.2f);

        Slam(1);

        yield return new WaitForSeconds(1.5f);
        isSlamming = false;
    }
    private void Slam(int mult)
    {
        Collider[] hits = Physics.OverlapSphere(ai.transform.position, 4f);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                I_Damage dmg = hit.GetComponent<I_Damage>();
                if (dmg != null)
                {
                    dmg.TakeDamage(slamDamage * mult, false);
                }
            }
        }
    }
}