using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class AimControl : MonoBehaviour
{
    [SerializeField] Transform WeaponArm;
    [SerializeField] float aimSpeed;
    [SerializeField] float verticalOffset;

    Quaternion origRotation;


    bool isAiming;
    float maxAngle = 90f;

    private void Start()
    {
        origRotation = WeaponArm.rotation;
    }
    public void SetAiming(bool value)
    {
        isAiming = value;
    }

    public void AimAtTarget(Vector3 targetPosition)
    {
        if (!isAiming)
        {
            return;
        }

        Vector3 target = targetPosition + Vector3.up * verticalOffset;
        Vector3 dir = target - WeaponArm.position;

        Vector3 forward = transform.forward;
        forward.y = 0f;
        float angle = Vector3.Angle(forward, dir);

        if(angle > maxAngle)
        {
            return;
        }

        Quaternion rot = Quaternion.LookRotation(dir);
        WeaponArm.rotation = Quaternion.Lerp(WeaponArm.rotation, rot, Time.deltaTime * aimSpeed);
    }

    public void ResetAim()
    {
        isAiming = false;
        WeaponArm.rotation = Quaternion.Lerp(WeaponArm.rotation, origRotation, Time.deltaTime * aimSpeed);
    }
}
