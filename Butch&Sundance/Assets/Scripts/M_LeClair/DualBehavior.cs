using UnityEngine;

public class DualBehavior : EnemyBehavior
{
    public override void Tick()
    {
        if (ai.seePlayer)
        {
            ai.rotateToTarget();
            bool rightInRange = ai.Weapon_R != null && ai.DistanceToPlayer < ai.Weapon_R.ShootDistance;
            bool leftInRange = ai.Weapon_L != null && ai.DistanceToPlayer < ai.Weapon_L.ShootDistance;
            foreach (AimControl aim in ai.aimControllers)
            {
                aim.SetAiming(true);
                aim.AimAtTarget(ai.player.position);
            }
            if (rightInRange)
            {
                ai.Weapon_R.Shoot("Enemy");
            }
            if (leftInRange)
            {
                ai.Weapon_L.Shoot("Enemy");
            }
            if (!rightInRange && !leftInRange)
            {
                ai.movement.Move(ai.playerDir, ai.MoveSpeed, ai.CanFly);
            }
        }
    }
}
