using UnityEngine;
public class CameraController : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] int Sensitivity;
    [SerializeField] int LockVertMin, LockVertMax;

    [Header("Sway & Bob")]
    [SerializeField] float SwayAmount = 0.05f; // How much the Camera sways on horizontal input
    [SerializeField] float SwaySpeed = 8f; // How fast the Sway returns to center
    [SerializeField] float BobAmount = 0.05f; // How much the Camera bobs while moving
    [SerializeField] float BobSpeed = 12f; // How fast the Camera bobs
    [SerializeField] float MomentumFOVMin = 60f; // FOV at zero momentum
    [SerializeField] float MomentumFOVMax = 70f; // FOV at full momentum

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
        HandleFOV();
    }

    void LookAround()
    {
        float MouseX = Input.GetAxisRaw("Mouse X") * Sensitivity * Time.deltaTime;
        float MouseY = Input.GetAxisRaw("Mouse Y") * Sensitivity * Time.deltaTime;

        CamRotX -= MouseY;
        CamRotX = Mathf.Clamp(CamRotX, LockVertMin, LockVertMax);

        transform.localRotation = Quaternion.Euler(CamRotX, 0, 0);
        Player.transform.Rotate(Vector3.up * MouseX);
    }

    void HandleSway()
    {
        float TargetSwayX = Input.GetAxisRaw("Horizontal") * -SwayAmount;
        float TargetSwayY = Input.GetAxisRaw("Vertical") * -SwayAmount;

        bool IsMoving = Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f
            || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f;

        if (IsMoving && PC.Controller.isGrounded)
        {
            float MomentumPercent = PC.CurrMomentum / 50f;
            BobTimer += Time.deltaTime * (BobSpeed * (1f + MomentumPercent * 0.5f));
            CamBasePos.y += Mathf.Sin(BobTimer) * BobAmount * (1f + MomentumPercent);
        }
        else
        { BobTimer = 0; }

        Vector3 TargetPos = CamBasePos + new Vector3(TargetSwayY, TargetSwayX, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, TargetPos, SwaySpeed * Time.deltaTime);
    }

    void HandleFOV()
    {
        if (Cam == null) return;
        float MomentumPercent = GameManager.Instance.Player.GetComponent<PlayerController>() != null ? GameManager.Instance.Player.GetComponent<PlayerController>().CurrMomentum / 50f : 0f;
        float TargetFOV = Mathf.Lerp(MomentumFOVMin, MomentumFOVMax, MomentumPercent);
        Cam.fieldOfView = Mathf.Lerp(Cam.fieldOfView, TargetFOV, 5f * Time.deltaTime);
    }
}