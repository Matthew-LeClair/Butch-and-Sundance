using UnityEngine;

public class AverageBehavior : EnemyBehavior
{
    public override void Tick()
    {
        if (ai.seePlayer)
        {
            ai.rotateToTarget();
            ai.playerDir = ai.player.position - ai.transform.position;
            ai.DistanceToPlayer = ai.playerDir.magnitude;
            bool rightInRange = ai.Weapon_R != null && ai.DistanceToPlayer < ai.Weapon_R.ShootDistance;
            foreach (AimControl aim in ai.aimControllers)
            {
                aim.SetAiming(true);
                aim.AimAtTarget(ai.player.position);
            }
            if (rightInRange)
            {
                ai.Weapon_R.Shoot("Enemy");
            }
            else
            {
                ai.movement.Move(ai.playerDir, ai.MoveSpeed, ai.CanFly);
            }
        }
        else
        {
            foreach(AimControl aim in ai.aimControllers)
            {
                aim.ResetAim();
            }
        }
    }
}
