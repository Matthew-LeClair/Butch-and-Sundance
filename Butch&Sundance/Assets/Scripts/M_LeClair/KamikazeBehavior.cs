// using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class KamikazeBehavior : EnemyBehavior
{
    [SerializeField] float explosionRange = 2f;

    public override void Tick()
    {
        if (ai.seePlayer)
        {
            ai.FOV = 180f;
            ai.MoveSpeed = 15;
            ai.agent.SetDestination(ai.player.transform.position);
            ai.rotateToTarget();

            if (ai.DistanceToPlayer <= explosionRange)
            {
                ExplosionScript explosion = ai.GetComponent<ExplosionScript>();
                if (explosion != null)
                {
                    explosion.Explode();
                }
                Destroy(gameObject);
            }
        }
        else
        {
            ai.CheckRoam();
            ai.FOV = ai.FOVOrig;
        }
    }
}
