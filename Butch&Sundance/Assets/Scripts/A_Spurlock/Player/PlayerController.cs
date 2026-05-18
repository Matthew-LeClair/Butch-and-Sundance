using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class PlayerController : MonoBehaviour, I_Damage
{
    [Header("Config")]
    [SerializeField] public CharacterController Controller;

    float Gravity = 35;

    Vector3 MoveDir;
    Vector3 PlayerVel;

    [Header("Health & Damage")]
    [SerializeField] public float Health;
    public float HealthMax;

    [SerializeField] public float Shield;
    public float ShieldMax;

    [SerializeField] public float AlienEnergy;
    public float AlienEnergyMax;

    [Header("Weapon")]
    public PlayerGun pGun;

    [Header("Gear")]


    [Header("Movement")]
    [SerializeField] public float SpeedBase;
    public float Speed;

    bool IsMoving;
    public float JumpSpeedBase;
    float JumpSpeed;
    int JumpCount;

    [Header("Wall Run")]
    public LayerMask WhatIsWall;
    public LayerMask WhatIsGround;
    public float WallRunForce;
    public float MaxWallRunTime;
    public float UpJumpForce;
    public float OutJumpForce;
    private float WallRunTimer;
    public bool IsWallRunning;

    private bool IsExitWallRun;
    public float ExitTime;
    private float ExitTimer;

    public float WallCheckDistance;
    public float MinJumpHeight;
    private RaycastHit LeftWall;
    private bool IsLeftWall;
    private RaycastHit RightWall;
    public bool IsRightWall;
    public float GravityCounterForce;

    [Header("Hook Shot")]
    [SerializeField] GameObject Cam;
    public Transform CamTransform;

    public LineRenderer GrappleLine;
    [SerializeField] int RopePoints = 20; // Number of points in the rope
    [SerializeField] float RopeSag = 2f; // How much the rope sags

    public float MaxGrappleDistance;
    public float GrappleDelayTime;
    private Vector3 GrapplePoint;

    public float GrapplingCooldown;
    private float GrapplingCooldownTimer;

    private bool IsGrapple;

    public float GrappleForceMultiplier = 0.1f;


    [Header("Momemtum")]
    [SerializeField] public float BaseMomentumBuildRate;
    [SerializeField] float MomentumDecayRate = 0.4f;

    public float MomentumBuildRate;
    public float CurrMomentum = 0;
    float MaxMomentum = 50;

    Vector3 MomentumVelocity = Vector3.zero;


    [Header("Parkour")]

    [Header("Tutorial")]
    public bool Aimed;
    public bool Shot;
    public bool Reloaded;
    public bool Moved;
    public bool Jumped;


    //===[Basic]===\\

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        GrappleLine = Cam.GetComponent<LineRenderer>();

        MomentumBuildRate = BaseMomentumBuildRate;
        Health = HealthMax;
        Shield = ShieldMax;

        UpdatePlayerUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (GrapplingCooldownTimer > 0)
        { GrapplingCooldownTimer -= Time.deltaTime; }

        if (IsGrapple)
        {
            UpdateRopePoints(new Vector3(CamTransform.position.x, CamTransform.position.y - .5f, CamTransform.position.z), GrapplePoint);

            MomentumVelocity = Vector3.MoveTowards(        // Bleed off Momentum Velocity during Grapple
                MomentumVelocity, Vector3.zero,
                Speed * 2f * Time.deltaTime);

            if (Vector3.Distance(transform.position, GrapplePoint) < 2f)
            {
                GrapplingCooldownTimer = GrapplingCooldown;
                GrappleLine.enabled = false;
                IsGrapple = false;
                MomentumVelocity = Vector3.zero;
            }
        }
        else
        {
            Debug.DrawRay(CamTransform.position, CamTransform.forward * pGun.ShootDistance, Color.red);
            IsMoving = Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f
                || Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f;
            Movement();
        }

        // Called In Update to Prevent Errors with Grapple
        if (Input.GetKeyDown(KeyCode.Q)) // Quit Grapple Input for just in case
        {
            GrapplingCooldownTimer = GrapplingCooldown;
            GrappleLine.enabled = false;
            IsGrapple = false;
            MomentumVelocity = Vector3.zero;
        }

        Controller.Move(MomentumVelocity * Time.deltaTime);
        Controller.Move(PlayerVel * Time.deltaTime);
        PlayerVel.y -= Gravity * Time.deltaTime;
    }

    void HandleInput() 
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        { pGun.Reload(); Reloaded = true; }

        if (Input.GetButtonDown("Fire2")) { pGun.Aim(); Aimed = true; }
        if (Input.GetButtonUp("Fire2")) { pGun.Aim(); }

        if (Input.GetButton("Fire1"))
        { pGun.Shoot(); Shot = true; }

        if (Input.GetKeyDown(KeyCode.Tab)) { StartGrapple(); }
    }


    //===[Movement]===\\

    void Movement()
    {
        HandleInput();

        if (Controller.isGrounded && !IsGrapple)
        {
            PlayerVel = new Vector3(0, 0, 0); // Reset all axes on landing
            JumpCount = 0; // Reset Jump Count on Landing
        }

        Vector3 InputDir =
            Input.GetAxis("Horizontal") * transform.right
            + Input.GetAxis("Vertical") * transform.forward;

        Jump();
        HandleWallRun();
        
        if (!IsWallRunning)
        {
            if (InputDir.magnitude > 0.1f) // If there is Input...
            {
                Moved = true;
                MoveDir = InputDir; // Only overwrite MoveDir when actively moving
                MomentumVelocity = MoveDir.normalized * (Speed + CurrMomentum * 0.1f); // Snap directly to Input Direction
            }
            else // If there is No Input...
            {
                MomentumVelocity = Vector3.MoveTowards(
                    MomentumVelocity,
                    Vector3.zero,
                    Speed * 10f * Time.deltaTime); // Quickly bleed to zero on no input
            }
        }

        HandleMomentum(InputDir); // Pass Input Direction to Momentum System
    }

    void Jump()
    {
        if (Input.GetButtonDown("Jump") && JumpCount < 2) // Allow Double Jump
        {
            if (!IsWallRunning)
            {
                Jumped = true;
                JumpCount++;

                float MomentumBoost = CurrMomentum / MaxMomentum; // Momentum as a Percentage
                PlayerVel.y = JumpSpeedBase * (1f + MomentumBoost * 0.4f); // Scale Jump Height with Momentum

                MomentumVelocity = new Vector3(
                    MomentumVelocity.x, 0, MomentumVelocity.z) // Strip Y from Momentum Velocity
                    * (1f + MomentumBoost * 0.3f); // Boost Horizontal on Jump
            } else { WallJump(); }
        }
    }



    //===[Momentum]===\\
    void HandleMomentum(Vector3 InputDir)
    {
        if (InputDir.magnitude > 0.1f) // If the Player is Moving...
        {
            CurrMomentum = Mathf.MoveTowards(
                CurrMomentum, MaxMomentum,
                MomentumBuildRate * Time.deltaTime); // Build Momentum toward Max
        }
        else // If the Player is NOT Moving...
        {
            CurrMomentum = Mathf.MoveTowards(
                CurrMomentum, 0,
                (MomentumBuildRate * MomentumDecayRate) * Time.deltaTime); // Decay Momentum slower than Build
        }

        HandleStats();
    }

    void HandleStats()
    {
        float MomentumPercent = CurrMomentum / MaxMomentum; // Momentum as a Percentage
        Speed = Mathf.Lerp(SpeedBase, SpeedBase * 2f, MomentumPercent); // Scale Speed between Base and 2x
        JumpSpeed = Mathf.Lerp(JumpSpeedBase, JumpSpeedBase * 1.5f, MomentumPercent); // Scale Jump between Base and 1.5x
    }



    //===[Damage]===\\
    public void TakeDamage(int Amount, bool AlienTech) 
    {
        Shield -= Amount;
        if (Shield <= 0)
        {
            if (CurrMomentum > 0)
            {
                float DamageRatio = Mathf.Clamp01((float)Amount / Health);
                CurrMomentum *= (1f - DamageRatio);
            }

            Health -= Amount; // Subtract Health by Amount
            if (Health <= 0) // If Health is Less Than or Equal To 0...
            {
                GameManager.Instance.YouLose();

                Death();
            } // Destroy the Object
            StartCoroutine(FlashDamageScreen());
        } else { FlashShieldScreen(); }


    }

    public void Death()
    {
        Debug.Log(Health);

        #if UNITY_EDITOR // If in Unity Editor...
        UnityEditor.EditorApplication.isPlaying = false; // Quit Debug
        #else // If NOT in Unity Editor...      Quit Game.
                        Application.Quit(); 
        #endif
    }


    //===[UI]===\\
    public void UpdatePlayerUI()
    {
        GameManager.Instance.PlayerHP_Bar.fillAmount = (float)Health / HealthMax;

        GameManager.Instance.PlayerShieldHP_Bar.fillAmount = (float)Shield / ShieldMax;
    }

    IEnumerator FlashDamageScreen()
    {
        GameManager.Instance.PlayerDamage_Screen.SetActive(true); // Activate the damage screen effect to indicate that the player has taken damage

        yield return new WaitForSeconds(0.5f); // Wait for a short duration (0.5 seconds) before deactivating the damage screen effect

        GameManager.Instance.PlayerDamage_Screen.SetActive(false); // Deactivate the damage screen effect after the wait time has elapsed
    }

    IEnumerator FlashShieldScreen()
    {
        GameManager.Instance.PlayerShield_Screen.SetActive(true); // Activate the damage screen effect to indicate that the player has taken damage

        yield return new WaitForSeconds(0.5f); // Wait for a short duration (0.5 seconds) before deactivating the damage screen effect

        GameManager.Instance.PlayerShield_Screen.SetActive(false); // Deactivate the damage screen effect after the wait time has elapsed
    }


    //===[Wall Run]===\\

    void CheckForWall()
    {
        IsRightWall = Physics.Raycast(transform.position, transform.right, out RightWall, WallCheckDistance, WhatIsWall); // Check for Wall on Right
        IsLeftWall = Physics.Raycast(transform.position, -transform.right, out LeftWall, WallCheckDistance, WhatIsWall); // Check for Wall on Left
    }

    bool AboveGround()
    { return !Physics.Raycast(transform.position, Vector3.down, MinJumpHeight, WhatIsGround); } // Check if Above Ground

    void HandleWallRun()
    {
        if (!IsExitWallRun)
        {        // Step 1 -- Wall Run Timer
            if (WallRunTimer >= MaxWallRunTime) { IsWallRunning = false; WallRunTimer = 0; }
            {
                WallRunTimer += Time.deltaTime;

                // Step 2 -- Wall Check
                CheckForWall();

                // Step 3 -- Wall Run Check
                if ((IsRightWall || IsLeftWall) && MoveDir.magnitude > 0.1f && AboveGround() && !Controller.isGrounded && !IsExitWallRun)
                { IsWallRunning = true; }
                else { IsWallRunning = false; }

                // Step 4 -- Wall Run
                if (IsWallRunning) { WallRunMovement(); }
            }
        }
        else 
        {
            if (ExitTimer >= ExitTime)
            {
                IsExitWallRun = false;
                ExitTimer = 0;
            }
            else { ExitTimer += Time.deltaTime; }
                
        }

    }

    void WallRunMovement()
    {
        PlayerVel.y -= Gravity * Time.deltaTime; // Apply Gravity while Wall Running
        PlayerVel.y += GravityCounterForce * Time.deltaTime; // Counter Gravity to slow the fall

        Vector3 Normal = IsRightWall ? RightWall.normal : LeftWall.normal; // Get Wall Normal
        Vector3 WallForward = Vector3.Cross(Normal, transform.up); // Calculate Wall Forward Direction

        if ((transform.forward - WallForward).magnitude > (transform.forward - -WallForward).magnitude)
        { WallForward = -WallForward; }

        if (Vector3.Dot(WallForward, transform.forward) < 0) // If Wall Forward is behind the Player...
        { WallForward = -WallForward; } // Flip Wall Forward Direction

        float VerticalDot = Vector3.Dot(Camera.main.transform.forward, Vector3.up); // How much Camera is pointing Up or Down (-1 to 1)
        PlayerVel.y = VerticalDot * Speed;
    }

    void WallJump()
    {
        IsWallRunning = false; // Stop Wall Running
        WallRunTimer = 0; // Reset Wall Run
        IsExitWallRun = true; // Exit Wall Running

        Vector3 Normal = IsRightWall ? RightWall.normal : LeftWall.normal; // Get Wall Normal
        Vector3 ForceToApply = transform.up * UpJumpForce + Normal * OutJumpForce; // Calculate Jump Force

        PlayerVel.y = 0f; // Reset Y Velocity before Jump
        PlayerVel = ForceToApply; // Apply Jump Force to Player Velocity
        MomentumVelocity = Vector3.zero;
    }


    //===[Hook Shot Parkour]===\\
    void StartGrapple()
    {
        Debug.Log("StartGrapple Called");
        if (GrapplingCooldownTimer > 0)
        {
            Debug.Log("On Cooldown: " + GrapplingCooldownTimer + " / " + 0);
            return;
        }
        RaycastHit GrappleHit;
        if (Physics.Raycast(CamTransform.position, CamTransform.forward, out GrappleHit, MaxGrappleDistance))
        {
            if (GrappleHit.collider.CompareTag("Grapple"))
            {
                IsGrapple = true;
                Debug.Log("Grapple Started");
                GrapplePoint = GrappleHit.point;
                Debug.Log("Grapple Hit: " + GrappleHit.collider.name + " at " + GrapplePoint);
                Invoke(nameof(Grapple), GrappleDelayTime);
                GrappleLine.enabled = true;
                GrappleLine.positionCount = RopePoints;
                StartCoroutine(ExtendGrappleLine(GrapplePoint)); // Extend to Grapple Point
            }
            else { Debug.Log("Hit " + GrappleHit.collider.name + " but not tagged Grapple"); }
        }
        else { Debug.Log("Grapple Missed — nothing hit"); }
    }

    IEnumerator ExtendGrappleLine(Vector3 Target)
    {
        float Timer = 0f;
        GrappleLine.positionCount = RopePoints; // Set number of rope points

        while (Timer < GrappleDelayTime)
        {
            Timer += Time.deltaTime;
            float Percent = Timer / GrappleDelayTime; // 0 to 1 over Delay Time
            UpdateRopePoints(CamTransform.position, Vector3.Lerp(CamTransform.position, Target, Percent)); // Extend rope
            yield return null;
        }
    }

    void UpdateRopePoints(Vector3 Start, Vector3 End)
    {
        for (int i = 0; i < RopePoints; i++)
        {
            float t = i / (float)(RopePoints - 1); // 0 to 1 along rope
            Vector3 Point = Vector3.Lerp(Start, End, t); // Linear point along rope
            float Sag = Mathf.Sin(t * Mathf.PI) * RopeSag * (1f - t * 0.5f); // Sag peaks in middle
            Point.y -= Sag; // Apply sag downward
            GrappleLine.SetPosition(i, Point); // Set rope point
        }
    }

    void Grapple()
    {
        Debug.Log("Grapple Function Called");

        Vector3 GrappleDir = (GrapplePoint - transform.position).normalized;
        float GrappleDist = Vector3.Distance(transform.position, GrapplePoint);

        float LaunchForce = GrappleDist * GrappleForceMultiplier; // Force purely based on distance
        MomentumVelocity = GrappleDir * LaunchForce; // Apply horizontal launch
        PlayerVel.y = GrappleDir.y * LaunchForce; // Apply vertical launch

        Debug.Log("Launch Force: " + LaunchForce); // Debug — check the actual force value
    }
}