using UnityEngine;

public class EnemyIK : MonoBehaviour
{
    Animator anim;

    public Transform rightHandTarget;
    public Transform leftHandTarget;

    [Range(0, 1)] public float rightHandWeight = 1f;
    [Range(0, 1)] public float leftHandWeight = 1f;

    public bool isAiming;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (anim == null) return;

        if (isAiming && rightHandTarget != null)
        {
            anim.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandWeight);
            anim.SetIKRotationWeight(AvatarIKGoal.RightHand, rightHandWeight);
            anim.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
            anim.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
        }
        else
        {
            anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 0f);
            anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 0f);
        }

        if (isAiming && leftHandTarget != null)
            {
            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandWeight);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftHandWeight);
            anim.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
            anim.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
        }
        else
        {
            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0f);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0f);
        }
    }
}
