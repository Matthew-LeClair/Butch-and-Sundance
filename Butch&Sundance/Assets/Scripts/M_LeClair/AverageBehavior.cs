using UnityEngine;

public class AverageBehavior : EnemyBehavior
{
    public override void Tick()
    {
        if (ai.seePlayer)
        {
            ai.rotateToTarget();
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
    }
}
