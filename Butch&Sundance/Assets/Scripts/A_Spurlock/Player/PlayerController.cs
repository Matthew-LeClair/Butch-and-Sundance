using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.XR.LegacyInputHelpers;

public class PlayerController : MonoBehaviour, I_Damage
{
    [Header("Config")]
    [SerializeField] public CharacterController Controller;
    float Gravity = 35;
    Vector3 MoveDir;
    Vector3 PlayerVel;

    [Header("Health & Damage")]
    [SerializeField] public float Health;
    [SerializeField] public float HealthMax;
    [SerializeField] public float Shield;
    [SerializeField] public float ShieldMax;

    [Header("Alien Energy")]
    [SerializeField] public float AlienEnergy;
    [SerializeField] public float AlienEnergyMax;
    [SerializeField] public float AlienEnergyRegenRate = 5f; // How fast Alien Energy regens per second
    [SerializeField] public float AlienEnergyRegenCap = 25f; // Max regen cap (upgradable)
    [SerializeField] public float WallRunEnergyCost = 5f; // Energy cost per second of wall running
    [SerializeField] public float GrappleEnergyCost = 10f; // Energy cost per grapple
    [SerializeField] public float AimEnergyCost = 3f; // Energy cost per second of aiming
    [SerializeField] public float AbsorbRatio = 0.3f; // How much incoming damage is absorbed as energy

    [Header("Dodge")]
    [SerializeField] public float DodgeEnergyCost = 15f; // Energy cost per dodge
    [SerializeField] public float DodgeDistance = 5f; // How far the dodge travels
    [SerializeField] public float DodgeDuration = 0.2f; // How long the dodge takes
    [SerializeField] public float DodgeCooldown = 1f; // Cooldown between dodges
    private float DodgeCooldownTimer;
    private bool IsDodging;

    [Header("Aim")]
    [SerializeField] public float AimFOV = 40f; // FOV when aiming
    [SerializeField] public float AimDamageMultiplier = 1.5f; // Damage multiplier when aiming

    [Header("Weapon")]
    [SerializeField] public PlayerGun pGun;

    [Header("Gear")]
    [SerializeField] public bool HasPhaseBoots; // Unlocks Parkour and Dodge
    [SerializeField] public bool HasEnergyConverter; // Unlocks Damage Absorption

    [Header("Movement")]
    [SerializeField] public float SpeedBase;
    [SerializeField] public float Speed;
    bool IsMoving;
    [SerializeField] public float JumpSpeedBase;
    float JumpSpeed;
    int JumpCount;

    [Header("Wall Run")]
    [SerializeField] public LayerMask WhatIsWall;
    [SerializeField] public LayerMask WhatIsGround;
    [SerializeField] public float WallRunForce;
    [SerializeField] public float MaxWallRunTime;
    [SerializeField] public float UpJumpForce;
    [SerializeField] public float OutJumpForce;
    private float WallRunTimer;
    [SerializeField] public bool IsWallRunning;
    private bool IsExitWallRun;
    [SerializeField] public float ExitTime;
    private float ExitTimer;
    [SerializeField] public float WallCheckDistance;
    [SerializeField] public float MinJumpHeight;
    private RaycastHit LeftWall;
    private bool IsLeftWall;
    private RaycastHit RightWall;
    [SerializeField] public bool IsRightWall;
    [SerializeField] public float GravityCounterForce;

    [Header("Hook Shot")]
    [SerializeField] GameObject Cam;
    [SerializeField] public Transform CamTransform;
    [SerializeField] public LineRenderer GrappleLine;
    [SerializeField] int RopePoints = 20; // Number of points in the rope
    [SerializeField] float RopeSag = 2f; // How much the rope sags
    [SerializeField] public float MaxGrappleDistance;
    [SerializeField] public float GrappleDelayTime;
    private Vector3 GrapplePoint;
    [SerializeField] public float GrapplingCooldown;
    private float GrapplingCooldownTimer;
    [SerializeField] public bool IsGrapple;
    private bool AttemptGrapple;
    [SerializeField] public float GrappleForceMultiplier = 0.1f;

