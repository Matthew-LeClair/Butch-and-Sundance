using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(CharacterController))]
public class LadderController : MonoBehaviour
{
    [SerializeField] public float climbSpeed;
    [SerializeField] public float jumpUpForce;
    [SerializeField] public float jumpBackForce;
    [SerializeField] public float snapSpeed;
    [SerializeField] public float exitCooldown;

    public bool IsOnLadder { get; set; }

    private Ladder currentLadder;
    private CharacterController controller;
    private float cooldownTimer;

    public event System.Action<float> OnLadderJump;
    public event System.Action OnLadderExit;
    public event System.Action OnLadderEnter;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;

        if (IsOnLadder)
        {
            HandleClimbing();
            HandleJumpOff();
        }
        else TryGrabLadder();
    }

    private void TryGrabLadder()
    {
        if (currentLadder == null) return;
        if (cooldownTimer > 0f) return;

        if (Input.GetAxisRaw("Vertical") > 0.1f) EnterLadder();
    }

    private void EnterLadder()
    {
        IsOnLadder = true;
        OnLadderEnter?.Invoke();
    }

    private void ExitLadder()
    {
        IsOnLadder = false;
        cooldownTimer = exitCooldown;
        OnLadderExit?.Invoke();
    }

    private void HandleClimbing()
    {
        if (currentLadder == null) { ExitLadder(); return; }

        float vertical = Input.GetAxisRaw("Vertical");
        controller.Move(Vector3.up * vertical * climbSpeed * Time.deltaTime);

        SnapToLadderCenter();

        if (controller.isGrounded && vertical < -0.1f) ExitLadder();
    }

    private void SnapToLadderCenter()
    {
        Vector3 target = transform.position;
        target.x = currentLadder.transform.position.x;
        target.z = currentLadder.transform.position.z;
        transform.position = Vector3.Lerp(transform.position, target, snapSpeed * Time.deltaTime);
    }

    private void HandleJumpOff()
    {
        if (!Input.GetButton("Jump")) return;

        Vector3 pushBack = -transform.forward * jumpBackForce;
        controller.Move(pushBack * Time.deltaTime);

        ExitLadder();
        OnLadderJump?.Invoke(jumpUpForce);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (currentLadder == null && other.TryGetComponent(out Ladder ladder)) currentLadder = ladder;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Ladder ladder) && ladder == currentLadder)
        {
            currentLadder = null;
            if (IsOnLadder) ExitLadder();
        }
    }
}
