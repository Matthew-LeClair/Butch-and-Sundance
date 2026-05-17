using UnityEngine;
public class CameraController : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] int Sensitivity;
    [SerializeField] int LockVertMin, LockVertMax;
    [SerializeField] LayerMask CamIgnoreLayer;

    [Header("Sway & Bob")]
    [SerializeField] float SwayAmount = 0.05f; // How much the Camera sways on horizontal input
    [SerializeField] float SwaySpeed = 8f; // How fast the Sway returns to center
    [SerializeField] float BobAmount = 0.05f; // How much the Camera bobs while moving
    [SerializeField] float BobSpeed = 12f; // How fast the Camera bobs
    [SerializeField] float MomentumFOVMin = 60f; // FOV at zero momentum
    [SerializeField] float MomentumFOVMax = 70f; // FOV at full momentum

    [Header("Wall Run")]
    [SerializeField] float WallRunFOV = 75f; // FOV during Wall Run
    [SerializeField] float WallRunTilt = 15f; // Camera Tilt during Wall Run
    [SerializeField] float WallRunTiltSpeed = 8f; // How fast the Tilt applies and returns
    float CurrentTilt; // Current Camera Tilt

    [Header("Do NOT Touch!")]
    [SerializeField] Transform Player;
    [SerializeField] Camera Cam;
    [SerializeField] PlayerController PC; // Cached Player Controller

    float CamRotX;
    float BobTimer;
    Vector3 CamBasePos; // Base Position of the Camera

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        CamBasePos = transform.localPosition; // Store Base Position on Start
    }

    void Update()
    {
        LookAround();
        HandleSway();
        HandleWallRunEffects(); // Add this
    }

    void LookAround()
    {
        float MouseX = Input.GetAxisRaw("Mouse X") * Sensitivity * Time.deltaTime;
        float MouseY = Input.GetAxisRaw("Mouse Y") * Sensitivity * Time.deltaTime;

        CamRotX -= MouseY;
        CamRotX = Mathf.Clamp(CamRotX, LockVertMin, LockVertMax);

        transform.localRotation = Quaternion.Euler(CamRotX, 0, 0);
        Player.transform.Rotate(Vector3.up * MouseX);

        // Prevent Camera from clipping into walls
        RaycastHit CamHit;
        Vector3 DirToCamera = transform.position - Player.position; // Direction from Player to Camera
        if (Physics.Raycast(Player.position, DirToCamera.normalized, out CamHit, DirToCamera.magnitude, ~CamIgnoreLayer))
        { transform.position = CamHit.point; }
    }

    void HandleSway()
    {
        float TargetSwayX = Input.GetAxisRaw("Horizontal") * -SwayAmount;
        float TargetSwayY = Input.GetAxisRaw("Vertical") * -SwayAmount;

        bool IsMoving = Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f
            || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f;

        if (IsMoving && PC.Controller.isGrounded) // Only Bob while Moving on the Ground
        {
            float MomentumPercent = PC.CurrMomentum / 50f;
            BobTimer += Time.deltaTime * (BobSpeed * (1f + MomentumPercent * 0.5f));
            CamBasePos.y += Mathf.Sin(BobTimer) * BobAmount * (1f + MomentumPercent);
        }
        else
        {
            BobTimer = 0;
            CamBasePos.y = Mathf.Lerp(CamBasePos.y, transform.localPosition.y, SwaySpeed * Time.deltaTime); // Reset Base Y when not bobbing
        }

        Vector3 TargetPos = CamBasePos + new Vector3(TargetSwayY, TargetSwayX, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, TargetPos, SwaySpeed * Time.deltaTime);
    }

    void HandleWallRunEffects()
    {
        float TargetFOV;
        float TargetTilt;

        if (PC.IsWallRunning) // If the Player is Wall Running...
        {
            TargetFOV = WallRunFOV; // Target Wall Run FOV
            TargetTilt = PC.IsRightWall ? WallRunTilt : -WallRunTilt; // Tilt toward the Wall
        }
        else // If not Wall Running...
        {
            TargetFOV = Mathf.Lerp(MomentumFOVMin, MomentumFOVMax, // Return to Momentum FOV
                PC.CurrMomentum / 50f);
            TargetTilt = 0f; // Return Tilt to Zero
        }

        Cam.fieldOfView = Mathf.Lerp(Cam.fieldOfView, TargetFOV, WallRunTiltSpeed * Time.deltaTime); // Smoothly Blend FOV
        CurrentTilt = Mathf.Lerp(CurrentTilt, TargetTilt, WallRunTiltSpeed * Time.deltaTime); // Smoothly Blend Tilt
        transform.localRotation = Quaternion.Euler(CamRotX, 0, CurrentTilt); // Apply Tilt to Camera Rotation
    }
}