    [Header("Swinging")]
    [SerializeField] public float MaxSwingDistance;
    [SerializeField] public float SwingSpeed = 10f; // How fast the player swings
    [SerializeField] public float SwingLengthMin = 5f; // Minimum rope length
    [SerializeField] public float SwingLengthMax = 25f; // Maximum rope length
    [SerializeField] public float SwingLengthChangeSpeed = 5f; // How fast rope length changes
    private float CurrentSwingLength; // Current rope length
    private bool IsSwinging; // Is the player currently swinging
    private Vector3 SwingPoint; // The point the player is swinging from

    [Header("Momentum")]
    [SerializeField] public float BaseMomentumBuildRate;
    [SerializeField] float MomentumDecayRate = 0.4f;
    [SerializeField] public float MomentumBuildRate;
    [SerializeField] public float CurrMomentum = 0;
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
        // Alien Energy Regen — only if Phase Boots equipped
        if (HasPhaseBoots && AlienEnergy < AlienEnergyRegenCap && !pGun.IsAiming)
        { AlienEnergy = Mathf.Min(AlienEnergy + AlienEnergyRegenRate * Time.deltaTime, AlienEnergyRegenCap); }

        // Dodge Cooldown
        if (DodgeCooldownTimer > 0)
        { DodgeCooldownTimer -= Time.deltaTime; }

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
                AttemptGrapple = false;
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
            if (IsGrapple) 
            {
                GrapplingCooldownTimer = GrapplingCooldown;
                GrappleLine.enabled = false;
                IsGrapple = false;
                MomentumVelocity = Vector3.zero;
                AttemptGrapple = false;
            }

