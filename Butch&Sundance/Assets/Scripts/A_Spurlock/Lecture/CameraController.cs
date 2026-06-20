using UnityEngine;
public class CameraController : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] int Sensitivity;
    [SerializeField] int LockVertMin, LockVertMax;
    [SerializeField] LayerMask CamIgnoreLayer;

    [Header("Wall Run")]
    [SerializeField] float MomentumFOVMin = 60f; // FOV at zero momentum
    [SerializeField] float MomentumFOVMax = 70f; // FOV at full momentum
    [SerializeField] float WallRunFOV = 75f; // FOV during Wall Run
    [SerializeField] float WallRunTilt = 15f; // Camera Tilt during Wall Run
    [SerializeField] float WallRunTiltSpeed = 8f; // How fast the Tilt applies and returns
    float CurrentTilt; // Current Camera Tilt

    [Header("Do NOT Touch!")]
    [SerializeField] Transform Player;
    [SerializeField] Camera Cam;
    [SerializeField] PlayerController PC; // Cached Player Controller
    float CamRotX;

    [Header("Recoil")]
    [SerializeField] float recoilAmount = 2f;
    [SerializeField] float recoilReturnSpeed = 8f;
    float currentRecoil = 0f;
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        LookAround();
        HandleWallRunEffects(); // Add this
    }

    void LookAround()
    {
        float MouseX = Input.GetAxisRaw("Mouse X") * Sensitivity * Time.deltaTime;
        float MouseY = Input.GetAxisRaw("Mouse Y") * Sensitivity * Time.deltaTime;

        CamRotX -= MouseY;
        CamRotX -= currentRecoil * Time.deltaTime * recoilReturnSpeed;
        currentRecoil = Mathf.Lerp(currentRecoil, 0f, Time.deltaTime * recoilReturnSpeed);
        CamRotX = Mathf.Clamp(CamRotX, LockVertMin, LockVertMax);

        Player.transform.Rotate(Vector3.up * MouseX);
    }

    void HandleWallRunEffects()
    {
        float TargetFOV;
        float TargetTilt;

        if (PC.IsWallRunning) // If the Player is Wall Running...
        {
            TargetFOV = WallRunFOV;
            TargetTilt = PC.IsRightWall ? WallRunTilt : -WallRunTilt;
        }
        else if (PC.pGun.IsAiming && !PC.IsGrapple) // If Aiming and not Grappling...
        {
            TargetFOV = PC.AimFOV; // Zoom in when Aiming
            TargetTilt = 0f;
        }
        else
        {
            TargetFOV = Mathf.Lerp(MomentumFOVMin, MomentumFOVMax, PC.CurrMomentum / 50f);
            TargetTilt = 0f;
        }

        Cam.fieldOfView = Mathf.Lerp(Cam.fieldOfView, TargetFOV, WallRunTiltSpeed * Time.deltaTime);
        CurrentTilt = Mathf.Lerp(CurrentTilt, TargetTilt, WallRunTiltSpeed * Time.deltaTime);
        transform.localRotation = Quaternion.Euler(CamRotX - currentRecoil, 0, CurrentTilt);
    }

    public void AddRecoil()
    {
        currentRecoil += recoilAmount;
    }
}