using UnityEngine;

public class CameraController : MonoBehaviour
{

    [SerializeField] int Sensitivity;
    [SerializeField] int LockVertMin, LockVertMax;
    [SerializeField] Transform Player;

    float CamRotX;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        LookAround(); // A Function for just in case I want to do crazy stuff with the camera later
    }

    void LookAround()
    {
        float MouseX = Input.GetAxisRaw("Mouse X") * Sensitivity // MouseX = Raw Mouse X Axis multiplied by Semsitivity
            * Time.deltaTime; // Make MouseX Time Relative

        float MouseY = Input.GetAxisRaw("Mouse Y") * Sensitivity // MouseY = Raw Mouse Y Axis multiplied by Semsitivity
            * Time.deltaTime; // Make MouseY Time Relative

        CamRotX -= MouseY; // CamRotX = Raw MouseY for vertical rotation

        CamRotX = Mathf.Clamp(CamRotX, LockVertMin, LockVertMax); // Clamp the Raw Mouse Y value by the Look Vert Min and Max

        transform.localRotation = Quaternion.Euler(CamRotX, 0, 0); // Make Local Transform of Camera Clamped CamRotX

        Player.transform.Rotate(Vector3.up * MouseX); // Rotate Player Relative to MouseX
    }
}