            if (IsSwinging) { StopSwing(); } // Also stop swing on Q
        }

        Controller.Move(MomentumVelocity * Time.deltaTime);
        Controller.Move(PlayerVel * Time.deltaTime);
        PlayerVel.y -= Gravity * Time.deltaTime;
    }

    void HandleInput()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        { pGun.Reload(); Reloaded = true; }

        if (Input.GetButtonDown("Fire2") && (!HasPhaseBoots || AlienEnergy > 0))
        {
            pGun.Aim();
            Aimed = true;
            if (HasPhaseBoots && !IsGrapple && !IsSwinging) // Only slow time if not Grappling or Swinging
            {
                Time.timeScale = 0.5f;
                Time.fixedDeltaTime = 0.02f * Time.timeScale;
            }
        }

        if (Input.GetButtonUp("Fire2"))
        {
            if (!AttemptGrapple && !IsSwinging) { pGun.Aim(); }
            else if (AttemptGrapple) { StartSwing(); }
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }

        if (HasPhaseBoots && pGun.IsAiming && AlienEnergy > 0) // Only drain energy if boots equipped
        {
            AlienEnergy = Mathf.Max(AlienEnergy - AimEnergyCost * Time.deltaTime, 0);
            if (AlienEnergy <= 0)
            {
                pGun.IsAiming = false;
                Time.timeScale = 1f;
                Time.fixedDeltaTime = 0.02f;
            }
        }

        if (Input.GetButton("Fire1"))
        {
            if (!AttemptGrapple) { pGun.Shoot(); Shot = true; }
            else { Invoke(nameof(Grapple), GrappleDelayTime); }
        }

        if (Input.GetKeyDown(KeyCode.Tab)) { StartGrapple(); }

        if (HasPhaseBoots && Input.GetKeyDown(KeyCode.LeftShift) // Only dodge if boots equipped
            && !IsDodging && DodgeCooldownTimer <= 0 && AlienEnergy >= DodgeEnergyCost)
        { StartCoroutine(Dodge()); }
    }

    IEnumerator Dodge()
    {
        IsDodging = true; // Set Dodging State
        AlienEnergy -= DodgeEnergyCost; // Consume Energy

        Vector3 DodgeDir = MoveDir.magnitude > 0.1f ? MoveDir.normalized : transform.forward; // Dodge in move direction or forward
        float Timer = 0f;

        Controller.enabled = false; // Disable Collision for Phase Effect

        while (Timer < DodgeDuration)
        {
            Timer += Time.deltaTime;
            Vector3 DodgeMove = DodgeDir * (DodgeDistance / DodgeDuration) * Time.deltaTime;
            DodgeMove.y = 0f; // Lock Y to prevent phasing through floor
            transform.position += DodgeMove;
            yield return null;
        }

        Controller.enabled = true; // Re-enable Collision after Phase
        DodgeCooldownTimer = DodgeCooldown; // Start Cooldown
        IsDodging = false; // End Dodge
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
        HandleSwing(); // Handle Swing Physics

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
        if (HasEnergyConverter) // Only absorb damage if Energy Converter is equipped
        {
            float AbsorbedAmount = Amount * AbsorbRatio; // Calculate Absorbed Amount
            AlienEnergy = Mathf.Min(AlienEnergy + AbsorbedAmount, AlienEnergyMax); // Add to Alien Energy
            Amount = (int)(Amount - AbsorbedAmount); // Reduce actual damage by absorbed amount
        }

        if (Shield > 0) { Shield -= Amount; }

        if (Shield <= 0)
        {
            if (CurrMomentum > 0)
            {
                float DamageRatio = Mathf.Clamp01((float)Amount / Health);
                CurrMomentum *= (1f - DamageRatio);
            }

            Health -= Amount;
            if (Health <= 0)
            {
                GameManager.Instance.YouLose();
                Death();
            }
            StartCoroutine(FlashDamageScreen());
        }
        else { StartCoroutine(FlashShieldScreen()); }

        UpdatePlayerUI();
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
        if (!HasPhaseBoots) return; // Wall Run requires Phase Boots

        if (!IsExitWallRun)
        {
            if (WallRunTimer >= MaxWallRunTime) { IsWallRunning = false; WallRunTimer = 0; }
            {
                WallRunTimer += Time.deltaTime;
                CheckForWall();

                if ((IsRightWall || IsLeftWall) && MoveDir.magnitude > 0.1f && AboveGround() && !Controller.isGrounded && !IsExitWallRun)
                { IsWallRunning = true; } 
                else { IsWallRunning = false; }

                if (IsWallRunning) { WallRunMovement(); }
            }
        }
        else
        {
            if (ExitTimer >= ExitTime) { IsExitWallRun = false; ExitTimer = 0; }
            else { ExitTimer += Time.deltaTime; }
        }
    }

    void WallRunMovement()
    {
        if (AlienEnergy <= 0) { IsWallRunning = false; return; } // Stop Wall Run if no Energy
        AlienEnergy = Mathf.Max(AlienEnergy - WallRunEnergyCost * Time.deltaTime, 0); // Drain Energy while Wall Running

        PlayerVel.y -= Gravity * Time.deltaTime;
        PlayerVel.y += GravityCounterForce * Time.deltaTime;

        Vector3 Normal = IsRightWall ? RightWall.normal : LeftWall.normal;
        Vector3 WallForward = Vector3.Cross(Normal, transform.up);

        if ((transform.forward - WallForward).magnitude > (transform.forward - -WallForward).magnitude)
        { WallForward = -WallForward; }

        if (Vector3.Dot(WallForward, transform.forward) < 0)
        { WallForward = -WallForward; }

        float VerticalDot = Vector3.Dot(Camera.main.transform.forward, Vector3.up);
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
        if (!HasPhaseBoots) return; // Grapple requires Phase Boots
        Debug.Log("StartGrapple Called");

        if (GrapplingCooldownTimer > 0) { Debug.Log("On Cooldown"); return; }
        if (AlienEnergy < GrappleEnergyCost) { Debug.Log("Not enough energy"); return; } // Guard

        RaycastHit GrappleHit;
        if (Physics.Raycast(CamTransform.position, CamTransform.forward, out GrappleHit, MaxGrappleDistance))
        {
            if (GrappleHit.collider.CompareTag("Grapple"))
            {
                IsGrapple = true;
                AlienEnergy -= GrappleEnergyCost; // Consume Energy on Grapple
                Debug.Log("Grapple Started");
                GrapplePoint = GrappleHit.point;
                Debug.Log("Grapple Hit: " + GrappleHit.collider.name + " at " + GrapplePoint);
                AttemptGrapple = true;
                GrappleLine.enabled = true;
                GrappleLine.positionCount = RopePoints;
                StartCoroutine(ExtendGrappleLine(GrapplePoint));
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


    //===[Swing]===\\

    void StartSwing()
    {
        if (!HasPhaseBoots) return; // Swing requires Phase Boots

        RaycastHit SwingHit;
        if (Physics.Raycast(CamTransform.position, CamTransform.forward, out SwingHit, MaxSwingDistance))
        {
            if (SwingHit.collider.CompareTag("Grapple")) // Only swing on Grapple tagged objects
            {
                IsSwinging = true;
                SwingPoint = SwingHit.point; // Set Swing Point
                CurrentSwingLength = Vector3.Distance(transform.position, SwingPoint); // Set initial rope length
                GrappleLine.enabled = true;
                GrappleLine.positionCount = RopePoints;
                Debug.Log("Swing Started at: " + SwingPoint);
            }
        }
    }

    void StopSwing()
    {
        IsSwinging = false; // Stop Swinging
        GrappleLine.enabled = false; // Disable Rope
        Debug.Log("Swing Stopped");
    }

    void HandleSwing()
    {
        if (!IsSwinging) return;

        // Change rope length with scroll wheel
        float Scroll = Input.GetAxis("Mouse ScrollWheel"); // Get Scroll Wheel Input
        if (Scroll != 0)
        {
            CurrentSwingLength = Mathf.Clamp( // Clamp rope length
                CurrentSwingLength - Scroll * SwingLengthChangeSpeed, // Change length by scroll
                SwingLengthMin, // Min length
                SwingLengthMax); // Max length
        }

        // Stop swing on key release
        if (Input.GetButtonUp("Fire2")) { StopSwing(); return; }

        Vector3 DirToPoint = SwingPoint - transform.position; // Direction to Swing Point
        float DistToPoint = DirToPoint.magnitude; // Distance to Swing Point

        // Pull toward swing point if rope is taut
        if (DistToPoint > CurrentSwingLength) // If rope is taut...
        {
            Vector3 Pull = DirToPoint.normalized * (DistToPoint - CurrentSwingLength); // Calculate Pull Force
            MomentumVelocity += Pull * SwingSpeed * Time.deltaTime; // Apply Pull to Momentum
        }

        // Apply gravity while swinging
        PlayerVel.y -= Gravity * Time.deltaTime; // Apply Gravity while Swinging

        // Omni directional — apply mouse input as swing direction influence
        float MouseX = Input.GetAxisRaw("Mouse X") * 0.1f; // Get Mouse X Input
        float MouseY = Input.GetAxisRaw("Mouse Y") * 0.1f; // Get Mouse Y Input
        Vector3 SwingInfluence = transform.right * MouseX + transform.up * MouseY; // Combine into Swing Influence
        MomentumVelocity += SwingInfluence * SwingSpeed * Time.deltaTime; // Apply Swing Influence

        // Update rope visual
        UpdateRopePoints(new Vector3(
            CamTransform.position.x,
            CamTransform.position.y - 0.5f,
            CamTransform.position.z), SwingPoint); // Update Rope Points
    }
}