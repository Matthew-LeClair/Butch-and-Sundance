using UnityEngine;

public class DualBehavior : EnemyBehavior
{

    protected override void Awake()
    {
        base.Awake();
    }
    public override void Tick()
    {
        if (ai.seePlayer)
        {
            ai.FOV = 180f;
            ai.agent.SetDestination(ai.player.transform.position);
            if (ai.ik != null)
            {
                ai.ik.isAiming = true;
                ai.ik.rightHandTarget.position = Vector3.Lerp(ai.ik.rightHandTarget.position, ai.player.position + Vector3.up * 1.2f, Time.deltaTime * 5f);
                ai.ik.leftHandTarget.position = Vector3.Lerp(ai.ik.leftHandTarget.position, ai.player.position + Vector3.up * 1.2f, Time.deltaTime * 5f);
            }
        }
        else
        {
            if(ai.ik != null)
            {
                ai.ik.isAiming = false;
            }
            ai.CheckRoam();
            ai.FOV = ai.FOVOrig;
        }
    }
}
